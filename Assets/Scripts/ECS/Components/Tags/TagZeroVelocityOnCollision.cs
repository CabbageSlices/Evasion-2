using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

//when this tagged entity collides with something, the velocity along the direction of collision will be zeroed out.
//direction si determiend by normal vector
[GenerateAuthoringComponent]
[Serializable]
public struct TagZeroVelocityOnCollision : IComponentData
{
    
}
