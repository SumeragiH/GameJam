using UnityEngine;
using System.Collections;

public class SafeZoneCover : CoverView
{
    public override bool IsReady => false; // 不可更新

    override protected void Start()
    {
        base.Start();
        _cooldownTime = 0;
    }

    public override void ActivateCover()
    {
        throw new System.NotImplementedException("SafeZoneCover does not implement ActivateCover because it is always active.");
    }
}