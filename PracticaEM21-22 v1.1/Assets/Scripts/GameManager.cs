using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;


public class GameManager : NetworkBehaviour
{

    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameObject prefab;
    [SerializeField] UIManager uiManager;
    private GameObject[] respawns;
   public NetworkVariable<bool> canStart = new NetworkVariable<bool>();
    int lastRespawn;
    int maxPlayers=4;
    int minPlayers = 2;
    Dictionary<ulong,PlayerSettings> PlayersConnected = new Dictionary<ulong, PlayerSettings>();
    public NetworkVariable<int> currentPlayers = new NetworkVariable<int>();
    private void Awake()
    {
        //Busca los puntos de respawn que hay en el mapa
        respawns = GameObject.FindGameObjectsWithTag("Respawn");
        currentPlayers.Value = 0;
        //Observa si se ha iniciado el servidor o si se han conectado clientes
        networkManager.OnServerStarted += OnServerReady;
        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        uiManager.OnStartGame.AddListener(StartGameOnServerRpc);
        currentPlayers.OnValueChanged += OnStateChanged;
        canStart.Value = false;
    }

    private void OnDestroy()
    {
        networkManager.OnServerStarted -= OnServerReady;
        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        currentPlayers.OnValueChanged -= OnStateChanged;
        uiManager.OnStartGame.RemoveListener(StartGameOnServerRpc);


    }
    private void OnServerReady()
    {

    }
    private void OnDisconnectedFromServer()
    {
        
    }
    private void OnClientConnected(ulong clientId)
    {


        //si un cliente se conecta lo inicializa en uno de los puntos de spawn de manera aleatoria
        if (networkManager.IsServer)
        {
            currentPlayers.Value++;
            if (currentPlayers.Value > maxPlayers)
            {
                var lastClient = networkManager.ConnectedClientsIds[currentPlayers.Value-1];
                //networkManager.DisconnectClient(lastClient);
                DisconnectedByServer_ClientRpc(lastClient);
            }
            else
            {
                int randomNumber = Random.Range(1, 7);
                if (randomNumber == lastRespawn)
                {
                    randomNumber = Random.Range(1, 7);
                }
                lastRespawn = randomNumber;
                PlayerSettings playerSettings = new PlayerSettings(clientId.ToString(), 0, randomNumber, clientId);
                PlayersConnected.Add(clientId, playerSettings);
                UpdateLobbyOnClientRpc(currentPlayers.Value);
            }
            
            // player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
    private void OnClientDisconnected(ulong clientId)
    {



        //si un cliente se desconecta se quita de la lista de jugadores en linea
        if (networkManager.IsServer)
        {
          PlayersConnected.Remove(clientId);
           currentPlayers.Value--;
            UpdateLobbyOnClientRpc(currentPlayers.Value);
            // player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
    void OnStateChanged(int previous, int current)
    {
        if (current>=minPlayers && current <=maxPlayers)
        {
            canStart.Value = true;
            uiManager.activatePlay();
        }
    }
    [ClientRpc]
    void DisconnectedByServer_ClientRpc(ulong lastClientID)
    {
        if(networkManager.LocalClientId == lastClientID)
        {
            Debug.Log("Se desconecta el ultimo");
            uiManager.ActivateLobbyFullWarning();
            NetworkManager.Singleton.Shutdown();
        }

    }

    [ClientRpc]
    void UpdateLobbyOnClientRpc(int currentPlayerIdx)
    {
        for (int i = 1; i <= maxPlayers; i++)
        {
            if (i <= currentPlayerIdx)
            {
                uiManager.setPlayerOnLobby(i, prefab);
            }
            else
            {
                uiManager.setNoneOnLobby(i);
            }
        }
        
    }
    [ServerRpc(RequireOwnership = false)]
    void StartGameOnServerRpc()
    {

        foreach (var item in PlayersConnected)
        {
            var client_id = item.Key;
            var plyrStt = item.Value;
            var player = Instantiate(prefab, respawns[plyrStt.respawnID].transform);     
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client_id);
        }
        StartGameOnClientRpc();
    }
    [ClientRpc]
    void StartGameOnClientRpc()
    {
        uiManager.ActivateInGameHUD();
    }
}
 class PlayerSettings
{
    public string name;
    public int skin;
    public int respawnID;
    public ulong clientID;

   public PlayerSettings(string _name, int _skin, int _respawnID, ulong _clientID)
    {
        this.name = _name;
        this.skin = _skin;
        this.respawnID = _respawnID;
        this.clientID = _clientID;
    }
    
}