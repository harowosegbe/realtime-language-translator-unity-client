/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/         
* 
*****************************************************************************/

namespace NRKernal
{
    using AOT;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using UnityEngine;
    using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary> A nr virtual displayer. </summary>
    [HelpURL("https://developer.xreal.com/develop/unity/customize-phone-controller")]
    [ScriptOrder(NativeConstants.NRVIRTUALDISPLAY_ORDER)]
    public class NRVirtualDisplayer : SingletonBehaviour<NRVirtualDisplayer>, ISystemButtonStateReceiver
    {
        /// <summary>
        /// Event queue for all listeners interested in onDisplayScreenChanged events. 
        /// </summary>
        public static event Action<Resolution> OnDisplayScreenChangedEvent;
        /// <summary> State of the system button. </summary>
        [NonSerialized] public static SystemInputState SystemButtonState = new SystemInputState();
        /// <summary> The camera. </summary>
        [SerializeField] Camera m_UICamera;
        /// <summary> The virtual controller. </summary>
        [SerializeField] MultiScreenController m_VirtualController;

        private ISystemButtonStateProvider m_ISystemButtonStateProvider;
        /// <summary> The screen resolution. </summary>
        private Vector2 m_ScreenResolution;
        /// <summary> Not supported on runtime. </summary>
        private const float ScaleFactor = 1f;

        public enum DisplayMode
        {
            None,
            Unity,
            AndroidNative
        }

        private DisplayMode m_DisplayMode = DisplayMode.AndroidNative;
        public static DisplayMode displayMode
        {
            get
            {
                if (Instance != null)
                {
                    return Instance.m_DisplayMode;
                }
                else
                {
                    return DisplayMode.AndroidNative;
                }
            }
        }

#if UNITY_EDITOR
        private static RenderTexture m_ControllerScreen;
#endif

        private NRDisplaySubsystem m_Subsystem;
        public NRDisplaySubsystem Subsystem
        {
            get
            {
                return m_Subsystem;
            }
        }
        /// <summary> Event queue for all listeners interested in OnMultiDisplayInited events. </summary>
        public event Action OnMultiDisplayInitialized;
        /// <summary> True if is initialize, false if not. </summary>
        private bool m_IsInitialized = false;

        protected override void Awake() {
            base.Awake();
            if (isDirty) return;
            
            Debug.Log("[NRVirtualDisplayer] Awake");
            NRSessionManager.Instance.VirtualDisplayer = this;
        }

        public void StartDisplay()
        {
            if (m_IsInitialized) return;
            if (isDirty) return;

            NRDebugger.Info("[NRVirtualDisplayer] Start.");

            // nrdisplay must be created after nrdevice.
            m_Subsystem = NRFrame.CreateSubsystem<NRDisplaySubsystemDescriptor, NRDisplaySubsystem>(NRDisplaySubsystemDescriptor.Name);

            Subsystem.ListenMainScrResolutionChanged(OnDisplayResolutionChanged);
            Subsystem.Start();

#if !UNITY_EDITOR && UNITY_ANDROID
            if (m_VirtualController == null)
            {
                var phoneScreenReplayceTool = FindObjectOfType<NRPhoneDisplayReplayceTool>();
                if (phoneScreenReplayceTool == null)
                {
                    NRDebugger.Info("[NRVirtualDisplayer] Use default phone sceen provider.");
                    this.BindVirtualDisplayProvider(new NRDefaultPhoneScreenProvider());
                }
                else
                {
                    NRDebugger.Info("[NRVirtualDisplayer] Use replayced phone sceen provider.");
                    this.BindVirtualDisplayProvider(phoneScreenReplayceTool.CreatePhoneScreenProvider());
                }
            }
            else
            {
                this.BindVirtualDisplayProvider(null);
            }
#else
            this.BindVirtualDisplayProvider(null);
#endif

            NRSessionManager.Instance.VirtualDisplayer = this;
            NRDebugger.Info("[NRVirtualDisplayer] Started");
            m_IsInitialized = true;
            OnMultiDisplayInitialized?.Invoke();
        }

