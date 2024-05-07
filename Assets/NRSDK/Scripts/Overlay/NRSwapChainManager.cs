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

// #define POPULATE_IN_LATEUPDATE

namespace NRKernal
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.Assertions;
    using AOT;
    using UnityEngine.Rendering;

    public enum LayerTextureType
    {
        RenderTexture,
        StandardTexture,
        EglTexture
    }

    public struct BufferSpec
    {
        public NativeResolution size;
        public NRTextureFormat colorFormat;
        public NRTextureFormat depthFormat;
        public int surfaceFlag;
        public UInt64 createFlag;
        public int samples;
        // Number of render buffers
        public int bufferCount;
        public bool useTextureArray;

        public void Copy(BufferSpec bufferspec)
        {
            this.size = bufferspec.size;
            this.colorFormat = bufferspec.colorFormat;
            this.depthFormat = bufferspec.depthFormat;
            this.samples = bufferspec.samples;
            this.bufferCount = bufferspec.bufferCount;
            this.surfaceFlag = bufferspec.surfaceFlag;
            this.useTextureArray = bufferspec.useTextureArray;
        }

        public override string ToString()
        {
            return string.Format("[size:{0} bufferCount:{1}, surfaceFlag:{2}, createFlag:{3}, useTextureArray:{4}]"
                , size.ToString(), bufferCount, surfaceFlag, createFlag, useTextureArray);
        }
    }

    public enum LayerSide
    {
        Left = 0,
        Right = 1,
        Both = 2,
        [HideInInspector]
        Count = Both
    };

    public struct ViewPort
    {
        public bool is3DLayer;
        public bool isExternalSurface;
        public int index;
        public UInt64 nativeHandler;
        public UInt64 swapchainHandler;
        public NativeRectf sourceUV;
        public NativeDevice targetDisplay;
        public NRViewportType viewportType;
        public NRReferenceSpaceType spaceType;
        // public Matrix4x4 transform;
        public NativeTransform nativePose;
        public Vector2 quadSize;
        public NativeFov4f fov;
        public int textureArraySlice;

        public override string ToString()
        {
            return string.Format("[index:{0} nativeHandler:{1} swapchainHandler:{2} targetDisplay:{3} viewportType:{4} spaceType:{5} is3DLayer:{6} isExternalSurface:{7} textureArraySlice:{8} nativePose:{9} sourceUV:{10} fov:{11}]",
                index, nativeHandler, swapchainHandler, targetDisplay, viewportType, spaceType, is3DLayer, isExternalSurface, textureArraySlice, nativePose, sourceUV.ToString(), fov.ToString());
        }
    };

    public class NRSwapChainManager : SingletonBehaviour<NRSwapChainManager>
    {
        private List<OverlayBase> Overlays = new List<OverlayBase>();
        private UInt64 m_RenderHandle;

        public UInt64 RenderHandler
        {
            get { return m_RenderHandle; }
        }
        internal NativeSwapchain NativeSwapchain { get; set; }
        private UInt64 m_FrameHandle = 0;
        public UInt64 FrameHandle
        {
            get { return m_FrameHandle; }
        }


        /// <summary> Gets or sets the nr renderer. </summary>
        /// <value> The nr renderer. </value>
        public NRRenderer NRRenderer { get; set; }

        private UInt64 m_CachFrameHandle = 0;
        private Dictionary<UInt64, IntPtr> m_LayerWorkingBufferDict;
        private Queue<TaskInfo> m_TaskQueue;

        /// <summary>
        /// Max overlay count is MaxOverlayCount-2, the two overlay are left display and right display.
        /// </summary>
        private const int MaxOverlayCount = 7;

        private struct TaskInfo
        {
            public Action<OverlayBase> callback;
            public OverlayBase obj;
        }

        internal Action onBeforeSubmitFrameInMainThread;

        private bool started { get; set; }

        private AndroidJavaObject m_ProtectedCodec;
        protected AndroidJavaObject ProtectedCodec
        {
            get
            {
                if (m_ProtectedCodec == null)
                {
                    m_ProtectedCodec = new AndroidJavaObject("ai.nreal.protect.session.ProtectSession");
                }

                return m_ProtectedCodec;
            }
        }

        private Camera m_ShadowCamera;
        
        private NRDisplayOverlay m_LeftDisplayOverlay;
        internal NRDisplayOverlay leftDisplayOverlay
        {
            get { return m_LeftDisplayOverlay; }
        }
        private NRDisplayOverlay m_RightDisplayOverlay;
        internal NRDisplayOverlay rightDisplayOverlay
        {
            get { return m_RightDisplayOverlay; }
        }

        private NRDisplayOverlay m_MonoDisplayOverlay;
        internal NRDisplayOverlay monoDisplayOverlay
        {
            get { return m_MonoDisplayOverlay; }
        }

        /// <summary> Renders the event delegate described by eventID. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        private delegate void RenderEventDelegate(int eventID);
        /// <summary> Handle of the render thread. </summary>
        private static RenderEventDelegate RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        /// <summary> The render thread handle pointer. </summary>
        private static IntPtr RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);

        private const int k_SwapChainEventStart = 0x0001;
        private const int k_SwapChainEventSubmit = 0x0002;

        private const float m_DefaultFocusDistance = 1.4f;

        private Vector3 m_FocusPoint = new Vector3(0, 0, m_DefaultFocusDistance);
        private Vector3 m_FocusPointGL = new Vector3(0, 0, -m_DefaultFocusDistance);
        public Vector3 FocusPoint
        {
            get { return m_FocusPoint; }
        }

        private Vector3 m_FocusPlaneNorm = -Vector3.forward;
        private Vector3 m_FocusPlaneNormGL = Vector3.forward;
        public Vector3 FocusPlaneNorm
        {
            get { return m_FocusPlaneNorm; }
        }

        private NRReferenceSpaceType m_FocusPlaneSpaceType = NRReferenceSpaceType.NR_REFERENCE_SPACE_VIEW;
        public NRReferenceSpaceType FocusPlaneSpaceType
        {
            get { return m_FocusPlaneSpaceType; }
        }

        private NRFrameFlags m_FrameChangedType = NRFrameFlags.NR_FRAME_CHANGED_NONE;

        /// <summary> Get the status of whether display overlay is being showing. </summary>
        public bool isDisplayOverlayShow
        {
            private set;
            get;
        } = false;


        public void Create()
        {
            NRDebugger.Info("[SwapChain] Create");
            m_LayerWorkingBufferDict = new Dictionary<UInt64, IntPtr>();
            m_TaskQueue = new Queue<TaskInfo>();

#if !UNITY_EDITOR 
#if USING_XR_SDK
            NativeXRPlugin.RegistEventCallback(OnNativeError, OnGfxThreadStart, OnGfxThreadPopulateFrame, OnGfxThreadSubmit);
            NRSessionManager.Instance.NativeAPI.NativeRenderring.Create(NativeXRPlugin.GetRenderHandle());
#else
            if (gameObject.GetComponent<NRRenderer>() == null)
                NRRenderer = gameObject.AddComponent<NRRenderer>();
            NRRenderer.Create();
#endif
            StartCoroutine(RenderCoroutine());
#endif
        }

        public void StartSwapChain()
        {
            NRDebugger.Info("[SwapChain] Start");

#if !UNITY_EDITOR
#if USING_XR_SDK
            NativeXRPlugin.SetMonoMode(NRFrame.MonoMode);
#else
            NRRenderer?.Start();
#endif
#endif
            if (Application.isPlaying)
            {
                InitDisplayOverlay();
            }

        }

        void InitDisplayOverlay()
        {
#if USING_XR_SDK || UNITY_EDITOR
            var centerCamera = NRSessionManager.Instance.NRHMDPoseTracker.centerCamera;
            if (NRFrame.MonoMode || NRFrame.isXRRenderMultiview)
            {
                var cOverlay = centerCamera.gameObject.GetComponent<NRDisplayOverlay>();
                if (cOverlay == null)
                {
                    cOverlay = centerCamera.gameObject.AddComponent<NRDisplayOverlay>();
                    cOverlay.targetDisplay = NativeDevice.HEAD_CENTER;
                }

                m_MonoDisplayOverlay = cOverlay;
            }
            else
            {
                var overlays = centerCamera.gameObject.GetComponents<NRDisplayOverlay>();
                if (overlays != null)
                {
                    for (int i = 0; i < overlays.Length; i++)
                        Destroy(overlays[i]);
                }
                
                var leftOverlay = centerCamera.gameObject.AddComponent<NRDisplayOverlay>();
                leftOverlay.targetDisplay = NativeDevice.LEFT_DISPLAY;
                m_LeftDisplayOverlay = leftOverlay;
                
                var rightOverlay = centerCamera.gameObject.AddComponent<NRDisplayOverlay>();
                rightOverlay.targetDisplay = NativeDevice.RIGHT_DISPLAY;
                m_RightDisplayOverlay = rightOverlay;
            }
#else
            if (NRFrame.MonoMode)
            {
                var centerCamera = NRSessionManager.Instance.NRHMDPoseTracker.centerCamera;
                var cOverlay = centerCamera.gameObject.GetComponent<NRDisplayOverlay>();
                if (cOverlay == null)
                {
                    cOverlay = centerCamera.gameObject.AddComponent<NRDisplayOverlay>();
                    cOverlay.targetDisplay = NativeDevice.HEAD_CENTER;
                }
                m_MonoDisplayOverlay = cOverlay;
            }
            else
            {
                var leftCamera = NRSessionManager.Instance.NRHMDPoseTracker.leftCamera;
                var lOverlay = leftCamera.gameObject.GetComponent<NRDisplayOverlay>();
                if (lOverlay == null)
                {
                    lOverlay = leftCamera.gameObject.AddComponent<NRDisplayOverlay>();
                    lOverlay.targetDisplay = NativeDevice.LEFT_DISPLAY;
                }
                m_LeftDisplayOverlay = lOverlay;

                var rightCamera = NRSessionManager.Instance.NRHMDPoseTracker.rightCamera;
                var rOverlay = rightCamera.gameObject.GetComponent<NRDisplayOverlay>();
                if (rOverlay == null)
                {
                    rOverlay = rightCamera.gameObject.AddComponent<NRDisplayOverlay>();
                    rOverlay.targetDisplay = NativeDevice.RIGHT_DISPLAY;
                }
                m_RightDisplayOverlay = rOverlay;
            }
#endif

            if (m_ShadowCamera != null)
            {
                m_ShadowCamera.enabled = false;
            }

            isDisplayOverlayShow = true;
        }

        void DestroyDisplayOverlay()
        {
            NRDebugger.Info("[SwapChain] DestroyDisplayOverlay");
            isDisplayOverlayShow = false;
            if (m_MonoDisplayOverlay != null)
            {
                Destroy(m_MonoDisplayOverlay);
                m_MonoDisplayOverlay = null;
            }

            if (m_LeftDisplayOverlay != null)
            {
                Destroy(m_LeftDisplayOverlay);
                m_LeftDisplayOverlay = null;
            }
            if (m_RightDisplayOverlay != null)
            {
                Destroy(m_RightDisplayOverlay);
                m_RightDisplayOverlay = null;
            }

#if !USING_XR_SDK
            if (m_ShadowCamera == null)
            {
                m_ShadowCamera = gameObject.AddComponent<Camera>();
                m_ShadowCamera.depth = 0;
                m_ShadowCamera.cullingMask = 0;
                m_ShadowCamera.clearFlags = CameraClearFlags.Nothing;

                // The render order of offline camera is top of online camera.
                var rt = UnityExtendedUtility.CreateRenderTexture(10, 10);
                m_ShadowCamera.targetTexture = rt;
            }
            m_ShadowCamera.enabled = true;
#endif
        }

        public void ShowDisplayOverlay(bool show)
        {
            NRDebugger.Info("[SwapChain] ShowDisplayOverlay: {0}", show);
            if (show)
                InitDisplayOverlay();
            else
                DestroyDisplayOverlay();
        }

        public void Pause()
        {
            NRRenderer?.Pause();
        }

        public void Resume()
        {
            NRRenderer?.Resume();
        }

