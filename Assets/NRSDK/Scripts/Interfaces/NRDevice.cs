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
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary> Values that represent session special event types. </summary>
    public enum SessionSpecialEventType
    {
        /// <summary> An enum constant representing the sdk creat. </summary>
        SDKCreated,
        /// <summary> An enum constant representing the sdk init. </summary>
        SDKInited,
        /// <summary> An enum constant representing the glasses is started just now. </summary>
        GlassesStarted,
        /// <summary> An enum constant representing the glasses is going to be paused after this event. </summary>
        GlassesPrePause,
        /// <summary> An enum constant representing the session is resumed just now. </summary>
        GlassesResumed,
        /// <summary> An enum constant representing the glasses is going to be stoped after this event. </summary>
        GlassesPreStop
    }

    /// <summary> Manage the HMD device. </summary>
    public class NRDevice : SingleTon<NRDevice>
    {
        /// <summary> Event queue for all listeners interested in OnGlassesStateChanged events. </summary>
        public static event GlassesEvent OnGlassesStateChanged;
        /// <summary> Event queue for all listeners interested in OnGlassesDisconnect events. </summary>
        public static event GlassesDisconnectEvent OnGlassesDisconnect;
        /// <summary> Event queue for all listeners interested in session events. </summary>
        public static SessionSpecialEvent OnSessionSpecialEvent;

        /// <summary> Values that represent glasses event types. </summary>
        public enum GlassesEventType
        {
            /// <summary> An enum constant representing the put on option. </summary>
            PutOn,
            /// <summary> An enum constant representing the put off option. </summary>
            PutOff
        }

        private const float SDK_RELEASE_TIMEOUT = 2f;

        private bool m_IsInitialized = false;

        private static NRDeviceSubsystem m_Subsystem;
        public static NRDeviceSubsystem Subsystem
        {
            get
            {
                return m_Subsystem;
            }
        }
        
#if USING_XR_SDK
        public static UnityEngine.XR.XRDisplaySubsystem XRDisplaySubsystem
        {
            get { return Subsystem.XRDisplaySubsystem; }
        }
#endif

        private static AndroidJavaObject m_UnityActivity;

        public static AndroidJavaObject unityActivity
        {
            get { return m_UnityActivity; }
        }

    
        private bool m_MonoMode = false;
        /// <summary> If sdk is running in mono mode. </summary>
        /// <value> If sdk is running in mono mode. </value>
        public bool MonoMode
        {
            get { return m_MonoMode; }
            set
            {
                if (m_MonoMode != value)
                {
                    m_MonoMode = value;
                }
            }
        }

        /// <summary> Init HMD device. </summary>
        public void Create()
        {
            if (m_IsInitialized)
            {
                return;
            }
            NRDebugger.Info("[NRDevice] Create");

            NRTools.Init();
            MainThreadDispather.Initialize();

            string version = string.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
            // Init before all actions.
            AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            m_UnityActivity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            NRDebugger.Info("[NRDevice] NRSDK Create");

            NativeAPI.Create(m_UnityActivity.GetRawObject());
#elif !UNITY_EDITOR
            NativeAPI.Create();
#endif
            OnSessionSpecialEvent?.Invoke(SessionSpecialEventType.SDKCreated);
#if !UNITY_EDITOR
            NativeAPI.Start();
            version = NativeAPI.GetVersion();
#endif
            OnSessionSpecialEvent?.Invoke(SessionSpecialEventType.SDKInited);
            m_Subsystem = NRFrame.CreateSubsystem<NRDeviceSubsystemDescriptor, NRDeviceSubsystem>(NRDeviceSubsystemDescriptor.Name);
            m_IsInitialized = true;
            NRDebugger.Info("[NRDevice] Created: nrapi version={0}", version);
        }

        public void Start()
        {
            NRDebugger.Info("[NRDevice] Start");
            Subsystem.Start();
            NRDeviceSubsystem.OnGlassesStateChanged += OnGlassesWear;
            NRDeviceSubsystem.OnGlassesDisconnect += OnGlassesDisconnectEvent;

            var supportMono = NRSessionManager.Instance.NRSessionBehaviour.SessionConfig.SupportMonoMode;
            var curStereoMode = Subsystem.GlassesStereoMode;
            MonoMode = supportMono && curStereoMode == NativeGlassesStereoMode.Mono;
            NRDebugger.Info("[NRDevice] Started: supportMono={0}, curStereoMode={1}, MonoMode={2}", supportMono, curStereoMode, MonoMode);

            // Throw exception here, as SDK have no chance to block starting as it can be worked on both 2d and 3d mode.
            if (!supportMono && curStereoMode == NativeGlassesStereoMode.Mono)
            {
                NativeErrorListener.Check(NativeResult.DisplayNoInStereoMode, this, "Start", true);
            }
        }

        public void Pause()
        {
            Subsystem.Pause();
        }

        public void Resume()
        {
            Subsystem.Resume();
        }

        public void Destroy()
        {
            NRDeviceSubsystem.OnGlassesStateChanged -= OnGlassesWear;
            NRDeviceSubsystem.OnGlassesDisconnect -= OnGlassesDisconnectEvent;

            NRDebugger.Info("[NRDevice] Destroy");
            Subsystem.Destroy();
            NRFrame.DestroySubsystem<NRDeviceSubsystemDescriptor, NRDeviceSubsystem>(NRDeviceSubsystemDescriptor.Name);

            NativeAPI.Stop();
            NativeAPI.Destroy();
            NRDebugger.Info("[NRDevice] Destroyed");

            m_IsInitialized = false;
        }

        private void OnGlassesWear(NRDevice.GlassesEventType eventtype)
        {
            NRDebugger.Info("[NRDevice] " + (eventtype == GlassesEventType.PutOn ? "Glasses put on" : "Glasses put off"));
            OnGlassesStateChanged?.Invoke(eventtype);
            NRSessionManager.OnGlassesStateChanged?.Invoke(eventtype);
        }

        private void OnGlassesDisconnectEvent(GlassesDisconnectReason reason)
        {
            NRDebugger.Info("[NRDevice] OnGlassesDisconnectEvent: reason={0}, running={1}", reason.ToString(), Subsystem.running);

            bool forceKill = true;
            // Some app contains 2D content and 3D content. It may wish to keep process alive after glasses switch mode.
            if (reason == GlassesDisconnectReason.NOTIFY_TO_QUIT_APP && !NRSessionManager.Instance.NRSessionBehaviour.SessionConfig.ForceKillWhileGlassesSwitchMode)
                forceKill = false;
            
            // Do ForceKill immediately for background mrapp, as the main thread is not running. 
            if (!Subsystem.running)
            {
                if (forceKill)
                    ForceKill(false);
                else
                    Subsystem.ResetStateOnNextResume();

                return;
            }

            // If NRSDK release time out in 2 seconds, FoceKill the process.
            AsyncTaskExecuter.Instance.RunAction(() =>
            {
                try
                {
                    OnGlassesDisconnect?.Invoke(reason);
                    NRSessionManager.OnGlassesDisconnect?.Invoke(reason);
                }
                catch (Exception e)
                {
                    NRDebugger.Info("[NRDevice] Operate OnGlassesDisconnect event error:" + e.ToString());
                    throw e;
                }
                finally
                {

                    NRDebugger.Info("[NRDevice] OnGlassesDisconnectEvent: forceKill={0}, ForceKillWhileGlassesSwitchMode={1}", forceKill, NRSessionManager.Instance.NRSessionBehaviour.SessionConfig.ForceKillWhileGlassesSwitchMode);
                    if (forceKill)
                        ForceKill(true);
                    else
                        Subsystem.ResetStateOnNextResume();
                }
            }, () =>
            {
                NRDebugger.Error("[NRDevice] Release sdk timeout, force kill the process!!!");
                if (forceKill)
                    ForceKill(false);
                else
                    Subsystem.ResetStateOnNextResume();
            }, SDK_RELEASE_TIMEOUT, true);
        }

        #region Quit
        /// <summary> Quit the app. </summary>
        public static void QuitApp()
        {
            NRDebugger.Info("[NRDevice] Start To Quit Application.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            ForceKill();
#endif
        }

        /// <summary> Force kill the app. Avoid timeout to pause UnityEngine. </summary>
        /// <exception cref="Exception"> Thrown when an exception error condition occurs.</exception>
        public static void ForceKill(bool needrelease = true)
        {
            NRDebugger.Info("[NRDevice] ForceKill: release={0}", needrelease);
            try
            {
                if (needrelease)
                    ReleaseSDK();
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                NativeAPI.Destroy();

                AndroidJNI.AttachCurrentThread();
                if (NRSessionManager.Instance.NRSessionBehaviour.SessionConfig.SupportMultiResume)
                {
                    NRMultiResumeMediator.ForceKill();
                }
                else
                {
                    m_UnityActivity?.Call("finish");
                }

#elif !UNITY_EDITOR
                NativeAPI.Destroy();
#endif
            }
        }


        /// <summary> Release sdk. Call this function in unity main thread only. </summary>
        public static void ReleaseSDK()
        {
            NRDebugger.Info("[NRDevice] Start to release sdk");
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            NRSessionManager.Instance.ReleaseSDK();

            NRDebugger.Info("[NRDevice] Release sdk end, cost:{0} ms", stopwatch.ElapsedMilliseconds);
        }
        #endregion
    }
}
