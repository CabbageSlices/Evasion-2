using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public struct GameAreaProperties {
    int gameAreaLeft;
    int gameAreaRight;
    int gameAreaTop;
    int gameAreaBottom;

    int sizeOfSideGaps;
    int basePlatformWidth;
}

[RequireComponent(typeof(GameAreaConfiguration))]
public class GameManager : MonoBehaviour
{
    //base platform players will stand on, will be scaled to fit the required size
    public GameObject basePlatformPrefab;

    private GameManager _instance;

    private BlobAssetStore blobAssetStore;

    public GameManager Instance {
        get {
            return _instance;
        }
    }

    private bool gameStarted = false;

    // Start is called before the first frame update
    void Awake()
    {
        if(_instance && _instance != this) {
            Destroy(this.gameObject);
        }

        _instance = this;

        blobAssetStore = new BlobAssetStore();
    }

    private void OnDestroy() {
        blobAssetStore.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && !gameStarted) {
            startGame();
        }

    }

    //setup game world and start
    //create base platofmr, block spawn region, death line, initial speeds for blocks and camera scrolling,
    //spawn players, create UI ffor players, blah blah blah
    public void startGame() {
        
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        Entity basePlatformEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(basePlatformPrefab, settings);

        
        Rect gameplayArea = GameAreaConfiguration.Instance.gameplayArea;
        
        int gameAreaBottom = Mathf.FloorToInt(gameplayArea.yMin);
        int gameAreaTop = Mathf.CeilToInt(gameplayArea.yMax);
        int gameAreaHeight = gameAreaTop - gameAreaBottom;
        int gameAreaVerticalCenter = Mathf.FloorToInt(gameplayArea.center.y);
        int gameAreaStartingPosition = Mathf.FloorToInt(gameplayArea.xMin);

        //calculate game bounds
        int gameAreaWidth =  Mathf.CeilToInt(gameplayArea.xMax) - Mathf.FloorToInt(gameplayArea.xMin);

        //10%  of the playable region should be free on either side  for players to fall of fand die
        int sizeOfSideGaps = Mathf.FloorToInt(gameAreaWidth * 0.1f);

        int basePlatformLeft = gameAreaStartingPosition + sizeOfSideGaps;
        int basePlatformWidth = gameAreaWidth - sizeOfSideGaps * 2;//multiply by two since theres a gap on either side
        int basePlatformBottom = gameAreaVerticalCenter + GameAreaConfiguration.Instance.initialPlatformVerticalPositionFromGameAreaCenter;

        //spawn platform
        Entity basePlatformInstance = entityManager.Instantiate(basePlatformEntity);

        //set platform position
        entityManager.SetComponentData<Translation>(basePlatformInstance, new Translation{ Value = new Vector3(basePlatformLeft, basePlatformBottom, 0)});
        entityManager.AddComponentData<NonUniformScale>(basePlatformInstance, new NonUniformScale{ Value = new Vector3(basePlatformWidth, 1, 1)});


        gameStarted = true;
    }
}
