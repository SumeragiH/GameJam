using UnityEngine;

public class ThirdsRegionProvider : RegionProviderBase
{
    [Range(0, 2)] public int LitRegionIndex = 1;

    protected override bool TryBuildRegionData(Camera camera, out RegionShaderData data)
    {
        float aspect = GetAspectCorrection(camera);
        float fullWidth = aspect;
        float thirdWidth = fullWidth / 3f;
        int index = Mathf.Clamp(LitRegionIndex, 0, 2);

        float centerX = (index + 0.5f) * thirdWidth;
        Vector2 center = new Vector2(centerX, 0.5f);
        Vector2 halfSize = new Vector2(thirdWidth * 0.5f, 0.5f);

        data = new RegionShaderData
        {
            ShapeType = RegionShapeType.Box,
            CenterViewport = center,
            SizeViewport = halfSize,
            RotationRadians = 0f,
            FeatherViewport = FeatherViewport
        };
        return true;
    }
}
