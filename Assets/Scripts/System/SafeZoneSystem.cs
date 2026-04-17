using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneSystem : SingletonBaseWithMono<SafeZoneSystem>
{
    // 此关卡所有的安全区
    [SerializeField] private List<SafeZoneView> safeZoneViews = new List<SafeZoneView>();

    void Start()
    {
        EventCenter.Instance.EventTrigger("同步安全区遮罩", safeZoneViews);
    }
}
