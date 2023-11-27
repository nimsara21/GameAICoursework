using UnityEngine;

public class TerrainGeneratorScript : MonoBehaviour
{
    public int gridSize = 10;
    public float cellSize = 1.0f;
    public float heightMultiplier = 5.0f;
    public float perlinScale = 0.1f;
    public int mountainRange = 3;
    public float treeDensity = 0.1f;
    public float riverWidth = 2.0f;

    private Vector3[] vertices;

    void Start()
    {
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];

        int[] triangles = new int[gridSize * gridSize * 6];

        // Generate vertices
        for (int z = 0, i = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                float perlinValue = Mathf.PerlinNoise(x * perlinScale, z * perlinScale);
                float height = perlinValue * heightMultiplier;

                // Add mountains
                for (int m = 0; m < mountainRange; m++)
                {
                    float mountainHeight = Mathf.PerlinNoise(x * (perlinScale * 2) + m, z * (perlinScale * 2) + m) * heightMultiplier * 2;
                    height += mountainHeight;
                }

                vertices[i] = new Vector3(x * cellSize, height, z * cellSize);
                i++;
            }
        }

        // Generate triangles
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < gridSize; z++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + gridSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + gridSize + 1;
                triangles[tris + 5] = vert + gridSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        // Assign colors
        Color[] colors = new Color[vertices.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = CalculateTerrainColor(vertices[i].y);
        }
        mesh.colors = colors;

        // Create a new material with the Standard Shader
        Material material = new Material(Shader.Find("Standard"));
        GetComponent<Renderer>().material = material;

        // Add trees and rivers
        AddTrees();
        AddRivers();
    }

    void AddTrees()
    {
        for (int z = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                if (Random.value < treeDensity)
                {
                    float treeHeight = vertices[z * (gridSize + 1) + x].y;
                    Vector3 treePosition = new Vector3(x * cellSize, treeHeight, z * cellSize);
                    // Instantiate your tree prefab at treePosition
                    // For example: Instantiate(treePrefab, treePosition, Quaternion.identity);
                }
            }
        }
    }

    void AddRivers()
    {
        int riverStartX = Random.Range(0, gridSize);
        int riverStartZ = Random.Range(0, gridSize);

        for (int z = 0; z <= gridSize; z++)
        {
            for (int x = 0; x <= gridSize; x++)
            {
                float distanceToRiver = Vector2.Distance(new Vector2(x, z), new Vector2(riverStartX, riverStartZ));

                if (distanceToRiver < riverWidth)
                {
                    // Lower the height of the terrain to create a river
                    vertices[z * (gridSize + 1) + x].y -= heightMultiplier * 0.5f;
                }
            }
        }

        GetComponent<MeshFilter>().mesh.vertices = vertices;
    }

    Color CalculateTerrainColor(float height)
    {
        // Adjust these values based on your desired colors and heights
        if (height < heightMultiplier * 0.4f)
        {
            // Brown for mountains
            return new Color(0.5f, 0.3f, 0.2f);
        }
        else if (height < heightMultiplier * 0.6f)
        {
            // Green for trees
            return new Color(0.2f, 0.7f, 0.2f);
        }
        else
        {
            // Blue for rivers
            return new Color(0.2f, 0.2f, 0.8f);
        }
    }
}
