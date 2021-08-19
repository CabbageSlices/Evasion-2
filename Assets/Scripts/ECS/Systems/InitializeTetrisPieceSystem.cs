using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Rendering;

//initialize tetris pieces with colour, velocity, etc
[UpdateInGroup(typeof(InitializationSystemGroup))]
[UpdateAfter(typeof(BeginInitializationEntityCommandBufferSystem))]
public class InitializeTetrisPieceSystem : SystemBase
{
    private EndInitializationEntityCommandBufferSystem endInitializationECBS;

    protected override void OnCreate()
    {
        base.OnCreate();
        endInitializationECBS = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        
        EntityCommandBuffer.ParallelWriter writer = endInitializationECBS.CreateCommandBuffer().AsParallelWriter();
        
        Entities.WithAll<TagUninitialized>().ForEach((Entity entity, int entityInQueryIndex, ref PhysicsVelocity velocity, ref DynamicBuffer<Child> children, in TetrisPiece tetrisPiece) => {
            velocity.Linear.y = tetrisPiece.velocity;

            foreach(Child child in children) {
                writer.SetComponent<MaterialColor>(entityInQueryIndex, child.Value, new MaterialColor{Value = tetrisPiece.color});
            }

            writer.RemoveComponent<TagUninitialized>(entityInQueryIndex, entity);
            
        }).Schedule();

        endInitializationECBS.AddJobHandleForProducer(Dependency);
    }
}
