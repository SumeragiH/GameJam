using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class GameSystem : SingletonBaseWithMono<GameSystem>
{
    [SerializeField] public float ShiftPressInterval = 1.0f;
    void Start()
    {
        SafeZoneSystem.Instance.InitActiveSafeZone();
    }
}
