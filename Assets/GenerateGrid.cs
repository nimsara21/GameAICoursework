using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class GenerateGrid : MonoBehaviour
{
    public GameObject blockGameObject;
    public GameObject objectToSpawn;
    public GameObject waterBlock;
    public GameObject playerPrefab; // Player prefab to spawn
    public GameObject healthPotionPrefab;
    public GameObject coinPrefab;
    public GameObject weaponPrefab;
    public GameObject magicSpellPrefab;
    public GameObject shieldPrefab;
    public GameObject trapPrefab;

    private int worldSizeX = 40;
    private int worldSizeZ = 40;
    public float gridOffset = 1.1f;
    public int noiseHeight = 5;
    public int objectsCount = 25;
    public int lakesCount = 3;

    public int numHealthPoints = 4;
    public int numCoins = 5;
    public int numSheilds = 6;
    public int numMagics = 2;
    public int numWeapons = 3;
    public int numTraps = 4;

    Mesh mesh;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    NavMeshSurface navMeshSurface;
    MeshCollider meshCollider;

    private List<Vector3> blockPositions = new List<Vector3>();

    private struct LakeParameters
    {
        public Vector2 center;
        public float radius;
    }

    private List<LakeParameters> lakes = new List<LakeParameters>();

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        mesh.name = "Procedural Terrain";
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Particles/Standard Unlit"));
        meshRenderer.material = material;

        navMeshSurface = GetComponent<NavMeshSurface>();
        meshCollider = GetComponent<MeshCollider>();

        GenerateGridAndLakes();
        SpawnObject();
        SpawnPlayer();  // Spawn a single player
        SpawnAssets();
        CreateTerrain();
    }

    private void GenerateGridAndLakes()
    {
        GenerateRandomLakes();

        for (int x = 0; x < worldSizeX; x++)
        {
            for (int z = 0; z < worldSizeZ; z++)
            {
                Vector3 pos = new Vector3(x * gridOffset,
                    generateNoise(x, z, 8f) * noiseHeight,
                    z * gridOffset);

                // Check if this block is part of any lake
                if (IsInAnyLake(x, z))
                {
                    // Create water block for the lake
                    GameObject water = Instantiate(waterBlock, pos, Quaternion.identity) as GameObject;
                    water.tag = "WaterBlock"; // Set the tag to WaterBlock

                    // Add NavMeshObstacle component during runtime
                    AddNavMeshObstacle(water);
                }
                else
                {
                    // Create regular block or other terrain feature
                    GameObject block = Instantiate(blockGameObject, pos, Quaternion.identity) as GameObject;
                    blockPositions.Add(block.transform.position);
                    block.transform.SetParent(this.transform);

                    // Set the tag to WalkableBlock and add a collider
                    block.tag = "blockGameObject";
                    AddColliderToBlock(block);

                    // Add a NavMeshAgent component only to walkable objects
                    if (block.CompareTag("blockGameObject"))
                    {
                        NavMeshAgent agent = block.AddComponent<NavMeshAgent>();
                        agent.radius = gridOffset / 2f; // Adjust the agent radius to half of the block's size
                        agent.height = 1.0f; // Adjust the agent height to the block's height
                        agent.baseOffset = 0.5f; // Adjust the base offset to half of the block's height
                        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                    }
                }
            }
        }
    }

    private void AddColliderToBlock(GameObject block)
    {
        // Add a collider to the block (you can adjust the collider type as needed)
        BoxCollider boxCollider = block.AddComponent<BoxCollider>();
        // Adjust the size and position of the collider to fit your block size
        boxCollider.size = new Vector3(gridOffset, 1f, gridOffset);
        boxCollider.center = new Vector3(gridOffset / 2f, 0.5f, gridOffset / 2f);
    }

    private void GenerateRandomLakes()
    {
        for (int i = 0; i < lakesCount; i++) // Adjust the number of lakes as needed
        {
            LakeParameters lake;
            lake.center = new Vector2(Random.Range(0, worldSizeX), Random.Range(0, worldSizeZ));
            lake.radius = Random.Range(5f, 15f);
            lakes.Add(lake);
        }
    }

    private void AddNavMeshObstacle(GameObject obj)
    {
        // Add a NavMeshObstacle to make the water block not walkable during runtime
        NavMeshObstacle navMeshObstacle = obj.AddComponent<NavMeshObstacle>();
        navMeshObstacle.shape = NavMeshObstacleShape.Box;
        navMeshObstacle.size = new Vector3(gridOffset, 1f, gridOffset);

        // Set carve to true to make it totally unwalkable
        navMeshObstacle.carveOnlyStationary = false;
        navMeshObstacle.carving = true;
    }

    private void SpawnObject()
    {
        for (int c = 0; c < objectsCount; c++)
        {
            GameObject toPlaceObject = Instantiate(objectToSpawn,
                ObjectSpawnLocation(),
                Quaternion.identity);
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

    private float generateNoise(int x, int z, float detailScale)
    {
        float xNoise = (x + this.transform.position.x) / detailScale;
        float zNoise = (z + this.transform.position.y) / detailScale;
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

    private void CreateTerrain()
    {
        navMeshSurface.BuildNavMesh();
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPosition = GetRandomWalkablePosition();

        if (spawnPosition != Vector3.zero)
        {
            GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            NavMeshAgent playerAgent = player.GetComponent<NavMeshAgent>();

            if (playerAgent != null)
            {
                // Set the destination for the player agent
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

            // Check if the position is not in the water and not on an object to spawn
            if (!IsInAnyLake((int)randomPosition.x, (int)randomPosition.z) &&
                !IsOnObjectToSpawn((int)randomPosition.x, (int)randomPosition.z))
            {
                return randomPosition;
            }

            currentAttempt++;
        }

        // If no suitable position is found, return Vector3.zero
        return Vector3.zero;
    }

    private bool IsOnObjectToSpawn(int x, int z)
    {
        foreach (var spawnObjectPosition in blockPositions)
        {
            if (Mathf.Approximately(spawnObjectPosition.x, x) && Mathf.Approximately(spawnObjectPosition.z, z))
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnAssets()
    {

        for (int i = 0; i < numHealthPoints; ++i)
        {
            SpawnAssetInRandomLocation(healthPotionPrefab);
        }

        for (int i = 0; i < numCoins; ++i)
        {
            SpawnAssetInRandomLocation(coinPrefab);
        }

        for (int i = 0; i < numMagics; ++i)
        {
            SpawnAssetInRandomLocation(magicSpellPrefab);
        }
        for (int i = 0; i < numSheilds; ++i)
        {
            SpawnAssetInRandomLocation(shieldPrefab);
        }
        for (int i = 0; i < numTraps; ++i)
        {
            SpawnAssetInRandomLocation(trapPrefab);
        }
        for (int i = 0; i < numWeapons; ++i)
        {
            SpawnAssetInRandomLocation(weaponPrefab);
        }


    }

    private void SpawnAssetInRandomLocation(GameObject assetPrefab)
    {
        int maxAttempts = 100;
        int currentAttempt = 0;

        while (currentAttempt < maxAttempts)
        {
            Vector3 randomPosition = blockPositions[Random.Range(0, blockPositions.Count)];

            // Check if the position is not in the water and not on an object to spawn
            if (!IsInAnyLake((int)randomPosition.x, (int)randomPosition.z) &&
                !IsOnObjectToSpawn((int)randomPosition.x, (int)randomPosition.z))
            {
                Instantiate(assetPrefab, new Vector3(randomPosition.x, randomPosition.y + 0.5f, randomPosition.z), Quaternion.identity);
                break;
            }

            currentAttempt++;
        }
    }
}
