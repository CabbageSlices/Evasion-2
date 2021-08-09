using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
[Serializable]
public struct TetrisPiece : IComponentData
{
    //falling velocity of this block.
    float velocity;

    //color to assign to all sub blocks, same color is assigned to all blocks that make up this piece.
    float3 color;
}
