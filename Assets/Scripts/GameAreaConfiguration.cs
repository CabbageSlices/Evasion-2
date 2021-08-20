using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GameAreaConfiguration : MonoBehaviour
{
    //specifies the size of a block relative to unity's unit
    //eg.in unity a distance of 1 means this many pixels
    public int unityUnitToBlockSize = 32;

    //set to the game area so it can auto calculate the number of columns, horizontal location of the base platform, etc.
    public Rect gameplayArea;


    //position of the intiail platform relative to the center of the play area
    public int initialPlatformVerticalPositionFromGameAreaCenter;

    private static GameAreaConfiguration _instance;

    public static GameAreaConfiguration Instance { get { return _instance; }}

    private void Awake() {
        
        if(_instance && _instance != this) {
            Destroy(this);
        }

        _instance = this;
    }

    private void Update() {
        //FOR EDIT MODE, make sure center of gameplay area is always aligned with transform
        gameplayArea.center = transform.position;
    }

}
