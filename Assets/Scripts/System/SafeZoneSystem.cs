using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneSystem : SingletonBaseWithMono<SafeZoneSystem>
{
    // 此关卡所有的安全区
    [SerializeField] private List<SafeZoneView> safeZoneViews = new List<SafeZoneView>();

    void Start()
    {
        CoverSystem.Instance.SyncSafeZoneCovers(safeZoneViews);
    }
}
