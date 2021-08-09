using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;


struct SpawnerComponent : IComponentData
{
    public Entity entityToSpawn;
    public double delayUntilNextSpawn;

    public double timeLastSpawnAt;

    public float2 initialVelocity;
}

//http://docs.unity3d.com/Packages/com.unity.entities@0.17/manual/conversion.html
[DisallowMultipleComponent]
public class SpawnerAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    //item that should be spawned
    public GameObject itemToSpawn;

    //time between item spawns
    public double spawnDelay;

    public float2 initialVelocity;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new SpawnerComponent
        {
            entityToSpawn = conversionSystem.GetPrimaryEntity(itemToSpawn),
            delayUntilNextSpawn = spawnDelay,
            timeLastSpawnAt = 0,
            initialVelocity = initialVelocity
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(itemToSpawn);
    }
}
