using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;

public class BallAuthoring : MonoBehaviour
{
    class Baker : Baker<BallAuthoring>
    {
        public override void Bake(BallAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Ball>(entity);
        }
    }
}