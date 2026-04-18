using UnityEngine;

public class ThirdsRegionProvider : RegionProviderBase
{
    [Range(0, 3)] public int LitRegionIndex = 0;
    [SerializeField, Range(-80f, 80f)] private float _skewAngleDegrees;
    [SerializeField, Min(0.01f)] public float RegionChangeDeltaTime = 1.0f;
    [SerializeField, Min(0.05f)] private float _regionWidthScale = 1.0f;

    private float _currentRegionPosition = 0.0f;

    protected override void OnEnable()
    {
        base.OnEnable();
        _currentRegionPosition = Mathf.Clamp(LitRegionIndex, 0, 3);
    }

    private void Update()
    {
        float speed = 1.0f / Mathf.Max(0.01f, RegionChangeDeltaTime);
        float target = Mathf.Clamp(LitRegionIndex, 0, 3);
        _currentRegionPosition = Mathf.MoveTowards(_currentRegionPosition, target, speed * Time.deltaTime);
    }

    private void OnValidate()
    {
        LitRegionIndex = Mathf.Clamp(LitRegionIndex, 0, 3);
        RegionChangeDeltaTime = Mathf.Max(0.01f, RegionChangeDeltaTime);
        _regionWidthScale = Mathf.Max(0.05f, _regionWidthScale);

        if (!Application.isPlaying)
        {
            _currentRegionPosition = LitRegionIndex;
        }
    }

    protected override void TryShiftNext()
    {
        if (LitRegionIndex < 3)
        {
            LitRegionIndex += 1;
        }
        else
        {
            regionEnabled = false;
        }
    }

    protected override void ResetRegion()
    {
        LitRegionIndex = 0;
        _currentRegionPosition = 0.0f;
        regionEnabled = true;
    }


    protected override bool TryBuildRegionData(Camera camera, out RegionShaderData data)
    {
        float aspect = GetAspectCorrection(camera);
        float fullWidth = aspect;
        float thirdWidth = fullWidth / 3f;
        float baseHalfWidth = thirdWidth * 0.5f;
        float halfWidth = baseHalfWidth * _regionWidthScale;
        float centerX = EvaluateCenterX(_currentRegionPosition, thirdWidth, fullWidth, baseHalfWidth);
        Vector2 center = new Vector2(centerX, 0.5f);
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
