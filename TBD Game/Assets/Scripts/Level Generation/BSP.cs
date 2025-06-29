using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class BSP : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 64;
    public int mapHeight = 64;
    public int minRoomSize = 6;
    public int maxLeafSize = 20;
    public int maxIterations = 3;

    public int level = 1;
    internal char[,] matrix;

    internal List<Leaf> leaves = new List<Leaf>();

    void Start()
    {
        //generate level 1 on start
        //GenerateDungeon(32, 32, 3, 8);
        //Debug();
    }

    public void Debug()
    {
        string path = "Assets/Scripts/debug.txt";

        using (StreamWriter writer = new StreamWriter(path))
        {
            for (int i = 0; i < mapWidth; i++)
            {
                for (int j = 0; j < mapHeight; j++)
                {
                    writer.Write(' ');
                    writer.Write(matrix[i, j]);
                    writer.Write(' ');
                }
                writer.WriteLine();
            }
        }

        print("Matrix written to file.");

        /*int counter = 0;

        foreach (Leaf l in leaves)
        {
            if (l.leftChild == null && l.rightChild == null)
            {
                print("room" + counter + "(" + l.room.x + ", " + l.room.y + ")");
                foreach(Vector2Int doorPosition in l.doorPositions)
                {
                    print(doorPosition);
                }
                counter++;
            }
        }*/
    }

    public void GenerateDungeon(int width, int height, int numberOfIterations, int maxSplits)
    {
        mapWidth = width;
        mapHeight = height;
        maxIterations = numberOfIterations;
        leaves = new List<Leaf>();

        CreateSplits(maxSplits);

        foreach (Leaf l in leaves)
        {
            l.CreateRoom();
        }

        WriteToMatrix();

        CreateRoads();

        AddDoorsToMatrix();
    }

    void AddDoorsToMatrix()
    {
        foreach (Leaf l in leaves)
        {
            if (l.room != Rect.zero)
            {
                foreach (Vector2Int doorPosition in l.doorPositions)
                {
                    matrix[doorPosition.x, doorPosition.y] = 'O';
                }
            }
        }
    }

    void CreateRoads()
    {
        foreach (Leaf l in leaves)
        {
            if (!(l.leftChild == null && l.rightChild == null))
                CreateCorridor(l);
        }
    }

    Rect? GetRandomRoom(Leaf node)
    {
        if (node.room != Rect.zero)
            return node.room;

        List<Rect> foundRooms = new List<Rect>();
        if (node.leftChild != null)
        {
            var leftRoom = GetRandomRoom(node.leftChild);
            if (leftRoom != null) foundRooms.Add(leftRoom.Value);
        }
        if (node.rightChild != null)
        {
            var rightRoom = GetRandomRoom(node.rightChild);
            if (rightRoom != null) foundRooms.Add(rightRoom.Value);
        }

        if (foundRooms.Count > 0)
            return foundRooms[UnityEngine.Random.Range(0, foundRooms.Count)];

        return null;
    }

    void WriteToMatrix()
    {
        matrix = new char[mapWidth, mapHeight];

        for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
                matrix[i, j] = '.';

        foreach (Leaf l in leaves)
        {
            if (l.leftChild == null && l.rightChild == null)
            {
                for (int i = (int)l.room.x; i < (int)l.room.x + l.room.width; i++)
                    for (int j = (int)l.room.y; j < (int)l.room.y + l.room.height; j++)
                        matrix[i, j] = '#';
            }
        }
    }

    void CreateSplits(int maxSplits)
    {
        int splits = 0;

        Leaf root = new Leaf(0, 0, mapWidth, mapHeight);
        leaves.Add(root);

        bool didSplit = true;
        int iterations = 0;

        while (didSplit && iterations < maxIterations && splits < maxSplits)
        {
            didSplit = false;
            List<Leaf> newLeaves = new List<Leaf>();
            foreach (Leaf l in leaves)
            {
                if (splits >= maxSplits)
                    break;

                if (l.leftChild == null && l.rightChild == null)
                {
                    if (l.width > maxLeafSize || l.height > maxLeafSize)
                    {
                        if (l.Split(minRoomSize))
                        {
                            newLeaves.Add(l.leftChild);
                            newLeaves.Add(l.rightChild);
                            didSplit = true;
                            splits++;
                        }
                    }
                }
            }
            leaves.AddRange(newLeaves);
            iterations++;
        }
    }

    public class Leaf
    {
        public int x, y, width, height;
        public Leaf leftChild, rightChild;
        public Rect room = Rect.zero;
        public List<Vector2Int> doorPositions = new List<Vector2Int>();

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


    void CreateCorridor(Leaf l)
    {
        List<Rect> leftRooms = GetAllRooms(l.leftChild);
        List<Rect> rightRooms = GetAllRooms(l.rightChild);

        if (leftRooms.Count == 0 || rightRooms.Count == 0)
            return;

        bool connectionMade = false;
        int maxAttempts = leftRooms.Count * rightRooms.Count;
        int attempts = 0;

        while (!connectionMade && attempts < maxAttempts)
        {
            Rect leftRoom = leftRooms[UnityEngine.Random.Range(0, leftRooms.Count)];
            Rect rightRoom = rightRooms[UnityEngine.Random.Range(0, rightRooms.Count)];

            if (CanConnectRooms(leftRoom, rightRoom))
            {
                ConnectRooms(leftRoom, rightRoom);
                connectionMade = true;
            }
            attempts++;
        }

        if (!connectionMade)
        {
            print("Could not find connectable rooms after " + maxAttempts + " attempts");
            Rect leftRoom = leftRooms[UnityEngine.Random.Range(0, leftRooms.Count)];
            Rect rightRoom = rightRooms[UnityEngine.Random.Range(0, rightRooms.Count)];
            DrawLShapedCorridor(leftRoom, rightRoom);
        }
    }

    List<Rect> GetAllRooms(Leaf node)
    {
        List<Rect> rooms = new List<Rect>();

        if (node == null)
            return rooms;

        if (node.room != Rect.zero)
        {
            rooms.Add(node.room);
        }
        else
        {
            if (node.leftChild != null)
                rooms.AddRange(GetAllRooms(node.leftChild));
            if (node.rightChild != null)
                rooms.AddRange(GetAllRooms(node.rightChild));
        }

        return rooms;
    }

    bool CanConnectRooms(Rect room1, Rect room2)
    {
        return CanConnectStraight(room1, room2) || HasClearPath(room1, room2);
    }

    bool CanConnectStraight(Rect room1, Rect room2)
    {
        if (room1.yMax >= room2.y && room1.y <= room2.yMax)
        {
            // Check if there's a clear horizontal path
            int minY = Mathf.Max((int)room1.y, (int)room2.y);
            int maxY = Mathf.Min((int)room1.yMax - 1, (int)room2.yMax - 1);

            for (int y = minY; y <= maxY; y++)
            {
                int startX = room1.x < room2.x ? (int)room1.xMax : (int)room2.xMax;
                int endX = room1.x < room2.x ? (int)room2.x - 1 : (int)room1.x - 1;

                if (IsClearPath(startX, y, endX, y))
                    return true;
            }
        }

        if (room1.xMax >= room2.x && room1.x <= room2.xMax)
        {
            // Check if there's a clear vertical path
            int minX = Mathf.Max((int)room1.x, (int)room2.x);
            int maxX = Mathf.Min((int)room1.xMax - 1, (int)room2.xMax - 1);

            for (int x = minX; x <= maxX; x++)
            {
                int startY = room1.y < room2.y ? (int)room1.yMax : (int)room2.yMax;
                int endY = room1.y < room2.y ? (int)room2.y - 1 : (int)room1.y - 1;

                if (IsClearPath(x, startY, x, endY))
                    return true;
            }
        }

        return false;
    }

    bool HasClearPath(Rect room1, Rect room2)
    {
        Vector2Int point1 = GetBestConnectionPoint(room1, room2);
        Vector2Int point2 = GetBestConnectionPoint(room2, room1);

        bool horizontalFirstClear = IsClearPath(point1.x, point1.y, point2.x, point1.y) &&
                                   IsClearPath(point2.x, point1.y, point2.x, point2.y);

        bool verticalFirstClear = IsClearPath(point1.x, point1.y, point1.x, point2.y) &&
                                 IsClearPath(point1.x, point2.y, point2.x, point2.y);

        return horizontalFirstClear || verticalFirstClear;
    }

    bool IsClearPath(int x1, int y1, int x2, int y2)
    {
        if (x1 == x2)
        {
            // Vertical path
            int minY = Mathf.Min(y1, y2);
            int maxY = Mathf.Max(y1, y2);
            for (int y = minY; y <= maxY; y++)
            {
                if (!IsValidCorridorPosition(x1, y))
                    return false;
            }
        }
        else if (y1 == y2)
        {
            // Horizontal path
            int minX = Mathf.Min(x1, x2);
            int maxX = Mathf.Max(x1, x2);
            for (int x = minX; x <= maxX; x++)
            {
                if (!IsValidCorridorPosition(x, y1))
                    return false;
            }
        }
        else
        {
            return false;
        }

        return true;
    }

    void ConnectRooms(Rect room1, Rect room2)
    {
        if (CanConnectStraight(room1, room2))
        {
            DrawStraightCorridorBetweenRooms(room1, room2);
        }
        else
        {
            DrawLShapedCorridor(room1, room2);
        }
    }

    void AddDoorToRoom(Rect room, Vector2Int doorPosition)
    {
        foreach (Leaf leaf in leaves)
        {
            if (leaf.room == room)
            {
                leaf.doorPositions.Add(doorPosition);
                break;
            }
        }
    }

    void DrawStraightCorridorBetweenRooms(Rect room1, Rect room2)
    {
        if (room1.yMax >= room2.y && room1.y <= room2.yMax)
        {
            int minY = Mathf.Max((int)room1.y, (int)room2.y);
            int maxY = Mathf.Min((int)room1.yMax - 1, (int)room2.yMax - 1);
            int corridorY = UnityEngine.Random.Range(minY + 1, maxY);

            Vector2Int startPoint, endPoint;
            Vector2Int door1, door2;

            if (room1.x < room2.x)
            {
                startPoint = new Vector2Int((int)room1.xMax, corridorY);
                endPoint = new Vector2Int((int)room2.x - 1, corridorY);
                door1 = new Vector2Int((int)room1.xMax - 1, corridorY);
                door2 = new Vector2Int((int)room2.x, corridorY);
            }
            else
            {
                startPoint = new Vector2Int((int)room1.x - 1, corridorY);
                endPoint = new Vector2Int((int)room2.xMax, corridorY);
                door1 = new Vector2Int((int)room1.x, corridorY);
                door2 = new Vector2Int((int)room2.xMax - 1, corridorY);
            }

            AddDoorToRoom(room1, door1);
            AddDoorToRoom(room2, door2);
            DrawStraightCorridor(startPoint, endPoint);
        }
        else if (room1.xMax >= room2.x && room1.x <= room2.xMax)
        {
            int minX = Mathf.Max((int)room1.x, (int)room2.x);
            int maxX = Mathf.Min((int)room1.xMax - 1, (int)room2.xMax - 1);
            int corridorX = UnityEngine.Random.Range(minX + 1, maxX);

            Vector2Int startPoint, endPoint;
            Vector2Int door1, door2;

            if (room1.y < room2.y)
            {
                startPoint = new Vector2Int(corridorX, (int)room1.yMax);
                endPoint = new Vector2Int(corridorX, (int)room2.y - 1);
                door1 = new Vector2Int(corridorX, (int)room1.yMax - 1);
                door2 = new Vector2Int(corridorX, (int)room2.y);
            }
            else
            {
                startPoint = new Vector2Int(corridorX, (int)room1.y - 1);
                endPoint = new Vector2Int(corridorX, (int)room2.yMax);
                door1 = new Vector2Int(corridorX, (int)room1.y);
                door2 = new Vector2Int(corridorX, (int)room2.yMax - 1);
            }

            AddDoorToRoom(room1, door1);
            AddDoorToRoom(room2, door2);
            DrawStraightCorridor(startPoint, endPoint);
        }
    }

    void DrawStraightCorridor(Vector2Int start, Vector2Int end)
    {
        if (start.x == end.x)
        {
            // Vertical corridor
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);
            for (int y = minY; y <= maxY; y++)
            {
                if (IsValidCorridorPosition(start.x, y))
                    matrix[start.x, y] = '+';
            }
        }
        else if (start.y == end.y)
        {
            // Horizontal corridor
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);
            for (int x = minX; x <= maxX; x++)
            {
                if (IsValidCorridorPosition(x, start.y))
                    matrix[x, start.y] = '+';
            }
        }
    }

    void DrawLShapedCorridor(Rect leftRoom, Rect rightRoom)
    {
        Vector2Int leftPoint = GetBestConnectionPoint(leftRoom, rightRoom);
        Vector2Int rightPoint = GetBestConnectionPoint(rightRoom, leftRoom);

        bool horizontalFirstClear = IsClearPath(leftPoint.x, leftPoint.y, rightPoint.x, leftPoint.y) &&
                                   IsClearPath(rightPoint.x, leftPoint.y, rightPoint.x, rightPoint.y);

        bool verticalFirstClear = IsClearPath(leftPoint.x, leftPoint.y, leftPoint.x, rightPoint.y) &&
                                 IsClearPath(leftPoint.x, rightPoint.y, rightPoint.x, rightPoint.y);

        bool horizontalFirst;
        if (horizontalFirstClear && !verticalFirstClear)
            horizontalFirst = true;
        else if (!horizontalFirstClear && verticalFirstClear)
            horizontalFirst = false;
        else
            horizontalFirst = UnityEngine.Random.Range(0f, 1f) > 0.5f;

        Vector2Int door1 = MoveDoorInsideRoom(leftRoom, leftPoint);
        Vector2Int door2 = MoveDoorInsideRoom(rightRoom, rightPoint);

        if (horizontalFirst)
        {
            DrawStraightCorridor(leftPoint, new Vector2Int(rightPoint.x, leftPoint.y));
            DrawStraightCorridor(new Vector2Int(rightPoint.x, leftPoint.y), rightPoint);
        }
        else
        {
            DrawStraightCorridor(leftPoint, new Vector2Int(leftPoint.x, rightPoint.y));
            DrawStraightCorridor(new Vector2Int(leftPoint.x, rightPoint.y), rightPoint);
        }

        AddDoorToRoom(leftRoom, door1);
        AddDoorToRoom(rightRoom, door2);
    }

    Vector2Int MoveDoorInsideRoom(Rect room, Vector2Int doorOutside)
    {
        int xMin = Mathf.FloorToInt(room.xMin);
        int xMax = Mathf.FloorToInt(room.xMax);
        int yMin = Mathf.FloorToInt(room.yMin);
        int yMax = Mathf.FloorToInt(room.yMax);

        if (doorOutside.x < xMin)
            return new Vector2Int(xMin, doorOutside.y); // Left wall
        if (doorOutside.x >= xMax)
            return new Vector2Int(xMax - 1, doorOutside.y); // Right wall
        if (doorOutside.y < yMin)
            return new Vector2Int(doorOutside.x, yMin); // Bottom wall
        if (doorOutside.y >= yMax)
            return new Vector2Int(doorOutside.x, yMax - 1); // Top wall

        return doorOutside;
    }

    Vector2Int GetBestConnectionPoint(Rect fromRoom, Rect toRoom)
    {
        int xMin = Mathf.FloorToInt(fromRoom.xMin);
        int xMax = Mathf.FloorToInt(fromRoom.xMax);
        int yMin = Mathf.FloorToInt(fromRoom.yMin);
        int yMax = Mathf.FloorToInt(fromRoom.yMax);

        Vector2Int roomCenter = new Vector2Int((xMin + xMax) / 2, (yMin + yMax) / 2);
        Vector2Int targetCenter = new Vector2Int(
            (int)(toRoom.x + toRoom.width / 2),
            (int)(toRoom.y + toRoom.height / 2)
        );

        Vector2Int direction = targetCenter - roomCenter;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Connect horizontally
            int safeYMin = yMin + 1;
            int safeYMax = yMax - 1;

            if (safeYMin >= safeYMax) safeYMin = yMin;
            int randomY = UnityEngine.Random.Range(safeYMin, safeYMax);

            if (direction.x > 0)
                return new Vector2Int(xMax, randomY); // right wall
            else
                return new Vector2Int(xMin - 1, randomY); // left wall
        }
        else
        {
            // Connect vertically
            int safeXMin = xMin + 1;
            int safeXMax = xMax - 1;

            if (safeXMin >= safeXMax) safeXMin = xMin;
            int randomX = UnityEngine.Random.Range(safeXMin, safeXMax);

            if (direction.y > 0)
                return new Vector2Int(randomX, yMax); // top wall
            else
                return new Vector2Int(randomX, yMin - 1); // bottom wall
        }
    }

    bool IsValidCorridorPosition(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
            return false;

        // Don't overwrite rooms
        if (matrix[x, y] == '#' || matrix[x, y] == '+' /*||
            matrix[x + 1, y] == '+' ||
            matrix[x, y + 1] == '+' ||
            matrix[x - 1, y] == '+' ||
            matrix[x, y - 1] == '+'*/)
            return false;

        return true;
    }
}