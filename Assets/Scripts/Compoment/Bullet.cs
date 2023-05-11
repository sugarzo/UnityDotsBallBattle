using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct Bullet : IComponentData
{
    public float maxSpeed; //小球最大速度
}