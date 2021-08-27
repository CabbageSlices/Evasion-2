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
    public bool isJumpKeyPressedThisFrame
    {
        get
        {
            return _isJumpKeyPressedThisFrame;
        }
    }

    private bool _isJumpKeyPressedThisFrame;
    public bool isJumpPressHandeled;
    public bool isJumpKeyHeld;

    public double jumpKeyPressStartTime;

    public void setJumpKeyPressedThisFrame(bool isPressed)
    {
        _isJumpKeyPressedThisFrame = isPressed;

        if (isJumpKeyPressedThisFrame)
        {
            isJumpPressHandeled = false;
        }
    }
}