#if !USING_XR_SDK && !POPULATE_IN_LATEUPDATE
        void OnEnable()
        {
            NRDebugger.Info("[SwapChain] OnEnable, built-in pipeline: {0}", GraphicsSettings.currentRenderPipeline == null);
            if (GraphicsSettings.currentRenderPipeline == null)
                Camera.onPreRender += OnPreRenderCallback;
            else
                RenderPipelineManager.beginCameraRendering += OnBeginRenderCallback;
        }

        void OnDisable()
        {
            NRDebugger.Info("[SwapChain] OnDisable, built-in pipeline: {0}", GraphicsSettings.currentRenderPipeline == null);
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
            if (!isRunning)
                return;

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[SwapChain] OnPreRenderImp: {0}, {1}, depth={2}", Time.frameCount, camera.name, camera.depth);

            // populate frame once only:
            //  1. if it's shadowCamera, always do it;
            //  2. if it's monoMode now, only do it for mono camera;
            //  3. if else, only do ite for left camera;
            bool doPopulate = false;
            if (camera == m_ShadowCamera)
                doPopulate = true;
            else if (NRFrame.MonoMode)
                doPopulate = m_MonoDisplayOverlay != null && camera == m_MonoDisplayOverlay.renderCamera;
            else
                doPopulate = m_LeftDisplayOverlay != null && camera == m_LeftDisplayOverlay.renderCamera;
            
            if (doPopulate)
                PopulateFrame();
        }
