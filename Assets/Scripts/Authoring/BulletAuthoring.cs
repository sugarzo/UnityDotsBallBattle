using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;
using System.Drawing;

public class BulletAuthoring : MonoBehaviour
{
    public float speed = 1f;

    class Baker : Baker<BulletAuthoring>
    {
        public override void Bake(BulletAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new URPMaterialPropertyBaseColor
            {

            });
            AddComponent(entity, new TeamColor
            {
                teamType = TeamType.Bullet
            });
            AddComponent(entity, new Bullet
            {
                maxSpeed = authoring.speed
            });
        }
    }
}