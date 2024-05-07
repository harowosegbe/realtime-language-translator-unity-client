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
    using UnityEngine.UI;
    using UnityEngine.Rendering;
    using System;
    using System.Collections;
    using System.Threading;
    using System.Collections.Generic;
    using NRKernal;
    
    [ScriptOrder(NativeConstants.NRPHASESYNC_ORDER)]
    public class NRVirtualPhaseSync : MonoBehaviour
    {
        public static int sleepTime = 0;
        public Text m_TxtInfo;


        private void Start()
        {
            NRInput.AddClickListener(ControllerHandEnum.Right, ControllerButton.APP, ()=>
            {
                sleepTime += 1;
                RefreshSleepTime();
            });

            NRInput.AddClickListener(ControllerHandEnum.Right, ControllerButton.HOME, ()=>
            {
                sleepTime -= 1;
                if (sleepTime < 0)
                    sleepTime = 0;
                RefreshSleepTime();
            });
        }

        void RefreshSleepTime()
        {
            NRDebugger.Info("[PhaseSync] SleepTime:{0}", sleepTime);            
            NRSessionManager.Instance.NRMetrics?.ResetMetrics();
        }

        void OnEnable()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                Camera.onPreRender += OnCameraPreRender;
            }
            else
            {
                RenderPipelineManager.beginCameraRendering += OnBeginCameraRender;
            }
        }

        void OnDisable()
        {
            if (GraphicsSettings.currentRenderPipeline == null)
            {
                Camera.onPreRender -= OnCameraPreRender;
            }
            else
            {
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRender;
            }
        }

        void OnCameraPreRender(Camera cam)
        {
            OnCameraPreRenderImp(cam);
        }

        void OnBeginCameraRender(ScriptableRenderContext context, Camera cam)
        {
            OnCameraPreRenderImp(cam);
        }

        void OnCameraPreRenderImp(Camera cam)
        {
#if UNITY_EDITOR || USING_XR_SDK
            if (cam.name == "CenterCamera")
            {
                DoPhaseSyncSleep();
            }
#else
            if ((NRFrame.MonoMode && cam.name == "CenterCamera") 
                || (!NRFrame.MonoMode && cam.name == "LeftCamera"))
            {
                DoPhaseSyncSleep();
            }
#endif
        }

        void DoPhaseSyncSleep()
        {
            var nativeRender = NRSessionManager.Instance.NativeAPI.NativeRenderring;
            ulong presentTime1 = 0;
            nativeRender.GetFramePresentTime(ref presentTime1);
            // NRDebugger.Info("[PhaseSync] DoPhaseSyncSleep: FrameCount={0}, presentTime1={1}, targetFrameRate={2}, presentTimeDelta={3}", Time.frameCount, presentTime1, Application.targetFrameRate, presentTime1 - NRFrame.CurrentPoseTimeStamp);
            
            if (sleepTime > 0)
            {
                var beginTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
                Thread.Sleep(sleepTime);

                ulong presentTime2 = 0;
                nativeRender.GetFramePresentTime(ref presentTime2);
                var endTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
                // NRDebugger.Info("[PhaseSync] DoPhaseSyncSleep after phaseSync: FrameCount={0}, sleep={1}, presentTime2={2}, presentTimeDelta={3}", Time.frameCount, endTime - beginTime, presentTime2, presentTime2 - presentTime1);
            }
        }

        void Update()
        {
            if (m_TxtInfo != null)
            {
                var metrics = NRSessionManager.Instance.NRMetrics.frameMetrics;
                m_TxtInfo.text = string.Format("PhaseSync: {0}\n avgUpdatePrd: {1}\n avgPrd: {2}\n avgPostPrd: {3}\n avgEFC: {4:F2}\n avgAllUpdatePrd: {5}\n avgAllPrd: {6}\n avgAllPostPrd: {7}\n avgAllEFC: {8:F2}",
                        sleepTime, 
                        metrics.avgUpdateToPresentT, metrics.avgPreToPresentT, metrics.avgPostToPresentT, metrics.avgExtraFrameCount,
                        metrics.avgUpdateToPresentTAll, metrics.avgPreToPresentTAll, metrics.avgPostToPresentTAll, metrics.avgExtraFrameCountAll);
            }
        }
    }
}
