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
    private List<SafeZoneCoverView> safezoneCoverViews = new List<SafeZoneCoverView>();

    [Serializable]
    class CoverEnumViewRelation
    {
        public CoverEnum coverEnum;
        public CoverView coverView;
    }
    [SerializeField] private List<CoverEnumViewRelation> integralCoverViews = new();
    /// <summary>
    /// 除了safezone外所有的整体作用的CoverViews
    /// </summary>
    [SerializeField] private Dictionary<CoverEnum, CoverView> _integralCoverViews = new Dictionary<CoverEnum, CoverView>();
    /// <summary>
    /// 代表当前激活的场景的可选CoverView
    /// </summary>
    [SerializeField] private List<CoverEnum> sceneCoverTypes = new List<CoverEnum>();
    private CoverEnum selectedCoverType = CoverEnum.None;
    public int selectedIndex = -1;

    /// <summary>
    /// 所有活跃的遮罩
    /// </summary>
    private readonly HashSet<CoverView> allCovers = new HashSet<CoverView>();
    /// <summary>
    /// 玩家在的遮罩
    /// </summary>
    private readonly HashSet<CoverView> activeCovers = new HashSet<CoverView>();
    private bool _suppressDeathEvent = false;
    public event Action<bool> SafeZoneStateChanged;
    /// <summary>
    /// 当前玩家是否在遮罩之中（cover之中）
    /// </summary>
    public bool IsPlayerInCover { get; private set; } = false;
    private bool FirstTimeLackEnergy = true;
    public IReadOnlyCollection<CoverView> ActiveCovers => activeCovers;
    private bool FirstTimeShiftScan = true;


    private void OnEnable()
    {
        EventCenter.Instance.AddListener<CoverView>("玩家进入遮罩", OnPlayerEnteredCover);
        EventCenter.Instance.AddListener<CoverView>("玩家离开遮罩", OnPlayerExitedCover);
        EventCenter.Instance.AddListener<List<SafeZoneCoverView>>("同步安全区遮罩", SyncSafeZoneCovers);
        EventCenter.Instance.AddListener("shift按下", OnShiftPressed);
        EventCenter.Instance.AddListener<int>("选择遮罩序号", OnSelectedCoverIndexChanged);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveListener<CoverView>("玩家进入遮罩", OnPlayerEnteredCover);
        EventCenter.Instance.RemoveListener<CoverView>("玩家离开遮罩", OnPlayerExitedCover);
        EventCenter.Instance.RemoveListener<List<SafeZoneCoverView>>("同步安全区遮罩", SyncSafeZoneCovers);
        EventCenter.Instance.RemoveListener("shift按下", OnShiftPressed);
        EventCenter.Instance.RemoveListener<int>("选择遮罩序号", OnSelectedCoverIndexChanged);
    }

    void Start()
    {
        // 将integralCoverViews加入_intgralCoverViews中
        foreach (var relation in integralCoverViews)
        {
            if (relation.coverView == null)
            {
                Debug.LogWarning($"CoverSystem: CoverView for {relation.coverEnum} is not assigned.");
                continue;
            }

            if (_integralCoverViews.ContainsKey(relation.coverEnum))
            {
                Debug.LogWarning($"CoverSystem: Duplicate CoverEnum {relation.coverEnum} in integralCoverViews.");
                continue;
            }

            _integralCoverViews.Add(relation.coverEnum, relation.coverView);
        }

        // disable 所有sceneCoverTypes对应的cover(除了SafeZone类型的cover)
        foreach (var coverType in sceneCoverTypes)
        {
            if (coverType == CoverEnum.SafeZone)
            {
                continue;
            }

            if (_integralCoverViews.ContainsKey(coverType))
            {
                _integralCoverViews[coverType].CoverEnabled = false;
            }
            else
            {
                Debug.LogWarning($"CoverSystem: sceneCoverTypes contains {coverType} but it's not defined in integralCoverViews.");
            }
        }

        if (_playerTransform == null)
        {
            Debug.LogError("CoverSystem: Player Transform is not assigned.");
        }
        RebuildCoverCache();
        RefreshCoverState();
    }

    private void Update()
    {
        
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

        if (_integralCoverViews.Count == 0)
        {
            Debug.LogWarning("CoverSystem: No CoverViews assigned for the current scene.");
        }
        else if (sceneCoverTypes.Contains(selectedCoverType) && _integralCoverViews.ContainsKey(selectedCoverType))
        {
            allCovers.Add(_integralCoverViews[selectedCoverType]);
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
        Debug.Log("Refreshing cover state...");
        activeCovers.RemoveWhere(cover => cover == null || !allCovers.Contains(cover) || !cover.CoverEnabled);

        bool previousSafeState = IsPlayerInCover;
        IsPlayerInCover = activeCovers.Count > 0;
        Debug.Log("activeCovers: " + activeCovers.Count);

        if (previousSafeState != IsPlayerInCover)
        {
            if (!IsPlayerInCover && !_suppressDeathEvent)
            {
                EventCenter.Instance.EventTrigger("玩家死亡");
            }
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

    private void OnShiftPressed()
    {
        if (sceneCoverTypes.Contains(selectedCoverType))
        {
            if (selectedCoverType == CoverEnum.SafeZone)
            {
                if (!CollectionSystem.Instance.EnergyConsumable())
                {
                    if (FirstTimeLackEnergy)
                    {
                        // FirstTimeLackEnergy = false;
                        GameSystem.Instance.ShowTip("注意，你的操作需要消耗能量", 3f);
                    }
                    return;
                }
                SafeZoneSystem.Instance.ShiftSafeZoneZoom();
                CollectionSystem.Instance.ConsumeEnergy();
            }
            else if (selectedCoverType == CoverEnum.Scan) 
            {
                // Scan 逻辑: 在Scan状态下仍然能离开安全区
                // TODO: 更多判断
                _integralCoverViews[selectedCoverType].ShiftState();
            }
            else
            {
                if (!CollectionSystem.Instance.EnergyConsumable())
                {
                    if (FirstTimeLackEnergy)
                    {
                        // FirstTimeLackEnergy = false;
                        GameSystem.Instance.ShowTip("注意，你的操作需要消耗能量", 3f);
                    }
                    return;
                }
                CoverView currecntCoverView = _integralCoverViews[selectedCoverType];
                if (currecntCoverView.shiftable)
                {
                    currecntCoverView.ShiftState();
                    CollectionSystem.Instance.ConsumeEnergy();
                }
            }
        }
        else
        {
            Debug.LogWarning("shift按下但是没有对应遮罩: " + selectedCoverType);
        }
    }

    private void OnSelectedCoverIndexChanged(int index)
    {
        if (!CollectionSystem.Instance.EnergyConsumable())
        {
            if (FirstTimeLackEnergy)
            {
                // FirstTimeLackEnergy = false;
                GameSystem.Instance.ShowTip("注意，你的操作需要消耗能量", 3f);
            }
            return;
        }
        if (index <= -1 || index >= sceneCoverTypes.Count)
        {
            Debug.LogError($"CoverSystem: Invalid scene cover view index {index}. It should be between 0 and {sceneCoverTypes.Count - 1}.");
            return;
        }

        if (selectedCoverType == CoverEnum.Scan)
        {
            // 不扫描完成无法切换遮罩
            ScanCoverView scanCoverView = _integralCoverViews[CoverEnum.Scan] as ScanCoverView;
            if (scanCoverView != null && !scanCoverView.scanFinished)
            {
                return;
            }
        }

        if (sceneCoverTypes[index] == CoverEnum.None || sceneCoverTypes[index] == selectedCoverType)
        {
            return;
        }

        // 特判：只有在安全区中才能扫描
        if (sceneCoverTypes[index] == CoverEnum.Scan && !SafeZoneSystem.Instance.IsPlayerInSafeZone())
        {
            Debug.LogWarning("无法选择Scan遮罩，因为玩家不在安全区内");
            return;
        }

        if (selectedCoverType == CoverEnum.SafeZone)
        {
            SafeZoneSystem.Instance.ResetSafeZone();
        }
        else
        {
            if (sceneCoverTypes.Contains(selectedCoverType) && _integralCoverViews.ContainsKey(selectedCoverType))
            {
                CoverView currentCoverView = _integralCoverViews[selectedCoverType];
                if (currentCoverView != null && currentCoverView.shiftable)
                {
                    currentCoverView.CoverEnabled = false;
                }
            }
        }
        selectedCoverType = sceneCoverTypes[index];
        selectedIndex = index;
        RebuildCoverCache();
        CollectionSystem.Instance.ConsumeEnergy();
        if (selectedCoverType == CoverEnum.SafeZone)
        {
            SafeZoneSystem.Instance.ShiftSafeZoneZoom();
        }
        else
        {
            if (sceneCoverTypes.Contains(selectedCoverType) && _integralCoverViews.ContainsKey(selectedCoverType))
            {
                CoverView currentCoverView = _integralCoverViews[selectedCoverType];
                if (currentCoverView != null && currentCoverView.shiftable)
                {
                    currentCoverView.ResetCover();
                    currentCoverView.CoverEnabled = true;
                    if (selectedCoverType == CoverEnum.Scan)
                    {
                        // Scan 逻辑: 切换到Scan状态需要重置扫描状态
                        ScanCoverView scanCoverView = currentCoverView as ScanCoverView;
                        if (scanCoverView != null)
                        {
                            scanCoverView.ShiftState();
                            if (FirstTimeShiftScan)
                            {
                                FirstTimeShiftScan = false;
                                GameSystem.Instance.ShowTip("继续按Shift以扫描", 3f);
                            }
                        }
                    }
                }
            }
        }
        Debug.Log($"选择遮罩序号: {index}, selectedCoverType: {selectedCoverType}");
        RefreshCoverState();
    }

    public void ResetCover(CheckPointData checkPointData)
    {
        bool previousSuppressState = _suppressDeathEvent;
        _suppressDeathEvent = true;

        try
        {
            // 重置所有integral cover
            foreach (var coverView in _integralCoverViews.Values)
            {
                if (coverView != null && coverView.shiftable)
                {
                    coverView.ResetCover();
                    coverView.CoverEnabled = false;
                }
            }

            // 重置所有safezone cover
            foreach (var safeZoneCover in safezoneCoverViews)
            {
                if (safeZoneCover != null)
                {
                    safeZoneCover.ResetCover();
                    safeZoneCover.CoverEnabled = false;
                }
            }

            // 激活检查点对应的遮罩
            if (checkPointData.safeZoneIndex >= 0 && checkPointData.safeZoneIndex < safezoneCoverViews.Count)
            {
                SafeZoneCoverView targetSafeZoneCover = safezoneCoverViews[checkPointData.safeZoneIndex];
                if (targetSafeZoneCover != null)
                {
                    targetSafeZoneCover.CoverEnabled = true;
                }
                if (checkPointData.safeZoneIndex > 0)
                {
                    SafeZoneCoverView previousSafeZoneCover = safezoneCoverViews[checkPointData.safeZoneIndex - 1];
                    if (previousSafeZoneCover != null)
                    {
                        previousSafeZoneCover.CoverEnabled = true;
                    }
                }
            }

            selectedCoverType = CoverEnum.None;
            selectedIndex = -1;
            RebuildCoverCache();
            RefreshCoverState();
        }
        finally
        {
            _suppressDeathEvent = previousSuppressState;
        }
    }

    public void ResetSelectedCover()
    {
        selectedCoverType = CoverEnum.None;
        selectedIndex = -1;
        RebuildCoverCache();
        RefreshCoverState();
    }
}
