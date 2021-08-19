using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Physics;

//spawns tetris blocks, THEY ARE DISABLED INITIALLY
[UpdateInGroup(typeof (LateSimulationSystemGroup))]
public class TetrisPieceSpawnSystem : SystemBase
{
    RNGManagerSystem rngSystem;

    BeginInitializationEntityCommandBufferSystem beginInitializationECB;

    protected override void OnCreate()
    {
        base.OnCreate();
        rngSystem = World.GetOrCreateSystem<RNGManagerSystem>();
        beginInitializationECB = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    unsafe protected override void OnUpdate()
    {
        var currentTime = Time.ElapsedTime;

        var rngs = rngSystem.randomNumberGenerators;

        var ecbWriter = beginInitializationECB.CreateCommandBuffer().AsParallelWriter();


        ComponentDataFromEntity<PhysicsCollider> colliders = GetComponentDataFromEntity<PhysicsCollider>(true);
        
        Entities
            .WithNativeDisableParallelForRestriction(rngs)
            .WithReadOnly(colliders)
            .ForEach((int entityInQueryIndex, ref SpawnerTimer timer, in SpawnAreaHorizontalBounds spawnBounds, in SpawnedItemInitialVelocity initialVelocity, in Translation translation, in DynamicBuffer<TetrisPieceBufferElement> tetrisPieces, in int nativeThreadIndex) => {
                if(timer.timeLastSpawnAt + timer.delayUntilNextSpawn > currentTime || tetrisPieces.Length == 0) {
                    return;
                }

                //spawn a block within the bounds of the area.
                //make sure the blocks bounding box is within the area

                int rngIndex = nativeThreadIndex - 1;
                var rng = rngs[rngIndex];

                int2 spawnPosition = new int2(rng.NextInt(spawnBounds.left, spawnBounds.right), UnityEngine.Mathf.RoundToInt(translation.Value.y));

                int numPossiblePieces = tetrisPieces.Length;
                Entity toSpawn = tetrisPieces[0].tetrisPiece;

                //ensure that block is spawned within the bounds
                Collider * collider = colliders[toSpawn].ColliderPtr;
                Aabb bounds = (*collider).CalculateAabb();

                float3 extents = bounds.Extents;

                if(spawnPosition.x + (int)extents.x > spawnBounds.right) {
                    spawnPosition.x = (int)(spawnBounds.right - extents.x);
                }

                float4 color = rng.NextFloat4(new float4(0, 0, 0, 1), new float4(1, 1, 1, 1));

                Entity spawned = ecbWriter.Instantiate(entityInQueryIndex, toSpawn);

                ecbWriter.AddComponent<Translation>(entityInQueryIndex, spawned, new Translation{ Value = new float3(spawnPosition.x, spawnPosition.y, 0)});
                ecbWriter.SetComponent<TetrisPiece>(entityInQueryIndex, spawned, new TetrisPiece{ velocity = initialVelocity.velocity, color = color });
                ecbWriter.AddComponent<TagUninitialized>(entityInQueryIndex, spawned, new TagUninitialized());


                timer.timeLastSpawnAt = currentTime;
                rngs[rngIndex] = rng;

            }).Schedule();

        beginInitializationECB.AddJobHandleForProducer(Dependency);
    }
}
