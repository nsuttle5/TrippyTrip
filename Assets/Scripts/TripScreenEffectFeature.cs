using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TripScreenEffectFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    [SerializeField] private Settings settings = new Settings();

    private TripScreenEffectPass _pass;

    public override void Create()
    {
        _pass = new TripScreenEffectPass(settings.material)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
        {
            return;
        }

        renderer.EnqueuePass(_pass);
    }

    private class TripScreenEffectPass : ScriptableRenderPass
    {
        private static readonly int TempTextureId = Shader.PropertyToID("_TripScreenEffectTempTexture");
        private readonly Material _material;
        private RTHandle _source;

        public TripScreenEffectPass(Material material)
        {
            _material = material;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _source = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (_material == null)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("Trip Screen Effect");
            RTHandle tempTexture = RTHandles.Alloc(renderingData.cameraData.cameraTargetDescriptor, name: "TripScreenEffectTempTexture");

            Blitter.BlitCameraTexture(cmd, _source, tempTexture, _material, 0);
            Blitter.BlitCameraTexture(cmd, tempTexture, _source);

            RTHandles.Release(tempTexture);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
