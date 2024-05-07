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
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using NRKernal.Record;

#if USING_XR_SDK 
    using UnityEngine.XR;
    using System.Runtime.InteropServices;
#endif

    using NRNotificationType = NRNotificationListener.NRNotificationType;
    using Notification = NRNotificationListener.Notification;

    /// <summary>
    /// Manages AR system state and handles the session lifecycle. this class, application can create
    /// a session, configure it, start/pause or stop it. </summary>
    public class NRSessionManager : SingleTon<NRSessionManager>
    {
        /// <summary> The lost tracking reason. </summary>
        private LostTrackingReason m_LostTrackingReason = LostTrackingReason.INITIALIZING;

        /// <summary> Current lost tracking reason. </summary>
        /// <value> The lost tracking reason. </value>
        public LostTrackingReason LostTrackingReason
        {
            get
            {
                return m_LostTrackingReason;
            }
        }

        /// <summary> State of the session. </summary>
        private SessionState m_SessionState = SessionState.UnInitialized;

        /// <summary> Gets or sets the state of the session. </summary>
        /// <value> The session state. </value>
        public SessionState SessionState
        {
            get
            {
                return m_SessionState;
            }
            private set
            {
                m_SessionState = value;
            }
        }

        /// <summary> Gets or sets the nr session behaviour. </summary>
        /// <value> The nr session behaviour. </value>
        public NRSessionBehaviour NRSessionBehaviour { get; private set; }

        /// <summary> Gets or sets the nrhmd pose tracker. </summary>
        /// <value> The nrhmd pose tracker. </value>
        public NRHMDPoseTracker NRHMDPoseTracker { get; private set; }

        /// <summary> Gets or sets the native a pi. </summary>
        /// <value> The native a pi. </value>
        public NativeInterface NativeAPI { get; set; }

        /// <summary> Gets or sets the nr swapchain manager. </summary>
        /// <value> The nr swapchain manager. </value>
        public NRSwapChainManager NRSwapChainMan { get; set; }

        /// <summary> Gets or sets the nr metrics. </summary>
        /// <value> The nr metrics. </value>
        public NRMetrics NRMetrics { get; set; }

        private NRMultiResumeMediator mMultiResumeMediator;

        /// <summary> Gets or sets the virtual displayer. </summary>
        /// <value> The virtual displayer. </value>
        public NRVirtualDisplayer VirtualDisplayer { get; set; }

        public NRTrackingModeChangedListener TrackingLostListener { get; set; }

        /// <summary> Gets or sets the trackable factory. </summary>
        /// <value> The trackable factory. </value>
        public NRTrackableManager TrackableFactory { get; set; }

        /// <summary> External function which helps to generate readable error msg. </summary>
        public Func<NRKernalError, string> ErrorMessageGenerator = null;
        public Func<NRNotificationType, Notification, string> NotificationMessageGenerator = null;

        /// <summary> Gets the center camera anchor. </summary>
        /// <value> The center camera anchor. </value>
        public Transform CenterCameraAnchor
        {
            get
            {
                if (NRHMDPoseTracker != null && NRHMDPoseTracker.centerAnchor != null)
                {
                    return NRHMDPoseTracker.centerAnchor;
                }
                else
                {
                    return Camera.main.transform;
                }
            }
        }

        /// <summary> Gets the left camera anchor. </summary>
        /// <value> The left camera anchor. </value>
        public Transform LeftCameraAnchor
        {
            get
            {
                return NRHMDPoseTracker?.leftCamera.transform;
            }
        }

        /// <summary> Gets the right camera anchor. </summary>
        /// <value> The right camera anchor. </value>
        public Transform RightCameraAnchor
        {
            get
            {
                return NRHMDPoseTracker?.rightCamera.transform;
            }
        }

        #region Events
        public delegate void SessionError(NRKernalError exception);

        /// <summary> Event queue for all listeners interested in OnHMDPoseReady events. </summary>
        public static HMDPoseTrackerEvent OnHMDPoseReady;
        /// <summary> Event queue for all listeners interested in OnHMDLostTracking events. </summary>
        public static HMDPoseTrackerEvent OnHMDLostTracking;
        /// <summary> Event queue for all listeners interested in OnChangeTrackingMode events. </summary>
        public static HMDPoseTrackerModeChangeEvent OnChangeTrackingMode;

        /// <summary> Event queue for all listeners interested in OnGlassesStateChanged events. </summary>
        public static GlassesEvent OnGlassesStateChanged;
        /// <summary> Event queue for all listeners interested in OnGlassesDisconnect events. </summary>
        public static GlassesDisconnectEvent OnGlassesDisconnect;

        /// <summary> Event queue for all listeners interested in kernal error, 
        /// such as NRRGBCameraDeviceNotFindError, NRPermissionDenyError, NRUnSupportedHandtrackingCalculationError. </summary>
        public static event SessionError OnKernalError;
        #endregion

        private NRTrackingSubsystem m_TrackingSystem;
        public NRTrackingSubsystem TrackingSubSystem
        {
            get
            {
                return m_TrackingSystem;
            }
        }

        /// <summary> Gets a value indicating whether this object is initialized. </summary>
        /// <value> True if this object is initialized, false if not. </value>
        public bool IsInitialized
        {
            get
            {
                return (SessionState != SessionState.UnInitialized
                    && SessionState != SessionState.Destroyed);
            }
        }

        public bool IsRunning
        {
            get
            {
                if (SessionState != SessionState.Running)
                    return false;

#if UNITY_EDITOR
                return true;
#elif USING_XR_SDK
                return NRDevice.XRDisplaySubsystem.running;
#else
                return NRSwapChainMan.isRunning;
#endif
            }
        }

        /// <summary> Creates a session. </summary>
        /// <param name="session"> The session behaviour.</param>
        public void CreateSession(NRSessionBehaviour session)
        {
            if (SessionState != SessionState.UnInitialized && SessionState != SessionState.Destroyed)
            {
                return;
            }

            NRDebugger.Info("[NRSessionManager] CreateSession.");
            SetAppSettings();

            if (NRSessionBehaviour != null)
            {
                NRDebugger.Error("[NRSessionManager] Multiple SessionBehaviour components cannot exist in the scene. " +
                  "Destroying the newest.");
                GameObject.DestroyImmediate(session.gameObject);
                return;
            }
            NRSessionBehaviour = session;
            NRHMDPoseTracker = session.GetComponent<NRHMDPoseTracker>();
            if (NRHMDPoseTracker == null)
            {
                NRDebugger.Error("[NRSessionManager] Can not find the NRHMDPoseTracker in the NRSessionBehaviour object.");
                HandleKernalError(new NRMissingKeyComponentError("Missing the key component of 'NRHMDPoseTracker'."));
                return;
            }

            NRSwapChainMan = session.GetComponent<NRSwapChainManager>();
            if (NRSwapChainMan == null)
            {
                NRDebugger.Error("[NRSessionManager] Can not find the NRSwapChainManager in the NRSessionBehaviour object.");
                HandleKernalError(new NRMissingKeyComponentError("Missing the key component of 'NRSwapChainManager'."));
                return;
            }

            // NRMetrics = session.GetComponent<NRMetrics>();
            // if (NRMetrics == null)
            // {
            //     NRMetrics = session.gameObject.AddComponent<NRMetrics>();
            // }

            try
            {
                NRDevice.Instance.Create();
            }
            catch (NRKernalError ex)
            {
                HandleKernalError(ex);
                return;
            }
            catch (Exception ex)
            {
                NRDebugger.Error("[NRSessionManager] NRDevice Create failed: {0}\n{1}", ex.Message, ex.StackTrace);
                return;
            }

            NativeAPI = new NativeInterface();

            TrackableFactory = new NRTrackableManager();

            m_TrackingSystem = NRFrame.CreateSubsystem<NRTrackingSubsystemDescriptor, NRTrackingSubsystem>(NRTrackingSubsystemDescriptor.Name);

            TrackingLostListener = new NRTrackingModeChangedListener();

            NRSwapChainMan.Create();

            NRKernalUpdater.OnPreUpdate -= OnPreUpdate;
            NRKernalUpdater.OnPreUpdate += OnPreUpdate;

            SessionState = SessionState.Initialized;

            LoadNotification();

            var nativeMediator = new GameObject("NRNativeMediator");
            mMultiResumeMediator = nativeMediator.AddComponent<NRMultiResumeMediator>();
            NRDebugger.Info("[NRSessionManager] CreateSession finish.");
        }

        public NRNotificationListener NotificationListener { get; private set; }
        /// <summary> Loads the notification. </summary>
        private void LoadNotification()
        {
            NotificationListener = GameObject.FindObjectOfType<NRNotificationListener>();
            if (NotificationListener == null)
            {
                NotificationListener = GameObject.Instantiate(Resources.Load<NRNotificationListener>("NRNotificationListener"));
            }
        }

        /// <summary> True if is session error, false if not. </summary>
        private bool m_IsSessionError = false;

        /// <summary> Handle kernal error. </summary>
        /// <param name="kernalError"> An kernal error to process.</param>
        internal void HandleKernalError(NRKernalError kernalError)
        {
            NRDebugger.Error("[NRSessionManager] HandleKernalError: {0}", kernalError.GetErrorMsg());
            if (m_IsSessionError)
                return;

            OnKernalError?.Invoke(kernalError);

            if (kernalError.level == Level.High)
            {
                m_IsSessionError = true;
                ShowFatalErrorTipsAndQuit(kernalError);
            }
        }

        /// <summary>
        /// Get error tip been shown for users.
        /// </summary>
        /// <param name="error"> The error exception.</param>
        /// <returns> Error tip. </returns>
        private string GetErrorTip(NRKernalError error)
        {
            string tip = GetErrorTipDesc(error);
            if (error is NRNativeError)
            {
                NativeResult result = (error as NRNativeError).result;
                tip = string.Format("{0}({1})", tip, (int)result);
            }
            return tip;
        }

        private string GetErrorTipDesc(NRKernalError error)
        {
            if (error is NRGlassesConnectError)
            {
                return NativeConstants.GlassesDisconnectErrorTip;
            }
            else if (error is NRSdkVersionMismatchError)
            {
                return NativeConstants.SdkVersionMismatchErrorTip;
            }
            else if (error is NRSdcardPermissionDenyError)
            {
                return NativeConstants.SdcardPermissionDenyErrorTip;
            }
            else if (error is NRUnSupportDeviceError)
            {
                return NativeConstants.UnSupportDeviceErrorTip;
            }
            else if (error is NRDPDeviceNotFindError)
            {
                return NativeConstants.DPDeviceNotFindErrorTip;
            }
            else if (error is NRGetDisplayFailureError)
            {
                return NativeConstants.GetDisplayFailureErrorTip;
            }
            else if (error is NRDisplayModeMismatchError)
            {
                return NativeConstants.DisplayModeMismatchErrorTip;
            }
            else if (error is NRRuntimeNotFoundError)
            {
                return NativeConstants.SDKRuntimeNotFoundErrorTip;
            }
            else if (error is NRGlassesNotAvailbleError)
            {
                return NativeConstants.GlassesNotAvailbleErrorTip;
            }
            else
            {
                return NativeConstants.UnknowErrorTip;
            }
        }

        /// <summary> Shows the error tips. </summary>
        /// <param name="msg"> The message.</param>
        private void ShowFatalErrorTipsAndQuit(NRKernalError error)
        {
            NRGlassesInitErrorTip errortips;
            if (NRSessionBehaviour != null && NRSessionBehaviour.SessionConfig.GlassesErrorTipPrefab != null)
            {
                errortips = GameObject.Instantiate<NRGlassesInitErrorTip>(NRSessionBehaviour.SessionConfig.GlassesErrorTipPrefab);
            }
            else
            {
                errortips = GameObject.Instantiate<NRGlassesInitErrorTip>(Resources.Load<NRGlassesInitErrorTip>("NRGlassesErrorTip"));
            }

            // Clear objects of scene.
            ReleaseSDK();

            GameObject.DontDestroyOnLoad(errortips);

            string tips = string.Empty;
            if (ErrorMessageGenerator != null)
                tips = ErrorMessageGenerator.Invoke(error);
            else
                tips = GetErrorTip(error);

            errortips.Init(tips, () =>
            {
                NRDevice.QuitApp();
            });
        }

        /// <summary> Executes the 'pre update' action. </summary>
        private void OnPreUpdate()
        {
            if (SessionState != SessionState.Running || m_IsSessionError)
            {
                Debug.LogError(SessionState.ToString());
                return;
            }

            if (NRHMDPoseTracker.IsTrackModeChanging || NRHMDPoseTracker.TrackingMode == TrackingType.Tracking0Dof)
                return;

            NRFrame.OnPreUpdate(ref m_LostTrackingReason);
        }

        /// <summary> Sets a configuration. </summary>
        /// <param name="config"> The configuration.</param>
        public void SetConfiguration(NRSessionConfig config)
        {
            if (SessionState == SessionState.UnInitialized
                || SessionState == SessionState.Destroyed
                || SessionState == SessionState.Paused
                || m_IsSessionError)
            {
                NRDebugger.Error("[NRSessionManager] Can not set configuration in this state:" + SessionState.ToString());
                return;
            }
#if !UNITY_EDITOR
            if (config == null)
            {
                return;
            }
            // AsyncTaskExecuter.Instance.RunAction(() =>
            // {
                NRDebugger.Info("[NRSessionManager] Update config");
                NativeAPI.Configuration.UpdateConfig(config);
            // });
#endif
        }

        /// <summary> Recenters this object. </summary>
        public void Recenter()
        {
            if (SessionState != SessionState.Running || m_IsSessionError)
            {
                return;
            }
            AsyncTaskExecuter.Instance.RunAction(() =>
            {
                TrackingSubSystem.Recenter();
            });
        }

        /// <summary> Sets application settings. </summary>
        private void SetAppSettings()
        {
            NRFrame.targetFrameRate = 1000;
            QualitySettings.maxQueuedFrames = -1;
            QualitySettings.vSyncCount = 0;
            Screen.fullScreen = true;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        /// <summary> Starts a session. </summary>
        public void StartSession()
        {
            if (SessionState == SessionState.Running
                || SessionState == SessionState.Destroyed
                || m_IsSessionError)
            {
                return;
            }

            NRDebugger.Info("[NRSessionManager] StartSession.");

            try
            {
                NRDevice.Instance.Start();
            }
            catch (NRKernalError ex)
            {
                HandleKernalError(ex);
                return;
            }
            catch (Exception ex)
            {
                NRDebugger.Error("[NRSessionManager] NRDevice Start failed: {0}\n{1}", ex.Message, ex.StackTrace);
                return;
            }

            VirtualDisplayer?.StartDisplay();

            var config = NRSessionBehaviour.SessionConfig;
            if (config != null)
            {
                var deviceCategory = NRDevice.Subsystem.GetDeviceCategory();
                NRDebugger.Info("[NRSessionManager] targetDeviceCategory : curDevice={0}, targetDevices={1}", deviceCategory, config.GetTargetDeviceTypesDesc());
                if (!config.IsTargetDevice(deviceCategory))
                {
                    HandleKernalError(new NRUnSupportDeviceError(string.Format("Unsuppport running on {0}!", deviceCategory)));
                    return;
                }
            }

            NRHMDPoseTracker.AutoAdaptTrackingType();
            NRDebugger.Info("[NRSessionManager] StartSession: InitTrackingType={0}", NRHMDPoseTracker.TrackingMode);
            TrackingSubSystem.InitTrackingType(NRHMDPoseTracker.TrackingMode);
            this.AutoAdaptSessionConfig();
            if (NRHMDPoseTracker.TrackingMode != TrackingType.Tracking6Dof)
            {
                NRSessionBehaviour.SessionConfig.PlaneFindingMode = TrackablePlaneFindingMode.DISABLE;
                NRSessionBehaviour.SessionConfig.ImageTrackingMode = TrackableImageFindingMode.DISABLE;
            }

            TrackableFactory.Start();

            NRDebugger.Info("[NRSessionManager] Tracking Start.");
            TrackingSubSystem.Start();
            NRSwapChainMan.StartSwapChain();
            SessionState = SessionState.Running;

            SetConfiguration(NRSessionBehaviour.SessionConfig);
            ThermalMgr.Init();
            NRDebugger.Info("[NRSessionManager] StartSession finish.");
        }

        /// <summary> Disables the session. </summary>
        public void DisableSession()
        {
            if (SessionState != SessionState.Running || m_IsSessionError)
            {
                return;
            }

            NRDebugger.Info("[NRSessionManager] Pause");
            // Do not put it in other thread...
            VirtualDisplayer?.Pause();

            NRInput.Pause();
            NRSwapChainMan.Pause();
            TrackableFactory.Pause();
            TrackingSubSystem.Pause();
            NRDevice.Instance.Pause();

            SessionState = SessionState.Paused;

            m_LostTrackingReason = LostTrackingReason.INITIALIZING;
            NRDebugger.Info("[NRSessionManager] Paused");
        }

        /// <summary> Resume session. </summary>
        public void ResumeSession()
        {
            if (SessionState != SessionState.Paused || m_IsSessionError)
            {
                return;
            }

            NRDebugger.Info("[NRSessionManager] Resume");
            // Do not put it in other thread...
            NRDevice.Instance.Resume();
            TrackableFactory.Resume();
            TrackingSubSystem.Resume();
            NRSwapChainMan.Resume();
            VirtualDisplayer?.Resume();
            NRInput.Resume();
            SessionState = SessionState.Running;
            NRDebugger.Info("[NRSessionManager] Resumed");
        }

        /// <summary> Destroys the session. </summary>
        public void DestroySession()
        {
            if (SessionState == SessionState.Destroyed || SessionState == SessionState.UnInitialized)
            {
                return;
            }

            NRDebugger.Info("[NRSessionManager] Destroy");
            GameObject.Destroy(mMultiResumeMediator.gameObject);
            // Do not put it in other thread...
            SessionState = SessionState.Destroyed;

            VirtualDisplayer?.Destroy();
            NRSwapChainMan.Destroy();            
            TrackableFactory.Destroy();
            TrackingSubSystem.Destroy();
            NRFrame.DestroySubsystem<NRTrackingSubsystemDescriptor, NRTrackingSubsystem>(NRTrackingSubsystemDescriptor.Name);

            NRInput.Destroy();
            NRDevice.Instance?.Destroy();

            if (TrackingLostListener != null)
            {
                TrackingLostListener.Dispose();
                TrackingLostListener = null;
            }
            if (NotificationListener != null)
            {
                GameObject.Destroy(NotificationListener.gameObject);
                NotificationListener = null;
            }
            NRKernalUpdater.OnPreUpdate -= OnPreUpdate;
            NRFrame.Destroy();
            NRDebugger.Info("[NRSessionManager] Destroyed");
        }

        /// <summary> Auto adaption for sessionConfig(PlaneFindingMode&ImageTrackingMode) based on supported feature on current device. </summary>
        private void AutoAdaptSessionConfig()
        {
            if (NRDevice.Subsystem.GetDeviceCategory() == NRDeviceCategory.VISION)
            {
                if (NRSessionBehaviour.SessionConfig.PlaneFindingMode != TrackablePlaneFindingMode.DISABLE)
                {
                    NRDebugger.Warning("[NRSessionManager] AutoAdaptConfig PlaneFindingMode : {0} => {1}", NRSessionBehaviour.SessionConfig.PlaneFindingMode, TrackablePlaneFindingMode.DISABLE);
                    NRSessionBehaviour.SessionConfig.PlaneFindingMode = TrackablePlaneFindingMode.DISABLE;
                }
                if (NRSessionBehaviour.SessionConfig.ImageTrackingMode != TrackableImageFindingMode.DISABLE)
                {
                    NRDebugger.Warning("[NRSessionManager] AutoAdaptConfig ImageTrackingMode : {0} => {1}", NRSessionBehaviour.SessionConfig.ImageTrackingMode, TrackableImageFindingMode.DISABLE);
                    NRSessionBehaviour.SessionConfig.ImageTrackingMode = TrackableImageFindingMode.DISABLE;
                }
            }
        }

        /// <summary>
        /// Release SDK. It will destroy all referenced SDK gameobjects including SingletonBehaviour targets, but will keep SingleTon instance.
        /// </summary>
        public void ReleaseSDK()
        {
            NRDebugger.Info("[NRSessionManager] ReleaseSDK");
            // NRSessionManager.Instance.DisableSession();
            // NRSessionManager.Instance.DestroySession();

            if (NRSessionBehaviour != null)
            {
                GameObject.DestroyImmediate(NRSessionBehaviour.gameObject);
            }

            var input = GameObject.FindObjectOfType<NRInput>();
            if (input != null)
            {
                GameObject.DestroyImmediate(input.gameObject);
            }

            var meshingMan = GameObject.FindObjectOfType<NRMeshingManager>();
            if (meshingMan != null)
            {
                GameObject.DestroyImmediate(meshingMan.gameObject);
            }

            var metrics = GameObject.FindObjectOfType<NRMetrics>();
            if (metrics != null)
            {
                GameObject.DestroyImmediate(metrics.gameObject);
            }

            NRDebugger.Info("[NRSessionManager] ReleaseSDK finish");
        }
    }
}
