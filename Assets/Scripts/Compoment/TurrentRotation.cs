using UnityEngine;
using Unity.Entities;

public struct TurrentRotation : IComponentData
{
    public float speed;
    //炮塔旋转的Y轴将在0到-90度之间循环
    //该数值判断目前旋转是否正朝向-90度
    public bool isPositiveDirection;
}
