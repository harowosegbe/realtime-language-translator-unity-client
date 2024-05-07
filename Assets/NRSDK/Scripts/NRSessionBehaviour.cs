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
    using UnityEngine;

    /// <summary>
    /// Operate AR system state and handles the session lifecycle for application layer. </summary>
    [HelpURL("https://developer.xreal.com/develop/discover/introduction-nrsdk")]
    [ScriptOrder(NativeConstants.NRSESSIONBEHAVIOUR_ORDER)]
    public class NRSessionBehaviour : SingletonBehaviour<NRSessionBehaviour>
    {
        /// <summary> LogVevel of NRSDK should be while release. </summary>
        [Tooltip("LogLevel of NRSDK.")]
		[SerializeField]
        LogLevel LogLevel = LogLevel.Info;
        /// <summary> The SessionConfig of nrsession. </summary>
        [Tooltip("A scriptable object specifying the NRSDK session configuration.")]
		[SerializeField]
        public NRSessionConfig SessionConfig;

        /// <summary>
        /// Base Awake method that sets the Singleton's unique instance. Called by Unity when
        /// initializing a MonoBehaviour. Scripts that extend Singleton should be sure to call
        /// base.Awake() to ensure the static Instance reference is properly created. </summary>
        new void Awake()
        {
            base.Awake();
            if (isDirty) return;

            Debug.LogFormat("[SessionBehaviour] NRSDKPackage Version: {0}", NRVersionInfo.GetNRSDKPackageVersion());

#if NR_PROFILER
            NRDebugger.logLevel = LogLevel;
#elif DEVELOPMENT_BUILD
            NRDebugger.logLevel = LogLevel.Debug;
#else
            NRDebugger.logLevel = LogLevel;
#endif

#if USING_XR_SDK && !UNITY_EDITOR
            NativeXRPlugin.SetLogLevel((int)NRDebugger.logLevel);
#endif
            Debug.LogFormat("[SessionBehaviour] Awake CreateSession: logLevel={0} => {1}, isXR={2}, multiThread={3}, supportMultiResume={4}", 
                LogLevel, NRDebugger.logLevel, NRFrame.IsXR, SessionConfig.UseMultiThread, SessionConfig.SupportMultiResume);
            NRSessionManager.Instance.CreateSession(this);
        }

        /// <summary> Starts this object. </summary>
        void Start()
        {
            if (isDirty) return;
            NRDebugger.Info("[SessionBehaviour] Start: StartSession");
            NRSessionManager.Instance.StartSession();
        }

        /// <summary> Executes the 'application pause' action. </summary>
        /// <param name="pause"> True to pause.</param>
        private void OnApplicationPause(bool pause)
        {
            if (isDirty) return;
            NRDebugger.Info("[SessionBehaviour] OnApplicationPause: {0}", pause);
            if (pause)
            {
                NRSessionManager.Instance.DisableSession();
            }
            else
            {
                NRSessionManager.Instance.ResumeSession();
            }
        }

        /// <summary> Executes the 'enable' action. </summary>
        void OnEnable()
        {
            if (isDirty) return;
            NRDebugger.Info("[SessionBehaviour] OnEnable: ResumeSession");
            NRSessionManager.Instance.ResumeSession();
        }

        /// <summary> Executes the 'disable' action. </summary>
        void OnDisable()
        {
            if (isDirty) return;
            NRDebugger.Info("[SessionBehaviour] OnDisable: DisableSession");
            NRSessionManager.Instance.DisableSession();
        }

        /// <summary>
        /// Base OnDestroy method that destroys the Singleton's unique instance. Called by Unity when
        /// destroying a MonoBehaviour. Scripts that extend Singleton should be sure to call
        /// base.OnDestroy() to ensure the underlying static Instance reference is properly cleaned up. </summary>
        new void OnDestroy()
        {
            if (isDirty) return;
            base.OnDestroy();
            NRDebugger.Info("[SessionBehaviour] OnDestroy: DestroySession");
            NRSessionManager.Instance.DestroySession();
        }
    }
}
