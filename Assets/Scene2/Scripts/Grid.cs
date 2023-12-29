using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class Grid : MonoBehaviour
{

    
    public GameObject[] treePrefabs;
    public Material terrainMaterial;
    public Material edgeMaterial;
    public float waterLevel = .4f;
    public float scale = .1f;
    public float treeNoiseScale = .05f;
    public float treeDensity = .5f;
    public int size = 100;
    public Vector3 startPoint { get; set; }
    public Vector3 endPoint { get; set; }

    public GameObject playerPrefab;
    public GameObject[] enemyPrefabs;
    public int numberOfEnemies = 6;

    public Transform patrolPointPrefab;
    public int numberOfPatrolPoints = 5;

    private List<Transform> patrolPoints = new List<Transform>();


    private int playerScore = 0;
    public static int playerHealth;
    public Text scoreText; 
    public Text healthText;


    Cell[,] grid;
    private GameObject player;
    public Cell[,] GridArray
    {
        get { return grid; }
    }

        public List<Transform> GetPatrolPoints()
    {
        return patrolPoints;
        
    }




    public GameObject coinPrefab;
    public GameObject healthPrefab;
    public GameObject potionPrefab;
    public GameObject poisonPrefab;
    public GameObject magicPrefab;
    public GameObject shieldPrefab;


    public int numberOfCoins = 10;
    public int numberOfHealths = 5;
    public int numberOfPotions = 3;
    public int numberOfPoisons = 2;
    public int numberOfShields = 1;

     private List<Transform> collectibles = new List<Transform>();



    void Start()
    {

        
        playerHealth = PlayerController.maxHealth;
         healthText.text = "Health: " + playerHealth;

        InstantiatePlane();
        float[,] noiseMap = new float[size, size];
        (float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * scale + xOffset, y * scale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        float[,] falloffMap = new float[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float xv = x / (float)size * 2 - 1;
                float yv = y / (float)size * 2 - 1;
                float v = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                falloffMap[x, y] = Mathf.Pow(v, 3f) / (Mathf.Pow(v, 3f) + Mathf.Pow(2.2f - 2.2f * v, 3f));
            }
        }

        grid = new Cell[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = noiseMap[x, y];
                noiseValue -= falloffMap[x, y];
                bool isWater = noiseValue < waterLevel;
                Cell cell = new Cell(isWater);
                grid[x, y] = cell;
            }
        }
        

        
        DrawTerrainMesh(grid);
        DrawEdgeMesh(grid);
        DrawTexture(grid);
        GenerateTrees(grid);
        AddGridCollider();
       
        SpawnEnemiesAtRandom();
        // SpawnPlayerOnNavMesh();
        
        GeneratePatrolPoints();
        //SpawnCollectibles(coinPrefab, 5);
        //SpawnCollectibles(coinPrefab, 6);
        //SpawnCollectibles(healthPrefab, 5);
        GenerateCollectibles(grid);
        player = GameObject.FindWithTag("Player");
    }

        void Update()
    {
        CheckCollectibleCollisions();
        UpdateScoreDisplay();
    }
    