        public void Pause()
        {
            if (!m_IsInitialized)
            {
                return;
            }

            NRDebugger.Info("[NRVirtualDisplayer] Pause");
            Subsystem?.Pause();
            NRDebugger.Info("[NRVirtualDisplayer] Paused");
        }

        public void Resume()
        {
            if (!m_IsInitialized)
            {
                return;
            }

            NRDebugger.Info("[NRVirtualDisplayer] Resume");
            Subsystem?.Resume();
            NRDebugger.Info("[NRVirtualDisplayer] Resumed");
        }

        private void Update()
        {
            if (!m_IsInitialized) return;

#if UNITY_EDITOR
            UpdateEmulator();
            if (m_VirtualController)
            {
                m_VirtualController.gameObject.SetActive(NRInput.EmulateVirtualDisplayInEditor);
            }
#endif

        }

        /// <summary> Destories this object. </summary>
        public void Destroy()
        {
            NRDebugger.Info("[NRVirtualDisplayer] Destroy");
            Subsystem?.Destroy(); 
            NRFrame.DestroySubsystem<NRDisplaySubsystemDescriptor, NRDisplaySubsystem>(NRDisplaySubsystemDescriptor.Name);
            m_Subsystem = null;
            
            m_ISystemButtonStateProvider?.Destroy();
            m_ISystemButtonStateProvider = null;
            NRDebugger.Info("[NRVirtualDisplayer] Destroyed");

#if UNITY_EDITOR
            m_ControllerScreen?.Release();
            m_ControllerScreen = null;
#endif
        }

        /// <summary> If m_VirtualController is null, use android native 
        ///           fragment as the virtual controller provider. </summary>
        private void BindVirtualDisplayProvider(NRPhoneScreenProviderBase provider)
        {
            if (m_ISystemButtonStateProvider != null)
            {
                return;
            }

            bool targetOSX = false;
#if UNITY_EDITOR
            targetOSX = EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.StandaloneOSX;
#endif
            if (Application.platform == RuntimePlatform.OSXPlayer || targetOSX)
            {
                // No virtual controller on Mac
                m_DisplayMode = DisplayMode.None;
                m_ISystemButtonStateProvider = null;
                if (m_VirtualController != null)
                {
                    m_VirtualController.gameObject.SetActive(false);
                }
            }
            else if (provider != null && m_VirtualController == null && Application.platform == RuntimePlatform.Android)
            {
                m_DisplayMode = DisplayMode.AndroidNative;
                m_ISystemButtonStateProvider = provider;
                NRDebugger.Info("[NRVirtualDisplayer] Bind android native controller");
            }
            else
            {
                m_DisplayMode = DisplayMode.Unity;
                transform.position = Vector3.one * 99999f;
                m_ISystemButtonStateProvider = m_VirtualController;
                NRDebugger.Info("[NRVirtualDisplayer] Bind unity virtual controller");
#if UNITY_EDITOR
                var canvas = transform.GetComponentInChildren<Canvas>();
                var scaler = canvas.transform.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.enabled = false;
                }
                SetVirtualDisplayResolution();
                InitEmulator();
#endif
            }

            NRMultiResumeMediator.BroadcastControllerDisplayMode((int)m_DisplayMode);
            m_ISystemButtonStateProvider?.BindReceiver(this);
        }


