using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

public class TurrentAuthoring : MonoBehaviour
{
    public float speed = 1f;
    public GameObject bulletPrefab;
    public GameObject bulletSpawnPos;
    [Space]
    public Color color = new Color(1,1,1,1);

    public float4 float4Color
    {
        get
        {
            return new Unity.Mathematics.float4(color.r, color.g, color.b, color.a);
        }
    }

    class Baker : Baker<TurrentAuthoring>
    {
        public override void Bake(TurrentAuthoring authoring)
        {
            //获取实体
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            //添加旋转组件和发射组件
            AddComponent(entity, new TurrentRotation
            {
                speed = authoring.speed,
                isPositiveDirection = true
            });
            AddComponent(entity, new TurrentShoot
            {
                bulletPrefab = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                bulletSpawnPos = GetEntity(authoring.bulletSpawnPos, TransformUsageFlags.Dynamic),
                bulletsCount = 1,
                bulletSpeed = 2,
                canShot = true,
            });
            AddComponent<URPMaterialPropertyBaseColor>(entity, new URPMaterialPropertyBaseColor
            {
                Value = authoring.float4Color
            });
        }
    }
}
