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

    void Start()
    {
        InitializeLevelValues();
        roomGenerator.InitializeTileDictionaries();
        //Debug();
        
        GenerateLevel(1);
    }

    void InitializeLevelValues()
    {
        level = new Dictionary<int, int[]>();

        level[1] = new int[] {16, 2, 3};
    }

    void GenerateLevel(int selectedLevel)
    {
        GenerateLayout(selectedLevel);
        PlaceBackground();
        PlaceCorridors();
        PlaceRooms();
        SpawnPlayer();
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
        int attempts = 0;

        while (attempts < 50)
        {
            layoutGenerator.GenerateDungeon(level[selectedLevel][0], level[selectedLevel][0],
                level[selectedLevel][1], level[selectedLevel][2]);
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

        layout = layoutGenerator.matrix;
        roomGenerator.mapHeight = layout.GetLength(0) * 4;
        layoutGenerator.Debug();
    }

    void PlaceRooms()
    {
        bool firstRoom = true;

        foreach (Leaf l in layoutGenerator.leaves)
        {
            if (l.leftChild == null && l.rightChild == null)
            {
                char[,] layoutMatrix = roomGenerator.GenerateRoomLayout(l, firstRoom);
                firstRoom = false;

                Vector2Int roomPosition = new Vector2Int((int)l.room.x * 4, (int)l.room.y * 4);

                char[,] floorMatrix = roomGenerator.GenerateSpritesFloor(layoutMatrix);
                floorMatrix = roomGenerator.DecorateFloor(floorMatrix);
                roomGenerator.PlaceMatrixOnTilemap(floorMatrix, roomGenerator.floorTilemap, roomPosition);

                char[,] wallsMatrix = roomGenerator.GenerateSpritesWalls(layoutMatrix);
                roomGenerator.PlaceMatrixOnTilemap(wallsMatrix, roomGenerator.wallTilemap, roomPosition);
            }
        }
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

    void Update()
    {
        
    }
}
