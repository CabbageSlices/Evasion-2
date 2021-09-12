using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Systems;
using Unity.Physics;

//when an object with gameplay state lands on top of another object, mark the gameplay object as grounded
//problems with this, if player walks off the edge of a platform, he won't be marked as grounded anymore
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ExportPhysicsWorld))]
[UpdateBefore(typeof(EndFramePhysicsSystem))]
public class GroundedOnCollisionSystem : SystemBase
{
    StepPhysicsWorld stepPhysicsWorldSystem;
    BuildPhysicsWorld buildPhysicsWorldSystem;

    ExportPhysicsWorld exportPhysicsWorld;

    protected override void OnCreate()
    {
        base.OnCreate();
        stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        exportPhysicsWorld = World.GetOrCreateSystem<ExportPhysicsWorld>();
        RequireForUpdate(GetEntityQuery(ComponentType.ReadWrite<GameplayState>()));
    }

    protected override void OnUpdate()
    {
        //mark as not grounded before collision event jobs run, that way all entities will need to be grounded after collision events occur.
        //might cause issues if collision caused entity to be gounded last frame, but pushed entity outside of collision enough that it doesn't collide this frame,
        //causing it to not be grounded anymore.
        Dependency = Entities.ForEach((ref GameplayState state) =>
        {
            state.isGrounded = false;
        }).Schedule(Dependency);

        Dependency = new CollisionEventGroundedOnCollisionJob
        {
            entitiesToGround = GetComponentDataFromEntity<GameplayState>(),
        }.Schedule(stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, Dependency);
        stepPhysicsWorldSystem.AddInputDependency(Dependency);
    }

    // [BurstCompile]
    struct CollisionEventGroundedOnCollisionJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<GameplayState> entitiesToGround;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;

            //if entity B is ot the LEFT of entityA, then the collision normal will be (1, 0)
            //where did entityA collide with respect to entityB
            //collission normal from entity B to entity A

            bool checkEntityARequiresGrounded = entitiesToGround.HasComponent(entityA);
            bool checkEntityBRequiresGrounded = entitiesToGround.HasComponent(entityB);


            //entityA needs to be ABOVE entity B for it to be grounded, collision should occur above entity B
            if (checkEntityARequiresGrounded && collisionEvent.Normal.y > 0.65)
            {
                groundEntity(entityA);
            }

            //entity B ABOVE entity A, normal FROM entty B TO entity A should point downwards, to indicate entity A is below entity B
            if (checkEntityBRequiresGrounded && collisionEvent.Normal.y < -0.65)
            {
                groundEntity(entityB);
            }
        }

        //if the body to ground is ABOVE the other body, AND the collision normal points upwards, then 
        //we know that the bodyToGround is standing on top of the toher, so ground it
        public void groundEntity(Entity entity)
        {
            var state = entitiesToGround[entity];
            state.isGrounded = true;
            entitiesToGround[entity] = state;
        }
    }
}
