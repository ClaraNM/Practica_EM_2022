using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Player : NetworkBehaviour {
    public const int MaxHealth = 6;
    public const int MaxLives = 2;

    #region Variables

    public Text nameField;
    public PlayerController controller;

    // https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable
    public NetworkVariable<int> Skin;

    public NetworkVariable<PlayerState> State;
    public NetworkVariable<int> Health;
    public NetworkVariable<int> Lives;
    public NetworkVariable<bool> Dead;

    #endregion

    #region Unity Event Functions

    private void Awake() {
        NetworkManager.OnClientConnectedCallback += ConfigurePlayer;

        State = new NetworkVariable<PlayerState>();
        Health = new NetworkVariable<int>(MaxHealth);
        Lives = new NetworkVariable<int>(MaxLives);
        Dead = new NetworkVariable<bool>(false);
        GameManager.Instance.AddPlayer(this);
    }

    private void OnEnable() {
        // https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkVariable-1.OnValueChangedDelegate
        State.OnValueChanged += OnPlayerStateValueChanged;
        Health.OnValueChanged += OnPlayerHealthValueChanged;
        Skin.OnValueChanged += OnPlayerSkinValueChanged;
    }

    private void OnDisable() {
        // https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkVariable-1.OnValueChangedDelegate
        State.OnValueChanged -= OnPlayerStateValueChanged;
        Health.OnValueChanged -= OnPlayerHealthValueChanged;
        Skin.OnValueChanged -= OnPlayerSkinValueChanged;
    }

    public override void OnDestroy() {
        base.OnDestroy();
        GameManager.Instance.RemovePlayer(this);
    }

    #endregion

    #region Config Methods

    void ConfigurePlayer(ulong clientID) {
        if (IsLocalPlayer) {
            ConfigurePlayer();
            ConfigureCamera();
            ConfigureControls();
            ChangePlayerSkinServerRpc(UIManager.Instance.skin);
            ChangePlayerNameServerRpc(UIManager.Instance.nameField.text);
            UIManager.Instance.UpdateLifeUI(MaxHealth - Health.Value);
        }


        InternalSetSkin(Skin.Value);
        nameField.text = UIManager.Instance.nameField.text;
    }

    void ConfigurePlayer() {
        UpdatePlayerStateServerRpc(PlayerState.Grounded);
    }

    void ConfigureCamera() {
        // https://docs.unity3d.com/Packages/com.unity.cinemachine@2.6/manual/CinemachineBrainProperties.html
        var virtualCam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera;

        virtualCam.LookAt = transform;
        virtualCam.Follow = transform;
    }

    void ConfigureControls() {
        GetComponent<InputHandler>().enabled = true;
    }

    #endregion

    #region RPC

    #region ServerRPC

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state) {
        State.Value = state;
    }

    [ServerRpc]
    public void ChangePlayerSkinServerRpc(int skin) {
        Skin.Value = skin;
    }

    [ServerRpc]
    public void ChangePlayerNameServerRpc(string name) {
        ChangePlayerNameClientRpc(name);
    }

    #endregion

    #region ClientRPC

    [ClientRpc]
    public void ChangePlayerNameClientRpc(string name) {
        nameField.text = name;
    }

    #endregion

    #endregion

    #region Netcode Related Methods

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    void OnPlayerStateValueChanged(PlayerState previous, PlayerState current) {
    }

    void OnPlayerHealthValueChanged(int previous, int current) {
        if (IsLocalPlayer) {
            UIManager.Instance.UpdateLifeUI(MaxHealth - current);
        }
    }

    void OnPlayerSkinValueChanged(int previous, int current) {
        InternalSetSkin(current);
    }

    #endregion

    public void Damage() {
        if (Dead.Value) return;
        var aux = Health.Value - 1;
        if (aux == 0) {
            Health.Value = MaxHealth;
            if (GameManager.Instance.status.Value == GameManager.Status.Playing) {
                Lives.Value--;
                if (Lives.Value == 0) {
                    // Dead
                    controller.enabled = false;
                    Dead.Value = true;
                    transform.rotation = Quaternion.Euler(0, 0, 90);
                    GameManager.Instance.CheckFinishConditions();
                }
            }

            if (!Dead.Value) {
                State.Value = PlayerState.Grounded;
                var points = GameManager.Instance.spawnPoints;
                if (points.Length > 0) {
                    var point = points[Random.Range(0, points.Length)];
                    transform.position = point.position;
                }
            }
        }
        else {
            Health.Value = aux;
        }
    }

    private void InternalSetSkin(int skin) {
        var animator = GetComponent<Animator>();
        if (skin >= 0 && skin < GameManager.Instance.skins.Length) {
            animator.runtimeAnimatorController = GameManager.Instance.skins[skin];
        }
    }
}

public enum PlayerState {
    Grounded = 0,
    Jumping = 1,
    Hooked = 2
}