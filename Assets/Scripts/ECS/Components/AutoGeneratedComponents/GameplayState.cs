using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

//various flags and data needed for gameplay
[GenerateAuthoringComponent]
[Serializable]
public struct GameplayState : IComponentData
{
    public bool isGrounded;
}
