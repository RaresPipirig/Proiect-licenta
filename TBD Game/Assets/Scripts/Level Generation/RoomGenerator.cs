using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour
{
    Sprite[] floorTiles;
    Sprite[] tallWalls;
    Sprite[] shortWalls;

    internal Dictionary<char, Sprite> tileDict;
    internal Dictionary<int, char[,]> roadWallsDict;
    internal Dictionary<int, char[,]> roadFloorDict;

    [Header("Tilemap References")]
    public Tilemap backgroundTilemap;
    public Tilemap wallTilemap;
    public Tilemap floorTilemap;

    public Material pixelSnapMaterial;

    HashSet<char> wallChars = new HashSet<char> { 'W', '|', '=', '(', ')', '{', '}' };

    internal int mapHeight;

    void Start()
    {
        //InitializeTileDictionaries();
    }

    internal void InitializeTileDictionaries()
    {
        backgroundTilemap.GetComponent<TilemapRenderer>().material = pixelSnapMaterial;
        wallTilemap.GetComponent<TilemapRenderer>().material = pixelSnapMaterial;
        floorTilemap.GetComponent<TilemapRenderer>().material = pixelSnapMaterial;

        floorTiles = Resources.LoadAll<Sprite>("Tiles/atlas_floor-16x16");
        tallWalls = Resources.LoadAll<Sprite>("Tiles/atlas_walls_high-16x32");
        shortWalls = Resources.LoadAll<Sprite>("Tiles/atlas_walls_low-16x16");
        tileDict = new Dictionary<char, Sprite>();

        tileDict['W'] = shortWalls[32]; // void
        tileDict['#'] = floorTiles[0]; // normal floor
        tileDict['['] = floorTiles[38]; // floor under wall left
        tileDict[']'] = floorTiles[39]; // floor under wall right
        tileDict['|'] = shortWalls[12]; // normal vertical wall
        tileDict['='] = shortWalls[37]; // normal horizontal wall
        tileDict['('] = shortWalls[1]; // top left corner wall
        tileDict[')'] = shortWalls[3]; // top right corner wall
        tileDict['{'] = shortWalls[24]; // bottom left corner wall
        tileDict['}'] = shortWalls[26]; // bottom right corner wall
        tileDict['1'] = floorTiles[1]; // decorative floor 1
        tileDict['2'] = floorTiles[2]; // decorative floor 2
        tileDict['3'] = floorTiles[7]; // decorative floor 3
        tileDict['4'] = floorTiles[8]; // decorative floor 4
        tileDict['5'] = floorTiles[9]; // decorative floor 5
        tileDict['6'] = floorTiles[14]; // decorative floor 6
        tileDict['7'] = floorTiles[15]; // decorative floor 7

        InitializeRoadDictionary();
    }

    private void InitializeRoadDictionary()
    {
        //. . .
        //. . .
        //. . .
        char[,] matrix = new char[,]
        {
            {'#', '#', '#', '#'},
            {'#', '#', '#', '#'},
            {'#', '#', '#', '#'},
            {'#', '#', '#', '#'}
        };

        //. . .
        //+ + +
        //. . .
        char[,] matrix1 = new char[,]
        {
            {'=', '=', '=', '='},
            {'#', '#', '#', '#'},
            {'#', '#', '#', '#'},
            {'=', '=', '=', '='}
        };

        //. + .
        //. + .
        //. + .
        char[,] matrix2 = new char[,]
        {
            {'|', '#', '#', '|'},
            {'|', '#', '#', '|'},
            {'|', '#', '#', '|'},
            {'|', '#', '#', '|'}
        };

        //. + .
        //. + +
        //. . .
        char[,] matrix3 = new char[,]
        {
            {'|', '#', '#', '{'},
            {'|', '#', '#', '#'},
            {'|', '#', '#', '#'},
            {'{', '=', '=', '='}
        };

        //. + .
        //+ + .
        //. . .
        char[,] matrix4 = new char[,]
        {
            {'}', '#', '#', '|'},
            {'#', '#', '#', '|'},
            {'#', '#', '#', '|'},
            {'=', '=', '=', '}'}
        };

        //. . .
        //. + +
        //. + .
        char[,] matrix5 = new char[,]
        {
            {'(', '=', '=', '='},
            {'|', '#', '#', '#'},
            {'|', '#', '#', '#'},
            {'|', '#', '#', '('}
        };

        //. . .
        //+ + .
        //. + .
        char[,] matrix6 = new char[,]
        {
            {'=', '=', '=', ')'},
            {'#', '#', '#', '|'},
            {'#', '#', '#', '|'},
            {')', '#', '#', '|'}
        };

        //. + .
        //+ + +
        //. . .
        char[,] matrix7 = new char[,]
        {
            {'}', '#', '#', '{'},
            {'#', '#', '#', '#'},
            {'#', '#', '#', '#'},
            {'=', '=', '=', '='}
        };

        //. + .
        //. + +
        //. + .
        char[,] matrix8 = new char[,]
        {
            {'|', '#', '#', '{'},
            {'|', '#', '#', '#'},
            {'|', '#', '#', '#'},
            {'|', '#', '#', '('}
        };

        //. . .
        //+ + +
        //. + .
        char[,] matrix9 = new char[,]
        {
            {'=', '=', '=', '='},
            {'#', '#', '#', '#'},
            {'#', '#', '#', '#'},
            {')', '#', '#', '('}
        };

        //. + .
        //+ + .
        //. + .
        char[,] matrix10 = new char[,]
        {
            {'}', '#', '#', '|'},
            {'#', '#', '#', '|'},
            {'#', '#', '#', '|'},
            {')', '#', '#', '|'}
        };

        //. + .
        //+ + +
        //. + .
        char[,] matrix11 = new char[,]
        {
            {'}', '#', '#', '{'},
            {'#', '#', '#', '#'},
            {'#', '#', '#', '#'},
            {')', '#', '#', '('}
        };

        roadWallsDict = new Dictionary<int, char[,]>();

        roadWallsDict[3] = matrix1;
        roadWallsDict[12] = matrix2;
        roadWallsDict[6] = matrix3;
        roadWallsDict[5] = matrix4;
        roadWallsDict[10] = matrix5;
        roadWallsDict[9] = matrix6;
        roadWallsDict[7] = matrix7;
        roadWallsDict[14] = matrix8;
        roadWallsDict[11] = matrix9;
        roadWallsDict[13] = matrix10;
        roadWallsDict[15] = matrix11;

        roadFloorDict = new Dictionary<int, char[,]>();

        //. + .
        //. + .
        //. + .
        char[,] matrix12 = new char[,]
        {
            {'[', '#', '#', ']'},
            {'[', '#', '#', ']'},
            {'[', '#', '#', ']'},
            {'[', '#', '#', ']'}
        };

        //. + .
        //. + +
        //. . .
        char[,] matrix13 = new char[,]
        {
            {'[', '#', '#', ']'},
            {'[', '#', '#', '#'},
            {'[', '#', '#', '#'},
            {'[', '=', '=', '='}
        };

        //. + .
        //+ + .
        //. . .
        char[,] matrix14 = new char[,]
        {
            {'[', '#', '#', ']'},
            {'#', '#', '#', ']'},
            {'#', '#', '#', ']'},
            {'=', '=', '=', ']'}
        };

        //. . .
        //. + +
        //. + .
        char[,] matrix15 = new char[,]
        {
            {'[', '=', '=', '='},
            {'[', '#', '#', '#'},
            {'[', '#', '#', '#'},
            {'[', '#', '#', ']'}
        };

        //. . .
        //+ + .
        //. + .
        char[,] matrix16 = new char[,]
        {
            {'=', '=', '=', ']'},
            {'#', '#', '#', ']'},
            {'#', '#', '#', ']'},
            {'[', '#', '#', ']'}
        };

        //. + .
        //+ + +
        //. . .
        char[,] matrix17 = new char[,]
        {
            {'[', '#', '#', ']'},
            {'#', '#', '#', '#'},
            {'#', '#', '#', '#'},
            {'=', '=', '=', '='}
        };

        //. + .
        //. + +
        //. + .
        char[,] matrix18 = new char[,]
        {
            {'[', '#', '#', ']'},
            {'[', '#', '#', '#'},
            {'[', '#', '#', '#'},
            {'[', '#', '#', ']'}
        };

        //. . .
        //+ + +
        //. + .
        char[,] matrix19 = new char[,]
        {
            {'=', '=', '=', '='},
            {'#', '#', '#', '#'},
            {'#', '#', '#', '#'},
            {'[', '#', '#', ']'}
        };

        //. + .
        //+ + .
        //. + .
        char[,] matrix110 = new char[,]
        {
            {'[', '#', '#', ']'},
            {'#', '#', '#', ']'},
            {'#', '#', '#', ']'},
            {'[', '#', '#', ']'}
        };

        //. + .
        //+ + +
        //. + .
        char[,] matrix111 = new char[,]
        {
            {'[', '#', '#', ']'},
            {'#', '#', '#', '#'},
            {'#', '#', '#', '#'},
            {'[', '#', '#', ']'}
        };

        roadFloorDict[3] = matrix1;
        roadFloorDict[12] = matrix12;
        roadFloorDict[6] = matrix13;
        roadFloorDict[5] = matrix14;
        roadFloorDict[10] = matrix15;
        roadFloorDict[9] = matrix16;
        roadFloorDict[7] = matrix17;
        roadFloorDict[14] = matrix18;
        roadFloorDict[11] = matrix19;
        roadFloorDict[13] = matrix110;
        roadFloorDict[15] = matrix111;
    }

    void Update()
    {
        
    }

    public char[,] GenerateSpritesWalls(char[,] layout)
    {
        char[,] matrix = new char[layout.GetLength(0), layout.GetLength(1)];
        Array.Copy(layout, matrix, layout.Length);

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (matrix[i, j] != 'W') continue;

                bool hasFloorLeft = HasNeighbor(matrix, i, j, 0, -1, '#');
                bool hasFloorRight = HasNeighbor(matrix, i, j, 0, 1, '#');
                bool hasFloorTop = HasNeighbor(matrix, i, j, -1, 0, '#');
                bool hasFloorBottom = HasNeighbor(matrix, i, j, 1, 0, '#');

                bool hasWallLeft = HasNeighbor(matrix, i, j, 0, -1, 'W');
                bool hasWallRight = HasNeighbor(matrix, i, j, 0, 1, 'W');
                bool hasWallTop = HasNeighbor(matrix, i, j, -1, 0, 'W');
                bool hasWallBottom = HasNeighbor(matrix, i, j, 1, 0, 'W');

                if (hasWallTop && hasWallBottom && (hasFloorLeft || hasFloorRight))
                    matrix[i, j] = '|';
                else if (hasWallRight && hasWallLeft && (hasFloorTop || hasFloorBottom))
                    matrix[i, j] = '=';
                else if (!hasWallLeft && !hasWallTop && !hasFloorRight && !hasFloorBottom)
                    matrix[i, j] = '(';
                else if (!hasWallRight && !hasWallTop && !hasFloorLeft && !hasFloorBottom)
                    matrix[i, j] = ')';
                else if (!hasWallLeft && !hasWallBottom && !hasFloorRight && !hasFloorTop)
                    matrix[i, j] = '{';
                else if (!hasWallRight && !hasWallBottom && !hasFloorLeft && !hasFloorTop)
                    matrix[i, j] = '}';
            }
        }

        return matrix;
    }

    private bool HasNeighbor(char[,] matrix, int x, int y, int dx, int dy, char target)
    {
        int newX = x + dx;
        int newY = y + dy;

        if (newX < 0 || newX >= matrix.GetLength(0) ||
            newY < 0 || newY >= matrix.GetLength(1))
        {
            return false;
        }

        if(target == '#')
            return matrix[newX, newY] == target;
        else
        {
            return matrix[newX, newY] != '#';
        }
    }

    public void PlaceMatrixOnTilemap(char[,] matrix, Tilemap targetTilemap, Vector2Int roomPosition)
    {
        Tile tempTile = ScriptableObject.CreateInstance<Tile>();
        Vector3 unityRoomPosition = GetUnityPosition(roomPosition.x, roomPosition.y);

        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                char tileChar = matrix[x, y];

                if (tileDict.TryGetValue(tileChar, out Sprite sprite))
                {
                    tempTile.sprite = sprite;

                    Vector3 aux = GetUnityPosition(roomPosition.x + x, roomPosition.y + y/*, matrix.GetLength(0)*/);
                    Vector3Int tilePos = new Vector3Int((int)aux.x, (int)aux.y, (int) aux.z);

                    if (tileChar == 'W')
                    {
                        targetTilemap.SetTileFlags(tilePos, TileFlags.None);
                        targetTilemap.SetColor(tilePos, Color.black);
                    }

                    if (targetTilemap == floorTilemap && wallChars.Contains(tileChar))
                        continue;

                    if (targetTilemap == wallTilemap && tileChar == '#')
                        continue;

                    targetTilemap.SetTile(tilePos, tempTile);
                }
                else
                {
                    print("Error: unknown tile.");
                }
            }
        }
    }

    internal Vector3 GetUnityPosition(float originalX, float originalY/*, int roomHeight*/)
    {
        float unityX = originalY;
        float unityY = mapHeight - 1 - originalX;
        return new Vector3(unityX, unityY, 0);
    }

    public char[,] GenerateSpritesFloor(char[,] layout)
    {
        char[,] matrix = new char[layout.GetLength(0), layout.GetLength(1)];
        Array.Copy(layout, matrix, layout.Length);

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

        matrix = DecorateFloor(matrix);

        return matrix;
    }

    internal char[,] DecorateFloor(char[,] layout)
    {
        char[,] matrix = (char[,])layout.Clone();
        System.Random rand = new System.Random();

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (matrix[i, j] == '#' && rand.Next(4) == 0)
                {
                    matrix[i, j] = (char)('1' + rand.Next(7));
                }
            }
        }
        return matrix;
    }

    public char[,] GenerateRoomLayout(BSP.Leaf leaf, bool isFirst)
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

        if (!isFirst)
        {
            // Add inner wall rectangles
            int rectCount = UnityEngine.Random.Range(0, 4); // 0 to 3 rectangles
            int attempt = 0;
            int rectangles = 0;

            while (attempt < 20 && rectangles < rectCount)
            {
                int rectWidth = UnityEngine.Random.Range(3, 7);
                int rectHeight = UnityEngine.Random.Range(3, 7);

                int maxX = width - rectWidth - 2;
                int maxY = height - rectHeight - 2;

                if (maxX <= 2 || maxY <= 2)
                    break; // not enough room

                int startX = UnityEngine.Random.Range(2, maxX);
                int startY = UnityEngine.Random.Range(2, maxY);

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
