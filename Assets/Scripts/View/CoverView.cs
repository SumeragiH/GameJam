using UnityEngine;
using System;
using System.Collections.Generic;


/// <summary>
/// CoverView，代表可以进入的区域，这个可以进入的区域有碰撞箱
/// </summary>
public abstract class CoverView : MonoBehaviour
{
    [SerializeField] protected float _cooldownTime = 5f;
    [SerializeField] private string _playerTag = "Player";

    public float CooldownTime => _cooldownTime;
    protected float _currentCooldown = 0f;
    public virtual bool IsReady => _currentCooldown <= 0f;
    private readonly HashSet<int> _insidePlayerColliderIds = new HashSet<int>();

    public static event Action<CoverView> PlayerEnteredCover;
    public static event Action<CoverView> PlayerExitedCover;

    virtual protected void Start()
    {
    }

    private void FixedUpdate()
    {
        if (_currentCooldown <= _cooldownTime)
        {
            _currentCooldown += Time.fixedDeltaTime;
        }
    }

    private void OnDisable()
    {
        if (_insidePlayerColliderIds.Count <= 0)
        {
            return;
        }

        _insidePlayerColliderIds.Clear();
        PlayerExitedCover?.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryRaiseEnter(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        TryRaiseExit(other);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryRaiseEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
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
            PlayerEnteredCover?.Invoke(this);
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
            PlayerExitedCover?.Invoke(this);
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

    public abstract void ActivateCover();
}
