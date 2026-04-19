using UnityEngine;

public class RingRegionProvide : RegionProviderBase
{
    public enum ScreenCorner
    {
        TopLeft = 0,
        TopRight = 1,
        BottomLeft = 2,
        BottomRight = 3
    }

    [Header("Ring Anchor")]
    [SerializeField] private ScreenCorner _screenCorner = ScreenCorner.TopLeft;
    [SerializeField, Min(0f)] private float _cornerInsetViewport = 0f;

    [Header("Ring State")]
    [Range(0, 2)] [SerializeField] private int _ringIndex = 0;
    [SerializeField, Min(0.01f)] private float _stateChangeDuration = 0.35f;

    [Header("Inner Radius by Index (Viewport Corrected)")]
    [SerializeField, Min(0.01f)] private float _innerRadiusIndex0 = 0.15f;
    [SerializeField, Min(0.01f)] private float _innerRadiusIndex1 = 0.3f;
    [SerializeField, Min(0.01f)] private float _innerRadiusIndex2 = 0.45f;

    [Header("Thickness Controls")]
    [SerializeField, Min(0.01f)] private float _thicknessBaseViewport = 0.18f;
    [SerializeField] private float _thicknessGrowthByInnerRadius = 0.25f;

    private bool _hasCurrentInnerRadius = false;
    private float _currentInnerRadius = 0f;
    private float _targetInnerRadius = 0f;
    private float _innerRadiusMoveSpeed = 0f;

    protected override void OnEnable()
    {
        base.OnEnable();
        _ringIndex = Mathf.Clamp(_ringIndex, 0, 2);
        _hasCurrentInnerRadius = false;
    }

    private void Update()
    {
        float desiredInnerRadius = EvaluateInnerRadiusByIndex(_ringIndex);
        if (!_hasCurrentInnerRadius)
        {
            _currentInnerRadius = desiredInnerRadius;
            _targetInnerRadius = desiredInnerRadius;
            _innerRadiusMoveSpeed = 0f;
            _hasCurrentInnerRadius = true;
            return;
        }

        if (!Mathf.Approximately(desiredInnerRadius, _targetInnerRadius))
        {
            _targetInnerRadius = desiredInnerRadius;
            _innerRadiusMoveSpeed = Mathf.Abs(_targetInnerRadius - _currentInnerRadius) / Mathf.Max(0.01f, _stateChangeDuration);
        }

        if (_innerRadiusMoveSpeed > 0f)
        {
            _currentInnerRadius = Mathf.MoveTowards(_currentInnerRadius, _targetInnerRadius, _innerRadiusMoveSpeed * Time.deltaTime);
        }
    }

    protected override void TryShiftState(int stateIndex)
    {
        _ringIndex = Mathf.Clamp(stateIndex, 0, 2);
    }

    protected override void ResetRegion()
    {
        _ringIndex = 0;
        _hasCurrentInnerRadius = false;
        regionEnabled = true;
    }

    public void SetStateIndex(int stateIndex)
    {
        TryShiftState(stateIndex);
    }

    public bool TryShiftNextState()
    {
        if (_ringIndex >= 2)
        {
            return false;
        }

        _ringIndex++;
        return true;
    }

    protected override bool TryBuildRegionData(Camera camera, out RegionShaderData data)
    {
        float aspect = GetAspectCorrection(camera);
        Vector2 centerViewport = EvaluateAnchorViewport(_screenCorner, Mathf.Clamp01(_cornerInsetViewport));
        Vector2 centerCorrected = ApplyAspectCorrection(centerViewport, aspect);

        if (!_hasCurrentInnerRadius)
        {
            _currentInnerRadius = EvaluateInnerRadiusByIndex(_ringIndex);
            _targetInnerRadius = _currentInnerRadius;
            _innerRadiusMoveSpeed = 0f;
            _hasCurrentInnerRadius = true;
        }

        float innerRadius = Mathf.Max(0.01f, _currentInnerRadius);
        float thickness = Mathf.Max(0.01f, _thicknessBaseViewport + innerRadius * _thicknessGrowthByInnerRadius);
        float outerRadius = innerRadius + thickness;

        data = new RegionShaderData
        {
            ShapeType = RegionShapeType.Ring,
            CenterViewport = centerCorrected,
            SizeViewport = new Vector2(innerRadius, outerRadius),
            RotationRadians = 0f,
            FeatherViewport = FeatherViewport,
            SkewTangent = 0f
        };
        return true;
    }

    private float EvaluateInnerRadiusByIndex(int index)
    {
        switch (Mathf.Clamp(index, 0, 2))
        {
            case 0:
                return Mathf.Max(0.01f, _innerRadiusIndex0);
            case 1:
                return Mathf.Max(0.01f, _innerRadiusIndex1);
            default:
                return Mathf.Max(0.01f, _innerRadiusIndex2);
        }
    }

    private static Vector2 EvaluateAnchorViewport(ScreenCorner corner, float inset)
    {
        float min = inset;
        float max = 1f - inset;
        switch (corner)
        {
            case ScreenCorner.TopLeft:
                return new Vector2(min, max);
            case ScreenCorner.TopRight:
                return new Vector2(max, max);
            case ScreenCorner.BottomLeft:
                return new Vector2(min, min);
            default:
                return new Vector2(max, min);
        }
    }

    private void OnValidate()
    {
        _ringIndex = Mathf.Clamp(_ringIndex, 0, 2);
        _cornerInsetViewport = Mathf.Max(0f, _cornerInsetViewport);
        _stateChangeDuration = Mathf.Max(0.01f, _stateChangeDuration);
        _innerRadiusIndex0 = Mathf.Max(0.01f, _innerRadiusIndex0);
        _innerRadiusIndex1 = Mathf.Max(0.01f, _innerRadiusIndex1);
        _innerRadiusIndex2 = Mathf.Max(0.01f, _innerRadiusIndex2);
        _thicknessBaseViewport = Mathf.Max(0.01f, _thicknessBaseViewport);
        if (!Application.isPlaying)
        {
            _hasCurrentInnerRadius = false;
        }
    }
}
