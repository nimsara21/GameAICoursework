using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GenerateGrid : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject blockGameObject;
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] private GameObject waterBlock;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject healthPotionPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private GameObject magicSpellPrefab;
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Grid Settings")]
    private int tileSize = 1;
    private int worldSizeX = 60;
    private int worldSizeZ = 60;
    // public float 1 = 0;
    public int noiseHeight = 5;
    public int objectsCount = 25;
    public int lakesCount = 3;
    public int numHealthPoints = 4;
    public int numCoins = 5;
    public int numSheilds = 6;
    public int numMagics = 2;
    public int numWeapons = 3;
    public int numTraps = 4;
    public int numEnemies = 4;
    public int numWaypoints = 5;
    public float distanceBetweenWaypoints = 5f;

    private List<Vector3> blockPositions = new List<Vector3>();
    private List<LakeParameters> lakes = new List<LakeParameters>();
    private struct LakeParameters
    {
        public Vector2 center;
        public float radius;
    }

    private NavMeshData navMeshData = null;
    private NavMeshSurface navMeshSurface = null;

    void Start()
    {
        navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.layerMask = LayerMask.GetMask("Walkable");
        navMeshData = new NavMeshData();
        navMeshSurface.navMeshData = navMeshData;

        GenerateInitialTiles();
        GenerateGridAndLakes();
        // CreateTerrain();
        // SpawnObjects();
       
        // SpawnAssets();
        // SpawnObjectToSpawn();
        // SpawnPlayer();
    }

    private void GenerateInitialTiles()
{
    for (int x = 0; x <= worldSizeX; x++)
    {
        for (int z = 0; z <= worldSizeZ; z++)
        {
            float randomVal = generateNoise(x, z, 8f);
            Vector3 pos = new Vector3(x * tileSize, randomVal * tileSize, z * tileSize);
            GameObject thisTile = Instantiate(blockGameObject, pos, Quaternion.identity);
            thisTile.layer = LayerMask.NameToLayer("Walkable");
        }
    }
    // navMeshSurface.BuildNavMesh();
}
    
    private void GenerateGridAndLakes()
    {
        GenerateRandomLakes();

        for (int x = 0; x < worldSizeX; x++)
        {
            for (int z = 0; z < worldSizeZ; z++)
            {
                Vector3 pos = new Vector3(x * 1,
                    generateNoise(x, z, 8f) * noiseHeight,
                    z * 1);

                if (IsInAnyLake(x, z))
                {
                    GameObject water = Instantiate(waterBlock, pos, Quaternion.identity);
                    water.layer = LayerMask.NameToLayer("Not Walkable");
                    // AddNavMeshObstacle(water);
                }
                else
                {
                    GameObject block = Instantiate(blockGameObject, pos, Quaternion.identity);
                    blockPositions.Add(block.transform.position);
                    block.transform.SetParent(this.transform);
                    block.layer = LayerMask.NameToLayer("Walkable");
                }
            }
        }
        navMeshSurface.BuildNavMesh();
    }

    private void GenerateRandomLakes()
    {
        for (int i = 0; i < lakesCount; i++)
        {
            LakeParameters lake;
            lake.center = new Vector2(Random.Range(0, worldSizeX), Random.Range(0, worldSizeZ));
            lake.radius = Random.Range(5f, 15f);
            lakes.Add(lake);
        }
    }

    private void AddNavMeshObstacle(GameObject obj)
    {
        NavMeshObstacle navMeshObstacle = obj.AddComponent<NavMeshObstacle>();
        navMeshObstacle.shape = NavMeshObstacleShape.Box;
        navMeshObstacle.size = new Vector3(1, 1f, 1);
        navMeshObstacle.carveOnlyStationary = false;
        navMeshObstacle.carving = true;
    }

    private void SpawnObjects()
    {
        SpawnAssetInRandomLocation(trapPrefab, numTraps);
    }

    private void SpawnAssetInRandomLocation(GameObject assetPrefab, int numInstances)
    {
        for (int i = 0; i < numInstances; ++i)
        {
            int maxAttempts = 100;
            int currentAttempt = 0;

            while (currentAttempt < maxAttempts)
            {
                Vector3 randomPosition = blockPositions[Random.Range(0, blockPositions.Count)];

                if (!IsInAnyLake((int)randomPosition.x, (int)randomPosition.z))
                {
                    Instantiate(assetPrefab, new Vector3(randomPosition.x, randomPosition.y + 0.5f, randomPosition.z), Quaternion.identity);
                    break;
                }

                currentAttempt++;
            }
        }
    }

    private float generateNoise(int x, int z, float detailScale)
{
    float xNoise = (x + this.transform.position.x) / detailScale;
    float zNoise = (z + this.transform.position.z) / detailScale; // Changed from y to z
    return Mathf.PerlinNoise(xNoise, zNoise);
}

    private bool IsInAnyLake(int x, int z)
    {
        foreach (var lake in lakes)
        {
            if (IsInLake(x, z, (int)lake.center.x, (int)lake.center.y, lake.radius))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsInLake(int x, int z, int centerX, int centerZ, float radius)
    {
        float distance = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));
        return distance <= radius;
    }


    private void SpawnPlayer()
    {
        Vector3 spawnPosition = GetRandomWalkablePosition();

        if (spawnPosition != Vector3.zero)
        {
            GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            NavMeshAgent playerAgent = player.GetComponent<NavMeshAgent>();

            if (playerAgent != null)
            {
                Vector3 targetPosition = GetRandomWalkablePosition();
                playerAgent.SetDestination(targetPosition);
            }
        }
        else
        {
            Debug.LogError("Unable to find a suitable spawn position for the player.");
        }
    }

    private Vector3 GetRandomWalkablePosition()
    {
        int maxAttempts = 100;
        int currentAttempt = 0;

        while (currentAttempt < maxAttempts)
        {
            Vector3 randomPosition = blockPositions[Random.Range(0, blockPositions.Count)];

            if (!IsInAnyLake((int)randomPosition.x, (int)randomPosition.z))
            {
                return randomPosition;
            }

            currentAttempt++;
        }

        return Vector3.zero;
    }

    private void SpawnAssets()
    {
        SpawnAssetInRandomLocation(healthPotionPrefab, numHealthPoints);
        SpawnAssetInRandomLocation(coinPrefab, numCoins);
        SpawnAssetInRandomLocation(magicSpellPrefab, numMagics);
        SpawnAssetInRandomLocation(shieldPrefab, numSheilds);
        SpawnAssetInRandomLocation(weaponPrefab, numWeapons);
        SpawnEnemyInRandomLocation(numEnemies);
    }
    private void SpawnObjectToSpawn()
    {
        for (int c = 0; c < objectsCount; c++)
        {
            GameObject toPlaceObject = Instantiate(objectToSpawn,
                ObjectSpawnLocation(),
                Quaternion.identity);
            //toPlaceObject.tag = "Obstacle";
        }
    }

    private Vector3 ObjectSpawnLocation()
    {
        int rndIndex = Random.Range(0, blockPositions.Count);
        Vector3 newPos = new Vector3(
            blockPositions[rndIndex].x,
            blockPositions[rndIndex].y + 0.5f,
            blockPositions[rndIndex].z);

        blockPositions.RemoveAt(rndIndex);

        return newPos;
    }

    private void SpawnEnemyInRandomLocation(int numEnemiesToSpawn)
    {
        for (int i = 0; i < numEnemiesToSpawn; ++i)
        {
            int maxAttempts = 100;
            int currentAttempt = 0;
            bool enemySpawned = false;

            while (currentAttempt < maxAttempts && !enemySpawned)
            {
                Vector3 randomPosition = blockPositions[Random.Range(0, blockPositions.Count)];

                if (!IsInAnyLake((int)randomPosition.x, (int)randomPosition.z))
                {
                    GameObject enemy = Instantiate(enemyPrefab, new Vector3(randomPosition.x, randomPosition.y, randomPosition.z), Quaternion.identity);

                    NavMeshAgent enemyAgent = enemy.GetComponent<NavMeshAgent>();
                    enemyAgent.radius = 1 / 2f;
                    enemyAgent.height = 1.0f;
                    enemyAgent.baseOffset = 0.5f;

                   // EnemyMovement enemyMovement = enemy.AddComponent<EnemyMovement>();

                    //enemyMovement.patrolPoints = new Transform[numWaypoints];
                    for (int j = 0; j < numWaypoints; j++)
                    {
                        float angle = j * 2 * Mathf.PI / numWaypoints;
                        float x = Mathf.Cos(angle) * distanceBetweenWaypoints;
                        float z = Mathf.Sin(angle) * distanceBetweenWaypoints;

                        Vector3 waypointPosition = new Vector3(x, 0f, z) + randomPosition;
                        GameObject waypointObject = new GameObject("Waypoint" + j);
                        waypointObject.transform.position = waypointPosition;
                       // enemyMovement.patrolPoints[j] = waypointObject.transform;
                    }

                    //enemyMovement.bulletPrefab = bulletPrefab;

                    enemySpawned = true;
                }

                currentAttempt++;
            }
        }
    }
}