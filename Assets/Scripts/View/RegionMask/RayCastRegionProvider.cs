using UnityEngine;

public class RayCastRegionProvider : RegionProviderBase
{
    private int _rayIndex = 0;
    private float _rayRangeViewport = 3f;
    private float _screenEdgeInset = 0f;

    private float _leftTopCenterAngle = -30f;
    private float _rightTopCenterAngle = -150f;
    private float _topCenterCenterAngle = -90f;

    private float _leftTopSpreadAngle = 30f;
    private float _rightTopSpreadAngle = 30f;
    private float _topCenterSpreadAngle = 45f;

    public int RayIndex
    {
        get => _rayIndex;
        set => _rayIndex = Mathf.Clamp(value, 0, 2);
    }

    public float RayRangeViewport
    {
        get => _rayRangeViewport;
        set => _rayRangeViewport = Mathf.Max(0.05f, value);
    }

    public float ScreenEdgeInset
    {
        get => _screenEdgeInset;
        set => _screenEdgeInset = Mathf.Max(0f, value);
    }

    public float LeftTopCenterAngle
    {
        get => _leftTopCenterAngle;
        set => _leftTopCenterAngle = Mathf.Clamp(value, -180f, 180f);
    }

    public float RightTopCenterAngle
    {
        get => _rightTopCenterAngle;
        set => _rightTopCenterAngle = Mathf.Clamp(value, -180f, 180f);
    }

    public float TopCenterCenterAngle
    {
        get => _topCenterCenterAngle;
        set => _topCenterCenterAngle = Mathf.Clamp(value, -180f, 180f);
    }

    public float LeftTopSpreadAngle
    {
        get => _leftTopSpreadAngle;
        set => _leftTopSpreadAngle = Mathf.Clamp(value, 1f, 178f);
    }

    public float RightTopSpreadAngle
    {
        get => _rightTopSpreadAngle;
        set => _rightTopSpreadAngle = Mathf.Clamp(value, 1f, 178f);
    }

    public float TopCenterSpreadAngle
    {
        get => _topCenterSpreadAngle;
        set => _topCenterSpreadAngle = Mathf.Clamp(value, 1f, 178f);
    }

    protected override void TryShiftState(int stateIndex)
    {
        RayIndex = stateIndex;
    }

    public void SetStateIndex(int stateIndex)
    {
        TryShiftState(stateIndex);
    }

    protected override void ResetRegion()
    {
        RayIndex = 0;
        regionEnabled = true;
    }

    public bool TryShiftNextState()
    {
        if (_rayIndex >= 2)
        {
            return false;
        }

        _rayIndex++;
        return true;
    }

    public bool TryGetScreenSpaceSectorData(
        Camera camera,
        out Vector2 centerCorrectedViewport,
        out float rangeCorrectedViewport,
        out float halfAngleRadians,
        out float directionAngleRadians)
    {
        if (camera == null)
        {
            centerCorrectedViewport = default;
            rangeCorrectedViewport = 0f;
            halfAngleRadians = 0f;
            directionAngleRadians = 0f;
            return false;
        }

        float aspect = GetAspectCorrection(camera);
        Vector2 anchorViewport = EvaluateAnchorViewport(_rayIndex, Mathf.Clamp01(_screenEdgeInset));
        centerCorrectedViewport = ApplyAspectCorrection(anchorViewport, aspect);
        rangeCorrectedViewport = Mathf.Max(0.05f, _rayRangeViewport);
        halfAngleRadians = Mathf.Clamp(EvaluateSpreadAngle(_rayIndex) * 0.5f, 0.5f, 89f) * Mathf.Deg2Rad;
        directionAngleRadians = EvaluateCenterAngle(_rayIndex) * Mathf.Deg2Rad;
        return true;
    }

    protected override bool TryBuildRegionData(Camera camera, out RegionShaderData data)
    {
        if (!TryGetScreenSpaceSectorData(
                camera,
                out Vector2 center,
                out float range,
                out float halfAngle,
                out float directionAngle))
        {
            data = default;
            return false;
        }

        data = new RegionShaderData
        {
            ShapeType = RegionShapeType.Sector,
            CenterViewport = center,
            SizeViewport = new Vector2(range, halfAngle),
            RotationRadians = directionAngle,
            FeatherViewport = FeatherViewport,
            SkewTangent = 0f
        };
        return true;
    }

    private static Vector2 EvaluateAnchorViewport(int index, float inset)
    {
        float min = inset;
        float max = 1f - inset;
        switch (Mathf.Clamp(index, 0, 2))
        {
            case 0:
                return new Vector2(min, max);
            case 1:
                return new Vector2(max, max);
            default:
                return new Vector2(0.5f, max);
        }
    }

    private float EvaluateCenterAngle(int index)
    {
        switch (Mathf.Clamp(index, 0, 2))
        {
            case 0:
                return _leftTopCenterAngle;
            case 1:
                return _rightTopCenterAngle;
            default:
                return _topCenterCenterAngle;
        }
    }

    private float EvaluateSpreadAngle(int index)
    {
        switch (Mathf.Clamp(index, 0, 2))
        {
            case 0:
                return _leftTopSpreadAngle;
            case 1:
                return _rightTopSpreadAngle;
            default:
                return _topCenterSpreadAngle;
        }
    }

    private void OnValidate()
    {
        _rayIndex = Mathf.Clamp(_rayIndex, 0, 2);
        _rayRangeViewport = Mathf.Max(0.05f, _rayRangeViewport);
        _leftTopSpreadAngle = Mathf.Clamp(_leftTopSpreadAngle, 1f, 178f);
        _rightTopSpreadAngle = Mathf.Clamp(_rightTopSpreadAngle, 1f, 178f);
        _topCenterSpreadAngle = Mathf.Clamp(_topCenterSpreadAngle, 1f, 178f);
        _screenEdgeInset = Mathf.Max(0f, _screenEdgeInset);
    }
}
