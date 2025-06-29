using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D.Animation;
using UnityEngine;
using static BSP;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class LevelGenerationController : MonoBehaviour
{
    public BSP layoutGenerator;
    public RoomGenerator roomGenerator;

    private Dictionary<int, int[]> level;

    public char[,] layout;

    public GameObject playerPrefab;
    public GameObject depravedPrefab;
    public GameObject magePrefab;

    void Start()
    {
        InitializeLevelValues();
        roomGenerator.InitializeTileDictionaries();
        //Debug();
        
        GenerateLevel(5);
    }

    void InitializeLevelValues()
    {
        level = new Dictionary<int, int[]>();

        level[1] = new int[] {16, 20, 3, 40};
        level[2] = new int[] {24, 20, 5, 45};
        level[3] = new int[] {24, 20, 6, 55};
        level[4] = new int[] {32, 20, 10, 65};
        level[5] = new int[] {48, 20, 12, 80};
        level[6] = new int[] {128, 20, 30, 100};
    }

    void GenerateLevel(int selectedLevel)
    {
        int attempts = 0;

        while (attempts < 500)
        {
            try
            {
                roomGenerator.floorTilemap.ClearAllTiles();
                roomGenerator.wallTilemap.ClearAllTiles();
                roomGenerator.backgroundTilemap.ClearAllTiles();

                GenerateLayout(selectedLevel);
                PlaceBackground();
                PlaceCorridors();
                PlaceRooms(level[selectedLevel][3]);
                SpawnPlayer();
            }
            catch (Exception ex)
            {
                print(ex);
                attempts++;
                continue;
            }

            break;
        }
    }

    void PlaceBackground()
    {
        int size = layoutGenerator.mapHeight + 500;
        char[,] matrix = new char[size, size];

        for (int i = 0; i < size; i++)
            for (int j = 0; j < size; j++)
                matrix[i, j] = 'W';

        roomGenerator.PlaceMatrixOnTilemap(matrix, roomGenerator.backgroundTilemap, new Vector2Int(-100, -100));
    }

    void SpawnPlayer()
    {
        Leaf leaf = null;

        foreach (Leaf l in layoutGenerator.leaves)
        {
            if (l.leftChild == null && l.rightChild == null)
            {
                leaf = l;
                break;
            }
        }

        Vector3 roomCenter = new Vector3(
            (leaf.room.x + leaf.room.width / 2f) * 4,
            (leaf.room.y + leaf.room.height / 2f) * 4,
            0);
        Vector3 spawnPosition = roomGenerator.GetUnityPosition(roomCenter.x, roomCenter.y);
        
        Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
    }

    void GenerateLayout(int selectedLevel)
    {
        layoutGenerator.GenerateDungeon(level[selectedLevel][0], level[selectedLevel][0],
                level[selectedLevel][1], level[selectedLevel][2]);

        int leavesCount = 0;

        foreach (Leaf l in layoutGenerator.leaves)
        {
            if (l.leftChild == null && l.rightChild == null)
            {
                leavesCount++;
                foreach (Vector2Int door in l.doorPositions)
                {
                    if (IsCorner(l.room, door) || IsOutside(l.room, door))
                    {
                        throw new Exception("Corner door in room");
                    }
                }
            }
        }

        if (selectedLevel > 1 && leavesCount < 5)
            throw new Exception("Too few rooms.");

        layout = layoutGenerator.matrix;
        roomGenerator.mapHeight = layout.GetLength(0) * 4;
        layoutGenerator.Debug();
    }

    void PlaceRooms(int targetDifficulty)
    {
        bool firstRoom = true;

        foreach (Leaf l in layoutGenerator.leaves)
        {
            if (l.leftChild == null && l.rightChild == null)
            {
                char[,] layoutMatrix = roomGenerator.GenerateRoomLayout(l, firstRoom);

                Vector2Int roomPosition = new Vector2Int((int)l.room.x * 4, (int)l.room.y * 4);

                char[,] floorMatrix = roomGenerator.GenerateSpritesFloor(layoutMatrix);
                floorMatrix = roomGenerator.DecorateFloor(floorMatrix);
                roomGenerator.PlaceMatrixOnTilemap(floorMatrix, roomGenerator.floorTilemap, roomPosition);

                char[,] wallsMatrix = roomGenerator.GenerateSpritesWalls(layoutMatrix);
                roomGenerator.PlaceMatrixOnTilemap(wallsMatrix, roomGenerator.wallTilemap, roomPosition);

                if (!firstRoom)
                    SpawnEnemies(l, layoutMatrix, targetDifficulty);
                firstRoom = false;
            }
        }
    }

    void SpawnEnemies(Leaf l, char[,] layout, int targetDifficulty)
    {
        float sizeRating = GetSizeRating(layout.GetLength(0) * layout.GetLength(1), 144, 960);
        float baseMultiplier = ApplyCoverReduction(sizeRating, layout);

        float currentDifficulty = 0;
        List<Vector2Int> floorTiles = GetAllFloorTiles(layout);
        while ((float)targetDifficulty - currentDifficulty >= 10)
        {
            currentDifficulty = SpawnEnemy(currentDifficulty, baseMultiplier, floorTiles, l.room.x * 4, l.room.y * 4);
        }
    }

    List<Vector2Int> GetAllFloorTiles(char[,] layout)
    {
        List<Vector2Int> floorTiles = new List<Vector2Int>();

        int rows = layout.GetLength(0);
        int cols = layout.GetLength(1);

        for (int i = 1; i < rows - 1; i++)
        {
            for (int j = 1; j < cols - 1; j++)
            {
                if (layout[i, j] == '#')
                {
                    floorTiles.Add(new Vector2Int(i, j));
                }
            }
        }

        return floorTiles;
    }

    float SpawnEnemy(float currentDifficulty, float multiplier, List<Vector2Int> floorTiles, float roomX, float roomY)
    {
        Vector2Int randomTile = floorTiles[UnityEngine.Random.Range(0, floorTiles.Count)];

        Vector3 spawnPos = roomGenerator.GetUnityPosition(roomX + randomTile.x, roomY + randomTile.y);

        if (UnityEngine.Random.Range(0f, 1f) < 0.5f)
        {
            Instantiate(depravedPrefab, spawnPos, Quaternion.identity);
            return currentDifficulty + 10f * multiplier;
        }
        else
        {
            Instantiate(magePrefab, spawnPos, Quaternion.identity);
            return currentDifficulty + 20f * multiplier;
        }
    }

    float GetSizeRating(float roomSize, float minSize, float maxSize)
    {
        float normalizedSize = (roomSize - minSize) / (maxSize - minSize);

        float difficulty = 100f - 50f * Mathf.Sqrt(normalizedSize);

        return difficulty;
    }

    float ApplyCoverReduction(float baseDifficulty, char[,] layout)
    {
        float coverPercentage = CalculateCoverPercentage(layout);
        float coverImpact = coverPercentage * coverPercentage;

        float maxReduction = 40f;
        float reduction = coverImpact * maxReduction;

        return Mathf.Max(baseDifficulty - reduction, 30f) / 100;
    }

    float CalculateCoverPercentage(char[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        int totalCells = 0;
        int coverCells = 0;

        for (int i = 1; i < rows - 1; i++)
        {
            for (int j = 1; j < cols - 1; j++)
            {
                totalCells++;
                if (matrix[i, j] == 'W')
                {
                    coverCells++;
                }
            }
        }

        return totalCells > 0 ? (float)coverCells / totalCells : 0f;
    }

    void PlaceCorridors()
    {
        for (int i = 0; i < layout.GetLength(0); i++)
        {
            for (int j = 0; j < layout.GetLength(1); j++)
            {
                if (layout[i, j] == '+')
                {
                    Vector2Int roadPosition = new Vector2Int(i * 4, j * 4);

                    bool hasRoadLeft = HasNeighbor(i, j, 0, -1);
                    bool hasRoadRight = HasNeighbor(i, j, 0, 1);
                    bool hasRoadTop = HasNeighbor(i, j, -1, 0);
                    bool hasRoadBottom = HasNeighbor(i, j, 1, 0);

                    int index = (hasRoadLeft ? 1 : 0) |
                        (hasRoadRight ? 2 : 0) |
                        (hasRoadTop ? 4 : 0) |
                        (hasRoadBottom ? 8 : 0);

                    char[,] floorMatrix = roomGenerator.roadFloorDict[index];
                    floorMatrix = roomGenerator.DecorateFloor(floorMatrix);
                    char[,] wallsMatrix = roomGenerator.roadWallsDict[index];

                    roomGenerator.PlaceMatrixOnTilemap(floorMatrix, roomGenerator.floorTilemap, roadPosition);
                    roomGenerator.PlaceMatrixOnTilemap(wallsMatrix, roomGenerator.wallTilemap, roadPosition);
                }
            }
        }
    }

    private bool HasNeighbor(int x, int y, int dx, int dy)
    {
        int newX = x + dx;
        int newY = y + dy;

        if (newX < 0 || newX >= layout.GetLength(0) ||
            newY < 0 || newY >= layout.GetLength(1))
        {
            return false;
        }

        return layout[newX, newY] == '+' || layout[newX, newY] == 'O';
    }

    void Debug()
    {
        int attempts = 0;

        while (attempts < 50)
        {
            layoutGenerator.GenerateDungeon(16, 16, 2, 3);
            bool retry = false;

            foreach (Leaf l in layoutGenerator.leaves)
            {
                if (l.leftChild == null && l.rightChild == null)
                {
                    foreach (Vector2Int door in l.doorPositions)
                    {
                        if (IsCorner(l.room, door))
                        {
                            retry = true;
                            break;
                        }
                    }
                }

                if (retry) break;
            }

            if (retry)
            {
                attempts++;
                continue;
            }

            break;
        }

        layoutGenerator.Debug();

        Leaf leaf = null;

        foreach (Leaf l in layoutGenerator.leaves)
        {
            if (l.leftChild == null && l.rightChild == null)
            {
                leaf = l;
                break;
            }
        }

        char[,] matrix = roomGenerator.GenerateRoomLayout(leaf, false);
        char[,] floor = roomGenerator.GenerateSpritesFloor(matrix);
        char[,] walls = roomGenerator.GenerateSpritesWalls(matrix);
        roomGenerator.InitializeTileDictionaries();
        roomGenerator.PlaceMatrixOnTilemap(floor, roomGenerator.floorTilemap, Vector2Int.zero);
        roomGenerator.PlaceMatrixOnTilemap(walls, roomGenerator.wallTilemap, Vector2Int.zero);

        matrix = walls;

        string path = "Assets/Scripts/debugRoom.txt";

        print(leaf.room.x + " " + leaf.room.y);

        using (StreamWriter writer = new StreamWriter(path))
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    writer.Write(' ');
                    writer.Write(matrix[i, j]);
                    writer.Write(' ');
                }
                writer.WriteLine();
            }
        }
    }

    bool IsCorner(Rect room, Vector2Int pos)
    {
        int xMin = (int)room.xMin;
        int xMax = (int)room.xMax - 1;
        int yMin = (int)room.yMin;
        int yMax = (int)room.yMax - 1;

        return (pos.x == xMin || pos.x == xMax) &&
               (pos.y == yMin || pos.y == yMax);
    }

    bool IsOutside(Rect room, Vector2Int pos)
    {
        int xMin = (int)room.xMin;
        int xMax = (int)room.xMax - 1;
        int yMin = (int)room.yMin;
        int yMax = (int)room.yMax - 1;

        return pos.x > xMax || pos.x < xMin ||
            pos.y > yMax || pos.y < yMin;
    }

    void Update()
    {
        
    }
}
