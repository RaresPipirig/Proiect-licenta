using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSP : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 64;
    public int mapHeight = 64;
    public int minRoomSize = 6;
    public int maxLeafSize = 20;
    public int maxIterations = 5;

    private List<Leaf> leaves = new List<Leaf>();

    void Start()
    {
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        Leaf root = new Leaf(0, 0, mapWidth, mapHeight);
        leaves.Add(root);

        // Split until all leaves are below the size or reach max iterations
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

        // Create rooms in all leaf nodes
        foreach (Leaf l in leaves)
        {
            l.CreateRoom();
        }
    }

    // Draw the rooms with Gizmos
    void OnDrawGizmos()
    {
        if (leaves == null) return;

        Gizmos.color = Color.green;
        foreach (Leaf l in leaves)
        {
            if (l.room != Rect.zero)
            {
                Gizmos.DrawWireCube(
                    new Vector3(l.room.x + l.room.width / 2, 0, l.room.y + l.room.height / 2),
                    new Vector3(l.room.width, 0.1f, l.room.height)
                );
            }
        }
    }

    // ---------- BSP Leaf Node ----------
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
            bool splitH = Random.Range(0f, 1f) > 0.5f;

            if (width > height && width / height >= 1.25f)
                splitH = false;
            else if (height > width && height / width >= 1.25f)
                splitH = true;

            int max = (splitH ? height : width) - minRoomSize;
            if (max <= minRoomSize)
                return false;

            int split = Random.Range(minRoomSize, max);

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
                int roomWidth = Random.Range(3, width - 2);
                int roomHeight = Random.Range(3, height - 2);
                int roomX = x + Random.Range(1, width - roomWidth - 1);
                int roomY = y + Random.Range(1, height - roomHeight - 1);

                room = new Rect(roomX, roomY, roomWidth, roomHeight);
            }
        }
    }
}