/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

#if USING_XR_MANAGEMENT && USING_XR_SDK_XREAL
#define USING_XR_SDK
#endif

namespace NRKernal
{
    using System;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Rendering;
    using AOT;
#if USING_XR_SDK 
    using UnityEngine.XR;
    using System.Runtime.InteropServices;
#endif

    [RequireComponent(typeof(Camera))]
    public class NRDisplayOverlay : OverlayBase
    {
        /// <summary>
        /// Which display this overlay should render to.
        /// </summary>
        [Tooltip("Which display this overlay should render to.")]
        public NativeDevice targetDisplay;
        private Camera m_RenderCamera;
        internal Camera renderCamera
        {
            get { return m_RenderCamera; }
        }
        private int m_EyeWidth;
        private int m_EyeHeight;

#if USING_XR_SDK
        /// <summary> Renders the event delegate described by eventID. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        private delegate void RenderEventDelegate(int eventID);
        /// <summary> Handle of the render thread. </summary>
        private static RenderEventDelegate RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        /// <summary> The render thread handle pointer. </summary>
        private static IntPtr RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);
        
        private const int k_CreateMonoDisplayTextures = 0x0001;
        private const int k_CreateLeftDisplayTextures = 0x0002;
        private const int k_CreateRightDisplayTextures = 0x0003;
        
        private IntPtr m_WorkingTexture = IntPtr.Zero;
        internal IntPtr WorkingTexture
        {
            get { return m_WorkingTexture; }
        }
#endif

        protected override void Initialize()
        {
            base.Initialize();

            m_RenderCamera = gameObject.GetComponent<Camera>();

            NativeResolution resolution = new NativeResolution(1920, 1080);
#if !UNITY_EDITOR
#if USING_XR_SDK
            if (NRFrame.MonoMode || !NRFrame.isXRRenderMultiview)
                resolution = NRFrame.GetDeviceResolution(targetDisplay);
            else
                resolution = NRFrame.GetDeviceResolution(NativeDevice.LEFT_DISPLAY);
#else
            resolution = NRFrame.GetDeviceResolution(targetDisplay);
#endif
#endif
            m_EyeWidth = resolution.width;
            m_EyeHeight = resolution.height;
            NRDebugger.Info("[NRDisplayOverlay] Initialize: cam={0}, targetDisplay={3}, resolution={1}x{2}.", m_RenderCamera.name, m_EyeWidth, m_EyeHeight, targetDisplay);

            this.compositionDepth = int.MaxValue - 1;
            m_BufferSpec.size = new NativeResolution((int)m_EyeWidth, (int)m_EyeHeight);
            m_BufferSpec.colorFormat = NRTextureFormat.NR_TEXTURE_FORMAT_COLOR_RGBA8;
            m_BufferSpec.depthFormat = NRTextureFormat.NR_TEXTURE_FORMAT_DEPTH_24;
            m_BufferSpec.samples = 1;
            m_BufferSpec.surfaceFlag = (int)NRExternalSurfaceFlags.NONE;
            m_BufferSpec.createFlag = (UInt64)NRSwapchainCreateFlags.NR_SWAPCHAIN_CREATE_FLAGS_NONE;
            m_BufferSpec.useTextureArray = !NRFrame.MonoMode && NRFrame.isXRRenderMultiview;

            isDynamic = true;
        }

#if !USING_XR_SDK
        void OnEnable()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
                Camera.onPreRender += OnPreRenderCallback;
            else
                RenderPipelineManager.beginCameraRendering += OnBeginRenderCallback;
        }

        void OnDisable()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
                Camera.onPreRender -= OnPreRenderCallback;
            else
                RenderPipelineManager.beginCameraRendering -= OnBeginRenderCallback;
        }

        void OnPreRenderCallback(Camera camera)
        {
            OnPreRenderImp(camera);
        }

        void OnBeginRenderCallback(ScriptableRenderContext context, Camera camera)
        {
            OnPreRenderImp(camera);
        }

        void OnPreRenderImp(Camera camera)
        {
            if (camera != m_RenderCamera)
                return;

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[NRDisplayOverlay] OnPreRenderCallback: {0}", m_RenderCamera.name);

            var bufferHandler = NRSessionManager.Instance.NRSwapChainMan.GetWorkingBufferHandler(m_SwapChainHandler);
            // Camera's targetTexture need to be set on OnPreRenderCallback of same camera.
            if (bufferHandler != IntPtr.Zero)
                PopulateBuffersImp(bufferHandler);
        }
