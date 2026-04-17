using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneSystem : SingletonBaseWithMono<SafeZoneSystem>
{
    // 此关卡所有的安全区
    private List<SafeZoneView> safeZoneViews = new List<SafeZoneView>();

    void Start()
    {
        safeZoneViews.Clear();
        safeZoneViews.AddRange(FindObjectsOfType<SafeZoneView>(true));

        CoverSystem coverSystem = FindObjectOfType<CoverSystem>();
        if (coverSystem == null)
        {
            Debug.LogWarning("SafeZoneSystem: CoverSystem not found in scene, cannot sync safe zones.");
            return;
        }

        coverSystem.SyncSafeZoneCovers(safeZoneViews);
    }
}
