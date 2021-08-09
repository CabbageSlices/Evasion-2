using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using System;
using Unity.Transforms;
using Unity.Physics;


[UpdateInGroup(typeof(LateSimulationSystemGroup ))]
public class BlockSpawnSystem : SystemBase
{
    RNGManagerSystem rngSystem;

    BeginSimulationEntityCommandBufferSystem beginSimulationECBS;

    protected override void OnCreate()
    {
        base.OnCreate();
        rngSystem = World.GetOrCreateSystem<RNGManagerSystem>();
        beginSimulationECBS = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {

        var time = Time.ElapsedTime;

        var rngs = rngSystem.randomNumberGenerators;

        var ecbWriter = beginSimulationECBS.CreateCommandBuffer().AsParallelWriter();

        var count = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerMaximumCount;

        //spawning for rect spawn areas
        //https://forum.unity.com/threads/nativethreadindex-vs-jobsutility-maxjobthreadcount.836473/
        //need to put nativeThreadIndex as in parameter otherwise it'll have the wrong value
        Entities.WithoutBurst().ForEach((int entityInQueryIndex, ref SpawnerComponent spawner, in RectSpawnAreaComponent spawnArea, in int nativeThreadIndex) =>
        {
            if (spawner.timeLastSpawnAt + spawner.delayUntilNextSpawn > time)
            {
                return;
            }

            int rngIndex = nativeThreadIndex - 1;
            var rng = rngs[rngIndex];

            float2 spawnPosition = new float2(
                rng.NextFloat(spawnArea.left, spawnArea.right),
                rng.NextFloat(spawnArea.top, spawnArea.bottom)
            );

            Entity newBlock = ecbWriter.Instantiate(entityInQueryIndex, spawner.entityToSpawn);

            ecbWriter.SetComponent(entityInQueryIndex, newBlock, new Translation
            {
                Value = new float3(spawnPosition, 0)
            });

            PhysicsVelocity velocity = new PhysicsVelocity();
            velocity.Angular = new float3(0, 0, 0);
            velocity.Linear = new float3(spawner.initialVelocity, 0);
            ecbWriter.SetComponent(entityInQueryIndex, newBlock, velocity);

            spawner.timeLastSpawnAt = time;

            rngs[rngIndex] = rng;

        }).Schedule();

        beginSimulationECBS.AddJobHandleForProducer(Dependency);
    }
}
