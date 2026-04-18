using UnityEngine;

public enum RegionShapeType
{
    Circle = 0,
    Box = 1
}

public struct RegionShaderData
{
    public RegionShapeType ShapeType;
    public Vector2 CenterViewport;
    public Vector2 SizeViewport;
    public float RotationRadians;
    public float FeatherViewport;
}

public interface IRegionMaskProvider
{
    bool IsProviderEnabled { get; }
    int RegionGroup { get; }
    bool TryGetRegionData(Camera camera, out RegionShaderData data);
}
