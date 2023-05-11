using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public enum TeamType
{
    Tank,
    Grid,
    Bullet,
}

public struct TeamColor : IComponentData
{
    public float4 color;
    public TeamType teamType;
}
