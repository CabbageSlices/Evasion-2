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

        Entities.WithAll<TagForce2DTetrisPhysics>().ForEach((ref Translation translation, ref Rotation rotation) =>
        {
            translation.Value = new float3(translation.Value.x, translation.Value.y, 0);

            rotation.Value = quaternion.identity;
        }).ScheduleParallel();

        //might not have a velocity 
        Entities.WithAll<TagForce2DTetrisPhysics>().ForEach((ref PhysicsVelocity velocity) =>
        {
            velocity.Linear.z = 0;
            velocity.Angular.x = 0;
            velocity.Angular.y = 0;
        }).ScheduleParallel();

        //force terminal velocity
        Entities.WithAll<TagForce2DTetrisPhysics>().ForEach((ref PhysicsVelocity velocity) =>
        {
            if(velocity.Linear.y < -50) {
                velocity.Linear.y = -50;
            }

            if(velocity.Linear.y > 50) {
                velocity.Linear.y = 50;
            }
        }).ScheduleParallel();

        //force tetris pieces to fall ATLEAST as fast as their initial velocity
        Entities.WithAll<TagForce2DTetrisPhysics>().WithNone<TagUninitialized, TagStatic>().ForEach((ref PhysicsVelocity velocity, in TetrisPiece piece) =>
        {
            velocity.Linear.z = 0;
            velocity.Linear.x = 0;

            //velocity is negative, so if block is falling SLOWER (more positive = slower), then force fall to default velocity.
            //this allows block to fall faster than usuall due to collisions, but NEVER slower
            if(velocity.Linear.y > piece.velocity) {
                velocity.Linear.y = piece.velocity;
            }
        }).ScheduleParallel();

        //force tetris pieces to be confied to the grid
        Entities.WithAll<TagForce2DTetrisPhysics>().WithNone<TagUninitialized>().ForEach((ref Translation translation, in TetrisPiece piece) =>
        {
            translation.Value = new float3(Mathf.RoundToInt(translation.Value.x), translation.Value.y, translation.Value.z);
        }).ScheduleParallel();
    }
}
