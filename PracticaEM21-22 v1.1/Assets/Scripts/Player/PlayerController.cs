using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(InputHandler))]
public class PlayerController : NetworkBehaviour
{

    #region Variables

    readonly float speed = 3.4f;
    readonly float jumpHeight = 6.5f;
    readonly float gravity = 1.5f;
    readonly int maxJumps = 2;

    LayerMask _layer;
    int _jumpsLeft;

    // https://docs.unity3d.com/2020.3/Documentation/ScriptReference/ContactFilter2D.html
    ContactFilter2D filter;
    InputHandler handler;
    Player player;
    Rigidbody2D rb;
    new CapsuleCollider2D collider;
    Animator anim;
    SpriteRenderer spriteRenderer;

    // https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable
    NetworkVariable<Vector2> vel;
    NetworkVariable<bool> FlipSprite;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<CapsuleCollider2D>();
        handler = GetComponent<InputHandler>();
        player = GetComponent<Player>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        vel = new NetworkVariable<Vector2>();
        FlipSprite = new NetworkVariable<bool>();
    }

    private void OnEnable()
    {
        handler.OnMove.AddListener(UpdatePlayerVisualsServerRpc);
        handler.OnJump.AddListener(PerformJumpServerRpc);
        handler.OnMoveFixedUpdate.AddListener(UpdatePlayerPositionServerRpc);
      //  handler.OnMousePosition.AddListener(UpdateMousePositionServerRpc);
      //  handler.OnHook.AddListener(PerformHookServerRpc);
      //  handler.OnHookRender.AddListener(UpdateHookVisualServerRpc);
        FlipSprite.OnValueChanged += OnFlipSpriteValueChanged;
    }

    private void OnDisable()
    {
        handler.OnMove.RemoveListener(UpdatePlayerVisualsServerRpc);
        handler.OnJump.RemoveListener(PerformJumpServerRpc);
        handler.OnMoveFixedUpdate.RemoveListener(UpdatePlayerPositionServerRpc);
       // handler.OnMousePosition.RemoveListener(UpdateMousePositionServerRpc);
       // handler.OnHook.RemoveListener(PerformHookServerRpc);
      //  handler.OnHookRender.RemoveListener(UpdateHookVisualServerRpc);
        FlipSprite.OnValueChanged -= OnFlipSpriteValueChanged;
    }

    void Start()
    {
        // Configure Rigidbody2D
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = gravity;

        // Configure LayerMask
        _layer = LayerMask.GetMask("Obstacles");

        // Configure ContactFilter2D
        filter.minNormalAngle = 45;
        filter.maxNormalAngle = 135;
        filter.useNormalAngle = true;
        filter.layerMask = _layer;
    }

    #endregion

    #region RPC

    #region ServerRPC

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void UpdatePlayerVisualsServerRpc(Vector2 input)
    {
        UpdateAnimatorStateServerRpc();
        UpdateSpriteOrientation(input);
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void UpdateAnimatorStateServerRpc()
    {
        if (IsGrounded)
        {
            anim.SetBool("isGrounded", true);
            anim.SetBool("isJumping", false);
        }
        else
        {
            anim.SetBool("isGrounded", false);
        }
      //  UpdateAnimatorStateClientRpc(anim.GetBool("isGrounded"), anim.GetBool("isJumping"));
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void PerformJumpServerRpc()
    {
        if (IsServer)
        {
           // PerformJumpClientRpc(player.State.Value);
            if (player.State.Value == PlayerState.Grounded)
            {
                _jumpsLeft = maxJumps;
            }
            else if (_jumpsLeft == 0)
            {
                return;
            }
            
            player.State.Value = PlayerState.Jumping;
            anim.SetBool("isJumping", true);
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
            
            _jumpsLeft--;

        }
        
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void UpdatePlayerPositionServerRpc(Vector2 input)
    {
        if (IsServer)
        {
            //var playerState = player.State.Value;
            //var vel = rb.velocity;
            if (IsGrounded)
            {
                player.State.Value = PlayerState.Grounded;
            }

            if ((player.State.Value != PlayerState.Hooked))
            {
                rb.velocity = new Vector2(input.x * speed, rb.velocity.y);
              //  UpdatePlayerPositionClientRpc(player.State.Value, input);
            }
        }
        

    }
   [ClientRpc]
    void UpdateAnimatorStateClientRpc(bool is_Grounded, bool is_Jumping)
    {

        anim.SetBool("isJumping", is_Grounded);
        anim.SetBool("isGrounded", is_Jumping);
    }
    [ClientRpc]
    void PerformJumpClientRpc(PlayerState value)
    {
        var player_ = player;
        player_.State.Value = value;
        var rb_ = rb;
        if (player.State.Value == PlayerState.Grounded)
        {
            _jumpsLeft = maxJumps;
        }
        else if (_jumpsLeft == 0)
        {
            return;
        }

        player_.State.Value = PlayerState.Jumping;
        anim.SetBool("isJumping", true);
        rb_.velocity = new Vector2(rb.velocity.x, jumpHeight);
        _jumpsLeft--;
    }
    [ClientRpc]
    void UpdatePlayerPositionClientRpc(PlayerState value, Vector2 input)
    {
        var player_ = player;
        player_.State.Value = value;
        var rb_ = rb;

        if ((value != PlayerState.Hooked))
        {
            rb_.velocity = new Vector2(input.x * speed, rb.velocity.y);
        }
        
    }
    #endregion

    #endregion

    #region Methods

    void UpdateSpriteOrientation(Vector2 input)
    {
        if (input.x < 0)
        {
            FlipSprite.Value = false;
        }
        else if (input.x > 0)
        {
            FlipSprite.Value = true;
        }
    }

    void OnFlipSpriteValueChanged(bool previous, bool current)
    {
        spriteRenderer.flipX = current;
    }

    bool IsGrounded => collider.IsTouching(filter);
     
    #endregion

}
