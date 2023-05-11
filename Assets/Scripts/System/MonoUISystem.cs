using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(LateSimulationSystemGroup))]
public partial struct MonoUISystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {

    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var (turrent, entity) in SystemAPI.Query<RefRO<TurrentShoot>>().WithEntityAccess())
        {
            TeamColorManager.instance.ShowTextData(entity, turrent.ValueRO.bulletsCount);
        }
    }
}