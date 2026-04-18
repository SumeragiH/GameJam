using UnityEngine;
using System;
using System.Collections.Generic;


/// <summary>
/// CoverView，代表可以进入的区域，这个可以进入的区域有碰撞箱
/// </summary>
public abstract class CoverView : MonoBehaviour
{
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private RegionProviderBase regionProvider;
    [SerializeField] private bool coverEnabled = true;
    [SerializeField] private bool shiftable = false;

    public bool CoverEnabled
    {
        get => coverEnabled;
        set
        {
            if (coverEnabled == value) return;
            coverEnabled = value;
            if (coverEnabled)
            {
                OnCoverEnabled();
            }
            else
            {
                OnCoverDisable();
            }
        }
    }

    private readonly HashSet<int> _insidePlayerColliderIds = new HashSet<int>();
    private readonly List<Collider2D> _selfColliders2D = new List<Collider2D>();
    private readonly List<Collider2D> _overlapResults2D = new List<Collider2D>();

    virtual protected void Start()
    {
    }

    protected virtual void OnCoverEnabled()
    {
        if (regionProvider != null)
        {
            regionProvider.regionEnabled = true;
        }

        SyncCurrentPlayerOverlaps2D();
    }

    protected virtual void OnCoverDisable()
    {
        if (_insidePlayerColliderIds.Count > 0)
        {
            _insidePlayerColliderIds.Clear();
            EventCenter.Instance.EventTrigger<CoverView>("玩家离开遮罩", this);
        }

        if (regionProvider != null)
        {
            regionProvider.regionEnabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (coverEnabled)
            TryRaiseEnter(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (coverEnabled)
            TryRaiseExit(other);
    }

    private void TryRaiseEnter(Component other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        int colliderId = other.GetInstanceID();
        if (_insidePlayerColliderIds.Add(colliderId) && _insidePlayerColliderIds.Count == 1)
        {
            EventCenter.Instance.EventTrigger<CoverView>("玩家进入遮罩", this);
        }
    }

    private void TryRaiseExit(Component other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        int colliderId = other.GetInstanceID();
        if (_insidePlayerColliderIds.Remove(colliderId) && _insidePlayerColliderIds.Count == 0)
        {
            EventCenter.Instance.EventTrigger<CoverView>("玩家离开遮罩", this);
        }
    }

    /// <summary>
    /// ai coding: 检测与玩家碰撞箱重叠
    /// </summary>
    private void SyncCurrentPlayerOverlaps2D()
    {
        GetComponents(_selfColliders2D);
        ContactFilter2D filter = default;
        filter.NoFilter();

        for (int i = 0; i < _selfColliders2D.Count; i++)
        {
            Collider2D selfCollider = _selfColliders2D[i];
            if (selfCollider == null || !selfCollider.enabled)
            {
                continue;
            }

            _overlapResults2D.Clear();
            int overlapCount = selfCollider.OverlapCollider(filter, _overlapResults2D);
            for (int j = 0; j < overlapCount && j < _overlapResults2D.Count; j++)
            {
                Collider2D other = _overlapResults2D[j];
                if (other == null || other == selfCollider)
                {
                    continue;
                }

                TryRaiseEnter(other);
            }
        }
    }

    private bool IsPlayerCollider(Component other)
    {
        if (other == null || other.gameObject == null)
        {
            return false;
        }

        if (other.CompareTag(_playerTag))
        {
            return true;
        }

        if (other is Collider2D collider2D && collider2D.attachedRigidbody != null && collider2D.attachedRigidbody.CompareTag(_playerTag))
        {
            return true;
        }

        if (other is Collider collider3D && collider3D.attachedRigidbody != null && collider3D.attachedRigidbody.CompareTag(_playerTag))
        {
            return true;
        }

        return other.transform.root != null && other.transform.root.CompareTag(_playerTag);
    }

    public abstract void ShiftState();
}
