using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

//player input infomration
[Serializable]
[GenerateAuthoringComponent]
public struct PlayerInputComponent : IComponentData
{

    public float2 inputDirection;
    public bool isJumpKeyPressedThisFrame;
    public bool isJumpKeyHeld;

    public double jumpKeyPressStartTime;
}
