using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BSP : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 64;
    public int mapHeight = 64;
    public int minRoomSize = 6;
    public int maxLeafSize = 20;
    public int maxIterations = 3;

    public int level = 1;

    private List<Leaf> leaves = new List<Leaf>();

    void Start()
    {
        //generate level 1 on start
        GenerateDungeon(24, 24, 2);
        Debug();
    }

    void Debug()
    {
        char[,] matrix = new char[mapWidth, mapHeight];

        for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
                matrix[i, j] = 'O';

        foreach(Leaf l in leaves)
        {
            if (l.leftChild == null && l.rightChild == null)
            {
                for (int i = (int)l.room.x; i < (int)l.room.x + l.room.width; i++)
                    for (int j = (int)l.room.y; j < (int)l.room.y + l.room.height; j++)
                        matrix[i, j] = '#';
            }
        }

        string path = "Assets/Scripts/debug.txt";

        using (StreamWriter writer = new StreamWriter(path))
        {
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    writer.Write(matrix[i, j]);
                }
                writer.WriteLine();
            }
        }

        print("Matrix written to file.");

    }

    void GenerateDungeon(int width, int height, int numberOfIterations)
    {
        mapWidth = width;
        mapHeight = height;
        maxIterations = numberOfIterations;

        Leaf root = new Leaf(0, 0, mapWidth, mapHeight);
        leaves.Add(root);

        bool didSplit = true;
        int iterations = 0;

        while (didSplit && iterations < maxIterations)
        {
            didSplit = false;
            List<Leaf> newLeaves = new List<Leaf>();
            foreach (Leaf l in leaves)
            {
                if (l.leftChild == null && l.rightChild == null)
                {
                    if (l.width > maxLeafSize || l.height > maxLeafSize)
                    {
                        if (l.Split(minRoomSize))
                        {
                            newLeaves.Add(l.leftChild);
                            newLeaves.Add(l.rightChild);
                            didSplit = true;
                        }
                    }
                }
            }
            leaves.AddRange(newLeaves);
            iterations++;
        }

        foreach (Leaf l in leaves)
        {
            l.CreateRoom();
        }
    }

    public class Leaf
    {
        public int x, y, width, height;
        public Leaf leftChild, rightChild;
        public Rect room = Rect.zero;

        public Leaf(int x, int y, int width, int height)
        {
            this.x = x; this.y = y;
            this.width = width; this.height = height;
        }

        public bool Split(int minRoomSize)
        {
            bool splitH = UnityEngine.Random.Range(0f, 1f) > 0.5f;

            if (width > height && width / height >= 1.25f)
                splitH = false;
            else if (height > width && height / width >= 1.25f)
                splitH = true;

            int max = (splitH ? height : width) - minRoomSize;
            if (max <= minRoomSize)
                return false;

            int split = UnityEngine.Random.Range(minRoomSize, max);

            if (splitH)
            {
                leftChild = new Leaf(x, y, width, split);
                rightChild = new Leaf(x, y + split, width, height - split);
            }
            else
            {
                leftChild = new Leaf(x, y, split, height);
                rightChild = new Leaf(x + split, y, width - split, height);
            }
            return true;
        }

        public void CreateRoom()
        {
            if (leftChild != null || rightChild != null)
            {
                if (leftChild != null) leftChild.CreateRoom();
                if (rightChild != null) rightChild.CreateRoom();
            }
            else
            {
                int roomWidth = UnityEngine.Random.Range(3, width - 2);
                int roomHeight = UnityEngine.Random.Range(3, height - 2);
                int roomX = x + UnityEngine.Random.Range(1, width - roomWidth - 1);
                int roomY = y + UnityEngine.Random.Range(1, height - roomHeight - 1);

                room = new Rect(roomX, roomY, roomWidth, roomHeight);
            }
        }
    }
}