        public void ChangeUnityDisplayController(MultiScreenController virtualController, Camera uiCamera)
        {
            if (m_DisplayMode != DisplayMode.None)
            {
                m_DisplayMode = DisplayMode.Unity;
                transform.position = Vector3.one * 99999f;
                m_VirtualController?.gameObject.SetActive(false);
                if (m_UICamera != null)
                    m_UICamera.enabled = false;


                virtualController?.gameObject.SetActive(true);
                if(uiCamera != null)
                    uiCamera.enabled = true;
                m_UICamera = uiCamera;
                m_VirtualController = virtualController;


                NRDebugger.Info("[NRVirtualDisplayer] Change unity virtual controller");
#if UNITY_EDITOR
                var canvas = transform.GetComponentInChildren<Canvas>();
                var scaler = canvas.transform.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.enabled = false;
                }
                SetVirtualDisplayResolution();
                InitEmulator();
#endif

                NRMultiResumeMediator.BroadcastControllerDisplayMode((int)m_DisplayMode);
                m_ISystemButtonStateProvider?.BindReceiver(null);
                m_ISystemButtonStateProvider = virtualController;
                m_ISystemButtonStateProvider?.BindReceiver(this);
            }

        }

        public void ChangePhoneDisplayController(NRPhoneScreenProviderBase provider)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (m_DisplayMode != DisplayMode.None)
            {
                m_DisplayMode = DisplayMode.AndroidNative;
                m_VirtualController?.gameObject.SetActive(false);
                if (m_UICamera != null)
                    m_UICamera.enabled = false;

                m_UICamera = null;
                m_VirtualController = null;

                NRMultiResumeMediator.BroadcastControllerDisplayMode((int)m_DisplayMode);
                m_ISystemButtonStateProvider?.BindReceiver(null);
                m_ISystemButtonStateProvider = provider;
                m_ISystemButtonStateProvider?.BindReceiver(this);
            }
#endif
        }

        public void OnDataReceived(SystemButtonState state)
        {
            lock (SystemButtonState)
            {
                state.TransformTo(SystemButtonState);
            }
        }

        /// <summary> Updates the resolution described by size. </summary>
        /// <param name="size"> The size.</param>
        public void UpdateResolution(Vector2 size)
        {
#if UNITY_EDITOR
            NRPhoneScreen.Resolution = size;
            this.SetVirtualDisplayResolution();
            this.UpdateEmulatorScreen(size * ScaleFactor);
#endif

            var m_PointRaycaster = gameObject.GetComponentInChildren<NRMultScrPointerRaycaster>();
            if (m_PointRaycaster != null)
            {
                m_PointRaycaster.UpdateScreenSize(size * ScaleFactor);
            }

            if (m_ISystemButtonStateProvider != null && m_ISystemButtonStateProvider is NRPhoneScreenProviderBase)
            {
                ((NRPhoneScreenProviderBase)m_ISystemButtonStateProvider).ResizeView((int)size.x, (int)size.y);
            }
        }

#if UNITY_EDITOR
        /// <summary> Sets virtual display resolution. </summary>
        private void SetVirtualDisplayResolution()
        {
            m_ScreenResolution = NRPhoneScreen.Resolution;
            if (m_ControllerScreen != null)
            {
                m_ControllerScreen.Release();
            }

            m_ControllerScreen = new RenderTexture(
                (int)(m_ScreenResolution.x * ScaleFactor),
                (int)(m_ScreenResolution.y * ScaleFactor),
                24
            );
            m_UICamera.targetTexture = m_ControllerScreen;
            m_UICamera.aspect = m_ScreenResolution.x / m_ScreenResolution.y;
            m_UICamera.orthographicSize = 6;
        }
#endif

        /// <summary> Executes the 'display resolution changed' action. </summary>
        /// <param name="w"> The width.</param>
        /// <param name="h"> The height.</param>
        [MonoPInvokeCallback(typeof(NRDisplayResolutionCallback))]
        public static void OnDisplayResolutionChanged(int w, int h)
        {
            NRDebugger.Info("[NRVirtualDisplayer] Display resolution changed width:{0} height:{1}", w, h);
            MainThreadDispather.QueueOnMainThread(delegate ()
            {
                NRVirtualDisplayer.Instance.UpdateResolution(new Vector2(w, h));
                OnDisplayScreenChangedEvent?.Invoke(new Resolution()
                {
                    width = w,
                    height = h
                });
            });
        }

        #region Emulator
