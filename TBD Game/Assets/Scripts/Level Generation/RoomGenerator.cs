using System.Collections;
using System.Collections.Generic;
using System.IO;
using TreeEditor;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public char[,] GenerateRoomLayout(BSP.Leaf leaf)
    {
        char[,] matrix = new char[(int)leaf.room.width * 4, (int)leaf.room.height * 4];

        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = '#';
            }
        }

        return matrix;
    }
}
