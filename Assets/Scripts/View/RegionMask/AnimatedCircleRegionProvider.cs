using UnityEngine;

public class AnimatedCircleRegionProvider : RegionProviderBase
{
    [Header("Center")]
    [SerializeField] private Transform _center;

    [Header("Radius (World Units)")]
    [SerializeField, Min(0f)] private float _baseRadiusWorld = 2f;
    [SerializeField, Min(0f)] private float _amplitudeWorld = 0.5f;
    [SerializeField, Min(0f)] private float _frequency = 1f;
    [SerializeField] private float _phaseOffset;

    [Header("Time Source")]
    [SerializeField] private bool _useUnscaledTime;
    [SerializeField] private bool _useExternalTimeParameter;
    [SerializeField] private float _externalTimeParameter;

    public void SetExternalTimeParameter(float value)
    {
        _externalTimeParameter = value;
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

        float time = _useExternalTimeParameter
            ? _externalTimeParameter
            : (_useUnscaledTime ? Time.unscaledTime : Time.time);

        float oscillation = Mathf.Sin((time * _frequency + _phaseOffset) * Mathf.PI * 2f);
        float radiusWorld = Mathf.Max(0f, _baseRadiusWorld + oscillation * _amplitudeWorld);

        if (radiusWorld <= 0f)
        {
            data = default;
            return false;
        }

        Vector3 edgeWorld = centerWorld + camera.transform.right * radiusWorld;
        Vector2 edgeViewport = ApplyAspectCorrection(ToViewport2(camera, edgeWorld), aspect);
        float radiusViewport = (edgeViewport - centerViewport).magnitude;

        if (radiusViewport <= 0f)
        {
            Vector3 fallbackEdgeWorld = centerWorld + camera.transform.up * radiusWorld;
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
            FeatherViewport = FeatherViewport
        };
        return true;
    }
}
