using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using Unity.Mathematics;
using System.Drawing;

public class TankAuthoring : MonoBehaviour
{
    public float4 float4Color
    {
        get { return GetComponentInChildren<TurrentAuthoring>().float4Color; }
    }

    class Baker : Baker<TankAuthoring>
    {
        public override void Bake(TankAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Tank>(entity, new Tank
            {
                color = authoring.float4Color,
            });
            AddComponent(entity,new URPMaterialPropertyBaseColor() 
            { 
                Value = authoring.float4Color
            });
            AddComponent<TeamColor>(entity, new TeamColor
            {
                color = authoring.float4Color,
                teamType = TeamType.Tank,
                
            });
        }
    }
}
