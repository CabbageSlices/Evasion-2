using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Systems;
using Unity.Physics;

//when an object colldies with another statiic object, turn the non static object into static, and stop it's movement
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ExportPhysicsWorld))]
[UpdateBefore(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
public class TurnStaticOnCollisionSystem : SystemBase
{

    StepPhysicsWorld stepPhysicsWorldSystem;
    BuildPhysicsWorld buildPhysicsWorldSystem;

    ExportPhysicsWorld exportPhysicsWorld;

    EndFixedStepSimulationEntityCommandBufferSystem ecbs;

    protected override void OnCreate()
    {
        base.OnCreate();
        stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        ecbs = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        exportPhysicsWorld = World.GetOrCreateSystem<ExportPhysicsWorld>();
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<TagTurnStaticOnCollision>()));
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = ecbs.CreateCommandBuffer();

        //want to turn static after physics has finished, so put it as dependency
        JobHandle dependency = JobHandle.CombineDependencies(exportPhysicsWorld.GetOutputDependency(), Dependency);

        Dependency = new CollisionEventTurnStaticJob
        {
            entitiesToTurnStatic = GetComponentDataFromEntity<TagTurnStaticOnCollision>(),
            staticEntities = GetComponentDataFromEntity<TagStatic>(),
            ecb = ecb,
        }.Schedule(stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, dependency);

        ecbs.AddJobHandleForProducer(Dependency);
    }

    // [BurstCompile]
    struct CollisionEventTurnStaticJob : ICollisionEventsJob
    {

        [ReadOnly]
        public ComponentDataFromEntity<TagTurnStaticOnCollision> entitiesToTurnStatic;

        [ReadOnly]
        public ComponentDataFromEntity<TagStatic> staticEntities;

        public EntityCommandBuffer ecb;

        public void Execute(CollisionEvent collisionEvent)
        {
            Entity entityA = collisionEvent.EntityA;
            Entity entityB = collisionEvent.EntityB;

            bool entityATurnStatic = entitiesToTurnStatic.HasComponent(entityA);
            bool entityBTurnStatic = entitiesToTurnStatic.HasComponent(entityB);

            bool isEntityAStatic = staticEntities.HasComponent(entityA);
            bool isEntityBStatic = staticEntities.HasComponent(entityB);

            if (UnityEngine.Mathf.Abs(collisionEvent.Normal.x) > 0.15)
            {
                return;
            }

            if (entityATurnStatic && isEntityBStatic)
            {
                turnEntityStatic(entityA);
            }
            else if (entityBTurnStatic && isEntityAStatic)
            {
                turnEntityStatic(entityB);
            }
        }

        void turnEntityStatic(Entity entity)
        {
            ecb.AddComponent(entity, new TagStatic());
            // ecb.RemoveComponent<TagTurnStaticOnCollision>(entity);
            ecb.RemoveComponent<PhysicsVelocity>(entity);
            // ecb.RemoveComponent<PhysicsMass>(entity);
            // ecb.RemoveComponent<PhysicsDamping>(entity);
            // ecb.RemoveComponent<PhysicsGravityFactor>(entity);
        }
    }
}
