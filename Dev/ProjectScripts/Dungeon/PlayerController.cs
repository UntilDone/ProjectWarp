using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using JetBrains.Annotations;

public class PlayerController : MonoBehaviour
{
    private DungeonGenerator dungeon;
    private CinemachineVirtualCamera virtualCamera;

    private bool isMoving;

    private void Awake()
    {
        dungeon = transform.parent.GetComponent<DungeonGenerator>();

        virtualCamera = Camera.main.GetComponentInChildren<CinemachineVirtualCamera>();
        virtualCamera.Follow = transform;
    }

    private void Update()
    {
        if (Input.anyKey)
        {
            CheckInput();
        }
    }

    private void CheckInput()
    {
        KeyCode key = GetPressedKey();

        switch (key)
        {
            case KeyCode.A:
            case KeyCode.LeftArrow:
                MovePlayer("Left");
                break;

            case KeyCode.D:
            case KeyCode.RightArrow:
                MovePlayer("Right");
                break;

            case KeyCode.W:
            case KeyCode.UpArrow:
                MovePlayer("Up");
                break;

            case KeyCode.S:
            case KeyCode.DownArrow:
                MovePlayer("Down");
                break;

            default:
                break;
        }
    }

    private KeyCode GetPressedKey()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKey(key))
            {
                return key;
            }
        }
        return KeyCode.None;
    }

    private void MovePlayer(string direction)
    {
        int x = 0;
        int y = 0;

        switch (direction)
        {
            case "Left":
                x = -1;
                break;

            case "Right":
                x = 1;
                break;

            case "Up":
                y = 1;
                break;

            case "Down":
                y = -1;
                break;
        }

        int a = (int)transform.position.x + x;
        int b = (int)transform.position.y + y;

        if (a > dungeon.rawCount - 1 || b > dungeon.colCount - 1 || a < 0 || b < 0 || dungeon.tiles[a, b] == DungeonGenerator.TileType.Wall)
        {
            return;
        }
        else
        {
            if (isMoving) return;
            StartCoroutine(CoMove(a, b));
        }
    }

    private IEnumerator CoMove(int a, int b)
    {
        isMoving = true;
        transform.position = new Vector3(a, b, 0);

        yield return new WaitForSeconds(0.1f);
        isMoving = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Tile tile = collision.GetComponent<Tile>();
        if (tile == null) return;

        if (tile.type == DungeonGenerator.TileType.Path)
        {
            int x = (int)tile.position.x;
            int y = (int)tile.position.y;
            BrightAround(x, y);

            Room room = GetRoomAtPosition(tile.transform.position);

            if (room != null)
            {
                BrightRoom(room);
            }
        }
        else if (tile.type == DungeonGenerator.TileType.Stair)
        {
            int childCount = transform.parent.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Destroy(transform.parent.GetChild(i).gameObject);
            }

            dungeon.GenerateDungeon(2, 5, 3, 6);
        }
    }

    public Room GetRoomAtPosition(Vector2 position)
    {
        foreach (Room room in dungeon.rooms)
        {
            if (position.x >= room.X && position.x < room.X + room.Width &&
                position.y >= room.Y && position.y < room.Y + room.Height)
            {
                return room;
            }
        }
        return null;
    }

    public void BrightAround(int x, int y)
    {
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (dungeon.tiles[i, j] == DungeonGenerator.TileType.Path)
                {
                    SpriteRenderer sprite = dungeon.tileObjects[i, j].GetComponent<SpriteRenderer>();
                    sprite.color = Color.white;
                }
                else if (dungeon.tiles[i, j] == DungeonGenerator.TileType.Wall)
                {
                    bool isPathAround = false;

                    for (int a = i - 1; a <= i + 1; a++)
                    {
                        if (isPathAround) break;
                        for (int b = j - 1; b <= j + 1; b++)
                        {
                            if (dungeon.tiles[a, b] == DungeonGenerator.TileType.Path)
                            {
                                isPathAround = true;
                                break;
                            }
                        }
                    }

                    if (!isPathAround) continue;
                    SpriteRenderer sprite = dungeon.tileObjects[i, j].GetComponent<SpriteRenderer>();
                    sprite.color = Color.white;
                }
            }
        }
    }

    public void BrightRoom(Room room)
    {
        for (int x = room.X - 1; x < room.X + room.Width + 1; x++)
        {
            for (int y = room.Y - 1; y < room.Y + room.Height + 1; y++)
            {
                if (x >= 0 && x < dungeon.rawCount && y >= 0 && y < dungeon.colCount)
                {
                    SpriteRenderer sprite = dungeon.tileObjects[x, y].GetComponent<SpriteRenderer>();

                    sprite.color = Color.white;
                }
            }
        }
    }
}
