using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
public partial struct RemainBulletSpeedSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        //使用RequireForUpdate本身就会启动一次查询
        //state.RequireForUpdate<Bullet>();
    }

    public void OnUpdate(ref SystemState state)
    {
        var job = new RemainBulletSpeedJob();
        //job.Schedule();
        job.ScheduleParallel();
    }

    public partial struct RemainBulletSpeedJob : IJobEntity
    {
        public void Execute(ref PhysicsVelocity rb, in Bullet bullet)
        {
            //保持最大速度
            rb.Linear *= bullet.maxSpeed / Vector3.Magnitude(rb.Linear);
        }
    }
}