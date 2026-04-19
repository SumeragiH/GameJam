using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DarkenOutsideRegionsRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader _shader;
    [SerializeField, Range(1, RegionMaskManager.MaxShaderRegions)] private int _maxRegions = RegionMaskManager.MaxShaderRegions;
    [SerializeField] private RenderPassEvent _renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;

    private Material _material;
    private DarkenOutsideRegionsPass _pass;

    public override void Create()
    {
        if (_shader == null)
        {
            _shader = Shader.Find("Hidden/GameJam/DarkenOutsideRegions");
        }

        if (_shader == null)
        {
            Debug.LogWarning("DarkenOutsideRegionsRendererFeature: shader not found.");
            return;
        }

        if (_material == null)
        {
            _material = CoreUtils.CreateEngineMaterial(_shader);
        }

        _pass ??= new DarkenOutsideRegionsPass();
        _pass.Configure(_material, _renderPassEvent, _maxRegions);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_material == null || _pass == null)
        {
            return;
        }

        CameraData cameraData = renderingData.cameraData;
        if (cameraData.cameraType != CameraType.Game)
        {
            return;
        }

        if (cameraData.renderType != CameraRenderType.Base)
        {
            return;
        }

        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        _pass = null;

        if (_material != null)
        {
            CoreUtils.Destroy(_material);
            _material = null;
        }
    }

    private sealed class DarkenOutsideRegionsPass : ScriptableRenderPass
    {
        private Material _material;
        private RTHandle _tempTexture;
        private int _maxRegions;

        public void Configure(Material material, RenderPassEvent passEvent, int maxRegions)
        {
            _material = material;
            _maxRegions = Mathf.Clamp(maxRegions, 1, RegionMaskManager.MaxShaderRegions);
            renderPassEvent = passEvent;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            if (_material == null)
            {
                return;
            }

            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref _tempTexture, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_DarkenOutsideTempTexture");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            if (_material == null || cameraColorTarget == null || _tempTexture == null)
            {
                return;
            }

            Camera camera = renderingData.cameraData.camera;
            RegionMaskManager.UploadShaderData(camera, _material, _maxRegions);

            CommandBuffer cmd = CommandBufferPool.Get("DarkenOutsideRegionsPass");
            try
            {
                Blitter.BlitCameraTexture(cmd, cameraColorTarget, _tempTexture);
                Blitter.BlitCameraTexture(cmd, _tempTexture, cameraColorTarget, _material, 0);
                context.ExecuteCommandBuffer(cmd);
            }
            finally
            {
                CommandBufferPool.Release(cmd);
            }
        }

        public void Dispose()
        {
            _tempTexture?.Release();
            _tempTexture = null;
            _material = null;
        }
    }
}
