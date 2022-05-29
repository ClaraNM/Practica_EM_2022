using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
   
    #region Variables

    // https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable
    public NetworkVariable<PlayerState> State;
    private GameObject[] respawns;


    //Vida
    public int NumHealth;

    public NetworkVariable<int> Vidas;
    public NetworkVariable<bool> isDead;

    public Player player;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        // NetworkManager.OnClientConnectedCallback += ConfigurePlayer;
        //ConfigurePlayer(this.OwnerClientId);
        respawns = GameObject.FindGameObjectsWithTag("Respawn");
        State = new NetworkVariable<PlayerState>();


        //Vida
        Vidas = new NetworkVariable<int>(NumHealth);
        isDead = new NetworkVariable<bool>(false);
    }
    private void Start()
    {
        ConfigurePlayer(this.OwnerClientId);
    }
    private void OnEnable()
    {
        // https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkVariable-1.OnValueChangedDelegate
        State.OnValueChanged += OnPlayerStateValueChanged;

        //Actualización de la vida
        Vidas.OnValueChanged += UpdateLives;
    }

    private void OnDisable()
    {
        // https://docs-multiplayer.unity3d.com/netcode/current/api/Unity.Netcode.NetworkVariable-1.OnValueChangedDelegate
        State.OnValueChanged -= OnPlayerStateValueChanged;

        Vidas.OnValueChanged -= UpdateLives;
    }

    #endregion

    #region Config Methods

    void ConfigurePlayer(ulong clientID)
    {
        if (IsLocalPlayer)
        {
            ConfigurePlayer();
            ConfigureCamera();
            ConfigureControls();
            UIManager.Instance.UpdateLifeUI(NumHealth - Vidas.Value);
        }
    }

    void ConfigurePlayer()
    {
       
       // PlayerRespawnPositionServerRpc();
        UpdatePlayerStateServerRpc(PlayerState.Grounded);
    }

    void ConfigureCamera()
    {
        // https://docs.unity3d.com/Packages/com.unity.cinemachine@2.6/manual/CinemachineBrainProperties.html
        var virtualCam = Camera.main.GetComponent<CinemachineBrain>().ActiveVirtualCamera;

        virtualCam.LookAt = transform;
        virtualCam.Follow = transform;
    }

    void ConfigureControls()
    {
        GetComponent<InputHandler>().enabled = true;
    }

    #endregion

    #region RPC

    #region ServerRPC

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    public void UpdatePlayerStateServerRpc(PlayerState state)
    {
        State.Value = state;
    }
    [ServerRpc]
    public void PlayerRespawnPositionServerRpc()
    {
        int randomNumber = Random.Range(1, 10);
        transform.position = respawns[randomNumber].transform.position;
    }
    #endregion

    #endregion

    #region Netcode Related Methods

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    void OnPlayerStateValueChanged(PlayerState previous, PlayerState current)
    {
        State.Value = current;
    }


    //Vida

    //Se actualiza la vida
    private void UpdateLives(int anterior, int actual)
    {
        //Si es el jugador local
        if (IsLocalPlayer)
        {
            UIManager.Instance.UpdateLifeUI(NumHealth - actual);
        }
    }


    //Se calcula la vida restante tras el impacto del disparo
    public void DamageDisparo()
    {
        if (isDead.Value == true)
        {
            //Si el personaje está muerto no se hace nada
            return;
        }
        else
        {
            //Se reducen las vidas que quedan
            var leftLives = Vidas.Value - 1;
            if (leftLives == 0)
            {

                Vidas.Value--;

                if (Vidas.Value == 0)
                {
                    //Muerte del personaje
                    isDead.Value = true;
                    player.GetComponent<NetworkObject>().Despawn();
                }

            }
            else
            {
                Vidas.Value = leftLives;

                //Aquí va el respawn

            }
        }
    }
    #endregion
}

public enum PlayerState
{
    Grounded = 0,
    Jumping = 1,
    Hooked = 2
}
