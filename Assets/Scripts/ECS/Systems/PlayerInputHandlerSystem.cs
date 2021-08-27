using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;

[UpdateInGroup(typeof(InputSystemGroup))]
[UpdateAfter(typeof(PlayerInputListenerSystem))]
public class PlayerInputHandlerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Assign values to local variables captured in your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     float deltaTime = Time.DeltaTime;

        // This declares a new kind of job, which is a unit of work to do.
        // The job is declared as an Entities.ForEach with the target components as parameters,
        // meaning it will process all entities in the world that have both
        // Translation and Rotation components. Change it to process the component
        // types you want.



        Entities.WithAll<TagPlayer>().ForEach((Entity entity, ref PlayerInputComponent input, ref PhysicsVelocity velocity, in MovementParametersComponent movementParameters) =>
        {
            //full speed left and right
            float horizontalDirection = input.inputDirection.x > 0.1 ? 1 : (input.inputDirection.x < -0.1 ? -1 : 0);
            float horizontalSpeed = movementParameters.horizontalSpeed * input.inputDirection.x;

            velocity.Linear = new float3(horizontalSpeed, velocity.Linear.y, velocity.Linear.z);

            if (input.isJumpKeyPressedThisFrame && !input.isJumpPressHandeled)
            {
                velocity.Linear.y = movementParameters.jumpSpeed;
            }

            input.isJumpPressHandeled = true;
        }).ScheduleParallel();
    }
}
