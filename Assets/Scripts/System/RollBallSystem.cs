using Unity.Entities;
using Unity.Physics;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using System;
using Random = Unity.Mathematics.Random;

public partial struct RollBallSystem : ISystem
{
    public NativeHashSet<Entity> ballHashMap; //待处理的球集合（主要是因为物理碰撞事件会触发很多次）
    public NativeHashMap<Entity,int> rollBallCount; //这个是为了解决同步问题

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RollBall>();
        //初始化容器
        ballHashMap = new NativeHashSet<Entity>(200,Allocator.Persistent);
        rollBallCount = new NativeHashMap<Entity, int>(100, Allocator.Persistent);
    }

    uint GetRandomUint()
    {
        // Use the current system time as a seed for the random number generator.
        int seed = (int)DateTime.Now.Ticks;
        System.Random random = new System.Random(seed);

        // Generate a random uint.
        byte[] buffer = new byte[4];
        random.NextBytes(buffer);
        uint randomUint = BitConverter.ToUInt32(buffer, 0);

        return randomUint;
    }

    public void OnUpdate(ref SystemState state)
    {
        new RollBallInstantiateJob
        {
            ECB = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged),
            localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>(true),
            localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
            randomSeed = GetRandomUint()
        }.Schedule();

        var ecbSystemSingleton = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystemSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        //处理Roll球碰撞结果
        RBCTriggerJob triggerJob = new RBCTriggerJob
        {
            ECB = ecb,
            ballLookup = SystemAPI.GetComponentLookup<Ball>(true),
            ballAreaLookup = SystemAPI.GetComponentLookup<BallArea>(true),
            turrentShootLookup = SystemAPI.GetComponentLookup<TurrentShoot>(true),
            ballHashMap = this.ballHashMap,
        };
        var jobHandle = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        jobHandle.Complete();

        //处理碰撞结果
        var getTurrentShoot = SystemAPI.GetComponentLookup<TurrentShoot>(true);
        var getRollBall = SystemAPI.GetComponentLookup<RollBall>();
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //记录改帧已修改的turrent
        var hashmap = new NativeHashSet<Entity>(5, Allocator.Temp);
        foreach (var (result,entity) in SystemAPI.Query<RefRO<BallCollisionResult>>().WithEntityAccess())
        {
            //炮塔被摧毁时不处理事件,同时避免处理同一个小球多次防止当前小球数量的数据对不上
            if (getTurrentShoot.HasComponent(result.ValueRO.turrentEntity) && ballHashMap.Contains(result.ValueRO.ballEntity))
            {
                //写入ecb，同一帧的修改turrent会覆盖之前的修改，所以同一帧只处理一个result对应的turrent
                if (hashmap.Contains(result.ValueRO.turrentEntity) || hashmap.Contains(result.ValueRO.rollBallEntity))
                {
                    continue;
                }
                ballHashMap.Remove(result.ValueRO.ballEntity);
                hashmap.Add(result.ValueRO.turrentEntity);
                hashmap.Add(result.ValueRO.rollBallEntity);

                RollBall rbData = getRollBall[result.ValueRO.rollBallEntity];
                //找到发射装置，发射小球
                if (result.ValueRO.areaType == AreaType.Shot)
                {
                    TurrentShoot tsData = getTurrentShoot[result.ValueRO.turrentEntity];
                    tsData.bulletsCount += 1;
                    tsData.canShot = true;
                    ecb.SetComponent(result.ValueRO.turrentEntity, tsData);
                }
                else if (result.ValueRO.areaType == AreaType.X2)
                {
                    TurrentShoot tsData = getTurrentShoot[result.ValueRO.turrentEntity];
                    tsData.bulletsCount *= 2;
                    ecb.SetComponent(result.ValueRO.turrentEntity, tsData);
                }
                else if (result.ValueRO.areaType == AreaType.Add1)
                {
                    //最大小球数+1
                    rbData.maxBallCount += 1;
                }
                //当前球数-1
                rbData.currentBallCount--;

                //ecb.DestroyEntity(result.ValueRO.ballEntity); //摧毁小球
                //不用ecb的原因时ecb执行时间不确定，可能会多次计算碰撞
                entityManager.DestroyEntity(result.ValueRO.ballEntity); 
                SystemAPI.SetComponent(result.ValueRO.rollBallEntity, rbData);
                //ecb.SetComponent(result.ValueRO.rollBallEntity, rbData);
            }
            ecb.DestroyEntity(entity);//摧毁该次封装事件
        }
        hashmap.Dispose();
    }
}
public partial struct RBCTriggerJob : ICollisionEventsJob
{
    public EntityCommandBuffer ECB;
    [ReadOnly]
    public ComponentLookup<Ball> ballLookup;
    [ReadOnly]
    public ComponentLookup<BallArea> ballAreaLookup;
    [ReadOnly]
    public ComponentLookup<TurrentShoot> turrentShootLookup;

