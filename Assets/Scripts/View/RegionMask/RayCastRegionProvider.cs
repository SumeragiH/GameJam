using UnityEngine;

public class RayCastRegionProvider : RegionProviderBase
{
    [Header("RayCast Region")]
    [Range(0, 2)] [SerializeField] private int _rayIndex = 0;
    [SerializeField, Min(0.05f)] private float _rayRangeViewport = 3f;
    [SerializeField, Range(0.5f, 89f)] private float _rayHalfAngleDegrees = 16f;
    [SerializeField, Min(0f)] private float _screenEdgeInset = 0f;

    [Header("Center Direction (Degrees)")]
    [SerializeField, Range(-180f, 180f)] private float _leftTopCenterAngle = -30f;
    [SerializeField, Range(-180f, 180f)] private float _rightTopCenterAngle = -150f;
    [SerializeField, Range(-180f, 180f)] private float _topCenterCenterAngle = -45f;

    public int RayIndex
    {
        get => _rayIndex;
        set => _rayIndex = Mathf.Clamp(value, 0, 2);
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

    protected override bool TryBuildRegionData(Camera camera, out RegionShaderData data)
    {
        float aspect = GetAspectCorrection(camera);
        Vector2 anchorViewport = EvaluateAnchorViewport(_rayIndex, Mathf.Clamp01(_screenEdgeInset));
        Vector2 center = ApplyAspectCorrection(anchorViewport, aspect);

        float centerAngleDeg = EvaluateCenterAngle(_rayIndex);
        float centerAngleRad = centerAngleDeg * Mathf.Deg2Rad;

        data = new RegionShaderData
        {
            ShapeType = RegionShapeType.Sector,
            CenterViewport = center,
            SizeViewport = new Vector2(
                Mathf.Max(0.05f, _rayRangeViewport),
                Mathf.Clamp(_rayHalfAngleDegrees, 0.5f, 89f) * Mathf.Deg2Rad
            ),
            RotationRadians = centerAngleRad,
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

    private void OnValidate()
    {
        _rayIndex = Mathf.Clamp(_rayIndex, 0, 2);
        _rayRangeViewport = Mathf.Max(0.05f, _rayRangeViewport);
        _rayHalfAngleDegrees = Mathf.Clamp(_rayHalfAngleDegrees, 0.5f, 89f);
        _screenEdgeInset = Mathf.Max(0f, _screenEdgeInset);
    }
}
