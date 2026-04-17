using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CoverSystem, 包含一个当前场景特定的CoverView, 以及当前场景的SafeZoneCover列表
/// </summary>
public class CoverSystem : SingletonBaseWithMono<CoverSystem>
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private List<SafeZoneCover> safezoneCoverViews = new List<SafeZoneCover>();
    [SerializeField] private CoverView currentSceneCoverView;

    private readonly HashSet<CoverView> allCovers = new HashSet<CoverView>();
    private readonly HashSet<CoverView> activeCovers = new HashSet<CoverView>();
    public event Action<bool> SafeZoneStateChanged;
    /// <summary>
    /// 当前玩家是否在遮罩之中（cover之中）
    /// </summary>
    public bool IsPlayerInCover { get; private set; }
    public IReadOnlyCollection<CoverView> ActiveCovers => activeCovers;

    private void OnEnable()
    {
        CoverView.PlayerEnteredCover += OnPlayerEnteredCover;
        CoverView.PlayerExitedCover += OnPlayerExitedCover;
    }

    private void OnDisable()
    {
        CoverView.PlayerEnteredCover -= OnPlayerEnteredCover;
        CoverView.PlayerExitedCover -= OnPlayerExitedCover;
    }

    void Start()
    {
        ResolvePlayerTransform();
        RebuildCoverCache();
        RefreshCoverState();
    }

    internal void SyncSafeZoneCovers(List<SafeZoneView> safeZoneViews)
    {
        safezoneCoverViews.Clear();

        HashSet<SafeZoneCover> uniqueCovers = new HashSet<SafeZoneCover>();
        for (int i = 0; i < safeZoneViews.Count; i++)
        {
            SafeZoneView safeZoneView = safeZoneViews[i];
            if (safeZoneView == null)
            {
                continue;
            }

            SafeZoneCover cover = safeZoneView.CoverView;
            if (cover != null)
            {
                uniqueCovers.Add(cover);
            }
        }

        safezoneCoverViews.AddRange(uniqueCovers);
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

        if (safezoneCoverViews.Count == 0)
        {
            safezoneCoverViews.AddRange(FindObjectsOfType<SafeZoneCover>(true));
        }

        if (currentSceneCoverView == null)
        {
            CoverView[] allCoverViews = FindObjectsOfType<CoverView>(true);
            for (int i = 0; i < allCoverViews.Length; i++)
            {
                CoverView coverView = allCoverViews[i];
                if (coverView is SafeZoneCover)
                {
                    continue;
                }

                currentSceneCoverView = coverView;
                break;
            }
        }

        if (currentSceneCoverView != null)
        {
            allCovers.Add(currentSceneCoverView);
        }

        for (int i = safezoneCoverViews.Count - 1; i >= 0; i--)
        {
            SafeZoneCover safeZoneCover = safezoneCoverViews[i];
            if (safeZoneCover == null)
            {
                safezoneCoverViews.RemoveAt(i);
                continue;
            }

            allCovers.Add(safeZoneCover);
        }

    }

    private void RefreshCoverState()
    {
        activeCovers.RemoveWhere(cover => cover == null || !allCovers.Contains(cover));

        bool previousSafeState = IsPlayerInCover;
        IsPlayerInCover = activeCovers.Count > 0;

        if (previousSafeState != IsPlayerInCover)
        {
            SafeZoneStateChanged?.Invoke(IsPlayerInCover);
        }
    }

    private void ResolvePlayerTransform()
    {
        if (_playerTransform != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
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
        }
    }
}
