using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
[Serializable]
public struct PlayerInputIndexComponent : IComponentData
{
    
    //playerindex of the playerinput component that corresponds to this player
    public int playerInputIndex;
    
}
