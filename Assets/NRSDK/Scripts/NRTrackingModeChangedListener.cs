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
    using System.Collections;
    using UnityEngine;

    public class NRTrackingModeChangedListener : IDisposable
    {
        public delegate void OnTrackStateChangedDel(bool trackChanging);
        public event OnTrackStateChangedDel OnTrackStateChanged;
        private NRTrackingModeChangedTip m_LostTrackingTip;
        private Coroutine m_EnableRenderCamera;
        private Coroutine m_DisableRenderCamera;
        private bool m_HMDPoseInitialized = false;
        private const float MinTimeLastLimited = 0.5f;
        private const float MaxTimeLastLimited = 6f;
        private static string m_CustomTips = null;
        public static void SetCustomTips(string tips)
        {
            m_CustomTips = tips;
        }

        public NRTrackingModeChangedListener()
        {
            NRHMDPoseTracker.OnHMDPoseReady += OnHMDPoseReady;
            NRHMDPoseTracker.OnHMDLostTracking += OnHMDLostTracking;
            NRHMDPoseTracker.OnChangeTrackingMode += OnChangeTrackingMode;
        }

        private void OnHMDLostTracking()
        {
            if (!m_HMDPoseInitialized)
                return;
            NRDebugger.Info("[NRTrackingModeChangedListener] OnHMDLostTracking: {0}", NRFrame.LostTrackingReason);
            ShowTips(string.Empty);
        }
        private void OnHMDPoseReady()
        {
            m_HMDPoseInitialized = true;
        }

        private void OnChangeTrackingMode(TrackingType origin, TrackingType target)
        {
            NRDebugger.Info("[NRTrackingModeChangedListener] OnChangeTrackingMode: {0} => {1}", origin, target);
            ShowTips(string.IsNullOrEmpty(m_CustomTips) ? NativeConstants.TRACKING_MODE_SWITCH_TIP : m_CustomTips);
        }


        private void ShowTips(string tips)
        {
            if (m_EnableRenderCamera != null)
            {
                NRKernalUpdater.Instance.StopCoroutine(m_EnableRenderCamera);
                m_EnableRenderCamera = null;
            }
            if (m_DisableRenderCamera != null)
            {
                NRKernalUpdater.Instance.StopCoroutine(m_DisableRenderCamera);
                m_DisableRenderCamera = null;
            }
            m_EnableRenderCamera = NRKernalUpdater.Instance.StartCoroutine(EnableTrackingInitializingRenderCamera(tips));
        }

        public IEnumerator EnableTrackingInitializingRenderCamera(string tips)
        {
            if (m_LostTrackingTip == null)
            {
                m_LostTrackingTip = NRTrackingModeChangedTip.Create();
            }
            m_LostTrackingTip.Show();
            var reason = NRFrame.LostTrackingReason;
            m_LostTrackingTip.SetMessage(tips);

            float begin_time = Time.realtimeSinceStartup;
            var endofFrame = new WaitForEndOfFrame();
            yield return endofFrame;
            yield return endofFrame;
            yield return endofFrame;
            NRDebugger.Info("[NRTrackingModeChangedListener] Enter tracking initialize mode...");
            OnTrackStateChanged?.Invoke(true);

            NRHMDPoseTracker postTracker = NRSessionManager.Instance.NRHMDPoseTracker;
            while ((NRFrame.LostTrackingReason != LostTrackingReason.NONE || postTracker.IsTrackModeChanging || (Time.realtimeSinceStartup - begin_time) < MinTimeLastLimited)
                && (Time.realtimeSinceStartup - begin_time) < MaxTimeLastLimited)
            {
                NRDebugger.Info("[NRTrackingModeChangedListener] Wait for tracking: modeChanging={0}, lostTrackReason={1}",
                    postTracker.IsTrackModeChanging, NRFrame.LostTrackingReason);
                yield return endofFrame;
            }

            if (m_DisableRenderCamera == null)
            {
                m_DisableRenderCamera = NRKernalUpdater.Instance.StartCoroutine(DisableTrackingInitializingRenderCamera());
            }
            m_EnableRenderCamera = null;
        }

        public IEnumerator DisableTrackingInitializingRenderCamera()
        {
            if (m_LostTrackingTip != null)
            {
                m_LostTrackingTip.Hide();
            }
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            OnTrackStateChanged?.Invoke(false);
            NRDebugger.Info("[NRTrackingModeChangedListener] Exit tracking initialize mode...");
            m_DisableRenderCamera = null;
        }

        public void Dispose()
        {
            NRHMDPoseTracker.OnHMDPoseReady -= OnHMDPoseReady;
            NRHMDPoseTracker.OnHMDLostTracking -= OnHMDLostTracking;
            NRHMDPoseTracker.OnChangeTrackingMode -= OnChangeTrackingMode;

            if (m_EnableRenderCamera != null)
            {
                NRKernalUpdater.Instance.StopCoroutine(m_EnableRenderCamera);
                m_EnableRenderCamera = null;
            }
            if (m_DisableRenderCamera != null)
            {
                NRKernalUpdater.Instance.StopCoroutine(m_DisableRenderCamera);
                m_DisableRenderCamera = null;
            }

            if (m_LostTrackingTip != null)
            {
                GameObject.Destroy(m_LostTrackingTip.gameObject);
                m_LostTrackingTip = null;
            }
            m_CustomTips = null;
        }
    }
}
