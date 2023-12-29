// using System.Collections.Generic;
// using UnityEngine;

// public static class AStar
// {
//     public static List<Node> FindPath(Node[,] nodeGrid, Node startNode, Node targetNode)
//     {
//         List<Node> openSet = new List<Node>();
//         HashSet<Node> closedSet = new HashSet<Node>();

//         openSet.Add(startNode);

//         while (openSet.Count > 0)
//         {
//             Node currentNode = openSet[0];
//             for (int i = 1; i < openSet.Count; i++)
//             {
//                 if (openSet[i].FCost < currentNode.FCost || (openSet[i].FCost == currentNode.FCost && openSet[i].HCost < currentNode.HCost))
//                 {
//                     currentNode = openSet[i];
//                 }
//             }

//             openSet.Remove(currentNode);
//             closedSet.Add(currentNode);

//             if (currentNode == targetNode)
//             {
//                 return RetracePath(startNode, targetNode);
//             }

//             foreach (Node neighbor in GetNeighbors(nodeGrid, currentNode))
//             {
//                 if (!neighbor.IsWalkable || closedSet.Contains(neighbor))
//                 {
//                     continue;
//                 }

//                 int newMovementCostToNeighbor = currentNode.GCost + GetDistance(currentNode, neighbor);

//                 if (newMovementCostToNeighbor < neighbor.GCost || !openSet.Contains(neighbor))
//                 {
//                     neighbor.GCost = newMovementCostToNeighbor;
//                     neighbor.HCost = GetDistance(neighbor, targetNode);
//                     neighbor.Parent = currentNode;

//                     if (!openSet.Contains(neighbor))
//                     {
//                         openSet.Add(neighbor);
//                     }
//                 }
//             }
//         }

//         return null;
//     }

//     private static List<Node> RetracePath(Node startNode, Node endNode)
//     {
//         List<Node> path = new List<Node>();
//         Node currentNode = endNode;

//         while (currentNode != startNode)
//         {
//             path.Add(currentNode);
//             currentNode = currentNode.Parent;
//         }

//         path.Reverse();
//         return path;
//     }

//     private static List<Node> GetNeighbors(Node[,] nodeGrid, Node node)
//     {
//         List<Node> neighbors = new List<Node>();

//         for (int x = -1; x <= 1; x++)
//         {
//             for (int y = -1; y <= 1; y++)
//             {
//                 if (x == 0 && y == 0)
//                     continue;

//                 int checkX = node.GridX + x;
//                 int checkY = node.GridY + y;

//                 if (checkX >= 0 && checkX < nodeGrid.GetLength(0) && checkY >= 0 && checkY < nodeGrid.GetLength(1))
//                 {
//                     neighbors.Add(nodeGrid[checkX, checkY]);
//                 }
//             }
//         }

//         return neighbors;
//     }

//     private static int GetDistance(Node nodeA, Node nodeB)
//     {
//         int distX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
//         int distY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

//         return distX > distY ? 14 * distY + 10 * (distX - distY) : 14 * distX + 10 * (distY - distX);
//     }
// }
