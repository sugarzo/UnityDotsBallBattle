using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;

public class GridAuthoring : MonoBehaviour
{
    class Baker : Baker<GridAuthoring>
    {
        public override void Bake(GridAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            float4 color = new float4(1, 1, 1, 1);
            AddComponent(entity, new Grid
            {

            });
            AddComponent(entity, new URPMaterialPropertyBaseColor
            {
                Value = color
            });
            AddComponent(entity,new TeamColor
            {
                color = color,
                teamType =  TeamType.Grid
            });

        }
    }
}
