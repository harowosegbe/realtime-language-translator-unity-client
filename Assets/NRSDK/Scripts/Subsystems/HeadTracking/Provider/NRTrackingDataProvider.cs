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
    using System.Collections.Generic;
    using UnityEngine;
#if USING_XR_SDK
    using UnityEngine.XR;
#endif

    public class NRTrackingDataProvider : ITrackingDataProvider
    {
        NativeHeadTracking m_NativeHeadTracking;
        NativePerception m_NativePerception;

#if USING_XR_SDK
        private const string k_idInputSubsystem = "NRSDK Head Tracking";
        private XRInputSubsystem m_XRInputSubsystem;
        public XRInputSubsystem XRInputSubsystem
        {
            get
            {
                return m_XRInputSubsystem;
            }
        }
#endif

        public NRTrackingDataProvider()
        {
            m_NativeHeadTracking = NRSessionManager.Instance.NativeAPI.NativeHeadTracking;
            m_NativePerception = NRSessionManager.Instance.NativeAPI.NativePerception;

            NRDebugger.Info("[NRTrackingDataProvider] Create");
#if USING_XR_SDK
            m_XRInputSubsystem = NRFrame.CreateXRSubsystem<XRInputSubsystemDescriptor, XRInputSubsystem>(k_idInputSubsystem);
            m_NativePerception.Create(NativeXRPlugin.GetPerceptionGroupHandle());
#else
            m_NativePerception.Create();
#endif
            NRDebugger.Info("[NRTrackingDataProvider] Created");
        }

        public void Start()
        {
            NRDebugger.Info("[NRTrackingDataProvider] Start");
#if USING_XR_SDK
            XRInputSubsystem?.Start();
            m_NativeHeadTracking.Create(NativeXRPlugin.GetHeadTrackingHandle());
#else
            m_NativePerception.Start();
            m_NativeHeadTracking.Create();
#endif
            NRDebugger.Info("[NRTrackingDataProvider] Started");
        }

        public void Pause()
        {
#if USING_XR_SDK
            XRInputSubsystem?.Stop();
#else
            m_NativePerception.Pause();
#endif
        }

        public void Resume()
        {
#if USING_XR_SDK
            XRInputSubsystem?.Start();
#else
            m_NativePerception.Resume();
#endif
        }

        public void Recenter()
        {
            m_NativeHeadTracking.Recenter();
        }

        public void Destroy()
        {
            NRDebugger.Info("[NRTrackingDataProvider] Destroy");
#if USING_XR_SDK
            XRInputSubsystem?.Destroy();
            m_XRInputSubsystem = null;
#else
            m_NativeHeadTracking.Destroy();

            m_NativePerception.Stop();
            m_NativePerception.Destroy();
            NRDebugger.Info("[NRTrackingDataProvider] Destroyed");
#endif
        }

        public bool SwitchTrackingType(TrackingType type)
        {
#if !USING_XR_SDK
            m_NativeHeadTracking.Destroy();
#endif
            m_NativePerception.SwitchTrackingType(type);
#if USING_XR_SDK
            m_NativeHeadTracking.Create(NativeXRPlugin.GetHeadTrackingHandle());
#else
            m_NativePerception.Start();
            m_NativeHeadTracking.Create();
#endif
            return true;
        }

        public bool GetFramePresentHeadPose(ref Pose pose, ref LostTrackingReason lostReason, ref ulong timestamp)
        {
            return m_NativeHeadTracking.GetFramePresentHeadPose(ref pose, ref lostReason, ref timestamp);
        }

        public bool GetHeadPose(ref Pose pose, ulong timestamp)
        {
            return m_NativeHeadTracking.GetHeadPose(ref pose, timestamp);
        }

        public bool InitTrackingType(TrackingType type)
        {
            return m_NativePerception.SwitchTrackingType(type);
        }

        public bool GetFramePresentTimeByCount(uint count, ref ulong timeStamp)
        {
			return m_NativeHeadTracking.GetFramePresentTimeByCount(count, ref timeStamp);
		}
		
        public ulong GetHMDTimeNanos()
        {
            return m_NativePerception.GetHMDTimeNanos();
        }
    }
}
