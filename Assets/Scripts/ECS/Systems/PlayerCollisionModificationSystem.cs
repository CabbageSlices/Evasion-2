using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Physics;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics.Systems;
using Unity.Collections;
using Unity.Mathematics;

//system that will modify player collision so that he doesn't get stuck while walking accross lbocks, or sliding down the sides of blocks
//player has a horizontal collision box, and a vertical collision box.
//horizontal collision box is wider than the vertical, and only collides horizontally.
//vertical collision box is taller and only collides vertically
//this way corner of player doens't "snag" along corners

//ISSUES WITH THIS, you get some diagnoal collisions being ignored becuase the collider is a cross shape, so if you collide against the corner of a block, you'll go inside it before
//the colliders actually overlap
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(StepPhysicsWorld))]
public class PlayerCollisionModificationSystem : SystemBase
{

    //bit positions of the horizontal/vertical collider flags for phsyhics materials tags
    public static int horizontalColliderTagBitPosition = 2;
    public static int verticalColliderTagBitPosition = 3;

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

        contactsJobCallback = (ref ISimulation simulation, ref PhysicsWorld world, JobHandle inputDep) =>
        {

            return new DisableContactsHorizontalCollisionJob { world = world, tagged = GetComponentDataFromEntity<TagPlayer>(), colliders = GetComponentDataFromEntity<PhysicsCollider>() }.Schedule(simulation, ref world, inputDep);
        };

        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<TagPlayer>()));
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

    //THIS IS A BIT JUMPY becuase we don't use raycast so if player moves too fast, this could cause artifacts.
    struct DisableContactsHorizontalCollisionJob : IContactsJob
    {

        [ReadOnly]
        public ComponentDataFromEntity<TagPlayer> tagged;
        public ComponentDataFromEntity<PhysicsCollider> colliders;
        public PhysicsWorld world;

        unsafe public void Execute(ref ModifiableContactHeader header, ref ModifiableContactPoint contact)
        {
            bool isEntityATagged = tagged.HasComponent(header.EntityA);
            bool isEntityBTagged = tagged.HasComponent(header.EntityB);

            if (isEntityATagged)
            {
                modifyContact(header.BodyIndexA, header.ColliderKeyA, ref header);
            }

            if (isEntityBTagged)
            {
                modifyContact(header.BodyIndexB, header.ColliderKeyB, ref header);
            }
        }

        //returns true if the current collision is not valid
        //i.e if the players horizontal only collision box collided with something, but the collision normal is vertical, then ignore this collision
        //similarliy for the vertical only collision box
        unsafe public void modifyContact(int playerBodyIndex, ColliderKey childColliderKey, ref ModifiableContactHeader header)
        {

            float3 collisionNormal = header.Normal;

            //have ot use world.bodies because using componentDataFromEntity to get the rigidBody doesn't seem to return colliders properly
            var body = world.Bodies[playerBodyIndex];
            var collider = (CompoundCollider*)body.Collider.GetUnsafePtr();

            ChildCollider child;
            collider->GetLeaf(childColliderKey, out child);

            var boxCollider = (Unity.Physics.BoxCollider*)child.Collider;

            var tag = boxCollider->Material.CustomTags;

            bool disableContact = false;

            float3 modifiedNormal = collisionNormal;

            //horizontal collision box
            if (((tag >> horizontalColliderTagBitPosition) & 1) == 1)
            {
                //if collision normal points too vertically, then we shouldn't be colliding horizontally, so disable collisio nagainst this horizontal box
                disableContact = math.abs(collisionNormal.y) > 0.7;

                //if we should collide horizontally, change normal so that it's purely horizontal
                if (!disableContact)
                {
                    modifiedNormal = collisionNormal.x < 0 ? new float3(-1, 0, 0) : new float3(1, 0, 0);
                }
            }

            //vertical collision box
            if (((tag >> verticalColliderTagBitPosition) & 1) == 1)
            {
                disableContact = math.abs(collisionNormal.y) < 0.7;

                if (!disableContact)
                {
                    modifiedNormal = collisionNormal.y < 0 ? new float3(0, -1, 0) : new float3(0, 1, 0);
                }
            }

            header.Normal = modifiedNormal;

            //remeber that when a contact is disabled, it means only the contact in the veritcal direciton, or horizontal direction is disabled, but NOT both.
            //the other contact will have it's collision normal modified so that the collision response is only handleed entirely horizontally or vertically.
            //this way player doesn't get suck on the corners of blocks as he's moving across the surface of adject blocks.
            if (disableContact)
            {
                header.JacobianFlags = JacobianFlags.Disabled;
            }
        }
    }
}