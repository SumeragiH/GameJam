using UnityEngine;

public class SafeZoneCoverView : CoverView 
{
    override protected void Start()
    {
        base.Start();
    }

    public override void ShiftState()
    {
        // SafeZoneCover常驻生效，不需要shift变化。
    }
}
