using UnityEngine;

public class RayCastCoverView : CoverView
{
    [Header("Bindings")]
    [SerializeField] private RayCastRegionProvider _rayCastRegionProvider;
    [SerializeField] private PolygonCollider2D _rayCastCollider;
    [SerializeField] private Camera _targetCamera;

    [Header("RayCast Controls")]
    [Range(0, 2)] [SerializeField] private int _rayIndex = 0;
    [SerializeField, Min(0.05f)] private float _rayRangeViewport = 3f;
    [SerializeField, Min(0f)] private float _screenEdgeInset = 0f;

    [Header("Center Direction (Degrees)")]
    [SerializeField, Range(-180f, 180f)] private float _leftTopCenterAngle = -30f;
    [SerializeField, Range(-180f, 180f)] private float _rightTopCenterAngle = -150f;
    [SerializeField, Range(-180f, 180f)] private float _topCenterCenterAngle = -90f;

    [Header("Spread Angle (Degrees)")]
    [SerializeField, Range(1f, 178f)] private float _leftTopSpreadAngle = 30f;
    [SerializeField, Range(1f, 178f)] private float _rightTopSpreadAngle = 30f;
    [SerializeField, Range(1f, 178f)] private float _topCenterSpreadAngle = 65f;

    [Header("Collider Sync")]
    [SerializeField, Range(3, 64)] private int _arcSegments = 24;
    [SerializeField] private bool _syncColliderToRegion = true;

    public float LeftTopCenterAngle
    {
        get => _leftTopCenterAngle;
        set => _leftTopCenterAngle = value;
    }
    public float RightTopCenterAngle
    {
        get => _rightTopCenterAngle;
        set => _rightTopCenterAngle = value;
    }
    public float TopCenterCenterAngle
    {
        get => _topCenterCenterAngle;
        set => _topCenterCenterAngle = value;
    }
    public float LeftTopSpreadAngle
    {
        get => _leftTopSpreadAngle;
        set => _leftTopSpreadAngle = value;
    }
    public float RightTopSpreadAngle
    {
        get => _rightTopSpreadAngle;
        set => _rightTopSpreadAngle = value;
    }
    public float TopCenterSpreadAngle
    {
        get => _topCenterSpreadAngle;
        set => _topCenterSpreadAngle = value;
    }


    private Vector2[] _sectorPathPoints;
    private Camera _runtimeCamera;
    private bool _hasDepth;
    private float _referenceDepth;

    protected override void Start()
    {
        base.Start();
        ResolveBindings();
        ApplyControlToRegion();
        SyncColliderToRegion();
    }

    private void LateUpdate()
    {
        if (_rayCastRegionProvider == null || _rayCastCollider == null || !_syncColliderToRegion)
        {
            return;
        }

        ApplyControlToRegion();
        SyncColliderToRegion();
    }

    public override void ShiftState()
    {
        _rayIndex = (_rayIndex + 1) % 3;
        ApplyControlToRegion();
        SyncColliderToRegion();
    }

    public override void ResetCover()
    {
        _rayIndex = 0;
        ApplyControlToRegion();
        SyncColliderToRegion();
        CoverEnabled = true;
    }

    protected override void OnCoverEnabled()
    {
        base.OnCoverEnabled();
        if (_rayCastRegionProvider != null)
        {
            _rayCastRegionProvider.regionEnabled = true;
        }
    }

    protected override void OnCoverDisable()
    {
        base.OnCoverDisable();
        if (_rayCastRegionProvider != null)
        {
            _rayCastRegionProvider.regionEnabled = false;
        }
    }

    private void ResolveBindings()
    {
        if (_rayCastRegionProvider == null)
        {
            _rayCastRegionProvider = GetComponent<RayCastRegionProvider>();
        }

        if (_rayCastCollider == null)
        {
            _rayCastCollider = GetComponent<PolygonCollider2D>();
        }
    }

    private void ApplyControlToRegion()
    {
        if (_rayCastRegionProvider == null)
        {
            return;
        }

        _rayCastRegionProvider.SetStateIndex(_rayIndex);
        _rayCastRegionProvider.RayRangeViewport = _rayRangeViewport;
        _rayCastRegionProvider.ScreenEdgeInset = _screenEdgeInset;
        _rayCastRegionProvider.LeftTopCenterAngle = _leftTopCenterAngle;
        _rayCastRegionProvider.RightTopCenterAngle = _rightTopCenterAngle;
        _rayCastRegionProvider.TopCenterCenterAngle = _topCenterCenterAngle;
        _rayCastRegionProvider.LeftTopSpreadAngle = _leftTopSpreadAngle;
        _rayCastRegionProvider.RightTopSpreadAngle = _rightTopSpreadAngle;
        _rayCastRegionProvider.TopCenterSpreadAngle = _topCenterSpreadAngle;
    }

