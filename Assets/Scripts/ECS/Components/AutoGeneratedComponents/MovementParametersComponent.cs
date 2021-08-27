using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct MovementParametersComponent : IComponentData
{
    public float horizontalSpeed;
    public float jumpSpeed;
}
