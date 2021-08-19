using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

[InternalBufferCapacity(6)]
public struct TetrisPieceBufferElement : IBufferElementData {
    public Entity tetrisPiece;
}

public class TetrisPiecesAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public List<GameObject> tetrisPieces;
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) {
        
        foreach(GameObject piece in tetrisPieces) {
            referencedPrefabs.Add(piece);
        }
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        DynamicBuffer<TetrisPieceBufferElement> buffer = dstManager.AddBuffer<TetrisPieceBufferElement>(entity);
        
        foreach(GameObject piece in tetrisPieces) {

            buffer.Add(new TetrisPieceBufferElement{ tetrisPiece = conversionSystem.GetPrimaryEntity(piece)});
        }
        
    }
}
