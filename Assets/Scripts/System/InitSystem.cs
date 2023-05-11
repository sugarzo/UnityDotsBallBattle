using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
/// <summary>
/// 初始化配置，只执行一次
/// </summary>
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct InitSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RollBall>();
        state.RequireForUpdate<TurrentShoot>();
    }

    public void OnUpdate(ref SystemState state)
    {
        //绑定显示屏的UI数据
        ComponentLookup<LocalToWorld> localToWorldLookup = SystemAPI.GetComponentLookup<LocalToWorld>();

        var tankArray = new NativeList<Entity>(10, Allocator.TempJob);
        var positionArray = new NativeList<Vector3>(10, Allocator.TempJob);
        var colorArray = new NativeList<float4>(10, Allocator.TempJob);

        foreach (var (turrent,entity) in SystemAPI.Query<RefRO<Tank>>().WithEntityAccess())
        {
            Vector3 pos = localToWorldLookup[entity].Position;
            
            tankArray.Add(entity);
            positionArray.Add(pos);
            colorArray.Add(turrent.ValueRO.color);

        }

        var turrentArray = new NativeList<Entity>(10, Allocator.TempJob);
        foreach (var (turrent, entity) in SystemAPI.Query<RefRO<TurrentShoot>>().WithEntityAccess())
        {
            Vector3 pos = localToWorldLookup[entity].Position;
            turrentArray.Add(entity);
            TeamColorManager.instance.BlindUIData(pos, entity);
        }

        //绑定Roll球机和turrent实体
        foreach (var (roll,entity) in SystemAPI.Query<RefRW<RollBall>>().WithEntityAccess())
        {
            //找到最小距离
            Vector3 selfPos = localToWorldLookup[entity].Position;

            Entity targetEntity = turrentArray[0];
            float dis = Vector3.Distance(selfPos, localToWorldLookup[turrentArray[0]].Position);

            for(int i = 0;i<tankArray.Length;i++)
            {
                float thisDis = Vector3.Distance(selfPos, localToWorldLookup[turrentArray[i]].Position);
                if(dis >= thisDis)
                {
                    dis = thisDis;
                    targetEntity = turrentArray[i];
                }
            }
            roll.ValueRW.turrentShootEntity = targetEntity;
        }
        turrentArray.Dispose();

        var ecbSystemSingleton = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystemSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        //初始化小球地盘,离得最近的格子变为同色
        foreach (var (teamColor, entity) in SystemAPI.Query<RefRO<TeamColor>>().WithAll<Grid>().WithEntityAccess())
        {
            //找到最小距离
            Vector3 selfPos = localToWorldLookup[entity].Position;

            Entity targetEntity = tankArray[0];
            int targetIndex = 0;
            float dis = Vector3.Distance(selfPos, positionArray[0]);

            for (int i = 0; i < tankArray.Length; i++)
            {
                float thisDis = Vector3.Distance(selfPos, positionArray[i]);
                if (dis >= thisDis)
                {
                    dis = thisDis;
                    targetEntity = tankArray[i];
                    targetIndex = i;
                }
            }
            float4 bullet_color = colorArray[targetIndex];
            ecb.SetComponent(entity, new URPMaterialPropertyBaseColor
            {
                Value = bullet_color,
            });
            ecb.SetComponent(entity, new TeamColor
            {
                color = bullet_color,
                teamType = TeamType.Grid,
            });
        }

        tankArray.Dispose();
        positionArray.Dispose();
        colorArray.Dispose();
        //只执行一次
        state.Enabled = false;
    }
}