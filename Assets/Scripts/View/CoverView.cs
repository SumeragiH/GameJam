using UnityEngine;
using System.Collections;

/// <summary>
/// CoverView，代表可以进入的区域，这个可以进入的区域有碰撞箱
/// </summary>
public abstract class CoverView : MonoBehaviour
{
    [SerializeField] protected float _cooldownTime = 5f;
    public float CooldownTime => _cooldownTime;
    protected float _currentCooldown = 0f;
    public virtual bool IsReady => _currentCooldown <= 0f;

    virtual protected void Start()
    {
    }

    void FixedUpdate()
    {
        if (_currentCooldown <= _cooldownTime)
            _currentCooldown += Time.fixedDeltaTime;
    }

    public abstract void ActivateCover();
}