using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Physics;

public struct GameInstanceData {
    public int gameAreaLeft;
    public int gameAreaRight;
    public int gameAreaTop;
    public int gameAreaBottom;

    public int gameAreaHorizontalCenter;
    public int gameAreaVerticalCenter;

    public int sizeOfSideGaps;
    public int basePlatformWidth;
    public int basePlatformLeft;
    public int basePlatformBottom;

    public int blockSpawnBoundsLeft;
    public int blockSpawnBoundsRight; //right edge of the spawn region, this cell SHOULDn'T have a block spawn here since the block will be off screen.

    public Entity basePlatformInstance;
    public Entity spawnerInstance;
}

[RequireComponent(typeof(GameAreaConfiguration))]
public class GameManager : MonoBehaviour
{
    //base platform players will stand on, will be scaled to fit the required size
    public GameObject basePlatformPrefab;
    public GameObject spawnerPrefab;

    private GameInstanceData _gameInstanceData;

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

    public bool gameInstanceDataCreated {
        get;
        private set;
    } = false;

    public GameInstanceData gameInstanceData {
        get {
            return _gameInstanceData;
        }
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
        int gameAreaHorizontalCenter = Mathf.FloorToInt(gameplayArea.center.x);

        //calculate game bounds
        int gameAreaWidth =  gameAreaRight - gameAreaLeft;

        //10%  of the playable region should be free on either side  for players to fall of fand die
        int sizeOfSideGaps = Mathf.FloorToInt(gameAreaWidth * 0.1f);

        int basePlatformLeft = gameAreaLeft + sizeOfSideGaps;
        int basePlatformWidth = gameAreaWidth - sizeOfSideGaps * 2;//multiply by two since theres a gap on either side
        int basePlatformBottom = gameAreaVerticalCenter + GameAreaConfiguration.Instance.initialPlatformVerticalPositionFromGameAreaCenter;

        _gameInstanceData = new GameInstanceData {
            gameAreaLeft = gameAreaLeft,
             gameAreaRight = gameAreaRight,
             gameAreaTop = gameAreaTop,
             gameAreaBottom = gameAreaBottom,

             gameAreaHorizontalCenter = gameAreaHorizontalCenter,
             gameAreaVerticalCenter = gameAreaVerticalCenter,

             sizeOfSideGaps = sizeOfSideGaps,
             basePlatformWidth = basePlatformWidth,
             basePlatformLeft = basePlatformLeft,
             basePlatformBottom = basePlatformBottom,

             blockSpawnBoundsLeft = basePlatformLeft,
             blockSpawnBoundsRight = basePlatformLeft + basePlatformWidth,
        };

        gameInstanceDataCreated = true;

        createBasePlatform();
        createTetrisPieceSpawner();

        gameInitialized = true;
    }

    unsafe private void createBasePlatform() {

        if(spawnerPrefab == null || basePlatformPrefab == null) {
            Debug.LogError("Prefab missing in game manager"!);
            return;
        }

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        Entity basePlatformEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(basePlatformPrefab, settings);

        _gameInstanceData.basePlatformInstance = entityManager.Instantiate(basePlatformEntity);
        
        //spawn platform
        Entity basePlatformInstance = _gameInstanceData.basePlatformInstance;
        entityManager.SetName(basePlatformInstance, "Base Platform");

        //set platform position
        entityManager.AddComponentData<NonUniformScale>(basePlatformInstance, new NonUniformScale{ Value = new Vector3(_gameInstanceData.basePlatformWidth, 1, 1)});
        entityManager.SetComponentData<Translation>(basePlatformInstance, new Translation{ Value = new Vector3(_gameInstanceData.basePlatformLeft, _gameInstanceData.basePlatformBottom, 0)});
        entityManager.RemoveComponent<PhysicsVelocity>(basePlatformInstance);
        entityManager.AddComponent<TagStatic>(basePlatformInstance);
        
        //get the authoring shape to inintialize the collider geometry
        Unity.Physics.Authoring.PhysicsShapeAuthoring shapeAuthoring = basePlatformPrefab.GetComponent<Unity.Physics.Authoring.PhysicsShapeAuthoring>();

        PhysicsCollider currentCollider = entityManager.GetComponentData<PhysicsCollider>(basePlatformInstance);
        var collider = (Unity.Physics.BoxCollider*)currentCollider.Value.GetUnsafePtr();

        BoxGeometry geometry = collider->Geometry;
        geometry.Size *= new Unity.Mathematics.float3(_gameInstanceData.basePlatformWidth, 1, 1);
        geometry.Center *= new Unity.Mathematics.float3(_gameInstanceData.basePlatformWidth, 1, 1);

        entityManager.AddComponentData<PhysicsCollider>(basePlatformInstance, new PhysicsCollider{ 
            Value = Unity.Physics.BoxCollider.Create(geometry)
        });
        
    }

    private void createTetrisPieceSpawner() {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        GameObjectConversionSettings settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore);
        Entity spawnerEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(spawnerPrefab, settings);

        _gameInstanceData.spawnerInstance = entityManager.Instantiate(spawnerEntity);
        Entity spawnerInstance = _gameInstanceData.spawnerInstance;

        entityManager.SetComponentData<SpawnAreaHorizontalBounds>(spawnerInstance, new SpawnAreaHorizontalBounds{ left = _gameInstanceData.blockSpawnBoundsLeft, right = _gameInstanceData.blockSpawnBoundsRight});
        entityManager.SetComponentData<Translation>(spawnerInstance, new Translation{ Value = new Vector3(
            _gameInstanceData.gameAreaHorizontalCenter, _gameInstanceData.gameAreaTop, 0
        )});

        entityManager.SetName(spawnerInstance, "Spawner");
    }
}
