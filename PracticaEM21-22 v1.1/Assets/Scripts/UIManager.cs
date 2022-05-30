using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{

    #region Variables

    public static UIManager Instance;
    public UnityEvent OnStartGame;

    [SerializeField] NetworkManager networkManager;
    UnityTransport transport;
    readonly ushort port = 7777;

    [SerializeField] Sprite[] hearts = new Sprite[3];

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenu;
  //  [SerializeField] private Button buttonHost;
    [SerializeField] private Button buttonClient;
    [SerializeField] private Button buttonServer;
    [SerializeField] private Button buttonExit;

    [Header("Selector Type HUD")]
    [SerializeField] private GameObject SelectorTypeHUD;
    [SerializeField] private InputField inputFieldNamePlayer;
    [SerializeField] private InputField inputFieldServerIP;
    [SerializeField] private Button buttonType;
    [SerializeField] private Button buttonType2;
    [SerializeField] private Button buttonType3;
    [SerializeField] private Button buttonType4;
    [SerializeField] private Button buttonPlay;
    [SerializeField] private Button buttonBackSelector;

    [Header("Server HUD")]
    [SerializeField] private GameObject ServerHUD;
    [SerializeField] private InputField inputFieldIP;
    [SerializeField] private Button buttonServerPlay;
    [SerializeField] private Button buttonBackServer;

    [Header("Lobby")]
    [SerializeField] private GameObject Lobby;
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    [SerializeField] private GameObject player3;
    [SerializeField] private GameObject player4;
    [SerializeField] private GameObject buttonReady;
    [SerializeField] private Button buttonExitLobby;

    [Header("Lobby Full Warning")]
    [SerializeField] private GameObject LobbyFull;
    [SerializeField] private Button buttonCloseWarning;

    [Header("In-Game HUD")]
    [SerializeField] private GameObject inGameHUD;



    [SerializeField] RawImage[] heartsUI = new RawImage[3];

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        transport = (UnityTransport)networkManager.NetworkConfig.NetworkTransport;
    }

    private void Start()
    {
        //buttonHost.onClick.AddListener(() => StartHost());
       // buttonClient.onClick.AddListener(() => StartSelectorPlayer());
        buttonServer.onClick.AddListener(() => StartServerIP());
        buttonBackServer.onClick.AddListener(() => ActivateMainMenu());
        buttonBackSelector.onClick.AddListener(() => ActivateMainMenu());
        buttonExit.onClick.AddListener(() => StartExit());
        buttonClient.onClick.AddListener(() => StartSelectorPlayer());
        buttonServer.onClick.AddListener(() => StartServer());
        buttonPlay.onClick.AddListener(() =>ActivateLobby());
        buttonPlay.onClick.AddListener(() => StartClient());
        buttonExitLobby.onClick.AddListener(() => StartSelectorPlayer());
        buttonExitLobby.onClick.AddListener(() => DisconectClient());
        buttonCloseWarning.onClick.AddListener(() => CloseWarning());
        buttonReady.GetComponent<Button>().onClick.AddListener(() => StartGame());
        ActivateMainMenu();
    }

    #endregion

    #region UI Related Methods

    private void ActivateMainMenu()
    {
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(false);
        Lobby.SetActive(false);
        LobbyFull.SetActive(false);

    }
    private void ActivateClient()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(true);
        ServerHUD.SetActive(false);
        LobbyFull.SetActive(false);
        Lobby.SetActive(false);

    }

    private void ActivateServer()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(true);
        LobbyFull.SetActive(false);

        Lobby.SetActive(false);

    }
    public void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        Lobby.SetActive(false);
        inGameHUD.SetActive(true);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(false);
        LobbyFull.SetActive(false);

        // for test purposes
        //UpdateLifeUI(Random.Range(1, 6));
    }
    private void ActivateModeViewer()
    {
        mainMenu.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(false);
        inGameHUD.SetActive(false);
        Lobby.SetActive(false);
        LobbyFull.SetActive(false);


    }
    private void ActivateLobby()
    {
        mainMenu.SetActive(false);
        Lobby.SetActive(true);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(false);
        LobbyFull.SetActive(false);

    }
    public void ActivateLobbyFullWarning()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(true);
        ServerHUD.SetActive(false);
        Lobby.SetActive(false);
        LobbyFull.SetActive(true);

    }
    private void DeactivateLobbyFullWarning()
    {

        LobbyFull.SetActive(false);


    }
    public void UpdateLifeUI(int hitpoints)
    {
        switch (hitpoints)
        {
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

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        ActivateInGameHUD();
    }
    private void StartSelectorPlayer()
    {
        ActivateClient();
    }

    private void StartClient()
    {
       var ip = inputFieldIP.text;
        if (!string.IsNullOrEmpty(ip))
        {
            transport.SetConnectionData(ip, port);
        }
        NetworkManager.Singleton.StartClient();
        //ActivateInGameHUD();
        //StartSelectorPlayer();
    }
    private void DisconectClient()
    {
        NetworkManager.Singleton.Shutdown();
    }
    private void StartServerIP()
    {

        ActivateServer();
    }

    private void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        ActivateModeViewer();
    }
    private void StartExit() { Application.Quit(); }
    private void StartGame()
    {
       OnStartGame?.Invoke();
    }

   public void setPlayerOnLobby(int idxPlayer, GameObject player)
    {
        Text name_Player = player.transform.GetChild(2).gameObject.transform.GetChild(0).GetComponent<Text>();
        SpriteRenderer sprite_Player = player.GetComponent<SpriteRenderer>();

        switch (idxPlayer-1)
        {
            case 0:
                GameObject namePlayer = player1.transform.GetChild(0).gameObject;
                GameObject imagePlayer = player1.transform.GetChild(1).gameObject;
                namePlayer.GetComponent<Text>().text = name_Player.text;
                imagePlayer.GetComponent<Image>().sprite=sprite_Player.sprite;
                break;
            case 1:
                GameObject namePlayer2 = player2.transform.GetChild(0).gameObject;
                GameObject imagePlayer2 = player2.transform.GetChild(1).gameObject;
                namePlayer2.GetComponent<Text>().text = name_Player.text;
                imagePlayer2.GetComponent<Image>().sprite = sprite_Player.sprite;
                break;
            case 2:
                GameObject namePlayer3 = player3.transform.GetChild(0).gameObject;
                GameObject imagePlayer3 = player3.transform.GetChild(1).gameObject;
                namePlayer3.GetComponent<Text>().text = name_Player.text;
                imagePlayer3.GetComponent<Image>().sprite = sprite_Player.sprite;
                break;
            case 3:
                GameObject namePlayer4 = player4.transform.GetChild(0).gameObject;
                GameObject imagePlayer4 = player4.transform.GetChild(1).gameObject;
                namePlayer4.GetComponent<Text>().text = name_Player.text;
                imagePlayer4.GetComponent<Image>().sprite = sprite_Player.sprite;
                break;
            default:
                break;
        }
    }
    public void setNoneOnLobby(int idxCell)
    {


        switch (idxCell - 1)
        {
            case 0:
                GameObject namePlayer = player1.transform.GetChild(0).gameObject;
                GameObject imagePlayer = player1.transform.GetChild(1).gameObject;
                namePlayer.GetComponent<Text>().text = "Waiting";
                imagePlayer.GetComponent<Image>().sprite = null;
                break;
            case 1:
                GameObject namePlayer2 = player2.transform.GetChild(0).gameObject;
                GameObject imagePlayer2 = player2.transform.GetChild(1).gameObject;
                namePlayer2.GetComponent<Text>().text = "Waiting";
                imagePlayer2.GetComponent<Image>().sprite = null;
                break;
            case 2:
                GameObject namePlayer3 = player3.transform.GetChild(0).gameObject;
                GameObject imagePlayer3 = player3.transform.GetChild(1).gameObject;
                namePlayer3.GetComponent<Text>().text = "Waiting";
                imagePlayer3.GetComponent<Image>().sprite = null;
                break;
            case 3:
                GameObject namePlayer4 = player4.transform.GetChild(0).gameObject;
                GameObject imagePlayer4 = player4.transform.GetChild(1).gameObject;
                namePlayer4.GetComponent<Text>().text = "Waiting";
                imagePlayer4.GetComponent<Image>().sprite = null;
                break;
            default:
                break;
        }
    }
    public void activatePlay()
    {
        buttonReady.SetActive(true);
    }
    private void CloseWarning()
    {
        DeactivateLobbyFullWarning();
    }
    #endregion

}