void InstantiatePlane()
{
    // Create a plane and position 
    GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
    plane.transform.position = new Vector3(75f, -0.8f, 75f);
    plane.transform.localScale = new Vector3(size / 10f, 1f, size / 10f);

    //Material
    Material planeMaterial = new Material(Shader.Find("Custom/SeaGradientShader"));

    // gradient color
    planeMaterial.color = Color.blue; 
    // Assign the material 
    Renderer planeRenderer = plane.GetComponent<Renderer>();
    if (planeRenderer != null)
    {
        planeRenderer.material = planeMaterial;
    }

   
    Destroy(plane.GetComponent<Collider>());
}

    void UpdateScoreDisplay()
    {
        
        if (scoreText != null)
        {
            scoreText.text = "Score: " + playerScore;
        }
    }

    void UpdateHealthDisplay()
    {
        healthText.text = "Health: " + playerHealth;
    }
    
    void DrawTerrainMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (!cell.isWater)
                {
                    Vector3 a = new Vector3(x - .5f, 0, y + .5f);
                    Vector3 b = new Vector3(x + .5f, 0, y + .5f);
                    Vector3 c = new Vector3(x - .5f, 0, y - .5f);
                    Vector3 d = new Vector3(x + .5f, 0, y - .5f);
                    Vector2 uvA = new Vector2(x / (float)size, y / (float)size);
                    Vector2 uvB = new Vector2((x + 1) / (float)size, y / (float)size);
                    Vector2 uvC = new Vector2(x / (float)size, (y + 1) / (float)size);
                    Vector2 uvD = new Vector2((x + 1) / (float)size, (y + 1) / (float)size);
                    Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                    Vector2[] uv = new Vector2[] { uvA, uvB, uvC, uvB, uvD, uvC };
                    for (int k = 0; k < 6; k++)
                    {
                        vertices.Add(v[k]);
                        triangles.Add(triangles.Count);
                        uvs.Add(uv[k]);
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        UnityEngine.AI.NavMeshSurface navMeshSurface = gameObject.AddComponent<UnityEngine.AI.NavMeshSurface>();
        navMeshSurface.collectObjects = UnityEngine.AI.CollectObjects.Children; 
        navMeshSurface.BuildNavMesh(); // Build the NavMesh
        
        SpawnPlayerOnNavMesh();
        //GenerateCollectibles();
    }




Vector3 GetRandomPointOnNavMesh(int attempts, float range)
    {
        for (int i = 0; i < attempts; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-range, range),
                0,
                Random.Range(-range, range)
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return Vector3.positiveInfinity;
    }

void GeneratePatrolPoints()
{
    for (int i = 0; i < numberOfPatrolPoints; i++)
    {
       
        GameObject newPatrolPointObject = new GameObject("PatrolPoint");

       
        newPatrolPointObject.transform.position = GetRandomNavMeshPosition();

        
        Transform newPatrolPoint = newPatrolPointObject.transform;
        patrolPoints.Add(newPatrolPoint);
    }
}

Vector3 GetRandomNavMeshPosition()
{
    Vector3 randomPosition = Vector3.zero;

   
    NavMeshHit hit;
    for (int i = 0; i < 500; i++)
    {
        // random position within the specified range
        randomPosition = new Vector3(Random.Range(-500, 500), 0f, Random.Range(-500, 500));

        // Check if the random position is on the NavMesh
        if (NavMesh.SamplePosition(randomPosition, out hit, 100, NavMesh.AllAreas))
        {
            return hit.position;
        }
    }

   
    return Vector3.zero;
}



    Transform InstantiateCube(Vector3 position)
{
    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    cube.transform.position = position;
    return cube.transform;
}

   Vector3 GetRandomGridPosition()
    {
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        return new Vector3(x, 0, z);
    }

 

   
    public void AddPatrolPoint()
    {
        Transform newPatrolPoint = Instantiate(patrolPointPrefab, GetRandomGridPosition(), Quaternion.identity);
        patrolPoints.Add(newPatrolPoint);
    }


 void AddGridCollider()
    {
       
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();

        
        collider.size = new Vector3(size, 0.1f, size);

        
        collider.center = new Vector3(size / 2f - 0.5f, 0.05f, size / 2f - 0.5f);

        
        collider.isTrigger = true;
    }


void SpawnPlayerOnNavMesh(){
    Vector3 randomPoint = GetRandomPointOnNavMeshForPlayer(100, 500f);
    if (randomPoint != Vector3.positiveInfinity)
    {
        Instantiate(playerPrefab, randomPoint, Quaternion.identity);
    }

}

Vector3 GetRandomPointOnNavMeshForPlayer(int attempts, float range)
{
    for (int i = 0; i < attempts; i++)
    {
        Vector3 randomPoint = new Vector3(
            Random.Range(size / 4f, 3 * size / 4f),  
            0,
            Random.Range(size / 4f, 3 * size / 4f)  
        );

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }
    }

    return Vector3.positiveInfinity;
}
void GenerateCollectibles(Cell[,] grid)
{
    
    float[,] noiseMap = new float[size, size];
    (float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
    for (int y = 0; y < size; y++)
    {
        for (int x = 0; x < size; x++)
        {
            float noiseValue = Mathf.PerlinNoise(x * treeNoiseScale + xOffset, y * treeNoiseScale + yOffset);
            noiseMap[x, y] = noiseValue;
        }
    }

    for (int y = 0; y < size; y++)
    {
        for (int x = 0; x < size; x++)
        {
            Cell cell = grid[x, y];
            if (!cell.isWater)
            {
                float v = Random.Range(0f, treeDensity);
                if (noiseMap[x, y] < v)
                {
                    
                    GameObject prefab = GetRandomCollectiblePrefab();
                   
                    if (prefab != null)
                    {
                        GameObject collectible = Instantiate(prefab, transform);
                        collectible.transform.position = new Vector3(x, 0, y);
                        
                        collectibles.Add(collectible.transform);
                    }
                }
            }
        }
    }
}

GameObject GetRandomCollectiblePrefab()
{
    if (coinPrefab != null || healthPrefab != null || potionPrefab != null || poisonPrefab != null || shieldPrefab != null)
    {
        
        GameObject[] collectiblePrefabs = { coinPrefab, healthPrefab, potionPrefab, poisonPrefab, shieldPrefab };
        
        return collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)];
    }
    else
    {
        Debug.LogError("One or more collectible prefabs are not assigned.");
        return null;
    }
}

