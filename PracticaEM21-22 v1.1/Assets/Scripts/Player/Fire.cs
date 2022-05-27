using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Fire : NetworkBehaviour
{
    #region Variables

    [SerializeField] public Transform bulletSpawn;
    [SerializeField] public GameObject bulletPrefab;
    public float speed;

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
        handler.OnFire.AddListener(FireBullet);
    }

    private void OnDisable()
    {
        handler.OnFire.RemoveListener(FireBullet);
    }

    #endregion

    #region RPC

    #region ServerRPC

    [ServerRpc]
    void FireOnServerRpc(Vector2 bulletForce,Vector3 spawnPos)
    {
        //se instancia la bala dada una posicion de spawn y sin ninguna rotacion
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        //se le añade la fuerza
        bullet.GetComponent<Rigidbody2D>().AddForce(bulletForce, ForceMode2D.Impulse);

        //se le añade un proprietario para luego usarlo en las colisiones
        bullet.GetComponent<Bullet>().owner = player.OwnerClientId;
        bullet.GetComponent<NetworkObject>().Spawn();
    }
    #endregion
    void FireBullet()
    {
        //se calcula la fuerza
        var force = bulletSpawn.right * speed;
        //se envia mensaje al servidor para que spawnee la bala
        FireOnServerRpc(force, bulletSpawn.position);
    }
    #endregion

}
