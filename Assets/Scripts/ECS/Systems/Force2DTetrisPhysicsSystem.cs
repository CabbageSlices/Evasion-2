using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

//forces 2d tetris phsyics. that is movement only along x and y axis, z set to 0
//no rotation at all
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(StepPhysicsWorld))]
public class Force2DTetrisPhysicsSystem : SystemBase
{

    protected override void OnUpdate()
    {

        Entities.WithAll<TagForce2DTetrisPhysicsComponent>().ForEach((ref Translation translation, ref Rotation rotation) =>
        {
            translation.Value = new float3(translation.Value.x, translation.Value.y, 0);

            rotation.Value = quaternion.identity;
        }).ScheduleParallel();

        Entities.WithAll<TagForce2DTetrisPhysicsComponent>().ForEach((ref PhysicsVelocity velocity) =>
        {

            velocity.Linear.z = 0;
            velocity.Angular = float3.zero;
        }).Schedule();
    }
}
