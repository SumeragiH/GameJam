using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CoverSystem, 包含一个当前场景特定的CoverView, 以及当前场景的SafeZoneCover列表
/// </summary>
public class CoverSystem : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private List<SafeZoneCover> safezoneCoverViews = new List<SafeZoneCover>();
    [SerializeField] private CoverView currentSceneCoverView;

    private Collider2D _playerCollider2D;

    void Start()
    {
        EnsurePlayerCollider();
        ResolveCurrentSceneCover();

        if (safezoneCoverViews.Count == 0)
        {
            safezoneCoverViews.AddRange(FindObjectsOfType<SafeZoneCover>(true));
        }
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

            SafeZoneCover cover = safeZoneView.GetComponentInChildren<SafeZoneCover>(true);
            if (cover != null)
            {
                uniqueCovers.Add(cover);
            }
        }

        safezoneCoverViews.AddRange(uniqueCovers);
    }

    /// <summary>
    /// 检测玩家是否与safezoneCoverViews及currentSceneCoverView碰撞箱发生碰撞，如果发生说明在安全区内，返回true，否则返回false
    /// </summary>
    /// <returns></returns>
    public bool IsInSafeZone()
    {
        if (!EnsurePlayerCollider())
        {
            Debug.LogWarning("CoverSystem: Player collider not found, cannot evaluate safe zone.");
            return false;
        }

        if (IsCollidingWithCover(currentSceneCoverView))
        {
            return true;
        }

        for (int i = 0; i < safezoneCoverViews.Count; i++)
        {
            if (IsCollidingWithCover(safezoneCoverViews[i]))
            {
                return true;
            }
        }

        return false;
    }

    private void ResolveCurrentSceneCover()
    {
        if (currentSceneCoverView != null)
        {
            return;
        }

        CoverView[] allCoverViews = FindObjectsOfType<CoverView>(true);
        for (int i = 0; i < allCoverViews.Length; i++)
        {
            CoverView coverView = allCoverViews[i];
            if (coverView is SafeZoneCover)
            {
                continue;
            }

            currentSceneCoverView = coverView;
            return;
        }
    }

    private bool EnsurePlayerCollider()
    {
        if (_playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }

        if (_playerTransform == null)
        {
            return false;
        }

        if (_playerCollider2D == null)
        {
            _playerCollider2D = _playerTransform.GetComponent<Collider2D>();
        }

        return _playerCollider2D != null;
    }

    private bool IsCollidingWithCover(CoverView coverView)
    {
        if (coverView == null)
        {
            return false;
        }

        if (!TryGetPlayerBounds(out Bounds playerBounds))
        {
            return false;
        }

        if (!TryGetCoverBounds(coverView, out Bounds coverBounds))
        {
            return false;
        }

        return playerBounds.Intersects(coverBounds);
    }

    private bool TryGetPlayerBounds(out Bounds bounds)
    {
        if (_playerCollider2D != null)
        {
            bounds = _playerCollider2D.bounds;
            return true;
        }

        bounds = default;
        return false;
    }

    private bool TryGetCoverBounds(CoverView coverView, out Bounds bounds)
    {
        Collider2D coverCollider2D = coverView.GetComponent<Collider2D>();
        if (coverCollider2D != null)
        {
            bounds = coverCollider2D.bounds;
            return true;
        }

        bounds = default;
        return false;
    }

}
