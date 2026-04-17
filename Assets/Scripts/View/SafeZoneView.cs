using UnityEngine;

/// <summary>
/// 默认圆形Sprite, 半径可以自己设置
/// 自带一个CoverView
/// </summary>
public class SafeZoneView : MonoBehaviour
{
    [SerializeField] private SafeZoneCover _coverView;
    public SafeZoneCover CoverView => _coverView;
    public bool isActive { get; private set; } = true;

    private void OnValidate()
    {
        if (_coverView == null)
        {
            _coverView = GetComponentInChildren<SafeZoneCover>(true);
        }
    }
}
