using UnityEngine;
using Unity.Entities;

public struct TurrentShoot : IComponentData
{
    public Entity bulletPrefab;
    public Entity bulletSpawnPos;

    //当前待发射子弹数
    public int bulletsCount;
    //子弹速度
    public float bulletSpeed;
    public bool canShot;

    public float shotCD;
}