void SpawnCollectibles(GameObject prefab, int count)
    {
    for (int i = 0; i < count; i++)
        {
            Vector3 spawnPosition = GetRandomPointOnNavMesh(100, 50f);
            if (spawnPosition != Vector3.positiveInfinity)
            {
                GameObject collectible = Instantiate(prefab, spawnPosition, Quaternion.identity);
                collectible.tag = "Collectible";
                collectibles.Add(collectible.transform);
            }
    }
}
    
    void VisualizePath(Vector3 start, Vector3 end)
{
    NavMeshPath path = new NavMeshPath();

    
    if (NavMesh.CalculatePath(start, end, NavMesh.AllAreas, path))
    {
       
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.yellow, 2f);
        }
    }
    else
    {
        Debug.LogError("Failed to calculate path between " + start + " and " + end);
    }
}


void CheckCollectibleCollisions()
{
   
        for (int i = 0; i < collectibles.Count; i++)
    {
        Transform collectible = collectibles[i];
        if (collectible != null)
        {
            float distanceToPlayer = Vector3.Distance(collectible.position, player.transform.position);


            if (distanceToPlayer < 5f) 
            {
                

                
                VisualizePath(player.transform.position, collectible.position);

                Collect(collectible.gameObject);
                Destroy(collectible.gameObject);
                collectibles.RemoveAt(i);
                i--; 
            }
        }
    }
}
    void Collect(GameObject collectible)
    {
         if (collectible.CompareTag("Coin"))
    {
       
        playerScore += 20;

        UpdateScoreDisplay();
    }
    else if (collectible.CompareTag("Health"))
    {
       
        playerHealth += 20;
        UpdateHealthDisplay();
        Debug.Log("Health collected!" + playerHealth);
    }
    else if (collectible.CompareTag("Poison"))
    {
       
        playerHealth -= 20;
        UpdateHealthDisplay();
    }
    else if (collectible.CompareTag("Magic"))
    {
        
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            if(PlayerController.currentSpeed < 22.5){
                 Debug.Log("now Speed " + PlayerController.currentSpeed);
                playerController.ApplySpeedPowerUp(1.5f, 3f);
                Debug.Log("Powered Up! " + PlayerController.currentSpeed);
            }else{
                Debug.Log("Max Speed Reached!");
            }
           
        }
    }else if (collectible.CompareTag("Trap")){
             PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            Debug.Log("now Speed " + PlayerController.currentSpeed);
            playerController.HitTrap(1.5f, 3f);
            Debug.Log("Slowed Down! " + PlayerController.currentSpeed);
        }

    }
        
        Destroy(collectible);
    }
    Vector3 GetRandomEnemySpawnPoint()
{
    for (int i = 0; i < 100; i++)
    {
       
        Vector3 candidatePosition = new Vector3(Random.Range(0, size), 1.7f, Random.Range(0, size));

       
        NavMeshHit hit;
        if (NavMesh.SamplePosition(candidatePosition, out hit, 10f, NavMesh.AllAreas))
        {
          
            return hit.position;
        }
    }
    

    
    return Vector3.zero;
}


void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Collectible"))
    {
        Collect(other.gameObject);
        Destroy(other.gameObject); 
    }
}

    



