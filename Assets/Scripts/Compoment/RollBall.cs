using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct RollBall : IComponentData
{
    public int maxBallCount; //最大球数
    public int currentBallCount; //当前球数
    //小球出生位置
    public Entity ballSpawnPos;
    //小球预制体
    public Entity ballPrefab;
    //turrent
    public Entity turrentShootEntity;
}