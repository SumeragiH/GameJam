using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneSystem : SingletonBaseWithMono<SafeZoneSystem>
{
    // 此关卡所有的安全区
    [SerializeField] private List<SafeZoneCoverView> safeZoneViews = new List<SafeZoneCoverView>();

    void Start()
    {
        EventCenter.Instance.EventTrigger("同步安全区遮罩", safeZoneViews);
    }

    /// <summary>
    /// 暂时用于设置显示的安全区，step表示当前在第几个存档点，从0计数
    /// </summary>
    /// <param name="step"></param>
    public void setActiveSafeZone(int step)
    {
        for (int i = 0; i < safeZoneViews.Count; i++)
        {
            if (i == step || i == step + 1)
            {
                safeZoneViews[i].CoverEnabled = true;
            }
            else
            {
                safeZoneViews[i].CoverEnabled = false;
            }
        }
        EventCenter.Instance.EventTrigger("同步安全区遮罩", safeZoneViews);
    }
}
