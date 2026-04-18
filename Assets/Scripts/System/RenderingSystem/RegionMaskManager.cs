using System.Collections.Generic;
using UnityEngine;

public class RegionMaskManager : MonoBehaviour
{
    public const int MaxShaderRegions = 32;

    private static readonly HashSet<IRegionMaskProvider> Providers = new HashSet<IRegionMaskProvider>();
    private static readonly Vector4[] RegionTypeData = new Vector4[MaxShaderRegions];
    private static readonly Vector4[] RegionParamsA = new Vector4[MaxShaderRegions];
    private static readonly Vector4[] RegionParamsB = new Vector4[MaxShaderRegions];

    private static readonly int RegionCountId = Shader.PropertyToID("_RegionCount");
    private static readonly int RegionTypeDataId = Shader.PropertyToID("_RegionTypeData");
    private static readonly int RegionParamsAId = Shader.PropertyToID("_RegionParamsA");
    private static readonly int RegionParamsBId = Shader.PropertyToID("_RegionParamsB");
    private static readonly int OutsideBrightnessId = Shader.PropertyToID("_OutsideBrightness");
    private static readonly int EffectEnabledId = Shader.PropertyToID("_EffectEnabled");

    private static RegionMaskManager _instance;

    [Header("Mask Settings")]
    [SerializeField] private bool _effectEnabled = true;
    [SerializeField, Range(0f, 1f)] private float _outsideBrightness = 0.35f;
    [SerializeField] private uint _activeGroupMask = uint.MaxValue;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("RegionMaskManager: multiple instances detected, keeping the first one.");
            return;
        }

        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    public static void RegisterProvider(IRegionMaskProvider provider)
    {
        if (provider != null)
        {
            Providers.Add(provider);
        }
    }

    public static void UnregisterProvider(IRegionMaskProvider provider)
    {
        if (provider != null)
        {
            Providers.Remove(provider);
        }
    }

    public void SetGroupEnabled(int groupIndex, bool enabled)
    {
        if (groupIndex < 0 || groupIndex > 31)
        {
            Debug.LogWarning($"RegionMaskManager: invalid group index {groupIndex}.");
            return;
        }

        uint bit = 1u << groupIndex;
        if (enabled)
        {
            _activeGroupMask |= bit;
        }
        else
        {
            _activeGroupMask &= ~bit;
        }
    }

    public void SetActiveGroupMask(uint mask)
    {
        _activeGroupMask = mask;
    }

    public static void UploadShaderData(Camera camera, Material material, int maxRegions)
    {
        if (camera == null || material == null)
        {
            return;
        }

        RegionMaskManager manager = GetInstance();
        bool effectEnabled = manager == null || manager._effectEnabled;
        float outsideBrightness = manager != null ? Mathf.Clamp01(manager._outsideBrightness) : 0.35f;
        uint activeMask = manager != null ? manager._activeGroupMask : uint.MaxValue;
        int capacity = Mathf.Clamp(maxRegions, 1, MaxShaderRegions);

        int count = 0;
        List<IRegionMaskProvider> invalidProviders = null;

        foreach (IRegionMaskProvider provider in Providers)
        {
            if (count >= capacity)
            {
                break;
            }

            if (!IsValidProvider(provider))
            {
                if (invalidProviders == null)
                {
                    invalidProviders = new List<IRegionMaskProvider>();
                }

                invalidProviders.Add(provider);
                continue;
            }

            if (!provider.IsProviderEnabled)
            {
                continue;
            }

            int group = Mathf.Clamp(provider.RegionGroup, 0, 31);
            if ((activeMask & (1u << group)) == 0)
            {
                continue;
            }

            if (!provider.TryGetRegionData(camera, out RegionShaderData data))
            {
                continue;
            }

            RegionTypeData[count] = new Vector4((float)data.ShapeType, 0f, 0f, 0f);
            RegionParamsA[count] = new Vector4(data.CenterViewport.x, data.CenterViewport.y, data.SizeViewport.x, data.SizeViewport.y);
            RegionParamsB[count] = new Vector4(Mathf.Cos(data.RotationRadians), Mathf.Sin(data.RotationRadians), Mathf.Max(0f, data.FeatherViewport), 0f);
            count++;
        }

        if (invalidProviders != null)
        {
            for (int i = 0; i < invalidProviders.Count; i++)
            {
                Providers.Remove(invalidProviders[i]);
            }
        }

        material.SetInt(RegionCountId, count);
        material.SetFloat(OutsideBrightnessId, outsideBrightness);
        material.SetInt(EffectEnabledId, effectEnabled ? 1 : 0);
        material.SetVectorArray(RegionTypeDataId, RegionTypeData);
        material.SetVectorArray(RegionParamsAId, RegionParamsA);
        material.SetVectorArray(RegionParamsBId, RegionParamsB);
    }

    private static RegionMaskManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = FindObjectOfType<RegionMaskManager>();
        }

        return _instance;
    }

    private static bool IsValidProvider(IRegionMaskProvider provider)
    {
        if (provider == null)
        {
            return false;
        }

        if (provider is Object unityObject)
        {
            return unityObject != null;
        }

        return true;
    }
}
