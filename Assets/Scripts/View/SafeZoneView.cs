using UnityEngine;

/// <summary>
/// 默认圆形Sprite, 半径可以自己设置
/// 自带一个CoverView
/// </summary>
public class SafeZoneView : MonoBehaviour
{
    [SerializeField] private SafeZoneCover _coverView;
    [SerializeField] private CircleRegionProvider regionProvider;
    public SafeZoneCover CoverView => _coverView;
    private bool _isActive = true;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value) return;
            _isActive = value;
            regionProvider.enabled = IsActive;
        }
    }

    private void OnValidate()
    {
        if (_coverView == null)
        {
            _coverView = GetComponentInChildren<SafeZoneCover>(true);
        }
    }
}
