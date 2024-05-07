/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using System.Collections;
using UnityEngine;

namespace NRKernal.NRExamples
{
    /// <summary> An ensure slam tracking mode. </summary>
    public class EnsureSlamTrackingMode : MonoBehaviour
    {
        /// <summary> Type of the tracking. </summary>
        [SerializeField]
        private TrackingType m_TrackingType = TrackingType.Tracking6Dof;

        /// <summary> Starts this object. </summary>
        void Start()
        {
            m_TrackingType = NRHMDPoseTracker.AdaptTrackingType(m_TrackingType);
            StartCoroutine(EnsureTrackingType());
        }

        private IEnumerator EnsureTrackingType()
        {
            WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
            if (m_TrackingType == TrackingType.Tracking0Dof && NRSessionManager.Instance.NRHMDPoseTracker.TrackingMode != m_TrackingType)
            {
                while (!NRSessionManager.Instance.NRHMDPoseTracker.ChangeTo0Dof())
                {
                    yield return waitForEndOfFrame;
                }
            }
            else if (m_TrackingType == TrackingType.Tracking0DofStable && NRSessionManager.Instance.NRHMDPoseTracker.TrackingMode != m_TrackingType)
            {
                while (!NRSessionManager.Instance.NRHMDPoseTracker.ChangeTo0DofStable())
                {
                    yield return waitForEndOfFrame;
                }
            }
            else if (m_TrackingType == TrackingType.Tracking3Dof && NRSessionManager.Instance.NRHMDPoseTracker.TrackingMode != m_TrackingType)
            {
                while (!NRSessionManager.Instance.NRHMDPoseTracker.ChangeTo3Dof())
                {
                    yield return waitForEndOfFrame;
                }
            }
            else if (m_TrackingType == TrackingType.Tracking6Dof && NRSessionManager.Instance.NRHMDPoseTracker.TrackingMode != m_TrackingType)
            {
                while (!NRSessionManager.Instance.NRHMDPoseTracker.ChangeTo6Dof())
                {
                    yield return waitForEndOfFrame;
                }
            }
        }
    }
}