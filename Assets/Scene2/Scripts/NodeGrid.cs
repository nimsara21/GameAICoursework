using System.Collections.Generic;
using UnityEngine;

public class NodeGrid : MonoBehaviour
{
    public Grid mainGrid; // Reference to the main grid script
    public Material walkableMaterial;
    public Material nonWalkableMaterial;
    public Material pathMaterial;

    private Node[,] nodeGrid;

    void Start()
    {
        CreateNodeGrid();
    }

    void CreateNodeGrid()
    {
        nodeGrid = new Node[mainGrid.size, mainGrid.size];

        for (int y = 0; y < mainGrid.size; y++)
        {
            for (int x = 0; x < mainGrid.size; x++)
            {
                Cell cell = mainGrid.GridArray[x, y];
                Vector3 position = new Vector3(x, 0, y) * mainGrid.scale;

                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.transform.position = position;

                Node node = new Node(x, y, position, tile, cell.isWater);
                nodeGrid[x, y] = node;

                // Set material based on walkability
                if (node.walkable)
                {
                    tile.GetComponent<Renderer>().material = walkableMaterial;
                }
                else
                {
                    tile.GetComponent<Renderer>().material = nonWalkableMaterial;
                }
            }
        }
    }

    public void FindPath(Vector3 startWorldPos, Vector3 endWorldPos)
    {
        Vector2Int startNode = WorldToNodeGridPosition(startWorldPos);
        Vector2Int endNode = WorldToNodeGridPosition(endWorldPos);

        BFS(startNode, endNode);
    }

    void BFS(Vector2Int startNode, Vector2Int endNode)
    {
        Queue<Node> queue = new Queue<Node>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parentMap = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(nodeGrid[startNode.x, startNode.y]);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            Node currentNode = queue.Dequeue();

            if (currentNode.gridPosition == endNode)
            {
                DrawPath(parentMap, startNode, endNode);
                return;
            }

            foreach (Vector2Int neighbor in GetNeighbors(currentNode.gridPosition))
            {
                if (!visited.Contains(neighbor) && nodeGrid[neighbor.x, neighbor.y].walkable)
                {
                    queue.Enqueue(nodeGrid[neighbor.x, neighbor.y]);
                    visited.Add(neighbor);
                    parentMap[neighbor] = currentNode.gridPosition;
                }
            }
        }

        Debug.Log("No path found.");
    }

    void DrawPath(Dictionary<Vector2Int, Vector2Int> parentMap, Vector2Int startNode, Vector2Int endNode)
    {
        Vector2Int currentNode = endNode;

        while (currentNode != startNode)
        {
            nodeGrid[currentNode.x, currentNode.y].tile.GetComponent<Renderer>().material = pathMaterial;
            currentNode = parentMap[currentNode];
        }

        nodeGrid[startNode.x, startNode.y].tile.GetComponent<Renderer>().material = pathMaterial;
    }

    Vector2Int WorldToNodeGridPosition(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / mainGrid.scale);
        int y = Mathf.FloorToInt(worldPos.z / mainGrid.scale);
        return new Vector2Int(x, y);
    }

    Vector2Int[] GetNeighbors(Vector2Int gridPos)
    {
        return new Vector2Int[]
        {
            new Vector2Int(gridPos.x - 1, gridPos.y),
            new Vector2Int(gridPos.x + 1, gridPos.y),
            new Vector2Int(gridPos.x, gridPos.y - 1),
            new Vector2Int(gridPos.x, gridPos.y + 1)
        };
    }

    public class Node
    {
        public int x;
        public int y;
        public Vector2Int gridPosition;
        public Vector3 worldPosition;
        public GameObject tile;
        public bool walkable;

        public Node(int x, int y, Vector3 worldPosition, GameObject tile, bool walkable)
        {
            this.x = x;
            this.y = y;
            this.gridPosition = new Vector2Int(x, y);
            this.worldPosition = worldPosition;
            this.tile = tile;
            this.walkable = walkable;
        }
    }
}
