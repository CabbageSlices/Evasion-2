using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;

public struct GameAreaProperties {
    public int gameAreaLeft;
    public int gameAreaRight;
    public int gameAreaTop;
    public int gameAreaBottom;

    public int sizeOfSideGaps;
    public int basePlatformWidth;
    public int basePlatformLeft;
    public int basePlatformBottom;

    public int blockSpawnBoundsLeft;
    public int blockSpawnBoundsRight; //right edge of the spawn region, this cell SHOULDn'T have a block spawn here since the block will be off screen.
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

    public bool gameStarted {
        get;
        private set;
    } = false;

    public bool gameInitialized {
        get;
        private set;
    } = false;

    public GameAreaProperties gameProperties {
        get;
        private set;
    }



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
        
        Rect gameplayArea = GameAreaConfiguration.Instance.gameplayArea;
        
        int gameAreaBottom = Mathf.FloorToInt(gameplayArea.yMin);
        int gameAreaTop = Mathf.CeilToInt(gameplayArea.yMax);
        int gameAreaHeight = gameAreaTop - gameAreaBottom;
        int gameAreaVerticalCenter = Mathf.FloorToInt(gameplayArea.center.y);
        int gameAreaLeft = Mathf.FloorToInt(gameplayArea.xMin);
        int gameAreaRight = Mathf.CeilToInt(gameplayArea.xMax);

        //calculate game bounds
        int gameAreaWidth =  gameAreaRight - gameAreaLeft;

        //10%  of the playable region should be free on either side  for players to fall of fand die
        int sizeOfSideGaps = Mathf.FloorToInt(gameAreaWidth * 0.1f);

        int basePlatformLeft = gameAreaLeft + sizeOfSideGaps;
        int basePlatformWidth = gameAreaWidth - sizeOfSideGaps * 2;//multiply by two since theres a gap on either side
        int basePlatformBottom = gameAreaVerticalCenter + GameAreaConfiguration.Instance.initialPlatformVerticalPositionFromGameAreaCenter;

        gameProperties = new GameAreaProperties {
            gameAreaLeft = gameAreaLeft,
             gameAreaRight = gameAreaRight,
             gameAreaTop = gameAreaTop,
             gameAreaBottom = gameAreaBottom,

             sizeOfSideGaps = sizeOfSideGaps,
             basePlatformWidth = basePlatformWidth,
             basePlatformLeft = basePlatformLeft,
             basePlatformBottom = basePlatformBottom,

             blockSpawnBoundsLeft = basePlatformLeft,
             blockSpawnBoundsRight = basePlatformLeft + basePlatformWidth,
        };

        createBasePlatform();

        gameInitialized = true;
    }

    private void createBasePlatform() {

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        Entity basePlatformEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(basePlatformPrefab, settings);

        //spawn platform
        Entity basePlatformInstance = entityManager.Instantiate(basePlatformEntity);

        //set platform position
        entityManager.SetComponentData<Translation>(basePlatformInstance, new Translation{ Value = new Vector3(gameProperties.basePlatformLeft, gameProperties.basePlatformBottom, 0)});
        entityManager.AddComponentData<NonUniformScale>(basePlatformInstance, new NonUniformScale{ Value = new Vector3(gameProperties.basePlatformWidth, 1, 1)});
        entityManager.RemoveComponent<PhysicsVelocity>(basePlatformInstance);
    }

    private void createBlockSpawner() {
        
    }
}
