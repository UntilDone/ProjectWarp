using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DungeonGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject tileObject;

    [SerializeField]
    private GameObject playerObject;

    [SerializeField]
    private GameObject dungeonObject;

    public TileType[,] tiles = new TileType[32, 32];
    public GameObject[,] tileObjects = new GameObject[32, 32];
    public Vector2 startPosition;
    public List<Room> rooms;

    public int rawCount;
    public int colCount;

    public enum TileType { Wall, Path, Start, Stair }

    private void Start()
    {
        rawCount = tiles.GetLength(0);
        colCount = tiles.GetLength(1);

        GenerateDungeon(2, 2, 3, 5);
    }

    public void GenerateDungeon(int minRooms, int maxRooms, int minRoomSize, int maxRoomSize)
    {
        for (int i = 0; i < rawCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                tiles[i, j] = TileType.Wall;
            }
        }

        rooms = GenerateRooms(minRooms, maxRooms, minRoomSize, maxRoomSize);

        foreach (var room in rooms)
        {
            CreateRoom(room);
        }

        List<Vector2Int> path = ConnectRooms(rooms);

        if (path.Count > 0)
        {
            Vector2Int stairPosition = path[path.Count - 1];
            tiles[stairPosition.x, stairPosition.y] = TileType.Stair;
        }

        startPosition = rooms[0].Center;
        tiles[(int)startPosition.x, (int)startPosition.y] = TileType.Start;

        InstantiateTileObjects();

        GameObject obj = Instantiate(playerObject, startPosition, Quaternion.Euler(Vector3.zero), transform);
        PlayerController player = obj.GetComponent<PlayerController>();
        Room startRoom = player.GetRoomAtPosition(startPosition);
        player.BrightRoom(startRoom);
        //FindPath((int)startPosition.x, (int)startPosition.y, minGridCount, maxGridCount);
    }

    private List<Room> GenerateRooms(int minRooms, int maxRooms, int minSize, int maxSize)
    {
        List<Room> rooms = new List<Room>();
        int roomCount = Random.Range(minRooms, maxRooms + 1);

        for (int i = 0; i < roomCount; i++)
        {
            int width = Random.Range(minSize, maxSize);
            int height = Random.Range(minSize, maxSize);
            int x = Random.Range(1, rawCount - width - 1);
            int y = Random.Range(1, colCount - height - 1);

            bool overlaps = false;
            foreach (var room in rooms)
            {
                if (IsOverlapping(room, x, y, width, height))
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                Room newRoom = new Room(x, y, width, height);
                rooms.Add(newRoom);

            }
            else
            {
                i--;
            }

        }

        return rooms;
    }

    private bool IsOverlapping(Room room, int x, int y, int width, int height)
    {
        return x < room.X + room.Width &&
               x + width > room.X &&
               y < room.Y + room.Height &&
               y + height > room.Y;
    }

    private void CreateRoom(Room room)
    {
        for (int x = room.X; x < room.X + room.Width; x++)
        {
            for (int y = room.Y; y < room.Y + room.Height; y++)
            {
                tiles[x, y] = TileType.Path;
            }
        }
    }

    private List<Vector2Int> ConnectRooms(List<Room> rooms)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int prevCenter = rooms[0].Center;
        path.Add(prevCenter);

        HashSet<Vector2Int> connectedRooms = new HashSet<Vector2Int>();
        connectedRooms.Add(prevCenter);

        while (connectedRooms.Count < rooms.Count)
        {
            Room nextRoom = null;
            Vector2Int nextCenter = Vector2Int.zero;

            foreach (var room in rooms)
            {
                if (!connectedRooms.Contains(room.Center))
                {
                    nextRoom = room;
                    nextCenter = room.Center;
                    break;
                }
            }

            if (nextRoom == null)
                break;

            while (prevCenter != nextCenter)
            {
                if (prevCenter.x != nextCenter.x)
                {
                    prevCenter.x += (prevCenter.x < nextCenter.x) ? 1 : -1;
                    path.Add(prevCenter);
                    tiles[prevCenter.x, prevCenter.y] = TileType.Path;
                }
                else if (prevCenter.y != nextCenter.y)
                {
                    prevCenter.y += (prevCenter.y < nextCenter.y) ? 1 : -1;
                    path.Add(prevCenter);
                    tiles[prevCenter.x, prevCenter.y] = TileType.Path;
                }
            }

            connectedRooms.Add(nextCenter);
            prevCenter = nextCenter;
        }

        return path;
    }

    private void CreatePath(Vector2Int start, Vector2Int end)
    {
        while (start.x != end.x || start.y != end.y)
        {
            if (start.x < end.x)
                start.x++;
            else if (start.x > end.x)
                start.x--;

            if (start.y < end.y)
                start.y++;
            else if (start.y > end.y)
                start.y--;

            tiles[start.x, start.y] = TileType.Path;
        }
    }

    //private void FindPath(int startX, int startY, int minGridCount, int maxGridCount)
    //{
    //    Queue<Vector2Int> queue = new Queue<Vector2Int>();
    //    queue.Enqueue(new Vector2Int(startX, startY));

    //    List<Vector2Int> pathEnds = new List<Vector2Int>();

    //    int size = 0;

    //    while (queue.Count > 0 && size < maxGridCount)
    //    {
    //        Vector2Int current = queue.Dequeue();
    //        bool isEnd = true;

    //        foreach (Vector2Int direction in new Vector2Int[]
    //        {
    //            new Vector2Int(0, 1),
    //            new Vector2Int(0, -1),
    //            new Vector2Int(-1, 0),
    //            new Vector2Int(1, 0),
    //        })
    //        {
    //            int newX = current.x + direction.x;
    //            int newY = current.y + direction.y;

    //            if (newX > 0 && newX < rawCount &&
    //                newY > 0 && newY < colCount &&
    //                tiles[newX, newY] == TileType.Wall)
    //            {
    //                if (Random.value < 0.4f || size < minGridCount)
    //                {
    //                    tiles[newX, newY] = TileType.Path;
    //                    queue.Enqueue(new Vector2Int(newX, newY));
    //                    isEnd = false;
    //                    size++;
    //                }
    //            }
    //        }

    //        if (isEnd)
    //        {
    //            pathEnds.Add(current);
    //        }

    //        if (size < minGridCount && queue.Count == 0)
    //        {
    //            break;
    //        }
    //    }

    //    if (size >= minGridCount && pathEnds.Count > 0)
    //    {
    //        Vector2Int stairPosition = pathEnds[Random.Range(0, pathEnds.Count)];
    //        tiles[stairPosition.x, stairPosition.y] = TileType.Stair;
    //    }
    //}

    private void InstantiateTileObjects()
    {
        for (int i = 0; i < rawCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                Vector2 position = new Vector2(i, j);

                GameObject tileObj = Instantiate(tileObject, position, Quaternion.Euler(Vector3.zero), transform);
                tileObjects[i, j] = tileObj;

                Tile tile = tileObj.GetComponent<Tile>();
                tile.type = tiles[i, j];
                tile.position = new Vector2(i, j);

                SpriteRenderer sprite = tileObj.GetComponent<SpriteRenderer>();
                Sprite[] sprites = Resources.LoadAll<Sprite>("Dungeon/dungeon_sprite");

                switch (tiles[i, j])
                {
                    case TileType.Wall:
                        sprite.color = Color.black;
                        sprite.sprite = sprites[0];
                        break;

                    case TileType.Path:
                        sprite.color = Color.black;
                        sprite.sprite = sprites[17];
                        break;

                    case TileType.Start:
                        sprite.sprite = sprites[17];
                        GameObject dungeonObj = Instantiate(dungeonObject, startPosition, Quaternion.Euler(Vector3.zero), transform);
                        SpriteRenderer dungeonObjSprite = dungeonObj.GetComponent<SpriteRenderer>();
                        dungeonObjSprite.sprite = sprites[28];

                        break;

                    case TileType.Stair:
                        sprite.color = Color.black;
                        sprite.sprite = sprites[27];
                        break;
                }
            }
        }
    }
}
