using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Collections;

public class GameManager : NetworkBehaviour
{

    [SerializeField] NetworkManager networkManager;
    [SerializeField] GameObject [] playerPrefabs;
    [SerializeField] UIManager uiManager;
     GameObject[] respawns;
   public NetworkVariable<bool> canStart = new NetworkVariable<bool>();
    int lastRespawn;
    int maxPlayers=4;
    int minPlayers = 2;
    int deaths = 0;
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
        uiManager.OnPlayerDead.AddListener(OnDeadSignClientRpc);
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
        uiManager.OnPlayerDead.AddListener(OnDeadSignClientRpc);


    }
    private void OnServerReady()
    {

    }
    private void OnDisconnectedFromServer()
    {
        
    }
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log(IsClient);
        Debug.Log(networkManager.IsClient);
        Debug.Log(IsOwner);
        if (networkManager.IsClient)
        {
            Debug.Log("Entra en networkManagerisclient");
            SendSkinChoiceToServerRpc(clientId,uiManager.skinChoice);
        }

        //si un cliente se conecta lo inicializa en uno de los puntos de spawn de manera aleatoria
        if (networkManager.IsServer)
        {
            currentPlayers.Value++;
            if (currentPlayers.Value > maxPlayers)
            {
                var lastClient = networkManager.ConnectedClientsIds[currentPlayers.Value-1];
                DisconnectedByServer_ClientRpc(lastClient);
            }
            else
            {
                //Posicion de spawn del jugador
                int randomNumber = Random.Range(1, 7);
                if (randomNumber == lastRespawn)
                {
                    randomNumber = Random.Range(1, 7);
                }
                lastRespawn = randomNumber;

                
                PlayerSettings playerSettings = new PlayerSettings("3", 0, randomNumber, clientId);
                PlayersConnected.Add(clientId, playerSettings);

               
                UpdateLobbyOnClientRpc(currentPlayers.Value);
            }

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
        }
    }
    void OnStateChanged(int previous, int current)
    {
        
        if (current>=minPlayers && current <=maxPlayers)
        {
            canStart.Value = true;
            uiManager.activatePlay();
        }
        else
        {
            canStart.Value=false;
        }
    }

    [ClientRpc]
    void DisconnectedByServer_ClientRpc(ulong lastClientID)
    {
        if(networkManager.LocalClientId == lastClientID)
        {
            uiManager.ActivateLobbyFullWarning();
            NetworkManager.Singleton.Shutdown();
        }

    }
    

    [ClientRpc]
    void UpdateLobbyOnClientRpc(int currentPlayerIdx)
    {
        for (int i = 0; i < maxPlayers; i++)
        {
            if (i < currentPlayerIdx)
            {
                uiManager.setPlayerOnLobby(i);
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
            var player = Instantiate(playerPrefabs[plyrStt.skin], respawns[plyrStt.respawnID].transform);     
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client_id);
        }
        StartGameOnClientRpc();
    }
    [ClientRpc]
    void OnDeadSignClientRpc(ulong clientTarget)
    {
         if(clientTarget == networkManager.LocalClientId)
        {
            uiManager.ActivateDeadSign();
            CountDeathsServerRpc();
        }
    }
    [ClientRpc]
    void StartGameOnClientRpc()
    {
        uiManager.ActivateInGameHUD();
    }
    [ServerRpc(RequireOwnership =false)]
    void CountDeathsServerRpc()
    {
        deaths++;
        if (networkManager.IsServer)
        {
            if(deaths == currentPlayers.Value-1)
            {
                deaths = 0;
                canStart.Value = false;
                GameEndedClientRpc();
            }
        }
    }
    [ServerRpc(RequireOwnership =false)]
    void SendSkinChoiceToServerRpc(ulong client, int skinChoosen)
    {
        PlayersConnected[client].skin = skinChoosen;
    }
    [ClientRpc]
    void GameEndedClientRpc()
    {
        uiManager.DeactivateDeadSign();
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