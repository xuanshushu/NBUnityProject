// using ConfigSystem.MConfig;
// using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace MhRender.RendererFeatures
{
    public class NBPostProcess : ScriptableRendererFeature
    {
        private NBPostProcessRenderPass _renderPass;
        private DisturbanceMaskRenderPass _disturbanceMaskRenderPass;
        private ScreenColorRenderPass _screenColorRenderPass;


        public static Material NBPostProcessMaterial;
        
        //public MaskFormat maskFormat = MaskFormat.RG32;
        public Downsampling downSampling = Downsampling._2xBilinear;
        public LayerMask disturbanceLayerMask=1 << 25;
        
        private Material _disturbanceDownSampleMat;
        private Material _screenColorDownSampleMat;
        private float _screenHeight;
        private ProfilingSampler _profilingSampler;
        
        // private PostProcessingManager manager;-+
        static Mesh s_FullscreenTriangle;
        /// <summary>
        /// A fullscreen triangle mesh.抄自Unity的后处理包,拿到一个全屏的Triangle。
        /// </summary>
        static Mesh fullscreenTriangle;
        // {
        //     get
        //     {
        //         if (s_FullscreenTriangle != null)
        //             return s_FullscreenTriangle;
        //
        //         s_FullscreenTriangle = new Mesh
        //         {
        //             name = "Fullscreen Triangle"
        //         };
        //
        //         // Because we have to support older platforms (GLES2/3, DX9 etc) we can't do all of
        //         // this directly in the vertex shader using vertex ids :(
        //         s_FullscreenTriangle.SetVertices(new List<Vector3>
        //         {
        //             new Vector3 (-1f, -1f, 0f),
        //             new Vector3 (-1f, 3f, 0f),
        //             new Vector3 (3f, -1f, 0f)
        //             // new Vector3 (3f, -1f, 0f),
        //             // new Vector3 (-1f, 3f, 0f),
        //             // new Vector3 (-1f, -1f, 0f)
        //         });
        //         s_FullscreenTriangle.SetIndices(new[]
        //         {
        //             0,
        //             1,
        //             2
        //         }, MeshTopology.Triangles, 0, false);
        //         s_FullscreenTriangle.UploadMeshData(false);
        //
        //         meshTest = s_FullscreenTriangle;
        //         return s_FullscreenTriangle;
        //     }
        // }


        private bool canFind = false;
        public override void Create()
        {
            if (Shader.Find("XuanXuan/ColorBlit") == null || 
                Shader.Find("XuanXuan/Postprocess/NBPostProcessUber") == null)
            {
                canFind = false;
                return;
            }
            else
            {
                canFind = true;
            }
            
            #if UNIVERSAL_RP_13_1_2_OR_NEWER
                _screenColorDownSampleMat = CoreUtils.CreateEngineMaterial(Shader.Find("XuanXuan/ColorBlit"));
            #else                
                _screenColorDownSampleMat = CoreUtils.CreateEngineMaterial(Shader.Find("XuanXuan/ColorBufferBlit"));
            #endif
            _screenColorRenderPass = new ScreenColorRenderPass(_screenColorDownSampleMat, downSampling);
            _screenColorRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

            
            _profilingSampler = new ProfilingSampler("DisturbanceRender");
            _disturbanceDownSampleMat = CoreUtils.CreateEngineMaterial(Shader.Find("XuanXuan/ColorBlit"));
            _disturbanceMaskRenderPass = new DisturbanceMaskRenderPass(_profilingSampler,_disturbanceDownSampleMat,downSampling,disturbanceLayerMask);
            _disturbanceMaskRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            
            
            
            #if !UNIVERSAL_RP_13_1_2_OR_NEWER
            _profilingSampler = new ProfilingSampler("DisturbanceDownRTBlit");
            #endif
            
            if (fullscreenTriangle == null)
            {
                /*UNITY_NEAR_CLIP_VALUE*/
                float nearClipZ = -1;
                if (SystemInfo.usesReversedZBuffer)
                    nearClipZ = 1;
                
                fullscreenTriangle = new Mesh();
                fullscreenTriangle.vertices = GetFullScreenTriangleVertexPosition(nearClipZ);
                fullscreenTriangle.uv = GetFullScreenTriangleTexCoord();
                fullscreenTriangle.triangles = new int[3] { 0, 1, 2 };
            }

            // Shader shader = Shader.Find("XuanXuan/Postprocess/NBPostProcessUber");
            NBPostProcessMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("XuanXuan/Postprocess/NBPostProcessUber"));

     
            // if (Application.isPlaying)
            // {
            //     manager = PostProcessingManager.Instance;
            // }
            _renderPass = new NBPostProcessRenderPass(NBPostProcessMaterial,fullscreenTriangle);
            _renderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }
        
        #if UNIVERSAL_RP_13_1_2_OR_NEWER
        
        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            PostProcessingManager.InitMat();
            if ((renderingData.cameraData.cameraType == CameraType.Game ||
                renderingData.cameraData.cameraType == CameraType.SceneView) && canFind)
            {
                //_disturbanceMaskRenderPass.SetUp(renderer.cameraDepthTargetHandle);
                //if (renderingData.cameraData.cameraType == CameraType.Game)
                //{
                //    _screenHeight = renderer.cameraDepthTargetHandle.rt.descriptor.height;
                //}
                
                _disturbanceMaskRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
                _disturbanceMaskRenderPass.SetUp(renderer.cameraColorTargetHandle);
                
                _screenColorRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
                _screenColorRenderPass.SetUp(renderer.cameraColorTargetHandle);
            }
        }

        #endif

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if ((renderingData.cameraData.cameraType == CameraType.Game ||
                renderingData.cameraData.cameraType == CameraType.SceneView) && canFind)
            {
                
                #if !UNIVERSAL_RP_13_1_2_OR_NEWER
                    // _disturbanceMaskRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
                    // _disturbanceMaskRenderPass.SetUp(renderer.cameraColorTargetHandle);
                    _screenColorRenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
                    _screenColorRenderPass.SetUp(renderer);
                #endif
                
                PostProcessingManager.InitMat();
                renderer.EnqueuePass(_screenColorRenderPass);
                renderer.EnqueuePass(_disturbanceMaskRenderPass);
                renderer.EnqueuePass(_renderPass);
            }
        }
        
        // Should match Common.hlsl
        static Vector3[] GetFullScreenTriangleVertexPosition(float z /*= UNITY_NEAR_CLIP_VALUE*/)
        {
            var r = new Vector3[3];
            for (int i = 0; i < 3; i++)
            {
                Vector2 uv = new Vector2((i << 1) & 2, i & 2);
                r[i] = new Vector3(uv.x * 2.0f - 1.0f, uv.y * 2.0f - 1.0f, z);
            }
            return r;
        }

        // Should match Common.hlsl
        static Vector2[] GetFullScreenTriangleTexCoord()
        {
            var r = new Vector2[3];
            for (int i = 0; i < 3; i++)
            {
                if (SystemInfo.graphicsUVStartsAtTop)
                    r[i] = new Vector2((i << 1) & 2, 1.0f - (i & 2));
                else
                    r[i] = new Vector2((i << 1) & 2, i & 2);
            }
            return r;
        }
        
        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_disturbanceDownSampleMat);
            //CoreUtils.Destroy(NBPostProcessMaterial);
            _disturbanceMaskRenderPass?.Dispose();
            CoreUtils.Destroy(_screenColorDownSampleMat);
            _screenColorRenderPass?.Dispose();


        }
    }
    public class DisturbanceMaskRenderPass : ScriptableRenderPass
    {
        
        private ProfilingSampler _profilingSampler;
        #if UNIVERSAL_RP_13_1_2_OR_NEWER
            private  RTHandle _DisturbanceMaskRTHandle;
            private RTHandle _cameraDepthRTHandle;
            private RTHandle _DownRT;
        #else
            private static readonly int _DisturbanceMaskRTID = Shader.PropertyToID("_DisturbanceMaskRT"); 
            private static readonly int _DownRTID = Shader.PropertyToID("_DisturbanceMaskTex"); 
            private RenderTargetIdentifier  _DisturbanceMaskRTHandle = new RenderTargetIdentifier (_DisturbanceMaskRTID);
            private RenderTargetIdentifier  _cameraDepthRTHandle;
            private RenderTargetIdentifier  _DownRT = new RenderTargetIdentifier (_DownRTID);
        #endif
        private Material tempMat;
        
        private readonly Downsampling _downSampling;
        //private readonly MaskFormat _maskFormat;

        private Material _renderMaskMat ;
        public LayerMask _DisturbanceMaskLayer = 1 << 25;
        private FilteringSettings _Filtering;
        private static readonly int CameraTexture = Shader.PropertyToID("_CameraTexture");
        private static readonly int SampleOffset = Shader.PropertyToID("_SampleOffset");
        
        private readonly List<ShaderTagId> _shaderTag = new List<ShaderTagId>()
        {
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("UniversalForwardOnly")
        };

        public DisturbanceMaskRenderPass(ProfilingSampler profilingSampler,Material DisturbanceMaskMat, Downsampling downSampling,LayerMask disturbanceMaskLayer)
        {
            _profilingSampler = profilingSampler;
            _renderMaskMat = DisturbanceMaskMat;
            _downSampling = downSampling;
            _DisturbanceMaskLayer = disturbanceMaskLayer;
        }

    #if UNIVERSAL_RP_13_1_2_OR_NEWER
        
        public void SetUp ( RTHandle cameraRTHandle )
        {

            RenderTextureDescriptor descrip = cameraRTHandle.rt.descriptor;
            
            descrip.colorFormat = RenderTextureFormat.RG32;
            descrip.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref _DisturbanceMaskRTHandle, descrip, name: "DisturbanceMaskRT");
            
            
            switch (_downSampling)
            {
                case Downsampling._2xBilinear:
                    descrip.width /= 2;
                    descrip.height /= 2;
                    break;
                case Downsampling._4xBilinear:
                    descrip.width /= 4;
                    descrip.height /= 4;
                    break;
                case Downsampling._4xBox:
                    descrip.width /= 4;
                    descrip.height /= 4;
                    break;
            }
            RenderingUtils.ReAllocateIfNeeded(ref _DownRT, descrip, name:"MaskDownCopyRT");
        }
    #else
        //在AddPass之前触发

        public void SetUpDisturbanceMask(RenderTextureDescriptor descrip ,CommandBuffer cmd)
        {
            
            descrip.colorFormat = RenderTextureFormat.RG32;
            descrip.depthBufferBits = 0;
            cmd.GetTemporaryRT(_DisturbanceMaskRTID, descrip,FilterMode.Bilinear);
            
            switch (_downSampling)
            {
                case Downsampling._2xBilinear:
                    descrip.width /= 2;
                    descrip.height /= 2;
                    break;
                case Downsampling._4xBilinear:
                    descrip.width /= 4;
                    descrip.height /= 4;
                    break;
                case Downsampling._4xBox:
                    descrip.width /= 4;
                    descrip.height /= 4;
                    break;
            }
            cmd.GetTemporaryRT(_DownRTID, descrip,FilterMode.Bilinear);
        }
    #endif
        
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            _Filtering = new FilteringSettings(RenderQueueRange.all, _DisturbanceMaskLayer);
            #if UNIVERSAL_RP_13_1_2_OR_NEWER
                _cameraDepthRTHandle = renderingData.cameraData.renderer.cameraDepthTargetHandle;
            #else
                _cameraDepthRTHandle = renderingData.cameraData.renderer.cameraDepthTarget;
                SetUpDisturbanceMask(renderingData.cameraData.cameraTargetDescriptor,cmd);
            #endif
            
        }

        private readonly Color _clearDisturbanceMaskColor = new Color(0.5f, 0.5f, 0f, 1f);
        
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
#if UNIVERSAL_RP_13_1_2_OR_NEWER
                ConfigureTarget(_DisturbanceMaskRTHandle, _cameraDepthRTHandle);
