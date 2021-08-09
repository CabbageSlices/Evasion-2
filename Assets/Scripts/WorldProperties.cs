using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldProperties : MonoBehaviour
{
    
    //specifies the size of a block relative to unity's unit
    //eg.in unity a distance of 1 means this many pixels
    public int unityUnitToBlockSize;

    private static WorldProperties _instance;

    public static WorldProperties Instance { get { return _instance; }}

    private void Awake() {
        
        if(_instance && _instance != this) {
            Destroy(this);
        }

        _instance = this;
    }

}
