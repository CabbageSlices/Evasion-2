using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct SpawnAreaHorizontalBounds : IComponentData
{
    
    public int left;
    public int right;
}
