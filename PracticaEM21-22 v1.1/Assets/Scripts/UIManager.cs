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
    public UnityEvent<ulong> OnPlayerDead;

    [SerializeField] NetworkManager networkManager;
    UnityTransport transport;
    readonly ushort port = 7777;
    public int skinChoice = 0;
    [SerializeField] Sprite[] hearts = new Sprite[3];

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private Button buttonClient;
    [SerializeField] private Button buttonServer;
    [SerializeField] private InputField ServerIP;
    [SerializeField] private Button buttonExit;

    [Header("Selector Type HUD")]
    [SerializeField] private GameObject SelectorTypeHUD;
    [SerializeField] public InputField inputFieldNamePlayer; 
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
    [SerializeField] private GameObject deadSignHUD;



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
        buttonServer.onClick.AddListener(() => StartServerIP());
        buttonBackServer.onClick.AddListener(() => ActivateMainMenu());
        buttonBackSelector.onClick.AddListener(() => ActivateMainMenu());
        buttonExit.onClick.AddListener(() => StartExit());
        buttonClient.onClick.AddListener(() => StartSelectorPlayer());
        buttonServerPlay.onClick.AddListener(() => StartServer());
        buttonPlay.onClick.AddListener(() =>ActivateLobby());
        buttonPlay.onClick.AddListener(() => StartClient());
        buttonExitLobby.onClick.AddListener(() => StartSelectorPlayer());
        buttonExitLobby.onClick.AddListener(() => DisconectClient());
        buttonCloseWarning.onClick.AddListener(() => CloseWarning());
        buttonType.onClick.AddListener(()=>ChooseGreenCH());
        buttonType2.onClick.AddListener(() => ChooseBlueCH());
        buttonType3.onClick.AddListener(() => ChoosePinkCH());
        buttonType4.onClick.AddListener(() => ChooseYellowCH());
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
        deadSignHUD.SetActive(false);

    }
    private void ActivateClient()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(true);
        ServerHUD.SetActive(false);
        LobbyFull.SetActive(false);
        Lobby.SetActive(false);
        deadSignHUD.SetActive(false);

    }

    private void ActivateServer()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(true);
        LobbyFull.SetActive(false);
        deadSignHUD.SetActive(false);
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
        deadSignHUD.SetActive(false);

    }
    private void ActivateModeViewer()
    {
        mainMenu.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(false);
        inGameHUD.SetActive(false);
        Lobby.SetActive(false);
        LobbyFull.SetActive(false);
        deadSignHUD.SetActive(false);

    }
    private void ActivateLobby()
    {
        mainMenu.SetActive(false);
        Lobby.SetActive(true);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(false);
        LobbyFull.SetActive(false);
        deadSignHUD.SetActive(false);

    }
    public void ActivateLobbyFullWarning()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(true);
        ServerHUD.SetActive(false);
        Lobby.SetActive(false);
        LobbyFull.SetActive(true);
        deadSignHUD.SetActive(false);

    }
    public void ActivateDeadSign()
    {
        mainMenu.SetActive(false);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(false);
        Lobby.SetActive(false);
        LobbyFull.SetActive(false);
        deadSignHUD.SetActive(true);

    }
    public void DeactivateDeadSign()
    {
        DisconectClient();
        mainMenu.SetActive(true);
        inGameHUD.SetActive(false);
        SelectorTypeHUD.SetActive(false);
        ServerHUD.SetActive(false);
        Lobby.SetActive(false);
        LobbyFull.SetActive(false);
        deadSignHUD.SetActive(false);

    }
    // Advertencia por si el lobby esta lleno
    private void DeactivateLobbyFullWarning()
    {
        LobbyFull.SetActive(false);
    }

    // La actualizacion del sprite de vida cada que se le dispara a un jugador
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

    //Activa la interfaz del cliente cuya primera pantalla es la etapa de seleccion de nombre y personaje
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
    }

    //El cliente se desconecta del servidor
    private void DisconectClient()
    {
        NetworkManager.Singleton.Shutdown();
    }

    //Activa la interfaz del servidor que consiste de un hueco para rellenar con la ip y un boton para levantar el servidor
    private void StartServerIP()
    {

        ActivateServer();
    }

    private void StartServer()
    {
        var ip = inputFieldIP.text;
        if (!string.IsNullOrEmpty(ip))
        {
            transport.SetConnectionData(ip, port);
        }
        NetworkManager.Singleton.StartServer();
        ActivateModeViewer();
    }

    //Cerrar la aplicaci�n directamente
    private void StartExit() { Application.Quit(); }

    //Llamada a un UnityEvent que provoca que los listeners que est�n en el GameManager ejecuten el comienze de partida simultaneo en todos los clientes
    private void StartGame()
    {
       OnStartGame?.Invoke();
    }
    //Invoca el evento que le comunica al servidor que un jugador ha muerto, el par�metro que pasa es la id del cliente muerto para que se sepa qu� jugador concreto es
    public void PlayerDead(ulong clientTarget)
    {
        OnPlayerDead?.Invoke(clientTarget);
    }
    #endregion

    #region Lobby
    //Recibe el indice para ver qu� hueco del lobby rellenar con el jugador
    public void setPlayerOnLobby(int idxPlayer)
    {

        switch (idxPlayer)
        {
            case 0:
                GameObject namePlayer = player1.transform.GetChild(1).gameObject;
                namePlayer.GetComponent<Text>().text = "Jugador Listo";
                break;
            case 1:
                GameObject namePlayer2 = player2.transform.GetChild(1).gameObject;
                namePlayer2.GetComponent<Text>().text = "Jugador Listo";
                break;
            case 2:
                GameObject namePlayer3 = player3.transform.GetChild(1).gameObject;
                namePlayer3.GetComponent<Text>().text = "Jugador Listo";
                break;
            case 3:
                GameObject namePlayer4 = player4.transform.GetChild(1).gameObject;
                namePlayer4.GetComponent<Text>().text = "Jugador Listo";
                break;
            default:
                break;
        }
    }
    //Rellena el hueco del lobby con un mensaje que indica que no hay un jugador ocupando ese hueco
    public void setNoneOnLobby(int idxCell)
    {
        switch (idxCell)
        {
            case 0:
                GameObject namePlayer = player1.transform.GetChild(1).gameObject;
                namePlayer.GetComponent<Text>().text = "Waiting";
                break;
            case 1:
                GameObject namePlayer2 = player2.transform.GetChild(1).gameObject;
                namePlayer2.GetComponent<Text>().text = "Waiting";
                break;
            case 2:
                GameObject namePlayer3 = player3.transform.GetChild(1).gameObject;
                namePlayer3.GetComponent<Text>().text = "Waiting";
                break;
            case 3:
                GameObject namePlayer4 = player4.transform.GetChild(1).gameObject;
                namePlayer4.GetComponent<Text>().text = "Waiting";
                break;
            default:
                break;
        }
    }
    // Aparece en pantalla el bot�n, previamente oculto, que permite dar comienzo a la partida
    public void activatePlay()
    {
        buttonReady.SetActive(true);
    }
    // Se desactiva la advertencia del lobby lleno
    private void CloseWarning()
    {
        DeactivateLobbyFullWarning();
    }
    // Escoges la skin del marciano verde
    private void ChooseGreenCH()
    {
        skinChoice = 0;
    }
    // Escoges la skin del marciano azul
    private void ChooseBlueCH()
    {
        skinChoice = 1;
    }
    // Escoges la skin del marciano rosa
    private void ChoosePinkCH()
    {
        skinChoice = 2;
    }
    // Escoges la skin del marciano amarillo
    private void ChooseYellowCH()
    {
        skinChoice = 3;
    }
    #endregion

}
