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

    public float WidthScale
    {
        get => _regionWidthScale;
        set => _regionWidthScale = Mathf.Max(0.05f, value);
    }

    public float MoveDuration
    {
        get => RegionChangeDeltaTime;
        set => RegionChangeDeltaTime = Mathf.Max(0.01f, value);
    }

    public bool IsMoving => _hasWorldCenter && Vector3.Distance(_currentWorldCenter, _targetWorldCenter) > 0.0001f;

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

    public void SetStateIndex(int stateIndex)
    {
        TryShiftState(Mathf.Clamp(stateIndex, 0, 3));
        RefreshTargetImmediatelyIfNeeded();
    }

    public bool TryShiftNextState()
    {
        if (IsMoving || LitRegionIndex >= 3)
        {
            return false;
        }

        SetStateIndex(LitRegionIndex + 1);
        return true;
    }

    private void RefreshTargetImmediatelyIfNeeded()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        Camera camera = ResolveRuntimeCamera();
        if (camera == null)
        {
            return;
        }

        InitializeWorldCenterIfNeeded(camera);

        int targetIndex = Mathf.Clamp(LitRegionIndex, 0, 3);
        if (targetIndex == _lastTargetIndex)
        {
            return;
        }

        _lastTargetIndex = targetIndex;
        _targetWorldCenter = CalculateWorldTarget(targetIndex, camera);
        _worldMoveSpeed = Vector3.Distance(_currentWorldCenter, _targetWorldCenter) / Mathf.Max(0.01f, RegionChangeDeltaTime);
    }

    public bool TryGetWorldGeometry(out Vector3 centerWorld, out float halfWidthWorld)
    {
        Camera camera = ResolveRuntimeCamera();
        if (camera == null)
        {
            centerWorld = default;
            halfWidthWorld = 0f;
            return false;
        }

        InitializeWorldCenterIfNeeded(camera);
        centerWorld = _currentWorldCenter;
        halfWidthWorld = CalculateHalfWidthWorld(camera, centerWorld);
        return halfWidthWorld > 0f;
    }

    public bool TryGetWorldParallelogram(
        out Vector3 centerWorld,
        out Vector3 topLeft,
        out Vector3 topRight,
        out Vector3 bottomRight,
        out Vector3 bottomLeft)
    {
        Camera camera = ResolveRuntimeCamera();
        if (camera == null)
        {
            centerWorld = default;
            topLeft = default;
            topRight = default;
            bottomRight = default;
            bottomLeft = default;
            return false;
        }

        InitializeWorldCenterIfNeeded(camera);
        centerWorld = _currentWorldCenter;

        Vector3 centerViewport3 = camera.WorldToViewportPoint(centerWorld);
        if (centerViewport3.z <= 0f)
        {
            topLeft = default;
            topRight = default;
            bottomRight = default;
            bottomLeft = default;
            return false;
        }

        float aspect = GetAspectCorrection(camera);
        float halfWidth = aspect / 6f * _regionWidthScale;
        float halfHeight = 0.5f;
        float skew = Mathf.Tan(_skewAngleDegrees * Mathf.Deg2Rad);
        Vector2 centerCorrected = ApplyAspectCorrection(new Vector2(centerViewport3.x, centerViewport3.y), aspect);

        Vector2 topLeftCorrected = new Vector2(centerCorrected.x + skew * halfHeight - halfWidth, centerCorrected.y + halfHeight);
        Vector2 topRightCorrected = new Vector2(centerCorrected.x + skew * halfHeight + halfWidth, centerCorrected.y + halfHeight);
        Vector2 bottomRightCorrected = new Vector2(centerCorrected.x - skew * halfHeight + halfWidth, centerCorrected.y - halfHeight);
        Vector2 bottomLeftCorrected = new Vector2(centerCorrected.x - skew * halfHeight - halfWidth, centerCorrected.y - halfHeight);

        topLeft = ViewportToWorld(camera, topLeftCorrected, aspect, centerViewport3.z);
        topRight = ViewportToWorld(camera, topRightCorrected, aspect, centerViewport3.z);
        bottomRight = ViewportToWorld(camera, bottomRightCorrected, aspect, centerViewport3.z);
        bottomLeft = ViewportToWorld(camera, bottomLeftCorrected, aspect, centerViewport3.z);
        return true;
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

    private float CalculateHalfWidthWorld(Camera camera, Vector3 centerWorld)
    {
        Vector3 centerViewport3 = camera.WorldToViewportPoint(centerWorld);
        if (centerViewport3.z <= 0f)
        {
            return 0f;
        }

        float halfWidthViewport = Mathf.Clamp01(_regionWidthScale / 6f);
        Vector3 rightWorld = camera.ViewportToWorldPoint(
            new Vector3(centerViewport3.x + halfWidthViewport, centerViewport3.y, centerViewport3.z)
        );
        return Vector3.Distance(centerWorld, rightWorld);
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

    private static Vector3 ViewportToWorld(Camera camera, Vector2 correctedViewport, float aspect, float depth)
    {
        Vector2 viewport = RemoveAspectCorrection(correctedViewport, aspect);
        return camera.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, depth));
    }
}
