using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //base platform players will stand on
    public GameObject basePlatformPrefab;

    //specifies the size of a block relative to unity's unit
    //eg.in unity a distance of 1 means this many pixels
    public int unityUnitToBlockSize;

    //initial height that blocks will spawn at
    public int initialBlockSpawnHeight;

    //set to the game area so it can auto calculate the number of columns, horizontal location of the base platform, etc.
    public Rect gameplayArea;

    public int initialPlatformVerticalPosition;

    private GameManager _instance;

    public GameManager Instance {
        get {
            return _instance;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        if(_instance && _instance != this) {
            Destroy(this.gameObject);
        }

        _instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
