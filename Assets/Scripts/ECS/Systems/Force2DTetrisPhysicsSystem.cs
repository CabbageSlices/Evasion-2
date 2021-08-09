using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

//forces 2d tetris phsyics. that is movement only along x and y axis, z set to 0
//no rotation at all
public class Force2DTetrisPhysicsSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAll<TagForce2DTetrisPhysics>().ForEach((ref Translation translation, ref Rotation rotation) =>
        {
            translation.Value = new float3(translation.Value.x, translation.Value.y, 0);
            translation.Value.z = 0;

            rotation.Value = quaternion.identity;
        }).Schedule();


        //might not have a velocity 
        Entities.WithAll<TagForce2DTetrisPhysics>().ForEach((ref PhysicsVelocity velocity) =>
        {
            velocity.Linear.z = 0;
            velocity.Angular.x = 0;
            velocity.Angular.y = 0;
        }).Schedule();
    }
}
