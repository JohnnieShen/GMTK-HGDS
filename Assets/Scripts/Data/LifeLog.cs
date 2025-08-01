using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LifeLog
{
    public List<PlayerInputFrame> frames;
    public Vector3 spawnPos;
    public float  startTime;
    public float  endTime;
}