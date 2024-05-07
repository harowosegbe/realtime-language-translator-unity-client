/****************************************************************************
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
    public class GetSurportedFeatures : MonoBehaviour
    {
        void Start()
        {
            var deviceCategory = NRDevice.Subsystem.GetDeviceCategory();

            bool rgbcamera = NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_RGB_CAMERA);
            bool glasses_wearing_status = NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_WEARING_STATUS_OF_GLASSES);
            bool controller = NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_CONTROLLER);
            bool handtracking_tracking_rotation = NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_PERCEPTION_HEAD_TRACKING_ROTATION);
            bool handtracking_tracking_position = NRDevice.Subsystem.IsFeatureSupported(NRSupportedFeature.NR_FEATURE_PERCEPTION_HEAD_TRACKING_POSITION);

            NRDebugger.Info("deviceCategory:{0}, rgbcamera:{1} glasses_wearing_status:{2} controller:{3} handtracking_tracking_rotation:{4} handtracking_tracking_position:{5}",
                deviceCategory, rgbcamera, glasses_wearing_status, controller, handtracking_tracking_rotation, handtracking_tracking_position);
        }
    }
}
