using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static BSP;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class LevelGenerationController : MonoBehaviour
{
    public BSP layoutGenerator;
    public RoomGenerator roomGenerator;

    void Start()
    {
        Debug();
    }

    void Debug()
    {
        int attempts = 0;

        while (attempts < 5)
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

        char[,] matrix = roomGenerator.GenerateRoomLayout(leaf);
        matrix = roomGenerator.GenerateSpritesFloor(matrix);

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
