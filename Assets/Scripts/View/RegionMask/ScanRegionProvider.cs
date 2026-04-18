using UnityEngine;

public class ScanRegionProvider : RegionProviderBase
{
    [Range(0, 3)] public int LitRegionIndex = 0;
    [SerializeField, Range(-80f, 80f)] private float _skewAngleDegrees;
    [SerializeField, Min(0.01f)] public float RegionChangeDeltaTime = 1.0f;
    [SerializeField, Min(0.05f)] private float _regionWidthScale = 1.0f;
    [SerializeField] private Transform _worldAnchor;

    private int _lastTargetIndex = -1;
    private bool _hasWorldCenter = false;
    private float _worldMoveSpeed = 0.0f;
    private Vector3 _currentWorldCenter;
    private Vector3 _targetWorldCenter;
    private Camera _runtimeCamera;

    protected override void OnEnable()
    {
        base.OnEnable();
        LitRegionIndex = Mathf.Clamp(LitRegionIndex, 0, 3);
        _lastTargetIndex = LitRegionIndex;
        _hasWorldCenter = false;
        _runtimeCamera = null;
    }

    private void Update()
    {
        Camera camera = ResolveRuntimeCamera();
        if (camera == null)
        {
            return;
        }

        InitializeWorldCenterIfNeeded(camera);

        int targetIndex = Mathf.Clamp(LitRegionIndex, 0, 3);
        if (targetIndex != _lastTargetIndex)
        {
            _lastTargetIndex = targetIndex;
            _targetWorldCenter = CalculateWorldTarget(targetIndex, camera);
            _worldMoveSpeed = Vector3.Distance(_currentWorldCenter, _targetWorldCenter) / Mathf.Max(0.01f, RegionChangeDeltaTime);
        }

        if (_worldMoveSpeed > 0f)
        {
            _currentWorldCenter = Vector3.MoveTowards(_currentWorldCenter, _targetWorldCenter, _worldMoveSpeed * Time.deltaTime);
        }
    }

    private void OnValidate()
    {
        LitRegionIndex = Mathf.Clamp(LitRegionIndex, 0, 3);
        RegionChangeDeltaTime = Mathf.Max(0.01f, RegionChangeDeltaTime);
        _regionWidthScale = Mathf.Max(0.05f, _regionWidthScale);

        if (!Application.isPlaying)
        {
            _lastTargetIndex = LitRegionIndex;
            _hasWorldCenter = false;
        }
    }

    protected override void TryShiftState(int stateIndex)
    {
        if (stateIndex >= 0 && stateIndex <= 3)
        {
            LitRegionIndex = stateIndex;
        }
    }

    protected override void ResetRegion()
    {
        LitRegionIndex = 0;
        _lastTargetIndex = 0;
        _hasWorldCenter = false;
        regionEnabled = true;
    }


    protected override bool TryBuildRegionData(Camera camera, out RegionShaderData data)
    {
        float aspect = GetAspectCorrection(camera);
        float fullWidth = aspect;
        float thirdWidth = fullWidth / 3f;
        float baseHalfWidth = thirdWidth * 0.5f;
        float halfWidth = baseHalfWidth * _regionWidthScale;
        InitializeWorldCenterIfNeeded(camera);

        Vector3 centerViewport3 = camera.WorldToViewportPoint(_currentWorldCenter);
        if (centerViewport3.z < 0f)
        {
            data = default;
            return false;
        }

        Vector2 center = ApplyAspectCorrection(new Vector2(centerViewport3.x, centerViewport3.y), aspect);
        Vector2 halfSize = new Vector2(halfWidth, 0.5f);

        data = new RegionShaderData
        {
            ShapeType = RegionShapeType.Box,
            CenterViewport = center,
            SizeViewport = halfSize,
            RotationRadians = 0f,
            FeatherViewport = FeatherViewport,
            SkewTangent = Mathf.Tan(_skewAngleDegrees * Mathf.Deg2Rad)
        };
        return true;
    }

    private Camera ResolveRuntimeCamera()
    {
        if (_runtimeCamera != null)
        {
            return _runtimeCamera;
        }

        _runtimeCamera = Camera.main;
        return _runtimeCamera;
    }

    private void InitializeWorldCenterIfNeeded(Camera camera)
    {
        if (_hasWorldCenter || camera == null)
        {
            return;
        }

        int startIndex = Mathf.Clamp(LitRegionIndex, 0, 3);
        _lastTargetIndex = startIndex;
        _targetWorldCenter = CalculateWorldTarget(startIndex, camera);
        _currentWorldCenter = _targetWorldCenter;
        _hasWorldCenter = true;
        _worldMoveSpeed = 0f;
    }

    private Vector3 CalculateWorldTarget(int index, Camera camera)
    {
        float aspect = GetAspectCorrection(camera);
        float fullWidth = aspect;
        float thirdWidth = fullWidth / 3f;
        float baseHalfWidth = thirdWidth * 0.5f;
        float correctedX = EvaluateCenterX(index, thirdWidth, fullWidth, baseHalfWidth);
        Vector2 viewport = RemoveAspectCorrection(new Vector2(correctedX, 0.5f), aspect);

        Transform anchor = _worldAnchor != null ? _worldAnchor : transform;
        float depth = GetReferenceDepth(camera, anchor.position);
        return camera.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, depth));
    }

    private static float EvaluateCenterX(float slot, float thirdWidth, float fullWidth, float baseHalfWidth)
    {
        slot = Mathf.Clamp(slot, 0.0f, 3.0f);

        if (slot <= 1.0f)
        {
            return Mathf.Lerp(-baseHalfWidth, thirdWidth, slot);
        }

        if (slot <= 2.0f)
        {
            return Mathf.Lerp(thirdWidth, thirdWidth * 2.0f, slot - 1.0f);
        }

        return Mathf.Lerp(thirdWidth * 2.0f, fullWidth + baseHalfWidth, slot - 2.0f);
    }
}