#endif

        [MonoPInvokeCallback(typeof(OnNativeErrorCallback))]
        private static void OnNativeError(NativeResult result, string funcName, bool needthrowerror)
        {
            NRDebugger.Info("[SwapChain] OnNativeError: {0}, {1}, {2}", result, funcName, needthrowerror);
            try
            {
                NativeErrorListener.Check((NativeResult)result, NRSessionManager.Instance.NRSwapChainMan, funcName, needthrowerror);
            }
            catch (Exception ex)
            {
                NRDebugger.Error("OnNativeError: {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        [MonoPInvokeCallback(typeof(OnGfxThreadStartCallback))]
        private static void OnGfxThreadStart(UInt64 renderingHandle)
        {
            try
            {
                NRSessionManager.Instance.NRSwapChainMan.GfxThreadStart(renderingHandle);
            }
            catch (Exception ex)
            {
                NRDebugger.Error("OnGfxThreadStart: {0}\n{1}", ex.Message, ex.StackTrace);
            }
        }

        [MonoPInvokeCallback(typeof(OnGfxThreadPopulateFrameCallback))]
        private static void OnGfxThreadPopulateFrame()
        {
#if !POPULATE_IN_LATEUPDATE
            try
            {
                NRSessionManager.Instance.NRSwapChainMan.PopulateFrame();
            }
            catch (Exception ex)
            {
                NRDebugger.Error("OnGfxThreadPopulateFrame: {0}\n{1}", ex.Message, ex.StackTrace);
            }
#endif
        }

        [MonoPInvokeCallback(typeof(OnGfxThreadSubmitCallback))]
        private static void OnGfxThreadSubmit(UInt64 frameHandle)
        {
            // NRSessionManager.Instance.NRSwapChainMan.GfxThread_SubmitFrame(frameHandle);
        }

        internal void GfxThreadStart(UInt64 renderHandler)
        {
            if (started)
            {
                return;
            }

            NRDebugger.Info("[SwapChain] GfxThreadStart: renderHandler={0}", renderHandler);
            m_RenderHandle = renderHandler;
            NativeSwapchain = new NativeSwapchain(renderHandler);
            started = true;
        }

        internal void SetRefreshScreen(bool refresh)
        {
            if (NativeSwapchain != null)
            {
                NRDebugger.Info($"[SwapChain] Begin SetRefreshScreen {refresh}");
                NativeSwapchain.SetRefreshScreen(refresh);
                NRDebugger.Info($"[SwapChain] Finish SetRefreshScreen {refresh}");
            }
        }

        void Update()
        {
            if (isDirty) return;
            if (!isRunning)
            {
#if !UNITY_EDITOR
                NRDebugger.Warning("[SwapChain] not ready: started={0}, sessionStatus={1}", started, NRFrame.SessionStatus);
#endif
                return;
            }

            if (m_TaskQueue.Count != 0)
            {
                while (m_TaskQueue.Count != 0)
                {
                    var task = m_TaskQueue.Dequeue();
                    task.callback.Invoke(task.obj);
                }
            }
        }

#if POPULATE_IN_LATEUPDATE
        private void LateUpdate()
        {
            if (isDirty) return;
            if (!isRunning)
                return;
            
            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[SwapChain] LateUpdate: {0}", Time.frameCount * 100);
            PopulateFrame();
        }
#endif

        /// <summary> Populate the frame. </summary>
        internal void PopulateFrame()
        {
            if (!isRunning)
                return;

#if USING_XR_SDK
            // wait for display layer has created texture
            bool isReady = true;
            if (m_MonoDisplayOverlay != null)
                isReady &= m_MonoDisplayOverlay.isReady;
            if (m_LeftDisplayOverlay != null)
                isReady &= m_LeftDisplayOverlay.isReady;
            if (m_RightDisplayOverlay != null)
                isReady &= m_RightDisplayOverlay.isReady;

            if (!isReady)
                return;
#endif

            if (!AnyOverlayActive())
            {
                return;
            }

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[SwapChain] PopulateFrame: {0}", Time.frameCount);

            if (Overlays.Count != 0)
            {
#if !UNITY_EDITOR
                PopulateOverlaysRenderBuffers();
                PopulateBufferViewports();
                PopulateFrameInfo();
#endif
            }
        }

        #region common functions
        internal bool isRunning
        {
            get
            {
#if UNITY_EDITOR
                return started;
#else
#if USING_XR_SDK
                return started && NRFrame.SessionStatus == SessionState.Running;
#else
                return started && NRFrame.SessionStatus == SessionState.Running && NRRenderer.CurrentState == NRRenderer.RendererState.Running;
#endif
#endif
            }
        }

        internal void Add(OverlayBase overlay)
        {
            if (Overlays.Contains(overlay))
            {
                NRDebugger.Warning("[SwapChain] Overlay has been existed: " + overlay.SwapChainHandler);
                return;
            }

            if (Overlays.Count == MaxOverlayCount)
            {
                throw new NotSupportedException("The current count of overlays exceeds the maximum!");
            }

            if (isRunning)
            {
                AddLayer(overlay);
            }
            else
            {
                m_TaskQueue.Enqueue(new TaskInfo()
                {
                    callback = AddLayer,
                    obj = overlay
                });
            }
        }

        internal void Remove(OverlayBase overlay)
        {
            if (!Overlays.Contains(overlay))
            {
                return;
            }

            if (isRunning)
            {
                RemoveLayer(overlay);
            }
            else
            {
                m_TaskQueue.Enqueue(new TaskInfo()
                {
                    callback = RemoveLayer,
                    obj = overlay
                });
            }
        }

        private void AddLayer(OverlayBase overlay)
        {
            Overlays.Add(overlay);
            Overlays.Sort();

            BufferSpec bufferSpec = overlay.BufferSpec;

            bool isExternalSurface = overlay.isExternalSurface;

#if !UNITY_EDITOR
            overlay.NativeSpecHandler = NativeSwapchain.CreateBufferSpec(overlay.BufferSpec);

            if (isExternalSurface)
            {
                IntPtr androidSurface = IntPtr.Zero;
                overlay.SwapChainHandler = NativeSwapchain.CreateSwapchainAndroidSurface(overlay.NativeSpecHandler, ref androidSurface);
                overlay.SurfaceId = androidSurface;
                NRDebugger.Info("[SwapChain] AddLayer externalSurface name:{0} SurfaceId:{1}", overlay.gameObject.name, androidSurface);
            }
            else
            {
                overlay.SwapChainHandler = NativeSwapchain.CreateSwapchain(overlay.NativeSpecHandler);
            }
            
            bufferSpec.bufferCount = NativeSwapchain.GetRecommandBufferCount(overlay.SwapChainHandler);
#else
            bufferSpec.bufferCount = 0;
#endif

            overlay.BufferSpec = bufferSpec;

            NRDebugger.Info("[SwapChain] AddLayer: name={0}, bufferSpec={1}, isExternalSurface={2}", overlay.gameObject.name, bufferSpec.ToString(), isExternalSurface);

            overlay.CreateOverlayTextures();
            overlay.CreateViewport();
            UpdateProtectContentSetting();
        }

        private void RemoveLayer(OverlayBase overlay)
        {
            NRDebugger.Info("[SwapChain] RemoveLayer: name={0}", overlay.gameObject.name);
            overlay.ReleaseOverlayTextures();

#if !UNITY_EDITOR
            NativeSwapchain.DestroyBufferSpec(overlay.NativeSpecHandler);
            var viewports = overlay.ViewPorts;
            for (int i = 0; i < viewports.Length; i++)
            {
                if (viewports[i].nativeHandler != 0)
                {
                    NativeSwapchain.DestroyBufferViewPort(viewports[i].nativeHandler);
                }
            }
            NativeSwapchain.DestroySwapChain(overlay.SwapChainHandler);
#endif

            Overlays.Remove(overlay);
            UpdateOverlayViewPortIndex();
            UpdateProtectContentSetting();
        }
        private bool AnyOverlayActive()
        {
            for (var i = 0; i < Overlays.Count; ++i)
            {
                if (Overlays[i].IsActive)
                {
                    return true;
                }
            }
            return false;
        }

        private bool useProtectContent = false;
        private void UpdateProtectContentSetting()
        {
            bool flag = false;
            for (int i = 0; i < Overlays.Count; i++)
            {
                var overlay = Overlays[i];
                if (overlay.isProtectedContent)
                {
                    flag = true;
                    break;
                }
            }

            if (flag != useProtectContent)
            {
                NRDebugger.Info("[SwapChain] Protect content setting changed.");
                try
                {
                    AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    var unityActivity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    if (flag)
                    {
                        NRDebugger.Info("[SwapChain] Use protect content.");
                        ProtectedCodec.Call("start", unityActivity);
                    }
                    else
                    {
                        NRDebugger.Info("[SwapChain] Use un-protect content.");
                        ProtectedCodec.Call("stop", unityActivity);
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }

                useProtectContent = flag;
            }
        }

        private void PrintOverlayInfo()
        {
            NRDebugger.Info("[SwapChain] Overlay info: cnt={0}", Overlays.Count);
            for (int i = 0; i < Overlays.Count; i++)
            {
                var overlay = Overlays[i];
                if (overlay.IsActive)
                {
                    NRDebugger.Info("[Overlay-{0}] {1}\n", i, overlay.ToString());
                }
            }
        }

        private void UpdateOverlayViewPortIndex()
        {
            int index = 0;
            for (int j = 0; j < Overlays.Count; j++)
            {
                var overlay = Overlays[j];
                if (overlay.IsActive)
                {
                    var viewports = overlay.ViewPorts;
                    Assert.IsTrue(viewports != null && viewports.Length >= 1);
                    int count = viewports.Length;
                    for (int i = 0; i < count; i++)
                    {
                        viewports[i].index = index;
                        index++;
                    }
                }
            }
            PrintOverlayInfo();
        }
        #endregion

        #region viewport
        private void PopulateBufferViewports()
        {
            for (int i = 0; i < Overlays.Count; i++)
            {
                var overlay = Overlays[i];
                if (overlay.IsActive)
                {
                    overlay.PopulateViewPort();
                }
            }
        }

        public void CreateBufferViewport(ref ViewPort viewport)
        {
#if !UNITY_EDITOR
            viewport.nativeHandler = NativeSwapchain.CreateBufferViewport();
#endif
            int perceptionId = NRSessionManager.Instance.NativeAPI.PerceptionId;
            if (viewport.is3DLayer)
            {
                NativeSwapchain.Populate3DBufferViewportData(viewport.nativeHandler, viewport.swapchainHandler,
                    ref viewport.sourceUV, viewport.targetDisplay, viewport.isExternalSurface,
                    viewport.viewportType, viewport.spaceType, ref viewport.nativePose, viewport.fov, viewport.textureArraySlice, perceptionId);
            }
            else
            {
                NativeSwapchain.PopulateBufferViewportData(viewport.nativeHandler, viewport.swapchainHandler,
                    ref viewport.sourceUV, viewport.targetDisplay,
                    viewport.viewportType, viewport.spaceType, ref viewport.nativePose, viewport.quadSize, perceptionId);
            }

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[SwapChain] CreateBufferViewport:{0}", viewport.ToString());
            UpdateOverlayViewPortIndex();
        }


        /// <summary>
        /// Sync viewport setting with native.
        /// </summary>
        public void PopulateViewportData(ref ViewPort viewport)
        {
            if (viewport.index == -1)
            {
                NRDebugger.Error("[SwapChain] PopulateViewportData index error: {0}", viewport.ToString());
                return;
            }

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[SwapChain] PopulateViewportData:{0}", viewport.ToString());

            var targetDisplay = viewport.targetDisplay;
            int perceptionId = NRSessionManager.Instance.NativeAPI.PerceptionId;
            if (viewport.is3DLayer)
            {
                NativeSwapchain.Populate3DBufferViewportData(viewport.nativeHandler, viewport.swapchainHandler,
                    ref viewport.sourceUV, targetDisplay, viewport.isExternalSurface,
                    viewport.viewportType, viewport.spaceType, ref viewport.nativePose, viewport.fov, viewport.textureArraySlice, perceptionId);

                NativeSwapchain.SetBufferViewportFocusPlane(viewport.nativeHandler, m_FocusPlaneSpaceType, m_FocusPointGL, m_FocusPlaneNormGL);
            }
            else
            {
                NativeSwapchain.PopulateBufferViewportData(viewport.nativeHandler, viewport.swapchainHandler,
                    ref viewport.sourceUV, targetDisplay,
                    viewport.viewportType, viewport.spaceType, ref viewport.nativePose, viewport.quadSize, perceptionId);
            }
            NativeSwapchain.SetBufferViewPort(m_FrameHandle, viewport.index, viewport.nativeHandler);
            // NRDebugger.Info("[SwapChain] PopulateViewportData:{0}", viewport.ToString());
        }

        public void DestroyBufferViewPort(UInt64 viewportHandle)
        {
            NativeSwapchain.DestroyBufferViewPort(viewportHandle);
            UpdateOverlayViewPortIndex();
        }
        #endregion

        #region submit frame

        private List<UInt64> mDynLayers = new List<UInt64>();
        private UInt64[] mDynLayerHandlerArr = null;
        private IntPtr[] mDynLayerWorkingBufferArr = null;
        private List<UInt64> GetDynamicLayerHandlers()
        {
            mDynLayers.Clear();
            for (int i = 0; i < Overlays.Count; i++)
            {
                var overlay = Overlays[i];
                if (overlay.IsActive)
                {
                    if (overlay.isDynamic && overlay.isReady)
                    {
                        mDynLayers.Add(overlay.SwapChainHandler);
                    }
                }
            }
            return mDynLayers;
        }

        /// <summary>
        /// Populate overlays's render buffers. 
        /// </summary>
        private void PopulateOverlaysRenderBuffers()
        {
            var swapchainHandlers = GetDynamicLayerHandlers();

            if (mDynLayerHandlerArr == null || mDynLayerHandlerArr.Length != swapchainHandlers.Count)
            {
                mDynLayerHandlerArr = swapchainHandlers.ToArray();
                mDynLayerWorkingBufferArr = new IntPtr[swapchainHandlers.Count];
            }
            else
            {
                for (int i = 0; i < swapchainHandlers.Count; i++)
                {
                    mDynLayerHandlerArr[i] = swapchainHandlers[i];
                    mDynLayerWorkingBufferArr[i] = IntPtr.Zero;
                }
            }

            NativeSwapchain.AcquireFrame(ref m_FrameHandle, mDynLayerHandlerArr, mDynLayerWorkingBufferArr);
            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[SwapChain] AcquireFrame: frameHandle={0}, layerNum={1}", m_FrameHandle, mDynLayerHandlerArr.Length);

#if USING_XR_SDK
            NativeXRPlugin.PopulateFrameHandle(m_FrameHandle);
#endif

            m_LayerWorkingBufferDict.Clear();
            for (int i = 0; i < mDynLayerWorkingBufferArr.Length; i++)
            {
                if (NRDebugger.logLevel <= LogLevel.Debug)
                    NRDebugger.Info("[SwapChain] AcquireFrame buffers: swapChain={0}, workingBuffer={1}", swapchainHandlers[i], mDynLayerWorkingBufferArr[i]);
                m_LayerWorkingBufferDict[swapchainHandlers[i]] = mDynLayerWorkingBufferArr[i];
            }

            for (int i = 0; i < Overlays.Count; i++)
            {
                var overlay = Overlays[i];
                if (!overlay.IsActive)
                    continue;

                overlay.PopulateBuffers(GetWorkingBufferHandler(overlay.SwapChainHandler));
            }
        }

        /// <summary>
        /// Populate frame info.
        /// </summary>
        private void PopulateFrameInfo()
        {
            if (m_FrameHandle == 0)
                return;

            NativeSwapchain.PopulateFrameInfo(m_FrameHandle, NRFrame.CurrentPoseTimeStamp);
        }

        public IntPtr GetWorkingBufferHandler(UInt64 swapchainHandler)
        {
            IntPtr bufferid;
            if (!m_LayerWorkingBufferDict.TryGetValue(swapchainHandler, out bufferid))
            {
                return IntPtr.Zero;
            }

            return bufferid;
        }

        internal void GfxThread_SubmitFrame(UInt64 frameHandle)
        {
            if (!isRunning || frameHandle == 0)
            {
                NRDebugger.Warning("[SwapChain] GfxThread_SubmitFrame Can not submit frame! isRunning={0}, frameHandle={1}", isRunning, frameHandle);
                return;
            }
            if (!AnyOverlayActive())
            {
                return;
            }

            if (NRDebugger.logLevel <= LogLevel.Debug)
            {
                NRDebugger.Info("[SwapChain] GfxThread_SubmitFrame started:{0} newFrameHandle:{1} submitFrameHandle:{2} curPresentTime:{3}",
                    started, m_FrameHandle, frameHandle, NRFrame.CurrentPoseTimeStamp);

                if (frameHandle != m_FrameHandle)
                    NRDebugger.Warning("[SwapChain] GfxThread_SubmitFrame frame handle not match");
            }

            NativeSwapchain.SubmitFrame(frameHandle);
        }

        public void UpdateExternalSurface(UInt64 swapchainHandler, int transformCount, NativeTransform[] transforms, NativeDevice[] targetEyes,
            Int64 timestamp, int frameIndex)
        {
            NativeSwapchain.UpdateExternalSurface(swapchainHandler, transformCount, transforms, targetEyes, timestamp, frameIndex);
        }

        public void SetSwapChainBuffers(UInt64 swapchainHandle, IntPtr[] bufferHandler)
        {
            NativeSwapchain.SetSwapChainBuffers(swapchainHandle, bufferHandler);
        }
        #endregion


        /// <summary> Set a plane in camera or global space that acts as the focal plane of the Scene for this frame. </summary>
        /// <param name="point"> The position of the focal point.</param>
        /// <param name="normal"> The normal of the plane being viewed at the focal point.</param>
        /// <param name="spaceType"> Space type of the plane. While NR_REFERENCE_SPACE_VIEW means that the plane is relative to the Camera, NR_REFERENCE_SPACE_GLOBAL means that the plane is in world space.</param>
        public void SetFocusPlane(Vector3 point, Vector3 normal, NRReferenceSpaceType spaceType)
        {
            m_FocusPoint = point;
            m_FocusPointGL = new Vector3(point.x, point.y, -point.z);

            m_FocusPlaneNorm = normal;
            m_FocusPlaneNormGL = new Vector3(normal.x, normal.y, -normal.z);

            m_FocusPlaneSpaceType = spaceType;
            m_FrameChangedType = NRFrameFlags.NR_FRAME_CHANGED_FOCUS_PLANE;
        }

        /// <summary> The renders coroutine. </summary>
        /// <returns> An IEnumerator. </returns>
        private IEnumerator RenderCoroutine()
        {
            WaitForEndOfFrame delay = new WaitForEndOfFrame();

            while (true)
            {
                yield return delay;
#if !USING_XR_SDK
                if (!started && NRFrame.SessionStatus == SessionState.Running && NRRenderer.CurrentState == NRRenderer.RendererState.Running)
                {
                    NRDebugger.Info("[SwapChain] RenderCoroutine: k_SwapChainEventStart");
                    GL.IssuePluginEvent(RenderThreadHandlePtr, k_SwapChainEventStart);
                    continue;
                }
#endif

                if (!isRunning)
                    continue;

                onBeforeSubmitFrameInMainThread?.Invoke();
                var evt = Time.frameCount * 100 + k_SwapChainEventSubmit;
                if (NRDebugger.logLevel <= LogLevel.Debug)
                    NRDebugger.Info("[SwapChain] RenderCoroutine: k_SwapChainEventSubmit, evt={2}, frameHandle={0} => {1}", m_CachFrameHandle, m_FrameHandle, evt);

                m_CachFrameHandle = m_FrameHandle;
                GL.IssuePluginEvent(RenderThreadHandlePtr, evt);
            }
        }

        /// <summary> Executes the 'on render thread' operation. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        private static void RunOnRenderThread(int eventID)
        {
            if (NRDebugger.logLevel <= LogLevel.Debug)
                UnityEngine.Debug.LogFormat("[SwapChain] RunOnRenderThread: eventID={0}", eventID);

            eventID %= 100;
            if (eventID == k_SwapChainEventStart)
            {
                OnGfxThreadStart(NRRenderer.NativeRenderring.RenderingHandle);
            }
            else if (eventID == k_SwapChainEventSubmit)
            {
                var swapChainMan = NRSessionManager.Instance.NRSwapChainMan;
                swapChainMan.GfxThread_SubmitFrame(swapChainMan.m_CachFrameHandle);
            }
        }

        internal void NRDebugSaveToPNG()
        {
            NRDebugger.Info("NRDebugSaveToPNG.");

#if USING_XR_SDK
            NRDebugger.Warning("NRSDK don't support to save png on XR_SDK mode currently.");
            return;
#else
            if (m_LeftDisplayOverlay != null)
            {
                m_LeftDisplayOverlay.NRDebugSaveToPNG();
            }
            if (m_RightDisplayOverlay != null)
            {
                m_RightDisplayOverlay.NRDebugSaveToPNG();
            }

            if (m_MonoDisplayOverlay != null)
            {
                m_MonoDisplayOverlay.NRDebugSaveToPNG();
            }
#endif
        }

        public void Destroy()
        {
            Overlays.Clear();
            NRRenderer?.Destroy();
            started = false;
        }

        protected override void OnDestroy()
        {
            if (isDirty) return;
            NRDebugger.Info("[NRSwapChainManager] OnDestroy.");
            Destroy();

            base.OnDestroy();
        }

        public void WakeUpNextFrame()
        {
#if !UNITY_EDITOR && USING_XR_SDK
            NativeXRPlugin.WakeUpNextFrame();
#endif
        }

        public void SetDisplayRTFilterMode(FilterMode filterMode)
        {
            m_LeftDisplayOverlay?.SetFilterMode(filterMode);
            m_RightDisplayOverlay?.SetFilterMode(filterMode);
            m_MonoDisplayOverlay?.SetFilterMode(filterMode);
        }
    }
}