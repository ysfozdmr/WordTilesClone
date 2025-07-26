using UnityEngine;
using System;

[Serializable]
public class Level
{
    public string title;
    public Tile[] tiles;
}

[Serializable]
public class Tile
{
    public int id;
    public Position position;
    public string character;
    public int[] children;
}

[Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}