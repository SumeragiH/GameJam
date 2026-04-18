using UnityEngine;

public class ScanCoverView : CoverView 
{
    /// <summary>
    /// 0: 屏幕外左边，3：屏幕外右边
    /// </summary>
    [Range(0, 3)]private int currentState = 0;
    override protected void Start()
    {
        base.Start();
    }
                                                                                                                                                                                                                                                               

    public override void ShiftState()
    {
        // ScanCoverView自行变化，不可以被shift改变
    }
}
