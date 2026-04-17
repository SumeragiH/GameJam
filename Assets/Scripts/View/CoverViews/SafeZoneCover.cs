using UnityEngine;

public class SafeZoneCover : CoverView
{
    public override bool IsReady => true;

    override protected void Start()
    {
        base.Start();
        _cooldownTime = 0f;
        _currentCooldown = 0f;
    }

    public override void ActivateCover()
    {
        // SafeZoneCover常驻生效，不需要主动触发。
    }
}
