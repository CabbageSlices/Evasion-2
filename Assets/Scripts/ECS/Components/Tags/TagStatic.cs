using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

//easy way to mark an object as static, easier than checking if entity ahs velocity, mass, etc
[Serializable]
[GenerateAuthoringComponent]
public struct TagStatic : IComponentData
{

}
