using UnityEngine;
using Unity.Entities;

public struct TurrentRotation : IComponentData
{
    public float speed;
    //������ת��Y�Ὣ��0��-90��֮��ѭ��
    //����ֵ�ж�Ŀǰ��ת�Ƿ�������-90��
    public bool isPositiveDirection;
}
