using UnityEngine;
using Unity.Netcode;

public class GrapplingHook : NetworkBehaviour
{
    #region Variables

    InputHandler handler;
    // https://docs.unity3d.com/2020.3/Documentation/ScriptReference/DistanceJoint2D.html
    DistanceJoint2D rope;
    // // https://docs.unity3d.com/2020.3/Documentation/ScriptReference/LineRenderer.html
    LineRenderer ropeRenderer;
    Transform playerTransform;
    [SerializeField] Material material;
    // https://docs.unity3d.com/2020.3/Documentation/ScriptReference/LayerMask.html
    LayerMask layer;
    Player player;

    readonly float climbSpeed = 2f;
    readonly float swingForce = 80f;

    Rigidbody2D rb;

    // https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable
    NetworkVariable<float> ropeDistance;

    #endregion

    #region Unity Event Functions

    void Awake()
    {
        handler = GetComponent<InputHandler>();
        player = GetComponent<Player>();

        //Configure Rope Renderer
        ropeRenderer = gameObject.AddComponent<LineRenderer>();
        ropeRenderer.startWidth = .05f;
        ropeRenderer.endWidth = .05f;
        ropeRenderer.material = material;
        ropeRenderer.sortingOrder = 3;
        ropeRenderer.enabled = false;

        // Configure Rope
        rope = gameObject.AddComponent<DistanceJoint2D>();
        rope.enableCollision = true;
        rope.enabled = false;

        playerTransform = transform;
        layer = LayerMask.GetMask("Obstacles");

        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();

        ropeDistance = new NetworkVariable<float>();
    }

    private void OnEnable()
    {
        handler.OnHookRender.AddListener(UpdateHookServerRpc);
        handler.OnMoveFixedUpdate.AddListener(SwingRopeServerRpc);
        handler.OnJump.AddListener(JumpPerformedServerRpc);
        handler.OnHook.AddListener(LaunchHookServerRpc);

        ropeDistance.OnValueChanged += OnRopeDistanceValueChanged;
    }

    private void OnDisable()
    {
        handler.OnHookRender.RemoveListener(UpdateHookServerRpc);
        handler.OnMoveFixedUpdate.RemoveListener(SwingRopeServerRpc);
        handler.OnJump.RemoveListener(JumpPerformedServerRpc);
        handler.OnHook.RemoveListener(LaunchHookServerRpc);

        ropeDistance.OnValueChanged -= OnRopeDistanceValueChanged;
    }

    #endregion

    #region Netcode RPC

    #region ServerRPC

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void UpdateHookServerRpc(Vector2 input)
    {
        if (player.State.Value == PlayerState.Hooked)
        {
          
            ClimbRope(input.y);

            //No se calcula el update de la cuerda solo en el cliente sino que se actualiza en el servidor para que tenga en cuenta los 
            //efectos que provoca en el movimiento del jugador, ya que los lleva el servidor, y se actualiza también en el cliente para
            //actualizar visualmente la cuerda.
            UpdateRope(); 
        }
        else if (player.State.Value == PlayerState.Grounded)
        {
            //Al igual que antes los cambios tienen que hacerse siempre sí o sí en el servidor
            RemoveRope();
            //En este caso también se envía un mensaje al cliente para que visualmente en el cliente también desaparezca la cuerda,
            //ya que no es un elemento con networkbehavior
            RemoveRopeClientRpc();
            rope.enabled = false;
            ropeRenderer.enabled = false;
        }
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void JumpPerformedServerRpc()
    {
        RemoveRope();
        RemoveRopeClientRpc();
        rope.enabled = false;
        ropeRenderer.enabled = false;
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void LaunchHookServerRpc(Vector2 input)
    {
        var hit = Physics2D.Raycast(playerTransform.position, input - (Vector2)playerTransform.position, Mathf.Infinity, layer);

        if (hit.collider)
        {
            var anchor = hit.centroid;
            rope.connectedAnchor = anchor;
            ropeRenderer.SetPosition(1, anchor);
            UpdateAnchor(hit.centroid);
            player.State.Value = PlayerState.Hooked;
        }
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void SwingRopeServerRpc(Vector2 input)
    {
        if (player.State.Value == PlayerState.Hooked)
        {
            // Player 2 hook direction
            var direction = (rope.connectedAnchor - (Vector2)playerTransform.position).normalized;

            // Perpendicular direction
            var forceDirection = new Vector2(input.x * direction.y, direction.x);

            var force = forceDirection * swingForce;

            rb.AddForce(force, ForceMode2D.Force);
        }
    }

    #endregion

    #region ClientRPC
    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/clientrpc
       [ClientRpc]
    void UpdateAnchorClientRpc(Vector2 anchor)
    {
        rope.connectedAnchor = anchor;
        ShowRope();
        ropeRenderer.SetPosition(1, anchor);
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/clientrpc
     [ClientRpc]
    void UpdateRopeClientRpc()
    {
        ropeRenderer.SetPosition(0, playerTransform.position);
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/clientrpc
       [ClientRpc]
    void RemoveRopeClientRpc()
    {
        RemoveRope();
    }

    #endregion

    #endregion

    #region Methods


    void UpdateAnchor(Vector2 anchor)
    {
        rope.connectedAnchor = anchor;
        ShowRope();
        ropeRenderer.SetPosition(1, anchor);
        UpdateAnchorClientRpc(anchor);
    }
    void UpdateRope()
    {
        ropeRenderer.SetPosition(0, playerTransform.position);
        UpdateRopeClientRpc();
    }

    void ShowRope()
    {
        rope.enabled = true;
        ropeRenderer.enabled = true;
    }

    void RemoveRope()
    {
        rope.enabled = false;
        ropeRenderer.enabled = false;
    }
    void ClimbRope(float input)
    {
        ropeDistance.Value = (input) * climbSpeed * Time.deltaTime;
    }

    void OnRopeDistanceValueChanged(float previous, float current)
    {
        rope.distance -= current;
    }


    #endregion
}