#else
                ConfigureTarget(_DisturbanceMaskRTID, _cameraDepthRTHandle);
            
#endif
            //将RT清空
            ConfigureClear(ClearFlag.Color, _clearDisturbanceMaskColor);
        }
        
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
            if (!(renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView))
                return;

            if (!_renderMaskMat)
            {
                return;
            }
            
            var DisturbanceDraw = CreateDrawingSettings(_shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, _profilingSampler))
            {
                context.DrawRenderers(renderingData.cullResults, ref DisturbanceDraw, ref _Filtering);

#if UNIVERSAL_RP_13_1_2_OR_NEWER
                _renderMaskMat.SetTexture(CameraTexture, _DisturbanceMaskRTHandle);
                switch (_downSampling)
                {
                    case Downsampling._2xBilinear:
                        Blitter.BlitTexture(cmd, _DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 0);
                        break;
                    case Downsampling._4xBilinear:
                        Blitter.BlitTexture(cmd, _DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 0);
                        break;
                    case Downsampling._4xBox:
                        _renderMaskMat.SetFloat(SampleOffset, 2);
                        Blitter.BlitTexture(cmd, _DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 1);
                        break;
                    default:
                        Blitter.BlitTexture(cmd, _DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 0);
                        break;
                }

#else

                // cmd.SetGlobalTexture(_DisturbanceMaskRTID, _DisturbanceMaskRTHandle);
                //Bug:在非播放状态时，_DisturbanceMaskRTID在这里会经常丢失。造成画面闪烁。但是，只要有一个DistortObject在Scene中，并LockInspector，就不会闪烁，非常奇怪。
                cmd.SetGlobalTexture(CameraTexture, _DisturbanceMaskRTHandle);
                // _renderMaskMat.SetTexture(CameraTexture, Shader.GetGlobalTexture(_DisturbanceMaskRTID));
                cmd.SetRenderTarget(_DownRT);
                switch (_downSampling)
                {
                    case Downsampling._2xBilinear:
                        // Blitter.BlitTexture(cmd, _DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 0);
                        // cmd.Blit(_DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 0);
                        // cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _renderMaskMat, 0, 2);
                        Blit(cmd, _DisturbanceMaskRTHandle,_DownRT,_renderMaskMat,0);
                        break;
                    case Downsampling._4xBilinear:
                        // Blitter.BlitTexture(cmd, _DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 0);
                        // cmd.Blit(_DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 0);
                        // cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _renderMaskMat, 0, 2);
                        Blit(cmd, _DisturbanceMaskRTHandle,_DownRT,_renderMaskMat,0);
                        break;
                    case Downsampling._4xBox:
                        _renderMaskMat.SetFloat(SampleOffset, 2);
                        // Blitter.BlitTexture(cmd, _DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 1);
                        // cmd.Blit(_DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 1);
                        // cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _renderMaskMat, 0, 3);
                        Blit(cmd, _DisturbanceMaskRTHandle,_DownRT,_renderMaskMat,1);
                        break;
                    default:
                        // Blitter.BlitTexture(cmd, _DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 0);  
                        // cmd.Blit(_DisturbanceMaskRTHandle, _DownRT, _renderMaskMat, 0);
                        // cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _renderMaskMat, 0, 2);
                        Blit(cmd, _DisturbanceMaskRTHandle,_DownRT,_renderMaskMat,0);
                        break;
                }
#endif
                cmd.SetGlobalTexture("_DisturbanceMaskTex", _DownRT);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            }
        }

        #if !UNIVERSAL_RP_13_1_2_OR_NEWER
        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_DisturbanceMaskRTID);
            cmd.ReleaseTemporaryRT(_DownRTID);
        }

        //JustForSimple
        public void Dispose()
        {
        }
        #else

        public void Dispose()
        {
            _DisturbanceMaskRTHandle?.Release();
            _DownRT?.Release();
        }
        #endif
    }
    public class NBPostProcessRenderPass : ScriptableRenderPass
    {
        private ProfilingSampler _profilingSampler;
        public static Material _material;
        public Mesh _fullScreenMesh;

        public NBPostProcessFlags _shaderFlag => PostProcessingManager.flags;

        private Vector4 _lastOutlineProps;
        public Vector4 outLinePorps = Vector4.one;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!(renderingData.cameraData.cameraType == CameraType.Game || renderingData.cameraData.cameraType == CameraType.SceneView))
                return;
   
            // if(!_shaderFlag.CheckFlagBits(NBPostProcessFlags.FLAG_BIT_NB_POSTPROCESS_ON))return;//Disturbance需要执行
            
            //ConfigureTarget()
            CommandBuffer cmdBuffer = CommandBufferPool.Get();
            cmdBuffer.Clear();
            // cmdBuffer.name = "NBPostProcess";
          
            using (new ProfilingScope(cmdBuffer,_profilingSampler))
            {
                Camera camera = renderingData.cameraData.camera;
                cmdBuffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
             
                if(_material == null) return;
                cmdBuffer.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _material, 0, 0);
                
                cmdBuffer.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                
            }
            
            context.ExecuteCommandBuffer(cmdBuffer);
            CommandBufferPool.Release(cmdBuffer);
        }

        public  NBPostProcessRenderPass(Material mat,Mesh mesh)
        {
            _material = mat;
            _fullScreenMesh = mesh;
            _profilingSampler ??= new ProfilingSampler("NBPostProcess");

        }
    }
}