    private void SyncColliderToRegion()
    {
        Camera camera = ResolveRuntimeCamera();
        if (camera == null || _rayCastRegionProvider == null || _rayCastCollider == null)
        {
            return;
        }

        if (!_rayCastRegionProvider.TryGetScreenSpaceSectorData(
                camera,
                out Vector2 centerCorrectedViewport,
                out float rangeCorrectedViewport,
                out float halfAngleRadians,
                out float directionAngleRadians))
        {
            return;
        }

        int segments = Mathf.Clamp(_arcSegments, 3, 64);
        EnsureSectorPathBuffer(segments + 2);

        float aspect = GetAspectCorrection(camera);
        float depth = GetReferenceDepth(camera);
        Vector2 centerViewport = RemoveAspectCorrection(centerCorrectedViewport, aspect);
        Vector3 centerWorld = camera.ViewportToWorldPoint(new Vector3(centerViewport.x, centerViewport.y, depth));
        transform.position = centerWorld;

        _sectorPathPoints[0] = Vector2.zero;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = directionAngleRadians - halfAngleRadians + (halfAngleRadians * 2f) * t;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 arcCorrectedViewport = centerCorrectedViewport + direction * rangeCorrectedViewport;
            Vector2 arcViewport = RemoveAspectCorrection(arcCorrectedViewport, aspect);
            Vector3 arcWorld = camera.ViewportToWorldPoint(new Vector3(arcViewport.x, arcViewport.y, depth));
            _sectorPathPoints[i + 1] = transform.InverseTransformPoint(arcWorld);
        }

        _rayCastCollider.SetPath(0, _sectorPathPoints);
    }

    private void EnsureSectorPathBuffer(int size)
    {
        if (_sectorPathPoints == null || _sectorPathPoints.Length != size)
        {
            _sectorPathPoints = new Vector2[size];
        }
    }

    private Camera ResolveRuntimeCamera()
    {
        if (_targetCamera != null)
        {
            return _targetCamera;
        }

        if (_runtimeCamera != null)
        {
            return _runtimeCamera;
        }

        _runtimeCamera = Camera.main;
        return _runtimeCamera;
    }

    private float GetReferenceDepth(Camera camera)
    {
        if (_hasDepth)
        {
            return _referenceDepth;
        }

        _referenceDepth = Vector3.Dot(transform.position - camera.transform.position, camera.transform.forward);
        _referenceDepth = Mathf.Max(_referenceDepth, camera.nearClipPlane + 0.01f);
        _hasDepth = true;
        return _referenceDepth;
    }

    private static float GetAspectCorrection(Camera camera)
    {
        if (camera == null || camera.pixelHeight <= 0)
        {
            return 1f;
        }

        return (float)camera.pixelWidth / camera.pixelHeight;
    }

    private static Vector2 RemoveAspectCorrection(Vector2 correctedViewport, float aspect)
    {
        if (Mathf.Abs(aspect) <= 0.00001f)
        {
            return correctedViewport;
        }

        return new Vector2(correctedViewport.x / aspect, correctedViewport.y);
    }

    private void OnValidate()
    {
        _rayIndex = Mathf.Clamp(_rayIndex, 0, 2);
        _rayRangeViewport = Mathf.Max(0.05f, _rayRangeViewport);
        _screenEdgeInset = Mathf.Max(0f, _screenEdgeInset);
        _leftTopCenterAngle = Mathf.Clamp(_leftTopCenterAngle, -180f, 180f);
        _rightTopCenterAngle = Mathf.Clamp(_rightTopCenterAngle, -180f, 180f);
        _topCenterCenterAngle = Mathf.Clamp(_topCenterCenterAngle, -180f, 180f);
        _leftTopSpreadAngle = Mathf.Clamp(_leftTopSpreadAngle, 1f, 178f);
        _rightTopSpreadAngle = Mathf.Clamp(_rightTopSpreadAngle, 1f, 178f);
        _topCenterSpreadAngle = Mathf.Clamp(_topCenterSpreadAngle, 1f, 178f);
        _arcSegments = Mathf.Clamp(_arcSegments, 3, 64);

        ResolveBindings();
        ApplyControlToRegion();
        if (!Application.isPlaying)
        {
            _hasDepth = false;
            SyncColliderToRegion();
        }
    }
}