    public NativeHashSet<Entity> ballHashMap; //与主线程共享数据

    public void Execute(CollisionEvent collisionEvent)
    {
        //找到并摧毁小球
        //Entity ballEntity = ballLookup.HasComponent(collisionEvent.EntityA) ? collisionEvent.EntityA : collisionEvent.EntityB;
        //Entity otherEntity = ballEntity == collisionEvent.EntityA ? collisionEvent.EntityB : collisionEvent.EntityA;

        //后面发现一次碰撞会触发两次CollisionEvent，上面的写法只会多判断一次逻辑，没有意义
        Entity ballEntity = collisionEvent.EntityA;
        Entity otherEntity = collisionEvent.EntityB;


        if (ballLookup.HasComponent(ballEntity) && !ballHashMap.Contains(ballEntity))
        {
            Ball ballData = ballLookup[ballEntity];
            //如果正在发射，不处理事件
            if (turrentShootLookup.HasComponent(ballData.turrentShootEntity) && turrentShootLookup[ballData.turrentShootEntity].canShot == true)
                return;
            BallArea ballAreaData = ballAreaLookup[otherEntity];
            //封装一个事件Compoment，并摧毁小球
            ECB.AddComponent(ECB.CreateEntity(), new BallCollisionResult
            {
                areaType = ballAreaData.areaType,
                rollBallEntity = ballData.rollBallEntity,
                turrentEntity = ballData.turrentShootEntity,
                ballEntity = ballEntity,
            });

            ballHashMap.Add(ballEntity);
            //ECB.DestroyEntity(ballEntity); 把摧毁逻辑放在update里
        }
        
    }
}

//实例化Roll球
public partial struct RollBallInstantiateJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    [ReadOnly]
    public ComponentLookup<LocalToWorld> localToWorldLookup;
    [ReadOnly]
    public ComponentLookup<LocalTransform> localTransformLookup;

    public uint randomSeed;

    public void Execute(ref RollBall rollBall,in Entity entity)
    {
        randomSeed += (uint)rollBall.ballSpawnPos.Index;

        if(rollBall.currentBallCount < rollBall.maxBallCount)
        {
            var ballEntity = ECB.Instantiate(rollBall.ballPrefab);
            var spawnPos = localToWorldLookup[rollBall.ballSpawnPos];

            ECB.SetComponent(ballEntity, new LocalTransform
            {
                Position = spawnPos.Position + Random.CreateFromIndex(randomSeed).NextFloat3(new float3(0,0,-1.3f), new float3(0, 0, 1.3f)),
                Rotation = quaternion.identity,
                Scale = localTransformLookup[rollBall.ballPrefab].Scale,
            });
            ECB.SetComponent(ballEntity, new PhysicsVelocity
            {
                Angular = Random.CreateFromIndex(randomSeed).NextFloat3(new float3(0, -5f, -5f), new float3(0, 5f, 5f)),
                Linear = Random.CreateFromIndex(randomSeed).NextFloat3(new float3(0, -5f, -5f), new float3(0, 5f, 5f))
            });
            ECB.SetComponent(ballEntity, new Ball
            {
                //配置炮塔实体和Roll球机的实体
                rollBallEntity = entity,
                turrentShootEntity = rollBall.turrentShootEntity,
            });
            rollBall.currentBallCount++;
        }
    }

}