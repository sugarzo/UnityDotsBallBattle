using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Collections;
using UnityEngine;
using Unity.Rendering;
using Unity.Mathematics;
using System.Collections.Generic;

[UpdateAfter(typeof(TurretRotationSystem))]
[UpdateAfter(typeof(ShootingSystem))]
public partial struct GridCollisionSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        //var ecbSystemSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecbSystemSingleton = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystemSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        //创建容器
        var nativeSet = new NativeHashSet<Entity>(30, Allocator.TempJob);
        TriggerJob triggerJob = new TriggerJob
        {
            ECB = ecb,
            bulletLookup = SystemAPI.GetComponentLookup<Bullet>(true),
            teamColorLookup = SystemAPI.GetComponentLookup<TeamColor>(true),
            entitiesHashMap = nativeSet, //虽然是值类型，但是能和主线程共享数据
        };
        var jobHandle = triggerJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        jobHandle.Complete();

        //释放容器
        nativeSet.Dispose();
    }
}
public partial struct TriggerJob : ITriggerEventsJob
{
    public EntityCommandBuffer ECB;
    [ReadOnly]
    public ComponentLookup<Bullet> bulletLookup;
    [ReadOnly]
    public ComponentLookup<TeamColor> teamColorLookup;

    public NativeHashSet<Entity> entitiesHashMap; //该帧已判断的Collider

    public void Execute(TriggerEvent triggerEvent)
    {
        //找到子弹
        var bulletEntity = bulletLookup.HasComponent(triggerEvent.EntityA) ? triggerEvent.EntityA : triggerEvent.EntityB;
        var otherEntity = triggerEvent.EntityA == bulletEntity ? triggerEvent.EntityB : triggerEvent.EntityA;

        //当一个子弹同时进入多个格子区域内时，会出现Entity已被摧毁的error，这里检测一下Entity是否存在
        if(!teamColorLookup.HasComponent(otherEntity))
        {
            return;
        }
        float4 bullet_color = teamColorLookup[bulletEntity].color;
        float4 other_color = teamColorLookup[otherEntity].color;
        //子弹格子碰撞
        if (teamColorLookup[otherEntity].teamType == TeamType.Grid)
        {
            //如果该帧该格子或者子弹已判定过，则忽视掉该次碰撞
            //可以防止多个子弹同时被一个格子吃掉，因为ECS的SetCompoment指令是延迟执行的）
            if (entitiesHashMap.Contains(otherEntity) || entitiesHashMap.Contains(bulletEntity))
                return;
            //如果是同一个颜色则不摧毁
            if (!bullet_color.Equals(other_color))
            {
                //变化格子颜色
                ECB.SetComponent(otherEntity, new URPMaterialPropertyBaseColor
                {
                    Value = bullet_color,
                });
                ECB.SetComponent(otherEntity, new TeamColor
                {
                    color = bullet_color,
                    teamType = TeamType.Grid
                });
                ECB.DestroyEntity(bulletEntity);
                //标记该格子/子弹
                entitiesHashMap.Add(otherEntity);
                entitiesHashMap.Add(bulletEntity);
            }
        }
        else if (teamColorLookup[otherEntity].teamType == TeamType.Tank) //子弹炮塔碰撞
        {
            //如果是同一个颜色则不摧毁
            if (!bullet_color.Equals(other_color))
            {
                //摧毁炮塔
                Debug.Log($"Tower(color-{other_color} has been destroyed!)");
                ECB.DestroyEntity(bulletEntity);
                ECB.DestroyEntity(otherEntity);
            }

        }
    }
}