void SpawnEnemiesAtRandom()
{
    if (enemyPrefabs == null || enemyPrefabs.Length == 0)
    {
        Debug.LogError("Enemy prefabs array is null or empty.");
        return;
    }

    for (int i = 0; i < numberOfEnemies; i++)
    {
        
        Vector3 randomPosition = GetRandomEnemySpawnPoint();

        if (randomPosition != Vector3.zero)
        {
           
            GameObject selectedEnemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            
            GameObject enemyInstance = Instantiate(selectedEnemyPrefab, randomPosition, Quaternion.identity);

           
            NavMeshAgent enemyNavMeshAgent = enemyInstance.GetComponent<NavMeshAgent>();

            if (enemyNavMeshAgent != null)
            {
                
                Vector3 randomDestination = GetRandomEnemySpawnPoint();
                enemyNavMeshAgent.SetDestination(randomDestination);
            }
            else
            {
                Debug.LogError("NavMeshAgent component not found on the spawned enemy GameObject.");
            }
        }
        else
        {
            Debug.LogError("Failed to find a valid position on the NavMesh for the enemy.");
        }
    }
}



    
    void DrawEdgeMesh(Cell[,] grid)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (!cell.isWater)
                {
                    if (x > 0)
                    {
                        Cell left = grid[x - 1, y];
                        if (left.isWater)
                        {
                            Vector3 a = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 c = new Vector3(x - .5f, -1, y + .5f);
                            Vector3 d = new Vector3(x - .5f, -1, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (x < size - 1)
                    {
                        Cell right = grid[x + 1, y];
                        if (right.isWater)
                        {
                            Vector3 a = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 c = new Vector3(x + .5f, -1, y - .5f);
                            Vector3 d = new Vector3(x + .5f, -1, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y > 0)
                    {
                        Cell down = grid[x, y - 1];
                        if (down.isWater)
                        {
                            Vector3 a = new Vector3(x - .5f, 0, y - .5f);
                            Vector3 b = new Vector3(x + .5f, 0, y - .5f);
                            Vector3 c = new Vector3(x - .5f, -1, y - .5f);
                            Vector3 d = new Vector3(x + .5f, -1, y - .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                    if (y < size - 1)
                    {
                        Cell up = grid[x, y + 1];
                        if (up.isWater)
                        {
                            Vector3 a = new Vector3(x + .5f, 0, y + .5f);
                            Vector3 b = new Vector3(x - .5f, 0, y + .5f);
                            Vector3 c = new Vector3(x + .5f, -1, y + .5f);
                            Vector3 d = new Vector3(x - .5f, -1, y + .5f);
                            Vector3[] v = new Vector3[] { a, b, c, b, d, c };
                            for (int k = 0; k < 6; k++)
                            {
                                vertices.Add(v[k]);
                                triangles.Add(triangles.Count);
                            }
                        }
                    }
                }
            }
        }
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GameObject edgeObj = new GameObject("Edge");
        edgeObj.transform.SetParent(transform);

        MeshFilter meshFilter = edgeObj.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = edgeObj.AddComponent<MeshRenderer>();
        meshRenderer.material = edgeMaterial;
    }

    void DrawTexture(Cell[,] grid)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] colorMap = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (cell.isWater)
                {
                    float gradientValue = (float)y / size; // Calculate gradient value based on y position
                    Color lightBlueColor = new Color(0.5f + gradientValue / 2f, 0.7f + gradientValue / 3f, 1f); // Create light blue color based on gradient value
                    colorMap[y * size + x] = lightBlueColor;
                }
                else
                {
                    float gradientValue = (float)y / size; // Calculate gradient value based on y position
                    Color greenColor = new Color(0f, 0.5f + gradientValue / 2f, 0f); // Create green color based on gradient value
                    colorMap[y * size + x] = greenColor;
                }
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colorMap);
        texture.Apply();

        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = terrainMaterial;
        meshRenderer.material.mainTexture = texture;
    }

    void GenerateTrees(Cell[,] grid)
    {
        float[,] noiseMap = new float[size, size];
        (float xOffset, float yOffset) = (Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float noiseValue = Mathf.PerlinNoise(x * treeNoiseScale + xOffset, y * treeNoiseScale + yOffset);
                noiseMap[x, y] = noiseValue;
            }
        }

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (!cell.isWater)
                {
                    float v = Random.Range(0f, treeDensity);
                    if (noiseMap[x, y] < v)
                    {
                        GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                        GameObject tree = Instantiate(prefab, transform);
                        tree.transform.position = new Vector3(x, 0, y);
                        tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360f), 0);
                        tree.transform.localScale = Vector3.one * Random.Range(.8f, 1.2f);
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Cell cell = grid[x, y];
                if (cell.isWater)
                    Gizmos.color = Color.blue;
                else
                    Gizmos.color = Color.green;
                Vector3 pos = new Vector3(x, 0, y);
                Gizmos.DrawCube(pos, Vector3.one);
            }
        }
    }

    


}