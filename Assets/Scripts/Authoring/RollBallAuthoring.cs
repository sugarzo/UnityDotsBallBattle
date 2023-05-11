using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;

public class RollBallAuthoring : MonoBehaviour
{
    public int maxBallCount; //最大球数
    public int currentBallCount; //当前球数

    public Transform ballSpawnPos;
    public GameObject ballPrefab;


    class Baker : Baker<RollBallAuthoring>
    {
        public override void Bake(RollBallAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new RollBall
            {
                maxBallCount = authoring.maxBallCount,
                currentBallCount = authoring.currentBallCount,
                ballSpawnPos = GetEntity(authoring.ballSpawnPos.gameObject, TransformUsageFlags.Dynamic),
                ballPrefab = GetEntity(authoring.ballPrefab, TransformUsageFlags.Dynamic),
            });
        }
    }
}