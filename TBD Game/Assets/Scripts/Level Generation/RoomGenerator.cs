using System.Collections;
using System.Collections.Generic;
using System.IO;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    Sprite[] floorTiles;
    Sprite[] tallWalls;
    Sprite[] shortWalls;

    Dictionary<char, Sprite> tileDict = new Dictionary<char, Sprite>();

    void Start()
    {
        floorTiles = Resources.LoadAll<Sprite>("Tiles/atlas_floor-16x16");
        tallWalls = Resources.LoadAll<Sprite>("Tiles/atlas_walls_high-16x32");
        floorTiles = Resources.LoadAll<Sprite>("Tiles/atlas_walls_low-16x16");
    }

    void InitializeTileDictionary()
    {
        tileDict['W'] = shortWalls[32]; // void
        tileDict['#'] = floorTiles[0]; // normal floor
        tileDict['['] = floorTiles[38]; // floor under wall left
        tileDict[']'] = floorTiles[39]; // floor under wall right
        tileDict['|'] = shortWalls[12]; // normal vertical wall
        tileDict['='] = shortWalls[37]; // normal horizontal wall
    }

    void Update()
    {
        
    }

    public char[,] GenerateSpritesFloor(char[,] layout)
    {
        char[,] matrix = layout;

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (j > 0)
                    if (matrix[i, j] == 'W' && matrix[i, j - 1] == '#')
                        matrix[i, j] = ']';
                if (j < matrix.GetLength(1) - 1)
                    if (matrix[i, j] == 'W' && matrix[i, j + 1] == '#')
                        matrix[i, j] = '[';
            }
        }

        return matrix;
    }

    public char[,] GenerateRoomLayout(BSP.Leaf leaf)
    {
        int width = (int)leaf.room.width * 4;
        int height = (int)leaf.room.height * 4;
        char[,] matrix = new char[width, height];

        // Fill with open space
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                matrix[i, j] = '#';
            }
        }

        // Add border walls
        for (int i = 0; i < width; i++)
        {
            matrix[i, 0] = 'W';
            matrix[i, height - 1] = 'W';
        }
        for (int j = 0; j < height; j++)
        {
            matrix[0, j] = 'W';
            matrix[width - 1, j] = 'W';
        }

        // Add inner wall rectangles
        int rectCount = Random.Range(0, 4); // 0 to 3 rectangles
        int attempt = 0;
        int rectangles = 0;

        print(rectCount);

        while ( attempt < 20 && rectangles < rectCount)
        {
            int rectWidth = Random.Range(3, 7);
            int rectHeight = Random.Range(3, 7);

            int maxX = width - rectWidth - 2;
            int maxY = height - rectHeight - 2;

            if (maxX <= 2 || maxY <= 2)
                break; // not enough room

            int startX = Random.Range(2, maxX);
            int startY = Random.Range(2, maxY);

            if (IsAreaEmptyWithBuffer(matrix, startX, startY, rectWidth, rectHeight))
            {
                rectangles++;
                for (int i = startX; i < startX + rectWidth; i++)
                {
                    for (int j = startY; j < startY + rectHeight; j++)
                    {
                        matrix[i, j] = 'W';
                    }
                }
            }

            attempt++;
        }

        // Create openings in the wall for the doors
        foreach (Vector2Int door in leaf.doorPositions)
        {
            int localX = (door.x - (int)leaf.room.xMin) * 4;
            int localY = (door.y - (int)leaf.room.yMin) * 4;

            bool onTop = door.x == (int)leaf.room.xMin;
            bool onBottom = door.x == (int)leaf.room.xMax - 1;
            bool onLeft = door.y == (int)leaf.room.yMin;
            bool onRight = door.y == (int)leaf.room.yMax - 1;

            print(localX + " " + localY + " " + onLeft +
                " " + onRight + " " + onTop + " " + onBottom);

            if(!IsInside(matrix, localX, localY))
            {
                print("Door is out of bounds!");
                continue;
            }

            if (onLeft)
            {
                matrix[localX + 1, localY] = '#';
                matrix[localX + 2, localY] = '#';
            }
            else if (onRight)
            {
                matrix[localX + 1, localY + 3] = '#';
                matrix[localX + 2, localY + 3] = '#';
            }
            else if (onTop)
            {
                matrix[localX, localY + 1] = '#';
                matrix[localX, localY + 2] = '#';
            }
            else if (onBottom)
            {
                matrix[localX + 3, localY + 1] = '#';
                matrix[localX + 3, localY + 2] = '#';
            }
        }

        return matrix;
    }

    bool IsAreaEmptyWithBuffer(char[,] matrix, int x, int y, int w, int h)
    {
        int startX = Mathf.Max(0, x - 2);
        int endX = Mathf.Min(matrix.GetLength(0), x + w + 2);
        int startY = Mathf.Max(0, y - 2);
        int endY = Mathf.Min(matrix.GetLength(1), y + h + 2);

        for (int i = startX; i < endX; i++)
        {
            for (int j = startY; j < endY; j++)
            {
                if (matrix[i, j] != '#') // occupied
                    return false;
            }
        }

        return true;
    }

    bool IsInside(char[,] matrix, int x, int y)
    {
        return x >= 0 && y >= 0 && x < matrix.GetLength(0) && y < matrix.GetLength(1);
    }
}
