# Procedural Terrain Survival Game

## Overview
This project is a 3D survival game developed in Unity featuring a procedurally generated island terrain. The terrain is created using Perlin noise combined with falloff maps to simulate realistic land and water biomes. The environment dynamically populates with trees, collectibles, and enemies.

Enemies utilize AI powered by finite state machines (FSM) with distinct states such as Patrol, Chase, and Attack. They navigate the terrain using Unity’s NavMesh system, which employs pathfinding algorithms like A* and Dijkstra's internally. Enemies perform ranged attacks using projectile physics and collision detection.

The player can explore the world, collect items, and manage health and score through an intuitive UI.

## Features
- Procedural terrain generation using Perlin noise and falloff maps  
- Dynamic environment with trees and collectibles  
- Enemy AI with FSM for intelligent behaviors  
- Navigation with Unity NavMesh for pathfinding  
- Ranged enemy attacks with projectile bullets  
- Player health and score UI display  

## Technologies Used
- Unity3D  
- C#  
- Perlin Noise & Falloff Maps  
- NavMesh (A* / Dijkstra’s pathfinding algorithms)  
- Finite State Machines (FSM) for AI  
- Unity Physics & Collision Detection  
- UI Canvas for health and score  

## How to Run
1. Clone this repository  
2. Open the project in Unity (version 2020.3 or later recommended)  
3. Open the main scene  
4. Press Play to start the game  

## Controls
- WASD or Arrow keys to move the player  
- Collectibles increase score or health  
- Avoid enemy attacks  

## License
This project is licensed under the MIT License - see the LICENSE file for details.

---

Feel free to customize or ask me for help with a detailed CONTRIBUTING or INSTALLATION guide!

