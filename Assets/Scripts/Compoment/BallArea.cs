using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public enum AreaType
{
    Shot,
    X2,
    Add1,
}

public struct BallArea : IComponentData
{
    public AreaType areaType;
}