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
    using System;
    using System.Runtime.InteropServices;

    /// <summary> Callback, called when the camera image. </summary>
    /// <param name="rgb_camera_handle">       Handle of the RGB camera.</param>
    /// <param name="rgb_camera_image_handle"> Handle of the RGB camera image.</param>
    /// <param name="userdata">                The userdata.</param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CameraImageCallback(UInt64 rgb_camera_handle,
               UInt64 rgb_camera_image_handle, UInt64 userdata);

    /// <summary> Interface for camera data provider. </summary>
    public interface ICameraDataProvider
    {
        /// <summary> Creates a new bool. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool Create();

        /// <summary> Gets raw data. </summary>
        /// <param name="imageHandle"> Handle of the image.</param>
        /// <param name="camera">      The camera.</param>
        /// <param name="ptr">         [in,out] The pointer.</param>
        /// <param name="size">        [in,out] The size.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool GetRawData(UInt64 imageHandle, NativeDevice camera, ref IntPtr ptr, ref int size);

        /// <summary> Gets a resolution. </summary>
        /// <param name="imageHandle"> Handle of the image.</param>
        /// <param name="camera">      The camera.</param>
        /// <returns> The resolution. </returns>
        NativeResolution GetResolution(UInt64 imageHandle, NativeDevice camera);

        /// <summary> Gets hmd time nanos. </summary>
        /// <param name="imageHandle"> Handle of the image.</param>
        /// <param name="camera">      The camera.</param>
        /// <returns> The hmd time nanos. </returns>
        UInt64 GetHMDTimeNanos(UInt64 imageHandle, NativeDevice camera);

        /// <summary> Get exposure time. </summary>
        /// <param name="imageHandle"> Handle of the image. </param>
        /// <param name="camera">      The camera. </param>
        /// <returns> Exposure time of the image. </returns>
        UInt32 GetExposureTime(UInt64 imageHandle, NativeDevice camera);

        /// <summary> Get Gain. </summary>
        /// <param name="imageHandle"> Handle of the image. </param>
        /// <param name="camera">      The camera. </param>
        /// <returns> Gain of the image. </returns>
        UInt32 GetGain(UInt64 imageHandle, NativeDevice camera);

        /// <summary> Callback, called when the set capture. </summary>
        /// <param name="callback"> The callback.</param>
        /// <param name="userdata"> (Optional) The userdata.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool SetCaptureCallback(CameraImageCallback callback, UInt64 userdata = 0);

        /// <summary> Sets image format. </summary>
        /// <param name="format"> Describes the format to use.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool SetImageFormat(CameraImageFormat format);

        /// <summary> Starts a capture. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool StartCapture();

        /// <summary>
        /// Pause Capture
        /// </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool PauseCapture();

        /// <summary>
        /// Resume Capture
        /// </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool ResumeCapture();

        /// <summary> Stops a capture. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool StopCapture();

        /// <summary> Destroys the image described by imageHandle. </summary>
        /// <param name="imageHandle"> Handle of the image.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool DestroyImage(UInt64 imageHandle);

        /// <summary> Releases this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        bool Release();
    }
}