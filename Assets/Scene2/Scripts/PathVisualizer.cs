using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class PathVisualization : MonoBehaviour
{
    public Grid grid; // Reference to the Grid script on your terrain
    public LineRenderer lineRenderer;

    void Start()
    {
        if (grid == null)
        {
            Debug.LogError("Grid reference is not set in the inspector.");
            return;
        }

        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer reference is not set in the inspector.");
            return;
        }

        // Call a function to visualize the shortest path from start to end
        VisualizeShortestPath();
    }

    void VisualizeShortestPath()
    {
        // Get start and end points for visualization
        Vector3 startPoint = new Vector3(0, 0, 0); // Set the start point
        Vector3 endPoint = new Vector3(grid.size - 1, 0, grid.size - 1); // Set the end point

        // Get the closest walkable positions to the start and end points using NavMesh
        Vector3 closestStartPoint = GetClosestWalkablePosition(startPoint);
        Vector3 closestEndPoint = GetClosestWalkablePosition(endPoint);

        // Get the shortest path using BFS
        List<Vector3> path = BFS(closestStartPoint, closestEndPoint);

        // Visualize the path using LineRenderer
        VisualizePath(path);
    }

    Vector3 GetClosestWalkablePosition(Vector3 position)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(position, out hit, 100f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Vector3.zero;
    }

    List<Vector3> BFS(Vector3 start, Vector3 end)
    {
        Queue<Vector3> queue = new Queue<Vector3>();
        Dictionary<Vector3, Vector3> cameFrom = new Dictionary<Vector3, Vector3>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        while (queue.Count > 0)
        {
            Vector3 current = queue.Dequeue();

            if (current == end)
            {
                break;
            }

            foreach (Vector3 neighbor in GetNeighbors(current))
            {
                if (!cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }

        // Reconstruct the path
        List<Vector3> path = new List<Vector3>();
        Vector3 currentPoint = end;
        while (currentPoint != start)
        {
            path.Add(currentPoint);
            currentPoint = cameFrom[currentPoint];
        }
        path.Add(start);
        path.Reverse();

        return path;
    }

    List<Vector3> GetNeighbors(Vector3 position)
    {
        List<Vector3> neighbors = new List<Vector3>();

        // Add all possible neighbors (up, down, left, right)
        neighbors.Add(new Vector3(position.x + 1, position.y, position.z));
        neighbors.Add(new Vector3(position.x - 1, position.y, position.z));
        neighbors.Add(new Vector3(position.x, position.y, position.z + 1));
        neighbors.Add(new Vector3(position.x, position.y, position.z - 1));

        // Remove neighbors that are outside the grid bounds or inside water cells
        neighbors.RemoveAll(neighbor => neighbor.x < 0 || neighbor.x >= grid.size || neighbor.z < 0 || neighbor.z >= grid.size || grid.GridArray[Mathf.FloorToInt(neighbor.x), Mathf.FloorToInt(neighbor.z)].isWater);

        return neighbors;
    }

    void VisualizePath(List<Vector3> path)
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer reference is not set.");
            return;
        }

        lineRenderer.positionCount = path.Count;
        lineRenderer.SetPositions(path.ToArray());
    }
}
