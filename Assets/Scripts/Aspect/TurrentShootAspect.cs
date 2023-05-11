using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

public readonly partial struct TurrentShootAspect : IAspect
{
    //[NativeDisableParallelForRestriction]
    private readonly RefRW<TurrentShoot> _turrentShoot;

    [Optional] readonly RefRO<URPMaterialPropertyBaseColor> m_BaseColor;

    public Entity BulletPrefab => _turrentShoot.ValueRO.bulletPrefab;
    public Entity BulletSpawnPos => _turrentShoot.ValueRO.bulletSpawnPos;

    public int BulletsCount
    {
        get { return _turrentShoot.ValueRO.bulletsCount; }
        set { _turrentShoot.ValueRW.bulletsCount = value; }
    }
    public float BulletsSpeed => _turrentShoot.ValueRO.bulletSpeed;
    public float4 BaseColor => m_BaseColor.ValueRO.Value;

    public bool CanShot
    {
        get => _turrentShoot.ValueRO.canShot;
        set => _turrentShoot.ValueRW.canShot = value;
    }
    public float CD
    {
        get => _turrentShoot.ValueRO.shotCD;
        set => _turrentShoot.ValueRW.shotCD = value;
    }
    
}
