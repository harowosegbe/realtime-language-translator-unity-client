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
    using UnityEngine;

#if UNITY_EDITOR
    public class NREmulatorTrackingDataProvider : ITrackingDataProvider
    {
        private NREmulatorHeadPose m_NREmulatorHeadPose;
        private GameObject m_NREmulatorManager;
        public NREmulatorTrackingDataProvider() { }

        public void Start()
        {
            MainThreadDispather.QueueOnMainThread(() =>
            {
                if (!NREmulatorManager.Inited && !GameObject.Find("NREmulatorManager"))
                {
                    NREmulatorManager.Inited = true;
                    m_NREmulatorManager = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/NREmulatorManager"));
                }
                if (!GameObject.Find("NREmulatorHeadPos"))
                {
                    m_NREmulatorHeadPose = GameObject.Instantiate<NREmulatorHeadPose>(
                        Resources.Load<NREmulatorHeadPose>("Prefabs/NREmulatorHeadPose")
                    );
                }
            });
        }

        public void Pause() { }

        public void Resume() { }

        public void Destroy() 
        {
            if (m_NREmulatorHeadPose != null)
            {
                GameObject.Destroy(m_NREmulatorHeadPose.gameObject);
                m_NREmulatorHeadPose = null;
            }
            if (m_NREmulatorManager != null)
            {
                GameObject.Destroy(m_NREmulatorManager);
                m_NREmulatorManager = null;
            }
            NREmulatorManager.Inited = false;
        }

        public bool GetFramePresentHeadPose(ref Pose pose, ref LostTrackingReason lostReason, ref ulong timestamp)
        {
            if (m_NREmulatorHeadPose == null)
            {
                return false;
            }
            pose = m_NREmulatorHeadPose.headPose;
            lostReason = LostTrackingReason.NONE;
            timestamp = NRTools.GetTimeStamp();
            return true;
        }

        public bool GetHeadPose(ref Pose pose, ulong timestamp)
        {
            if (m_NREmulatorHeadPose == null)
            {
                return false;
            }
            pose = m_NREmulatorHeadPose.headPose;
            return true;
        }

        public bool InitTrackingType(TrackingType type)
        {
            return true;
        }

        public bool SwitchTrackingType(TrackingType type)
        {
            return true;
        }

        public void Recenter() { }

        public bool GetFramePresentTimeByCount(uint count, ref ulong timeStamp)
        {
            timeStamp = NRTools.GetTimeStamp();
			return true;
		}
		
        public ulong GetHMDTimeNanos()
        {
            return 0;
        }
    }
#endif
}
