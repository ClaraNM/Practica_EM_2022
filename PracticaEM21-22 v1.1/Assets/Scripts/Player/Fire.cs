using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Fire : NetworkBehaviour
{
    #region Variables

    [SerializeField] public Transform crossHair;
    [SerializeField] public Transform bulletSpawn;
    [SerializeField] public Transform weapon;
    [SerializeField] public GameObject bulletPrefab;


    InputHandler handler;
    Player player;

    #endregion

    #region Unity Event Functions
    private void Awake()
    {
        handler = GetComponent<InputHandler>();
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        handler.OnFire.AddListener(CalculateDirectionRpc);
    }

    private void OnDisable()
    {
        handler.OnFire.RemoveListener(CalculateDirectionRpc);
    }

    #endregion

    #region RPC

    #region ServerRPC

    [ServerRpc]
    void FireServerRpc(Vector2 dir)
    {
        dir.Normalize();

        var position = weapon.transform.position;

        var bullet = Instantiate(bulletPrefab,
            position + new Vector3(dir.x, dir.y, 0) * 0.5f, Quaternion.identity);

        bullet.GetComponent<Rigidbody2D>().velocity = dir * bullet.GetComponent<Bullet>().speed;
        bullet.GetComponent<Bullet>().owner = player.OwnerClientId;

        bullet.GetComponent<NetworkObject>().Spawn(true);
    }
    #endregion

    #endregion

    #region Métodos
    void CalculateDirectionRpc()
    {
        var dir = bulletSpawn.transform.position - weapon.transform.position;
        FireServerRpc(dir);
    }
    #endregion
}
