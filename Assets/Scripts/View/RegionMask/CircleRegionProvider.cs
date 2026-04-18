using UnityEngine;

public class CircleRegionProvider : RegionProviderBase
{
    [SerializeField] private Transform _center;
    [SerializeField, Min(0f)] private float _radiusWorld = 2f;
    [SerializeField] private bool _isZoomed;
    [SerializeField, Min(0.01f)] private float _zoomTransitionDuration = 0.2f;

    private float _zoomedScale = 1f;
    private float _currentScale = 1f;

    public bool IsZoomed
    {
        get => _isZoomed;
        set => _isZoomed = value;
    }

    public void SetZoomScale(float zoomedScale)
    {
        _zoomedScale = Mathf.Max(1f, zoomedScale);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _currentScale = GetTargetScale();
    }

    private void Update()
    {
        float targetScale = GetTargetScale();
        float speed = 1f / Mathf.Max(0.01f, _zoomTransitionDuration);
        _currentScale = Mathf.MoveTowards(_currentScale, targetScale, speed * Time.deltaTime);
    }

    private void OnValidate()
    {
        _zoomedScale = Mathf.Max(1f, _zoomedScale);
        _zoomTransitionDuration = Mathf.Max(0.01f, _zoomTransitionDuration);

        if (!Application.isPlaying)
        {
            _currentScale = GetTargetScale();
        }
    }

    protected override bool TryBuildRegionData(Camera camera, out RegionShaderData data)
    {
        Transform centerTransform = _center != null ? _center : transform;
        Vector3 centerWorld = centerTransform.position;
        Vector3 centerViewport3 = camera.WorldToViewportPoint(centerWorld);

        if (centerViewport3.z < 0f)
        {
            data = default;
            return false;
        }

        float aspect = GetAspectCorrection(camera);
        Vector2 centerViewport = ApplyAspectCorrection(new Vector2(centerViewport3.x, centerViewport3.y), aspect);

        float scaledRadiusWorld = _radiusWorld * _currentScale;
        Vector3 edgeWorld = centerWorld + camera.transform.right * scaledRadiusWorld;
        Vector2 edgeViewport = ApplyAspectCorrection(ToViewport2(camera, edgeWorld), aspect);
        float radiusViewport = (edgeViewport - centerViewport).magnitude;

        if (radiusViewport <= 0f)
        {
            Vector3 fallbackEdgeWorld = centerWorld + camera.transform.up * scaledRadiusWorld;
            Vector2 fallbackEdgeViewport = ApplyAspectCorrection(ToViewport2(camera, fallbackEdgeWorld), aspect);
            radiusViewport = (fallbackEdgeViewport - centerViewport).magnitude;
        }

        if (radiusViewport <= 0f)
        {
            data = default;
            return false;
        }

        data = new RegionShaderData
        {
            ShapeType = RegionShapeType.Circle,
            CenterViewport = centerViewport,
            SizeViewport = new Vector2(radiusViewport, radiusViewport),
            RotationRadians = 0f,
            FeatherViewport = FeatherViewport,
            SkewTangent = 0f
        };
        return true;
    }

    private float GetTargetScale()
    {
        return _isZoomed ? _zoomedScale : 1f;
    }
}
