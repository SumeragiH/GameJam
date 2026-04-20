using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChangeRayCastCheckPointLogic : CheckPointLogicView
{
    [Header("Center Direction (Degrees)")]
    [SerializeField, Range(-180f, 180f)] private float _leftTopCenterAngle = -30f;
    [SerializeField, Range(-180f, 180f)] private float _rightTopCenterAngle = -150f;
    [SerializeField, Range(-180f, 180f)] private float _topCenterCenterAngle = -90f;

    [Header("Spread Angle (Degrees)")]
    [SerializeField, Range(1f, 178f)] private float _leftTopSpreadAngle = 30f;
    [SerializeField, Range(1f, 178f)] private float _rightTopSpreadAngle = 30f;
    [SerializeField, Range(1f, 178f)] private float _topCenterSpreadAngle = 65f;

    public override void ReachCheckpoint()
    {
        RayCastCoverView view = CoverSystem.Instance.GetCoverView(CoverEnum.RayCast) as RayCastCoverView;
        if (view != null)
        {
            view.LeftTopCenterAngle = _leftTopCenterAngle;
            view.RightTopCenterAngle = _rightTopCenterAngle;
            view.TopCenterCenterAngle = _topCenterCenterAngle;
            view.LeftTopSpreadAngle = _leftTopSpreadAngle;
            view.RightTopSpreadAngle = _rightTopSpreadAngle;
            view.TopCenterSpreadAngle = _topCenterSpreadAngle;
        }
    }
}