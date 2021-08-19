using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

//tag to indicate item is uninitialized
[GenerateAuthoringComponent]
[Serializable]
public struct TagUninitialized : IComponentData
{
   
}
