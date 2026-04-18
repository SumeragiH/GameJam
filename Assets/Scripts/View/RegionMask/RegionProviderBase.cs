using UnityEngine;
using UnityEngine.Serialization;

public abstract class RegionProviderBase : MonoBehaviour, IRegionMaskProvider
{
    [SerializeField, FormerlySerializedAs("_providerEnabled")] public bool regionEnabled = true;
    [SerializeField, Range(0, 31)] private int _regionGroup = 0;
    [SerializeField, Min(0f)] private float _featherViewport = 0f;

    public bool IsProviderEnabled => regionEnabled && isActiveAndEnabled;
    public int RegionGroup => Mathf.Clamp(_regionGroup, 0, 31);
    protected float FeatherViewport => Mathf.Max(0f, _featherViewport);

    protected virtual void OnEnable()
    {
        RegionMaskManager.RegisterProvider(this);
    }

    protected virtual void OnDisable()
    {
        RegionMaskManager.UnregisterProvider(this);
    }

    /// <summary>
    /// 使用shift操控遮罩为下一个类型的函数，返回是否有下一个类型（成功转换为下一个类型）
    /// </summary>
    /// <returns></returns>
    protected virtual void TryShiftNext()
    {
    }

    /// <summary>
    /// 重制region shader
    /// </summary>
    /// <returns></returns>
    protected virtual void ResetRegion()
    {
    }

    public bool TryGetRegionData(Camera camera, out RegionShaderData data)
    {
        if (!IsProviderEnabled || camera == null)
        {
            data = default;
            return false;
        }

        return TryBuildRegionData(camera, out data);
    }

    protected static float GetAspectCorrection(Camera camera)
    {
        if (camera == null || camera.pixelHeight <= 0)
        {
            return 1f;
        }

        return (float)camera.pixelWidth / camera.pixelHeight;
    }

    protected static Vector2 ToViewport2(Camera camera, Vector3 worldPosition)
    {
        Vector3 viewport = camera.WorldToViewportPoint(worldPosition);
        return new Vector2(viewport.x, viewport.y);
    }

    protected static Vector2 ApplyAspectCorrection(Vector2 viewport, float aspect)
    {
        return new Vector2(viewport.x * aspect, viewport.y);
    }

    protected abstract bool TryBuildRegionData(Camera camera, out RegionShaderData data);
}
