using UnityEngine;

public class CircleRegionProvider : RegionProviderBase
{
    [SerializeField] private Transform _center;
    [SerializeField, Min(0f)] private float _radiusWorld = 2f;

    private bool _enabled = true;

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

        Vector3 edgeWorld = centerWorld + camera.transform.right * _radiusWorld;
        Vector2 edgeViewport = ApplyAspectCorrection(ToViewport2(camera, edgeWorld), aspect);
        float radiusViewport = (edgeViewport - centerViewport).magnitude;

        if (radiusViewport <= 0f)
        {
            Vector3 fallbackEdgeWorld = centerWorld + camera.transform.up * _radiusWorld;
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
