using System;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;

/// <summary>
/// CoverSystem, 包含一个当前场景特定的CoverView, 以及当前场景的SafeZoneCover列表
/// </summary>
public class CoverSystem : SingletonBaseWithMono<CoverSystem>
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private List<SafeZoneCoverView> safezoneCoverViews = new List<SafeZoneCoverView>();
    [SerializeField] private List<CoverView> sceneCoverViews = new List<CoverView>();
    /// <summary>
    /// 代表当前激活的场景的CoverView的index，-1代表没有指定
    /// </summary>
    private int currentSceneCoverViewIndex = -1;

    /// <summary>
    /// 所有活跃的遮罩
    /// </summary>
    private readonly HashSet<CoverView> allCovers = new HashSet<CoverView>();
    /// <summary>
    /// 玩家在的遮罩
    /// </summary>
    private readonly HashSet<CoverView> activeCovers = new HashSet<CoverView>();
    public event Action<bool> SafeZoneStateChanged;
    /// <summary>
    /// 当前玩家是否在遮罩之中（cover之中）
    /// </summary>
    public bool IsPlayerInCover { get; private set; } = false;
    public IReadOnlyCollection<CoverView> ActiveCovers => activeCovers;

    private void OnEnable()
    {
        EventCenter.Instance.AddListener<CoverView>("玩家进入遮罩", OnPlayerEnteredCover);
        EventCenter.Instance.AddListener<CoverView>("玩家离开遮罩", OnPlayerExitedCover);
        EventCenter.Instance.AddListener<int>("设定遮罩序号", SetCurrentSceneCoverViewIndex);
        EventCenter.Instance.AddListener<List<SafeZoneCoverView>>("同步安全区遮罩", SyncSafeZoneCovers);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveListener<CoverView>("玩家进入遮罩", OnPlayerEnteredCover);
        EventCenter.Instance.RemoveListener<CoverView>("玩家离开遮罩", OnPlayerExitedCover);
        EventCenter.Instance.RemoveListener<int>("设定遮罩序号", SetCurrentSceneCoverViewIndex);
        EventCenter.Instance.RemoveListener<List<SafeZoneCoverView>>("同步安全区遮罩", SyncSafeZoneCovers);
    }

    void Start()
    {
        if (_playerTransform == null)
        {
            Debug.LogError("CoverSystem: Player Transform is not assigned.");
        }
        RebuildCoverCache();
        RefreshCoverState();
    }

    internal void SetCurrentSceneCoverViewIndex(int index)
    {
        if (index < -1 || index >= sceneCoverViews.Count)
        {
            Debug.LogError($"CoverSystem: Invalid scene cover view index {index}. It should be between -1 and {sceneCoverViews.Count - 1}.");
            return;
        }

        currentSceneCoverViewIndex = index;
        RebuildCoverCache();
        RefreshCoverState();
    }

    internal void SyncSafeZoneCovers(List<SafeZoneCoverView> safeZoneViews)
    {
        safezoneCoverViews.Clear();

        HashSet<SafeZoneCoverView> uniqueCovers = new HashSet<SafeZoneCoverView>();
        for (int i = 0; i < safeZoneViews.Count; i++)
        {
            SafeZoneCoverView safeZoneView = safeZoneViews[i];
            if (safeZoneView == null)
            {
                continue;
            }

            uniqueCovers.Add(safeZoneView);
        }
        safezoneCoverViews.AddRange(uniqueCovers);
        Debug.Log("safezoneCoverViews: " + safezoneCoverViews.Count);
        RebuildCoverCache();
        RefreshCoverState();
    }

    /// <summary>
    /// 当前是否处在可进入区域（当前场景Cover或SafeZoneCover）
    /// </summary>
    /// <returns></returns>
    public bool IsInSafeZone()
    {
        return IsPlayerInCover;
    }

    private void RebuildCoverCache()
    {
        allCovers.Clear();

        if (sceneCoverViews.Count == 0)
        {
            Debug.LogWarning("CoverSystem: No CoverViews assigned for the current scene.");
        }
        else if (currentSceneCoverViewIndex >= 0 && currentSceneCoverViewIndex < sceneCoverViews.Count)
        {
            allCovers.Add(sceneCoverViews[currentSceneCoverViewIndex]);
        }

        for (int i = safezoneCoverViews.Count - 1; i >= 0; i--)
        {
            SafeZoneCoverView safeZoneCoverView = safezoneCoverViews[i];
            if (safeZoneCoverView != null && safeZoneCoverView.CoverEnabled)
            {
                allCovers.Add(safeZoneCoverView);
            }
        }
    }

    private void RefreshCoverState()
    {
        activeCovers.RemoveWhere(cover => cover == null || !allCovers.Contains(cover) || !cover.CoverEnabled);

        bool previousSafeState = IsPlayerInCover;
        IsPlayerInCover = activeCovers.Count > 0;
        Debug.Log("activeCovers: " + activeCovers.Count);

        if (previousSafeState != IsPlayerInCover)
        {
            EventCenter.Instance.EventTrigger<bool>("玩家进出安全区", IsPlayerInCover);
            Debug.Log("玩家在安全区中：" + IsPlayerInCover);
        }
    }

    private void OnPlayerEnteredCover(CoverView coverView)
    {
        if (coverView == null || !allCovers.Contains(coverView))
        {
            return;
        }

        if (activeCovers.Add(coverView))
        {
            RefreshCoverState();
            Debug.Log("玩家进入遮罩");
        }
    }

    private void OnPlayerExitedCover(CoverView coverView)
    {
        if (coverView == null)
        {
            return;
        }

        if (activeCovers.Remove(coverView))
        {
            RefreshCoverState();
            Debug.Log("玩家离开遮罩");
        }
    }
}
