using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public partial struct TurretRotationSystem : ISystem
{
    //根据LocalTransform找到对应的实体
    public ComponentLookup<TurrentRotation> turrentRotationLookup;
    //public ComponentDataFromEntity<TurrentRotation> turrentRotationFromEntity;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurrentRotation>();
        turrentRotationLookup = SystemAPI.GetComponentLookup<TurrentRotation>(true);
    }


    public void OnUpdate(ref SystemState state)
    {
        var dt = SystemAPI.Time.DeltaTime;
        //这个表需要每帧更新
        turrentRotationLookup.Update(ref state);

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>().WithEntityAccess().WithAll<TurrentRotation>())
        {
            var rotationCompoment = turrentRotationLookup[entity];
            Vector3 euler = QuaternionToEuler(transform.ValueRO.Rotation);

            if (euler.y < -92f)
                rotationCompoment.isPositiveDirection = false;
            else if (euler.y > 2f)
                rotationCompoment.isPositiveDirection = true;

            ecb.SetComponent(entity, rotationCompoment);

            var spin = quaternion.RotateY(dt * math.PI * rotationCompoment.speed *(rotationCompoment.isPositiveDirection ? -1 : 1));
            transform.ValueRW.Rotation = math.mul(spin, transform.ValueRO.Rotation);
        }
    }

    public static Vector3 QuaternionToEuler(Quaternion q)
    {
        float pitch = Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * Mathf.Pow(q.y, 2) - 2 * Mathf.Pow(q.z, 2)) * Mathf.Rad2Deg;
        float yaw = Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * Mathf.Pow(q.x, 2) - 2 * Mathf.Pow(q.z, 2)) * Mathf.Rad2Deg;
        float roll = Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w) * Mathf.Rad2Deg;
        return new Vector3(yaw, pitch, roll);
    }
}