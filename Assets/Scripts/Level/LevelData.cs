using UnityEngine;
using System;

[System.Serializable]
public class Level
{
    public string title;
    public Tile[] tiles;
}

[System.Serializable]
public class Tile
{
    public int id;
    public Position position;
    public string character;
    public int[] children;
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}
