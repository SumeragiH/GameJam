using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointData
{
    public Vector2 playerPosition;//玩家的位置
    public int energyPoint;
    public int scanPoint;
    public void SetCheckPointData(Vector3 position, Vector2 playerPosition, int energyPoint, int scanPoint)
    {
        this.playerPosition = playerPosition;
        this.energyPoint = energyPoint;
        this.scanPoint = scanPoint;
    }

    public CheckPointData()
    {
        this.playerPosition = Vector2.zero;
        this.energyPoint = 0;
        this.scanPoint = 0;
    }

}
