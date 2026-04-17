using UnityEngine;
using System.Collections;

public class SafeZoneCover : CoverView
{
    public override bool IsReady => false; // 不可更新
    public override void ActivateCover()
    {
        throw new System.NotImplementedException("SafeZoneCover does not implement ActivateCover because it is always active.");
    }
}