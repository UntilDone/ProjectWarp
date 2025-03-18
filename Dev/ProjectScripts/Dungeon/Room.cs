using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    public Vector2Int Center
    {
        get { return new Vector2Int(X + Width / 2, Y + Height / 2); }
    }

    public Room(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
