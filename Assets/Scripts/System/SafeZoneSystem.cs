using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeZoneSystem : SingletonBaseWithMono<SafeZoneSystem>
{
    // 此关卡所有的安全区
    [SerializeField] private List<SafeZoneCoverView> safeZoneViews = new List<SafeZoneCoverView>();
    private int presentSafeZoneIndex = 0;
    private bool isPlayerInSafeZone = true;

    void Start()
    {
        // 设置safeZoneView的index
        for (int i = 0; i < safeZoneViews.Count; i++)
        {
            safeZoneViews[i].safeZoneIndex = i;
            safeZoneViews[i].SyncLinkedCheckPointBySafeZoneIndex();
        }
        presentSafeZoneIndex = 0;
        EventCenter.Instance.EventTrigger("同步安全区遮罩", safeZoneViews);
        EventCenter.Instance.AddListener<int>("玩家进入安全区", OnPlayerEnterSafeZone);
        EventCenter.Instance.AddListener<int>("玩家离开安全区", OnPlayerExitSafeZone);
    }

    /// <summary>
    /// 初始化安全区: 显示第0个
    /// </summary>
    /// <param name="step"></param>
    public void InitActiveSafeZone()
    {
        for (int i = 0; i < safeZoneViews.Count; i++)
        {
            if (i == 0)
            {
                safeZoneViews[i].CoverEnabled = true;
            }
            else
            {
                safeZoneViews[i].CoverEnabled = false;
            }
        }
        presentSafeZoneIndex = 0;
        EventCenter.Instance.EventTrigger("同步安全区遮罩", safeZoneViews);
    }

    public void ActivateSafeZone(int index)
    {
        if (index < 0 || index >= safeZoneViews.Count)
        {
            Debug.LogError($"SafeZoneSystem: Invalid safe zone index {index}. It should be between 0 and {safeZoneViews.Count - 1}.");
            return;
        }

        if (index != presentSafeZoneIndex + 1)
        {
            Debug.LogWarning($"SafeZoneSystem: Attempting to activate safe zone {index} while the current active safe zone is {presentSafeZoneIndex}. Safe zones should be activated in order.");
            return;
        }

        safeZoneViews[index].CoverEnabled = true;
        EventCenter.Instance.EventTrigger("同步安全区遮罩", safeZoneViews);
    }

    public void DeactivateSafeZone(int index)
    {
        if (index < 0 || index >= safeZoneViews.Count)
        {
            Debug.LogError($"SafeZoneSystem: Invalid safe zone index {index}. It should be between 0 and {safeZoneViews.Count - 1}.");
            return;
        }

        if (index != presentSafeZoneIndex - 1)
        {
            Debug.LogWarning($"SafeZoneSystem: Attempting to deactivate safe zone {index} while the current active safe zone is {presentSafeZoneIndex}. Only the currently active safe zone can be deactivated.");
            return;
        }

        safeZoneViews[index].CoverEnabled = false;
        EventCenter.Instance.EventTrigger("同步安全区遮罩", safeZoneViews);
    }

    public void ArriveAtSafeZone(int index)
    {
        if (index < 0 || index >= safeZoneViews.Count)
        {
            Debug.LogError($"SafeZoneSystem: Invalid safe zone index {index}. It should be between 0 and {safeZoneViews.Count - 1}.");
            return;
        }

        presentSafeZoneIndex = index;
    }

    public int GetPresentSafeZoneIndex()
    {
        return presentSafeZoneIndex;
    }

    public bool IsPlayerInSafeZone()
    {
        return isPlayerInSafeZone;
    }

    public void ShiftSafeZoneZoom()
    {
        for (int i = 0; i < safeZoneViews.Count; i++)
        {
            safeZoneViews[i].ShiftState();
        }
    }

    public void ResetSafeZone()
    {
        for (int i = 0; i < safeZoneViews.Count; i++)
        {
            safeZoneViews[i].ResetCover();
        }
    }

    public void OnPlayerEnterSafeZone(int index)
    {
        Debug.Log($"玩家进入安全区 {index}");
        presentSafeZoneIndex = index;
        isPlayerInSafeZone = true;
    }

    public void OnPlayerExitSafeZone(int index)
    {
        Debug.Log($"玩家离开安全区 {index}");
        isPlayerInSafeZone = false;
    }

    public void TryActivateNextSafeZoneByScan(int scannedSafeZoneIndex)
    {
        int nextIndex = presentSafeZoneIndex + 1;
        if (scannedSafeZoneIndex != nextIndex || nextIndex < 0 || nextIndex >= safeZoneViews.Count)
        {
            return;
        }

        if (presentSafeZoneIndex < 0 || presentSafeZoneIndex >= safeZoneViews.Count)
        {
            return;
        }


        SafeZoneCoverView currentSafeZone = safeZoneViews[presentSafeZoneIndex];
        SafeZoneCoverView nextSafeZone = safeZoneViews[nextIndex];
        SafeZoneCoverView previousSafeZone = presentSafeZoneIndex - 1 >= 0 && presentSafeZoneIndex - 1 < safeZoneViews.Count ? safeZoneViews[presentSafeZoneIndex - 1] : null;

        if (previousSafeZone != null && !previousSafeZone.CoverEnabled)
        {
            return;
        }

        if (currentSafeZone == null || nextSafeZone == null)
        {
            return;
        }

        if (nextSafeZone.CoverEnabled || !currentSafeZone.CoverEnabled)
        {
            return;
        }

        nextSafeZone.CoverEnabled = true;
        if (previousSafeZone != null)
        {
            previousSafeZone.CoverEnabled = false;
        }
        EventCenter.Instance.EventTrigger("同步安全区遮罩", safeZoneViews);
    }

}
