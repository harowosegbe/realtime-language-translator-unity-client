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
    /// <summary> Values that represent native results. </summary>
    public enum NativeResult
    {
        /// <summary> Success. </summary>
        Success = 0,

        /// <summary> Failed. </summary>
        Failure = 1,

        /// <summary> Ivalid argument error. </summary>
        InvalidArgument = 2,

        /// <summary> Memory is not enough. </summary>
        NotEnoughMemory = 3,

        /// <summary> Unsupported error. </summary>
        UnSupported = 4,

        /// <summary> Glasses diconnect error. </summary>
        GlassesDisconnect = 5,

        /// <summary> SDK version is not matched with server. </summary>
        SdkVersionMismatch = 6,

        /// <summary> Sdcard read permission is denied. </summary>
        SdcardPermissionDeny = 7,

        /// <summary> RGB camera is not found. </summary>
        RGBCameraDeviceNotFind = 8,

        /// <summary> DP device is not found. </summary>
        DPDeviceNotFind = 9,

        /// <summary> Tracking system is not running. </summary>
        TrackingNotRunning = 10,

        /// <summary> Get glasses display failed. </summary>
        GetDisplayFailure = 11,

        /// <summary> Glasses display mode is not 3d. </summary>
        GetDisplayModeMismatch = 12,

        /// <summary> Find a cooldown when call the function. </summary>
        InTheCoolDown = 13,

        /// <summary> Not support hand tracking calculation. </summary>
        UnSupportedHandtrackingCalculation = 14,

        /// <summary> The operation is busy. </summary>
        Busy = 15,

        /// <summary> The operation is processing. </summary>
        Processing = 16,

        /// <summary> The number is limited. </summary>
        NumberLimited = 17,

        /// <summary> The display is not in stereo mode. </summary>
        DisplayNoInStereoMode = 18,

        /// <summary> The data is invalid. </summary>
        InvalidData = 19,
        
        /// <summary> Runtime is not found. </summary>
        NR_RESULT_NOT_FIND_RUNTIME = 20,
            
        // Control channel internal error
        ControlChannelInternalError = 100,
        // Control channel initialize fail
        ControlChannelInitFail = 101,
        // Control channel start fail
        ControlChannelStartFail = 102,

        // IMU channel internal error
        ImuChannelInternalError = 200,
        // IMU channel initialize fail
        ImuChannelInitFail = 201,
        // IMU channel start faile
        ImuChannelStartFail = 202,
        // IMU channel data frequency too low
        ImuChannelFrequencyCritical = 203,

        // Display control channel internal error
        DisplayControlChannelInternalError = 300,
        // Display control initialize fail
        DisplayControlChannelInitFail = 301,
        // Display control start fail
        DisplayControlChannelStartFail = 302,
        // Display control channel data frequency too low
        DisplayControlChannelFrequencyCritical = 303,
    }
}
