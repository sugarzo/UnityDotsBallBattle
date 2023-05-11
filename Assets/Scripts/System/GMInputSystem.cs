using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 调试使用
/// </summary>
public partial struct GMInputSystem : ISystem
{
    public EntityCommandBuffer ECB;
    public bool isFirstTime;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurrentShoot>();
        isFirstTime = true;
    }
    public void OnUpdate(ref SystemState state)
    {
        if (isFirstTime)
        {
            //foreach (var turrentShoot in SystemAPI.Query<RefRW<TurrentShoot>>())
            //{
            //    int addBullet = (int)math.pow(2, UnityEngine.Random.Range(8, 16));
            //    Debug.Log("AddBullet - " + addBullet);
            //    turrentShoot.ValueRW.bulletsCount += addBullet;
            //}
            isFirstTime = false;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            foreach(var turrentShoot in SystemAPI.Query<RefRW<TurrentShoot>>())
            {
                turrentShoot.ValueRW.bulletsCount += (int)(200 * UnityEngine.Random.value);
            }
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            foreach (var (turrent,entity) in SystemAPI.Query<RefRW<TurrentRotation>>().WithEntityAccess())
            {
                var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                ECB = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
                ECB.DestroyEntity(entity);
                break;
            }
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            foreach (var turrentShoot in SystemAPI.Query<RefRW<TurrentShoot>>())
            {
                int addBullet = (int)math.pow(2, UnityEngine.Random.Range(8, 16));
                Debug.Log("AddBullet - " + addBullet);
                turrentShoot.ValueRW.bulletsCount += addBullet;
            }
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            foreach (var turrentShoot in SystemAPI.Query<RefRW<TurrentShoot>>())
            {
                int addBullet = (int)math.pow(2, 15);
                Debug.Log("AddBullet - " + addBullet);
                turrentShoot.ValueRW.bulletsCount += addBullet;

                break;
            }
        }
    }
}