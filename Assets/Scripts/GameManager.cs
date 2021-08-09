using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GameAreaProperties))]
public class GameManager : MonoBehaviour
{
    //base platform players will stand on, will be scaled to fit the required size
    public GameObject basePlatformPrefab;

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
