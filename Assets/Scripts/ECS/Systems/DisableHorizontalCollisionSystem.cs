using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Physics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics.Systems;
using Unity.Collections;

//disables collisions between two entities that are tagged with ignore mutual horizontal collision if the collision normal has a large enough horizontal component
//disables collisions according to physics body block tags.
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(StepPhysicsWorld))]
public class DisableHorizontalCollisionSystem : SystemBase
{
    StepPhysicsWorld stepPhysicsWorldSystem;
    BuildPhysicsWorld buildPhysicsWorldSystem;

    ExportPhysicsWorld exportPhysicsWorld;

    SimulationCallbacks.Callback contactsJobCallback;

    protected override void OnCreate()
    {
        base.OnCreate();
        stepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        buildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        exportPhysicsWorld = World.GetOrCreateSystem<ExportPhysicsWorld>();

        contactsJobCallback = (ref ISimulation simulation, ref PhysicsWorld world, JobHandle inputDep) => {
            
            return new DisableContactsHorizontalCollisionJob{ taggedToIgnore = GetComponentDataFromEntity<TagIgnoreMutualHorizontalCollision>()}.Schedule(simulation, ref world, inputDep);
        };

        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<TagIgnoreMutualHorizontalCollision>()));
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void OnUpdate()
    {
        stepPhysicsWorldSystem.EnqueueCallback(SimulationCallbacks.Phase.PostCreateContacts, contactsJobCallback, Dependency);
    }


    [BurstCompile]
    struct DisableContactsHorizontalCollisionJob : IContactsJob
    {

        [ReadOnly]
        public ComponentDataFromEntity<TagIgnoreMutualHorizontalCollision> taggedToIgnore;

        unsafe public void Execute(ref ModifiableContactHeader header, ref ModifiableContactPoint contact)
        {
            bool isEntityATagged = taggedToIgnore.HasComponent(header.EntityA);
            bool isEntityBTagged = taggedToIgnore.HasComponent(header.EntityB);

            if(isEntityATagged && isEntityBTagged && Unity.Mathematics.math.abs(header.Normal.x) > 0.1f) {
                header.JacobianFlags = JacobianFlags.Disabled;
            }
        }
    }
}
