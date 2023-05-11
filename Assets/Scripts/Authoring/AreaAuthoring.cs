using Unity.Entities;
using UnityEngine;
using Unity.Physics.Authoring;

[RequireComponent(typeof(PhysicsShapeAuthoring))]
public class AreaAuthoring : MonoBehaviour
{
    public AreaType areaType;

    class Baker : Baker<AreaAuthoring>
    {
        public override void Bake(AreaAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<BallArea>(entity, new BallArea
            {
                areaType = authoring.areaType
            });
        }
    }
}