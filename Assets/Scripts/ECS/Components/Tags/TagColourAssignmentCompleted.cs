using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

//tag to indicatge this entity already had it's colour assigned after initialization
[GenerateAuthoringComponent]
[Serializable]
public struct TagColourAssignmentCompleted : IComponentData
{

    
}
