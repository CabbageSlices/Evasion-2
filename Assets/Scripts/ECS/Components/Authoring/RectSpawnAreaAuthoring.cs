using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

struct RectSpawnAreaComponent : IComponentData
{
    public float left;
    public float right;
    public float top;
    public float bottom;
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Transform))]
public class RectSpawnAreaAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Colour used to display the spawn area within the scene view. NOT used for the component")]
    public Color spawnAreaColor = new Color(0.9f, 1f, 0f, 0.15f);
    //transform scale/position is used to determine spawn area location. 
    //Set using the spawn area editor script


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        Vector3 position = transform.position;
        Vector3 scale = transform.lossyScale;

        dstManager.AddComponentData(entity, new RectSpawnAreaComponent
        {
            left = position.x - scale.x / 2,
            right = position.x + scale.x / 2,
            top = position.y + scale.y / 2,
            bottom = position.y - scale.y / 2
        });
    }
}
