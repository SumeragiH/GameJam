using UnityEngine;

public class BoxRegionProvider : RegionProviderBase
{
    [SerializeField] private Transform _center;
    [SerializeField] private Vector2 _halfSizeWorld = new Vector2(2f, 1f);
    [SerializeField] private float _rotationOffsetDegrees = 0f;

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
        Quaternion rotation = centerTransform.rotation * Quaternion.Euler(0f, 0f, _rotationOffsetDegrees);
        Vector3 rightWorld = rotation * Vector3.right * _halfSizeWorld.x;
        Vector3 upWorld = rotation * Vector3.up * _halfSizeWorld.y;

        Vector2 centerViewport = ApplyAspectCorrection(new Vector2(centerViewport3.x, centerViewport3.y), aspect);
        Vector2 rightViewport = ApplyAspectCorrection(ToViewport2(camera, centerWorld + rightWorld), aspect) - centerViewport;
        Vector2 upViewport = ApplyAspectCorrection(ToViewport2(camera, centerWorld + upWorld), aspect) - centerViewport;

        float halfWidthViewport = rightViewport.magnitude;
        float halfHeightViewport = upViewport.magnitude;

        if (halfWidthViewport <= 0f || halfHeightViewport <= 0f)
        {
            data = default;
            return false;
        }

        data = new RegionShaderData
        {
            ShapeType = RegionShapeType.Box,
            CenterViewport = centerViewport,
            SizeViewport = new Vector2(halfWidthViewport, halfHeightViewport),
            RotationRadians = Mathf.Atan2(rightViewport.y, rightViewport.x),
            FeatherViewport = FeatherViewport
        };
        return true;
    }
}
