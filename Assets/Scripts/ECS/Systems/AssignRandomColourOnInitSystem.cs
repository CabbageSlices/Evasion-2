using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateBefore(typeof(EndInitializationEntityCommandBufferSystem))]
public class AssignRandomColourOnInitSystem : SystemBase
{
    EndInitializationEntityCommandBufferSystem endInitializationECBS;
    RNGManagerSystem rngSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        endInitializationECBS = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
        rngSystem = World.GetOrCreateSystem<RNGManagerSystem>();
    }
    protected override void OnUpdate()
    {
        var randomSystems = rngSystem.randomNumberGenerators;

        EntityCommandBuffer.ParallelWriter writer = endInitializationECBS.CreateCommandBuffer().AsParallelWriter();

        Entities.WithAll<TagRandomColourOnInitComponent>()
            .WithNativeDisableParallelForRestriction(randomSystems)
            .ForEach((Entity entity, int entityInQueryIndex, ref MaterialColor color, in int nativeThreadIndex) =>
            {
                var rng = randomSystems[nativeThreadIndex - 1];
                color.Value = rng.NextFloat4(0, 1);
                writer.RemoveComponent<TagRandomColourOnInitComponent>(entityInQueryIndex, entity);

                randomSystems[nativeThreadIndex - 1] = rng;
            }).ScheduleParallel();

        endInitializationECBS.AddJobHandleForProducer(this.Dependency);
    }
}
