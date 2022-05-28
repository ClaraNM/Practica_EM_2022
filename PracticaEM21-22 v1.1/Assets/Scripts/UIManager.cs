using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public static UIManager Instance;

    #region Variables

    [SerializeField] NetworkManager networkManager;
    UnityTransport transport;
    readonly ushort port = 7777;

    [SerializeField] Sprite[] hearts = new Sprite[3];

    [Header("Main Menu")] [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button buttonHost;
    [SerializeField] private Button buttonClient;
    [SerializeField] private Button buttonServer;
    [SerializeField] private InputField inputFieldIP;

    [Header("In-Game HUD")] [SerializeField]
    private GameObject inGameHUD;

    [Header("Name and Skin Selector")] [SerializeField]
    private GameObject nameSkinSelector;

    [SerializeField] public InputField nameField;
    [SerializeField] private Button buttonSkin0;
    [SerializeField] private Button buttonSkin1;
    [SerializeField] private Button buttonPlay;

    [SerializeField] RawImage[] heartsUI = new RawImage[3];

    public bool enterHostMode = false;
    public int skin = 0;

    #endregion

    #region Unity Event Functions

    private void Awake() {
        transport = (UnityTransport)networkManager.NetworkConfig.NetworkTransport;
        Instance = this;
    }

    private void Start() {
        buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartClient());
        buttonServer.onClick.AddListener(() => StartServer());
        buttonPlay.onClick.AddListener(() => OnEnterGame());

        buttonSkin0.onClick.AddListener(() => skin = 0);
        buttonSkin1.onClick.AddListener(() => skin = 1);
        ActivateMainMenu();
    }

    #endregion

    #region UI Related Methods

    private void ActivateMainMenu() {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        nameSkinSelector.SetActive(false);
    }

    private void ActivateInGameHUD() {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(true);
        nameSkinSelector.SetActive(false);
    }

    private void ActivateSkinHUD() {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        nameSkinSelector.SetActive(true);
    }

    public void UpdateLifeUI(int hitpoints) {
        switch (hitpoints) {
            case 6:
                heartsUI[0].texture = hearts[2].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 5:
                heartsUI[0].texture = hearts[1].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 4:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[2].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 3:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[1].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 2:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[0].texture;
                heartsUI[2].texture = hearts[2].texture;
                break;
            case 1:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[0].texture;
                heartsUI[2].texture = hearts[1].texture;
                break;
            case 0:
                heartsUI[0].texture = hearts[0].texture;
                heartsUI[1].texture = hearts[0].texture;
                heartsUI[2].texture = hearts[0].texture;
                break;
        }
    }

    #endregion

    #region Netcode Related Methods

    private void StartHost() {
        enterHostMode = true;
        ActivateSkinHUD();
    }

    private void StartClient() {
        enterHostMode = false;
        ActivateSkinHUD();
    }

    private void StartServer() {
        GameManager.Instance.EnableApprovalCallback();
        NetworkManager.Singleton.StartServer();
        ActivateInGameHUD();
    }

    private void OnEnterGame() {
        if (nameField.text.Length == 0) return;
        if (enterHostMode) {
            GameManager.Instance.EnableApprovalCallback();
            NetworkManager.Singleton.StartHost();
            ActivateInGameHUD();
        }
        else {
            var ip = inputFieldIP.text;
            if (!string.IsNullOrEmpty(ip)) {
                transport.SetConnectionData(ip, port);
            }

            if (NetworkManager.Singleton.StartClient()) {
                ActivateInGameHUD();
            }
        }
    }

    #endregion
}