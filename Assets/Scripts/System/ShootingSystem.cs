using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

public partial struct ShootingSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<TurrentShoot>();
    }
    public void OnUpdate(ref SystemState state)
    {
        var ecbSystemSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystemSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        //限制最多2k个球
        if(World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(Ball)).CalculateEntityCount() > 2000)
        {
            return;
        }

        var shootJob = new TurretShootJob
        {
            ECB = ecb.AsParallelWriter(),
            //这个表需要每帧更新
            //可使用.Update(ref state);
            localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
            timedt = SystemAPI.Time.DeltaTime,
        };
        //多线程跑
        shootJob.ScheduleParallel();
    }
}

[WithAll(typeof(TurrentShoot))]
public partial struct TurretShootJob : IJobEntity
{
    // A regular EntityCommandBuffer cannot be safely used directly
    // in a parallel scheduled job, so we need a ParallelWriter.
    public EntityCommandBuffer.ParallelWriter ECB;
    [ReadOnly]
    public ComponentLookup<LocalTransform> localTransformLookup;
    [ReadOnly] 
    public ComponentLookup<LocalToWorld> localToWorldLookup;
    [ReadOnly]
    public float timedt;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chunkIndex">sort参数</param>
    /// <param name="tAspect"></param>
    /// <param name="transform"></param>

    //在将命令写入并发命令缓冲区的所有并行作业中，添加到该缓冲区的每组命令的唯一索引。实体提供的entityInQueryIndex参数是用于此参数的适当值。
    //通过将块中的当前实体索引添加到Execute（ArchetypeChunk，Int32，Int32）方法的firstEntityIndex参数，可以在IJobChunk中计算类似的索引。

    //int参数的ChunkIndexInQuery属性为我们提供了实体的“chunk索引”。
    //每个区块只能由一个线程处理，因此这些索引对每个线程都是唯一的。
    //它们也是完全确定的，无论发生多少并行处理。
    //因此，当在EntityCommandBuffer中记录命令时，这些索引被用作排序关键字，
    //通过这种方式，我们可以确保命令的回放始终是确定性的。
    public void Execute([ChunkIndexInQuery] int chunkIndex, ref TurrentShootAspect tAspect,in LocalToWorld transform)
    {
        if(tAspect.CanShot == false)
        {
            return;
        }
        if (tAspect.BulletsCount <= 1)
        {
            //发射完成，将不可发射设置为true
            tAspect.CanShot = false;
            return;
        }
        tAspect.CD -= timedt;
        if(tAspect.CD > 0f)
        {
            return;
        }
        else
        {
            //0.2f射一球
            tAspect.CD = 0.05f;
        }

        //实例化一个子弹
        var bulletInstance = ECB.Instantiate(chunkIndex,tAspect.BulletPrefab);
        //获取炮塔口位置
        var spawnPos = localToWorldLookup[tAspect.BulletSpawnPos];

        ECB.SetComponent(chunkIndex,bulletInstance, new LocalTransform
        {
            Position = spawnPos.Position,
            Rotation = quaternion.identity,
            Scale = localTransformLookup[tAspect.BulletPrefab].Scale,
        });
        //设置方向和速度（y = 0）
        var direction = Vector3.Normalize(spawnPos.Position - transform.Position);
        direction.x = 0;

        ECB.SetComponent(chunkIndex,bulletInstance, new PhysicsVelocity
        {
            Linear = direction.normalized * tAspect.BulletsSpeed,
            Angular = float3.zero
        });

        //设置炮弹颜色
        ECB.SetComponent(chunkIndex, bulletInstance, new URPMaterialPropertyBaseColor()
        {
            Value = tAspect.BaseColor,
        });
        ECB.SetComponent(chunkIndex, bulletInstance, new Bullet()
        {
            maxSpeed = tAspect.BulletsSpeed,
        });
        ECB.SetComponent(chunkIndex, bulletInstance, new TeamColor()
        {
            color = tAspect.BaseColor,
            teamType = TeamType.Bullet,
        });
        tAspect.BulletsCount--;
    }
}