#if UNITY_EDITOR
        /// <summary> The emulator touch. </summary>
        private static Vector2 m_EmulatorTouch = Vector2.zero;
        /// <summary> The emulator phone screen anchor. </summary>
        private Vector2 m_EmulatorPhoneScreenAnchor;
        /// <summary> Width of the emulator raw image. </summary>
        private float m_EmulatorRawImageWidth;
        /// <summary> Height of the emulator raw image. </summary>
        private float m_EmulatorRawImageHeight;
        /// <summary> The emulator phone raw image. </summary>
        private RawImage emulatorPhoneRawImage;
        /// <summary> The emulator Virtual Controller. </summary>
        private GameObject emulatorVirtualController;

        /// <summary> Gets emulator screen touch. </summary>
        /// <returns> The emulator screen touch. </returns>
        public static Vector2 GetEmulatorScreenTouch()
        {
            return m_EmulatorTouch;
        }

        /// <summary> Initializes the emulator. </summary>
        private void InitEmulator()
        {
            if (emulatorVirtualController != null)
                GameObject.Destroy(emulatorVirtualController);
            emulatorVirtualController = new GameObject("NREmulatorVirtualController");
            DontDestroyOnLoad(emulatorVirtualController);
            Canvas controllerCanvas = emulatorVirtualController.AddComponent<Canvas>();
            controllerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            GameObject rawImageObj = new GameObject("RamImage");
            rawImageObj.transform.parent = controllerCanvas.transform;
            emulatorPhoneRawImage = rawImageObj.AddComponent<RawImage>();
            emulatorPhoneRawImage.raycastTarget = false;
            UpdateEmulatorScreen(m_ScreenResolution * ScaleFactor);
        }

        /// <summary> Updates the emulator screen described by size. </summary>
        /// <param name="size"> The size.</param>
        private void UpdateEmulatorScreen(Vector2 size)
        {
            float scaleRate = 0.18f;
            m_EmulatorRawImageWidth = size.x * scaleRate;
            m_EmulatorRawImageHeight = size.y * scaleRate;
            emulatorPhoneRawImage.rectTransform.pivot = Vector2.right;
            emulatorPhoneRawImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0f, 0f);
            emulatorPhoneRawImage.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0f, 0f);
            emulatorPhoneRawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_EmulatorRawImageWidth);
            emulatorPhoneRawImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_EmulatorRawImageHeight);
            emulatorPhoneRawImage.texture = m_ControllerScreen;

            Vector2 gameViewSize = Handles.GetMainGameViewSize();
            m_EmulatorPhoneScreenAnchor = new Vector2(gameViewSize.x - m_EmulatorRawImageWidth, 0f);
        }

        /// <summary> Updates the emulator. </summary>
        private void UpdateEmulator()
        {
            if (NRInput.EmulateVirtualDisplayInEditor
                && Input.GetMouseButton(0)
                && Input.mousePosition.x > m_EmulatorPhoneScreenAnchor.x
                && Input.mousePosition.y < (m_EmulatorPhoneScreenAnchor.y + m_EmulatorRawImageHeight))
            {
                m_EmulatorTouch.x = (Input.mousePosition.x - m_EmulatorPhoneScreenAnchor.x) / m_EmulatorRawImageWidth * 2f - 1f;
                m_EmulatorTouch.y = (Input.mousePosition.y - m_EmulatorPhoneScreenAnchor.y) / m_EmulatorRawImageHeight * 2f - 1f;
            }
            else
            {
                m_EmulatorTouch = Vector2.zero;
            }
        }
#endif
        #endregion

        protected override void OnDestroy()
        {
            if (isDirty) return;
            NRDebugger.Info("[NRVirtualDisplayer] OnDestroy.");
            base.OnDestroy();
            Destroy();
#if UNITY_EDITOR
            if (emulatorVirtualController != null)
            {
                GameObject.Destroy(emulatorVirtualController);
                emulatorVirtualController = null;
            }
#endif
        }
    }
}