#endif

        override protected void Update() {
            base.Update();

            if (NRSessionManager.Instance.NRSwapChainMan.isRunning && !m_RenderCamera.enabled)
            {
                m_RenderCamera.enabled = true;
            }
        }

        public override void PopulateBuffers(IntPtr bufferHandler)
        {
#if USING_XR_SDK
            // Camera's targetTexture need to be set on OnPreRenderCallback of same camera.
            PopulateBuffersImp(bufferHandler);
#endif
        }

        void PopulateBuffersImp(IntPtr bufferHandler)
        {
            if (!Textures.ContainsKey(bufferHandler))
            {
                NRDebugger.Error("[NRDisplayOverlay] Can not find the texture: {0}", bufferHandler);
                return;
            }

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[NRDisplayOverlay] PopulateBuffers: workingTex={0}", bufferHandler);
#if USING_XR_SDK
            m_WorkingTexture = bufferHandler;
            
            UInt32 multiPassEye = 0;
            if  (!NRFrame.MonoMode && !NRFrame.isXRRenderMultiview)
            {
                multiPassEye = (UInt32)((targetDisplay == NativeDevice.LEFT_DISPLAY) ? 1 : 2);
            }

            // swap render texture in XR native
            NativeXRPlugin.PopulateDisplayTexture(bufferHandler, multiPassEye);
#else
            Texture targetTexture;
            Textures.TryGetValue(bufferHandler, out targetTexture);
            RenderTexture renderTexture = targetTexture as RenderTexture;
            if (renderTexture == null)
            {
                NRDebugger.Error("[NRDisplayOverlay] The texture is null...");
                return;
            }

            m_RenderCamera.targetTexture = renderTexture;
#endif
        }

        private NativeTransform CalculatePose(NativeDevice targetDisplay)
        {
            var headPose = NRFrame.HeadPose;
            var eyePose = Pose.identity;
            switch (targetDisplay)
            {
                case NativeDevice.LEFT_DISPLAY:
                    eyePose = NRFrame.EyePoseFromHead.LEyePose;
                    break;
                case NativeDevice.RIGHT_DISPLAY:
                    eyePose = NRFrame.EyePoseFromHead.REyePose;
                    break;
                case NativeDevice.HEAD_CENTER:
                    eyePose = NRFrame.EyePoseFromHead.CEyePose;
                    break;
            }

            var view = ConversionUtility.GetTMatrix(headPose) * ConversionUtility.GetTMatrix(eyePose);
            return ConversionUtility.UnityMatrixToApiPose(view);
        }

        internal void NRDebugSaveToPNG()
        {
            UnityExtendedUtility.SaveTextureAsPNG(m_RenderCamera.targetTexture);
        }

        public override void CreateViewport()
        {
            NRDebugger.Info("[NRDisplayOverlay] CreateViewport: {0}", gameObject.name);
#if USING_XR_SDK
            if (NRFrame.MonoMode || !NRFrame.isXRRenderMultiview)
            {
                m_ViewPorts = new ViewPort[1];
                m_ViewPorts[0].viewportType = NRViewportType.NR_VIEWPORT_PROJECTION;
                m_ViewPorts[0].sourceUV = new NativeRectf(0, 0, 1, 1);
                m_ViewPorts[0].targetDisplay = targetDisplay;
                m_ViewPorts[0].swapchainHandler = m_SwapChainHandler;
                m_ViewPorts[0].is3DLayer = true;
                m_ViewPorts[0].textureArraySlice = -1;
                m_ViewPorts[0].spaceType = NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL;
                m_ViewPorts[0].nativePose = CalculatePose(targetDisplay);
                NRFrame.GetEyeFov(targetDisplay, ref m_ViewPorts[0].fov);
                NRSessionManager.Instance.NRSwapChainMan.CreateBufferViewport(ref m_ViewPorts[0]);
            }
            else
            {
                m_ViewPorts = new ViewPort[2];
                m_ViewPorts[0].viewportType = NRViewportType.NR_VIEWPORT_PROJECTION;
                m_ViewPorts[0].sourceUV = new NativeRectf(0, 0, 1, 1);
                m_ViewPorts[0].targetDisplay = NativeDevice.LEFT_DISPLAY;
                m_ViewPorts[0].swapchainHandler = m_SwapChainHandler;
                m_ViewPorts[0].is3DLayer = true;
                m_ViewPorts[0].textureArraySlice = 0;
                m_ViewPorts[0].spaceType = NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL;
                m_ViewPorts[0].nativePose = CalculatePose(NativeDevice.LEFT_DISPLAY);
                NRFrame.GetEyeFov(NativeDevice.LEFT_DISPLAY, ref m_ViewPorts[0].fov);
                NRSessionManager.Instance.NRSwapChainMan.CreateBufferViewport(ref m_ViewPorts[0]);

                m_ViewPorts[1].viewportType = NRViewportType.NR_VIEWPORT_PROJECTION;
                m_ViewPorts[1].sourceUV = new NativeRectf(0, 0, 1, 1);
                m_ViewPorts[1].targetDisplay = NativeDevice.RIGHT_DISPLAY;
                m_ViewPorts[1].swapchainHandler = m_SwapChainHandler;
                m_ViewPorts[1].is3DLayer = true;
                m_ViewPorts[1].textureArraySlice = 1;
                m_ViewPorts[1].spaceType = NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL;
                m_ViewPorts[1].nativePose = CalculatePose(NativeDevice.RIGHT_DISPLAY);
                NRFrame.GetEyeFov(NativeDevice.RIGHT_DISPLAY, ref m_ViewPorts[1].fov);
                NRSessionManager.Instance.NRSwapChainMan.CreateBufferViewport(ref m_ViewPorts[1]);
            }
#else
            m_ViewPorts = new ViewPort[1];
            m_ViewPorts[0].viewportType = NRViewportType.NR_VIEWPORT_PROJECTION;
            m_ViewPorts[0].sourceUV = new NativeRectf(0, 0, 1, 1);
            m_ViewPorts[0].targetDisplay = targetDisplay;
            m_ViewPorts[0].swapchainHandler = m_SwapChainHandler;
            m_ViewPorts[0].is3DLayer = true;
            m_ViewPorts[0].textureArraySlice = -1;
            m_ViewPorts[0].spaceType = NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL;
            m_ViewPorts[0].nativePose = CalculatePose(targetDisplay);
            NRFrame.GetEyeFov(targetDisplay, ref m_ViewPorts[0].fov);
            NRSessionManager.Instance.NRSwapChainMan.CreateBufferViewport(ref m_ViewPorts[0]);
#endif
        }

        public override void PopulateViewPort()
        {
            if (!isReady)
                return;

            if (m_ViewPorts == null)
            {
                NRDebugger.Warning("Can not update view port for this layer:{0}", gameObject.name);
                return;
            }

#if USING_XR_SDK
            if (NRFrame.MonoMode || !NRFrame.isXRRenderMultiview)
            {
                m_ViewPorts[0].nativePose = CalculatePose(targetDisplay);
                NRSessionManager.Instance.NRSwapChainMan.PopulateViewportData(ref m_ViewPorts[0]);
            }
            else
            {
                m_ViewPorts[0].nativePose = CalculatePose(NativeDevice.LEFT_DISPLAY);
                m_ViewPorts[1].nativePose = CalculatePose(NativeDevice.RIGHT_DISPLAY);
                NRSessionManager.Instance.NRSwapChainMan.PopulateViewportData(ref m_ViewPorts[0]);
                NRSessionManager.Instance.NRSwapChainMan.PopulateViewportData(ref m_ViewPorts[1]);
            }
#else
            m_ViewPorts[0].nativePose = CalculatePose(targetDisplay);
            NRSessionManager.Instance.NRSwapChainMan.PopulateViewportData(ref m_ViewPorts[0]);
#endif
        }

        public override void CreateOverlayTextures()
        {
            NRDebugger.Info("[NRDisplayOverlay] CreateOverlayTextures: cam={0}, targetDisplay={1}.", m_RenderCamera.name, targetDisplay);
            ReleaseOverlayTextures();

#if USING_XR_SDK
            // For XR mode, Display textures need to be created in render thread.
            var createTexEvent = 0;
            if (targetDisplay == NativeDevice.HEAD_CENTER)
                createTexEvent = k_CreateMonoDisplayTextures;
            else if (targetDisplay == NativeDevice.LEFT_DISPLAY)
                createTexEvent = k_CreateLeftDisplayTextures;
            else if (targetDisplay == NativeDevice.RIGHT_DISPLAY)
                createTexEvent = k_CreateRightDisplayTextures;
            
            GL.IssuePluginEvent(RenderThreadHandlePtr, createTexEvent);
#else
            for (int i = 0; i < m_BufferSpec.bufferCount; i++)
            {
                RenderTexture rt = UnityExtendedUtility.CreateRenderTexture((int)m_EyeWidth, (int)m_EyeHeight, 24, RenderTextureFormat.ARGB32);
                IntPtr texturePtr = rt.GetNativeTexturePtr();
                Textures.Add(texturePtr, rt);

                // NRDebugger.Info("[NROverlay] CreateOverlayTextures: rsl={6}*{7}, aa={0}, fmt={1}, gfxFmt={2}, mmCnt={3}, autoMM={4}, useMM={5}, depth={8}",
                //     rt.antiAliasing, rt.format, rt.graphicsFormat, rt.mipmapCount, rt.autoGenerateMips, rt.useMipMap, rt.width, rt.height, rt.depth);
            }

            isReady = true;
            if (Textures.Count > 0)
                NRSessionManager.Instance.NRSwapChainMan.NativeSwapchain.SetSwapChainBuffers(m_SwapChainHandler, Textures.Keys.ToArray());
#endif
        }

        public void SetFilterMode(FilterMode filterMode)
        {
            foreach (var kv in Textures)
            {
                var rt = kv.Value;
                if (rt != null)
                    rt.filterMode = filterMode;
            }
        }

