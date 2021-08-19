using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
[Serializable]
public struct TetrisPiece : IComponentData
{
    //falling velocity of this block.
    public float velocity;

    //color to assign to all sub blocks, same color is assigned to all blocks that make up this piece.
    public float4 color;
}
