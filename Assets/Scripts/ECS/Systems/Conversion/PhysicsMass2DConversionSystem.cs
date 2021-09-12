using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Authoring;

//disable rotation for forced 2d tetris physics
[UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
public class PhysicsMass2DConversionSystem : GameObjectConversionSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((TagForce2DTetrisPhysics input) =>
        {
            var entity = GetPrimaryEntity(input);

            var mass = DstEntityManager.GetComponentData<PhysicsMass>(entity);

            mass.InverseInertia = float3.zero;
            DstEntityManager.SetComponentData<PhysicsMass>(entity, mass);
            DstEntityManager.AddComponent<TagForce2DTetrisPhysicsComponent>(entity);
        });
    }
}
