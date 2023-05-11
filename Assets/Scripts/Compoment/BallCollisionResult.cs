using Unity.Entities;

//用来封装碰撞后的处理数据
public struct BallCollisionResult : IComponentData
{
    public AreaType areaType;
    public Entity turrentEntity; //对应的tank球数*2 或控制发射开关
    public Entity rollBallEntity; //对应的roll球机上限+1
    public Entity ballEntity;
}
