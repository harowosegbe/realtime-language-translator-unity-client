﻿/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using UnityEngine;

namespace NRKernal.NRExamples
{
    public class CameraResetController : MonoBehaviour
    {
        void Start()
        {
            NRInput.AddClickListener(ControllerHandEnum.Right, ControllerButton.APP, () =>
            {
                var poseTracker = NRSessionManager.Instance.NRHMDPoseTracker;
                poseTracker.ResetWorldMatrix(true);
            });
        }

        public void ResetCameraToOrigin(bool resetPitch)
        {
            var poseTracker = NRSessionManager.Instance.NRHMDPoseTracker;
            poseTracker.ResetWorldMatrix(resetPitch);
        }
    }
}