#if USING_XR_SDK
        void GfxThread_CreateOverlayTextures()
        {
            NRDebugger.Info("[NRDisplayOverlay] GfxThread_CreateOverlayTextures: targetDisplay={0}.", targetDisplay);

            // Create render texture from XR native
            // In multiview XR plugin framework, left/right eye should use same texture array target.
            bool isMultiview = NRFrame.isXRRenderMultiview;
            bool isMonoMode = NRFrame.MonoMode;
            int textureArrLen = (!isMonoMode && isMultiview) ? 2 : 0;
            UInt32 multiPassEye = 0;
            if  (!isMonoMode && !isMultiview)
            {
                multiPassEye = (UInt32)((targetDisplay == NativeDevice.LEFT_DISPLAY) ? 1 : 2);
            }

            var texturePtrs = NativeXRPlugin.CreateDisplayTextures(m_BufferSpec.bufferCount, m_EyeWidth, m_EyeHeight, textureArrLen, multiPassEye);
            for (int i = 0; i < m_BufferSpec.bufferCount; i++)
            {
                if (NRDebugger.logLevel <= LogLevel.Debug)
                    NRDebugger.Info("[NRDisplayOverlay] CreateOverlayTextures-[{0}]: {1}", i, texturePtrs[i]);
                Textures.Add(texturePtrs[i], null);
            }

            isReady = true;
            if (Textures.Count > 0)
                NRSessionManager.Instance.NRSwapChainMan.NativeSwapchain.SetSwapChainBuffers(m_SwapChainHandler, Textures.Keys.ToArray());
        }

        /// <summary> Executes the 'on render thread' operation. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        private static void RunOnRenderThread(int eventID)
        {
            if (NRDebugger.logLevel <= LogLevel.Debug)
                UnityEngine.Debug.LogFormat("[NRDisplayOverlay] RunOnRenderThread : eventID={0}", eventID);

            NRDisplayOverlay targetDisplayOverlay = null;
            if (eventID == k_CreateMonoDisplayTextures)
                targetDisplayOverlay = NRSessionManager.Instance.NRSwapChainMan.monoDisplayOverlay;
            else if (eventID == k_CreateLeftDisplayTextures)
                targetDisplayOverlay = NRSessionManager.Instance.NRSwapChainMan.leftDisplayOverlay;
            else if (eventID == k_CreateRightDisplayTextures)
                targetDisplayOverlay = NRSessionManager.Instance.NRSwapChainMan.rightDisplayOverlay;
            
            targetDisplayOverlay?.GfxThread_CreateOverlayTextures();
        }
#endif

        public override void ReleaseOverlayTextures()
        {
            if (Textures.Count == 0)
            {
                return;
            }
            
            NRDebugger.Info("[NRDisplayOverlay] ReleaseOverlayTextures: cam={0}, targetDisplay={1}.", m_RenderCamera.name, targetDisplay);
#if !USING_XR_SDK
            foreach (var item in Textures)
            {
                RenderTexture rt = item.Value as RenderTexture;
                if (rt != null) rt.Release();
            }
#endif
            Textures.Clear();
        }

        new protected void OnDestroy() {
            NRDebugger.Info("[NRDisplayOverlay] OnDestroy: cam={0}, targetDisplay={1}.", m_RenderCamera.name, targetDisplay);
            base.OnDestroy();

            m_RenderCamera.targetTexture = null;
            m_RenderCamera.enabled = false;
        }
    }
}
