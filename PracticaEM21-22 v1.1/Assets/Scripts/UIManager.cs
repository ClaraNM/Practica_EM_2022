using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class UIManager : MonoBehaviour
{

    #region Variables

    public static UIManager Instance;

    [SerializeField] NetworkManager networkManager;
    UnityTransport transport;
    readonly ushort port = 7777;

    [SerializeField] Sprite[] hearts = new Sprite[3];

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenu;
    //[SerializeField] private Button buttonHost;
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

    [Header("In-Game HUD")]
    [SerializeField] private GameObject inGameHUD;
    [SerializeField] RawImage[] heartsUI = new RawImage[3];

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        transport = (UnityTransport)networkManager.NetworkConfig.NetworkTransport;
        Instance = this;
    }

    private void Start()
    {
        //buttonHost.onClick.AddListener(() => StartHost());
        buttonClient.onClick.AddListener(() => StartSelectorPlayer());
        buttonServer.onClick.AddListener(() => StartServerIP());
        buttonBackServer.onClick.AddListener(() => ActivateMainMenu());
        buttonBackSelector.onClick.AddListener(() => ActivateMainMenu());
        buttonExit.onClick.AddListener(() => StartExit());
        buttonPlay.onClick.AddListener(() => StartClient());
        buttonServerPlay.onClick.AddListener(() => StartServer());
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
    }

    private void ActivateClient()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(true);
        ServerHUD.SetActive(false);
    }

    private void ActivateServer()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(true);
    }

    private void ActivateInGameHUD()
    {
        mainMenu.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(false);
        inGameHUD.SetActive(true);

        // for test purposes
        //UpdateLifeUI(Random.Range(1, 6));
    }

    private void ActivateModeViewer() 
    {
        mainMenu.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(false);
        inGameHUD.SetActive(false);
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

    //private void StartHost()
    //{
    //    NetworkManager.Singleton.StartHost();
    //    ActivateInGameHUD();
    //}

    private void StartSelectorPlayer()
    {
        ActivateClient();
    }

    private void StartClient() 
    {
        var ip = inputFieldServerIP.text;
        if (!string.IsNullOrEmpty(ip))
        {
            transport.SetConnectionData(ip, port);
        }
        NetworkManager.Singleton.StartClient();
        ActivateInGameHUD();
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

    #endregion

}
