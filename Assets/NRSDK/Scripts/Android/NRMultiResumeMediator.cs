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
    using System;
    using UnityEngine;


    /// <summary>
    /// The singleton mediator of native. It is used to accept message from native. </summary>
    public class NRMultiResumeMediator : SingletonBehaviour<NRMultiResumeMediator>
    {
        /// <summary> Whether is in the multi-resume background state. </summary>
        public static bool isMultiResumeBackground = false;

        /// <summary> FloatingWindow show/hide state change event </summary>
        public static event Action<bool> FloatingWindowStateChanged;
        /// <summary> FloatingWindow click event </summary>
        public static event Action FloatingWindowClicked;

        private static AndroidJavaClass mMultiResumeNativeInstance;

        static AndroidJavaClass nativeInstance
        {
            get
            {
                if (mMultiResumeNativeInstance == null)
                    mMultiResumeNativeInstance = new AndroidJavaClass("ai.nreal.activitylife.NRXRApp");
                return mMultiResumeNativeInstance;
            }
        }

        private class FloatingManagerListener : AndroidJavaProxy
        {
            public FloatingManagerListener() : base("ai.nreal.activitylife.IFloatingManagerCallback")
            {

            }
            public void onFloatingViewShown()
            {
                NRMultiResumeMediator.FloatingWindowStateChanged?.Invoke(true);
            }

            public void onFloatingViewDismissed()
            {
                NRMultiResumeMediator.FloatingWindowStateChanged?.Invoke(false);
            }

            public void onFloatingViewClicked()
            {
                NRMultiResumeMediator.FloatingWindowClicked?.Invoke();
            }
        }

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ListenFloatingManager()
        {
            var cls = new AndroidJavaClass("ai.nreal.activitylife.FloatingManager");
            cls.CallStatic("setNRXRAppCallback", new FloatingManagerListener());
        }
#endif

        /// <summary> Set the state of multi-resume background. </summary>
        /// <param name="state">   The state.</param>
        void SetMultiResumeBackground(string state)
        {
            isMultiResumeBackground = state == "true";
            NRDebugger.Info("SetMultiResumeBackground: state={0}, isMultiResumeBackground={1}", state, isMultiResumeBackground);
        }

        /// <summary> Broadcast the controller display mode. </summary>
        /// <param name="displayMode">   The display mode.</param>
        public static void BroadcastControllerDisplayMode(int displayMode)
        {
            if (!NRSessionManager.Instance.NRSessionBehaviour.SessionConfig.SupportMultiResume)
                return;

            try
            {
                nativeInstance.CallStatic("broadcastControllerDisplayMode", displayMode);
            }
            catch (Exception e)
            {
                NRDebugger.Error("BroadcastDisplayMode: {0}", e.Message);
            }
        }

        public static void ForceKill()
        {
            if (!NRSessionManager.Instance.NRSessionBehaviour.SessionConfig.SupportMultiResume)
                return;

            try
            {
                nativeInstance.CallStatic("forceKill");
            }
            catch (Exception e)
            {
                NRDebugger.Error("ForceKill: {0}", e.Message);
            }
        }

        public static void MoveToBackOnNR()
        {
            try
            {
                nativeInstance.CallStatic("moveToBackOnNR");
            }
            catch (Exception e)
            {
                NRDebugger.Error("moveToBackOnNR: {0}", e.Message);
                throw;
            }
        }

        public static AndroidJavaObject GetFakeActivity()
        {
            return nativeInstance.CallStatic<AndroidJavaObject>("getFakeActivity");
        }
    }
}
