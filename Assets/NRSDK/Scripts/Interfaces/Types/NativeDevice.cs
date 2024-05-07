﻿/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal
{
    /// <summary> Enum for device names. </summary>
    public enum NativeDevice
    {
        /// <summary> 
        /// Left display. 
        /// </summary>
        LEFT_DISPLAY = 0,

        /// <summary> 
        /// Right display. 
        /// </summary>
        RIGHT_DISPLAY = 1,

        /// <summary> 
        /// RGB camera. 
        /// </summary>
        RGB_CAMERA = 2,

        /// <summary> 
        ///  The left grayscale camera.
        ///  Only supported in the version of Enterprise.
        /// </summary>
        LEFT_GRAYSCALE_CAMERA = 3,

        /// <summary> 
        ///  The right grayscale camera.
        ///  Only supported in the version of Enterprise.
        /// </summary>
        RIGHT_GRAYSCALE_CAMERA = 4,

        /// <summary> 
        ///  The magnetic of HMD.
        ///  Only supported in the version of Enterprise.
        /// </summary>
        MAGENTICE = 5,

        /// <summary> 
        ///  The center of the displays.
        /// </summary>
        HEAD_CENTER = 6,

        /// <summary> 
        ///  The imu of HMD.
        /// </summary>
        IMU = 7,
    }
}
