using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointData
{
    public Vector2 playerPosition;//玩家的位置
    public int scanPoint;
    public int safeZoneIndex;//关联的安全区遮罩ID
    public void SetCheckPointData(Vector2 playerPosition, int scanPoint, int safeZoneIndex)
    {
        this.playerPosition = playerPosition;
        this.scanPoint = scanPoint;
        this.safeZoneIndex = safeZoneIndex;
    }

    public CheckPointData()
    {
        this.playerPosition = Vector2.zero;
        this.scanPoint = 0;
        this.safeZoneIndex = 0;
    }

}
