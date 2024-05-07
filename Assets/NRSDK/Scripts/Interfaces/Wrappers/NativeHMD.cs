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
    using UnityEngine;
    using System.Runtime.InteropServices;

    /// <summary> HMD Eye offset Native API . </summary>
    public partial class NativeHMD
    {
        /// <summary> Handle of the hmd. </summary>
        private UInt64 m_HmdHandle;
        /// <summary> Gets the handle of the hmd. </summary>
        /// <value> The hmd handle. </value>
        public UInt64 HmdHandle
        {
            get
            {
                return m_HmdHandle;
            }
        }

        /// <summary> Create this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Create(UInt64 hmdHandle = 0)
        {
            NRDebugger.Info("[NativeHMD] Create: hmdHandle={0}", hmdHandle);
            if (hmdHandle == 0)
            {
                NativeResult result = NativeApi.NRHMDCreate(ref hmdHandle);
                NativeErrorListener.Check(result, this, "Create", true);
            }

            m_HmdHandle = hmdHandle;
            return m_HmdHandle != 0;
        }

        /// <summary> Start this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Start()
        {
            NRDebugger.Info("[NativeHMD] Start");
            NativeResult result = NativeApi.NRHMDStart(m_HmdHandle);
            NativeErrorListener.Check(result, this, "Start", true);
            return result == NativeResult.Success;
        }

        /// <summary> Pauses this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Pause()
        {
            NativeResult result = NativeApi.NRHMDPause(m_HmdHandle);
            NativeErrorListener.Check(result, this, "Pause", true);
            return result == NativeResult.Success;
        }

        /// <summary> Resumes this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Resume()
        {
            NativeResult result = NativeApi.NRHMDResume(m_HmdHandle);
            NativeErrorListener.Check(result, this, "Resume", true);
            return result == NativeResult.Success;
        }

        /// <summary> Gets device pose from head. </summary>
        /// <param name="device"> The device type.</param>
        /// <returns> The device pose from head. </returns>
        public Pose GetDevicePoseFromHead(NativeDevice device)
        {
            Pose outDevicePoseFromHead = Pose.identity;
            NativeMat4f mat4f = new NativeMat4f(Matrix4x4.identity);
            NativeResult result = NativeApi.NRHMDGetComponentPoseFromHead(m_HmdHandle, device, ref mat4f);
            if (result == NativeResult.Success)
            {
                ConversionUtility.ApiPoseToUnityPose(mat4f, out outDevicePoseFromHead);
            }
            else
            {
                NRDebugger.Warning($"[{GetType()}] {nameof(GetDevicePoseFromHead)} {device}  result: {result}  posemat: {mat4f}");
            }
            return outDevicePoseFromHead;
        }

        /// <summary> Gets projection matrix. </summary>
        /// <param name="outEyesProjectionMatrix"> [in,out] The out eyes projection matrix.</param>
        /// <param name="znear">                   The znear.</param>
        /// <param name="zfar">                    The zfar.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool GetProjectionMatrix(ref EyeProjectMatrixData outEyesProjectionMatrix, float znear, float zfar)
        {
            NativeFov4f fov = new NativeFov4f();
            NativeResult result_left = NativeApi.NRHMDGetComponentFov(m_HmdHandle, NativeDevice.LEFT_DISPLAY, ref fov);
            NativeErrorListener.Check(result_left, this, "GetProjectionMatrix-L");
            NRDebugger.Info("[GetProjectionMatrix] LEFT_DISPLAY: {0}", fov.ToString());
            outEyesProjectionMatrix.LEyeMatrix = ConversionUtility.GetProjectionMatrixFromFov(fov, znear, zfar).ToUnityMat4f();

            NativeResult result_right = NativeApi.NRHMDGetComponentFov(m_HmdHandle, NativeDevice.RIGHT_DISPLAY, ref fov);
            NativeErrorListener.Check(result_right, this, "GetProjectionMatrix-R");
            NRDebugger.Info("[GetProjectionMatrix] RIGHT_DISPLAY: {0}", fov.ToString());
            outEyesProjectionMatrix.REyeMatrix = ConversionUtility.GetProjectionMatrixFromFov(fov, znear, zfar).ToUnityMat4f();

            NativeResult result_Center = NativeApi.NRHMDGetComponentFov(m_HmdHandle, NativeDevice.HEAD_CENTER, ref fov);
            NativeErrorListener.Check(result_Center, this, "GetProjectionMatrix-C");
            NRDebugger.Info("[GetProjectionMatrix] HEAD_CENTER: {0}", fov.ToString());
            outEyesProjectionMatrix.CEyeMatrix = ConversionUtility.GetProjectionMatrixFromFov(fov, znear, zfar).ToUnityMat4f();

            NativeResult result_RGB = NativeApi.NRHMDGetComponentFov(m_HmdHandle, NativeDevice.RGB_CAMERA, ref fov);
            NativeErrorListener.Check(result_RGB, this, "GetProjectionMatrix-RGB");
            NRDebugger.Info("[GetProjectionMatrix] RGBCamera: {0}", fov.ToString());
            outEyesProjectionMatrix.RGBEyeMatrix = ConversionUtility.GetProjectionMatrixFromFov(fov, znear, zfar).ToUnityMat4f();

            return (result_left == NativeResult.Success && result_right == NativeResult.Success && result_Center == NativeResult.Success && result_RGB == NativeResult.Success);
        }

        /// <summary> Gets camera intrinsic matrix. </summary>
        /// <param name="device">               The target device.</param>
        /// <param name="CameraIntrinsicMatix"> [in,out] The camera intrinsic matix.</param>
        /// <returns> The intrinsic matrix of target device. </returns>
        public NativeFov4f GetEyeFov(NativeDevice device)
        {
            NativeFov4f fov = new NativeFov4f();
            var result = NativeApi.NRHMDGetComponentFov(m_HmdHandle, device, ref fov);
            NativeErrorListener.Check(result, this, "GetEyeFov");
            return fov;
        }

        /// <summary> Gets camera intrinsic matrix. </summary>
        /// <param name="camera">               The target camera.</param>
        /// <param name="CameraIntrinsicMatix"> [in,out] The camera intrinsic matix.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool GetCameraIntrinsicMatrix(NativeDevice camera, ref NativeMat3f CameraIntrinsicMatix)
        {
            var result = NativeApi.NRHMDGetComponentIntrinsic(m_HmdHandle, camera, ref CameraIntrinsicMatix);
            return result == NativeResult.Success;
        }

        /// <summary> Get the refresh rate of HMD display. </summary>
        /// <returns> The refresh rate of display. </returns>
        public UInt32 GetRefreshRate()
        {
            UInt32 refreshRate = 60;
            var result = NativeApi.NRHMDGetComponentRefreshRate(m_HmdHandle, NativeDevice.LEFT_DISPLAY, ref refreshRate);
            NativeErrorListener.Check(result, this, "GetRefreshRate");

            return refreshRate;
        }

        /// <summary> Gets camera distortion. </summary>
        /// <param name="camera">        The camera.</param>
        /// <param name="distortion"> A variable-length parameters list containing distortion.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool GetCameraDistortion(NativeDevice camera, ref NRDistortionParams distortion)
        {
            var result = NativeApi.NRHMDGetComponentDistortion(m_HmdHandle, camera, ref distortion);
            return result == NativeResult.Success;
        }

        /// <summary> Gets device resolution. </summary>
        /// <param name="device"> The device.</param>
        /// <returns> The eye resolution. </returns>
        public NativeResolution GetDeviceResolution(NativeDevice device)
        {
            NativeResolution resolution = new NativeResolution(1920, 1080);
#if UNITY_EDITOR
            return resolution;
#else
            var result = NativeApi.NRHMDGetComponentResolution(m_HmdHandle, device, ref resolution);
            NativeErrorListener.Check(result, this, "GetDeviceResolution");
            return resolution;
#endif
        }

        /// <summary> Gets device type of running device. </summary>
        /// <returns> The device type. </returns>
        public NRDeviceType GetDeviceType()
        {
            NRDeviceType deviceType = NRDeviceType.XrealLight;
            NativeApi.NRHMDGetDeviceType(m_HmdHandle, ref deviceType);
            return deviceType;
        }

        /// <summary> Gets device category of running device. </summary>
        /// <returns> The device category. </returns>
        public NRDeviceCategory GetDeviceCategory()
        {
            NRDeviceCategory deviceCategory = NRDeviceCategory.INVALID;
            NativeApi.NRHMDGetDeviceCategory(m_HmdHandle, ref deviceCategory);
            return deviceCategory;
        }

        /// <summary> Gets device type of running device. </summary>
        /// <param name="feature"> The request feature.</param>
        /// <returns> Is the feature supported. </returns>
        public bool IsFeatureSupported(NRSupportedFeature feature)
        {
            bool result = false;
            NativeApi.NRHMDIsFeatureSupported(m_HmdHandle, feature, ref result);
            return result;
        }

        /// <summary> Stop this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Stop()
        {
            NativeResult result = NativeApi.NRHMDStop(m_HmdHandle);
            NativeErrorListener.Check(result, this, "Stop", true);
            return result == NativeResult.Success;
        }

        /// <summary> Destroys this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Destroy()
        {
            NativeResult result = NativeApi.NRHMDDestroy(m_HmdHandle);
            NativeErrorListener.Check(result, this, "Destroy", true);
            m_HmdHandle = 0;
            return result == NativeResult.Success;
        }

        /// <summary> A native api. </summary>
        private struct NativeApi
        {
            /// <summary> Nrhmd create. </summary>
            /// <param name="out_hmd_handle"> [in,out] Handle of the out hmd.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDCreate(ref UInt64 out_hmd_handle);

            /// <summary> Nrhmd start. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDStart(UInt64 hmd_handle);

            /// <summary> Nrhmd get device type. </summary>
            /// <param name="hmd_handle">         Handle of the hmd.</param>
            /// <param name="out_device_type"> [in,out] The out device type.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDGetDeviceType(UInt64 hmd_handle, ref NRDeviceType out_device_type);

            /// <summary> Query the hmd device category. </summary>
            /// <param name="hmd_handle"> The handle of HMD object. </param>
            /// <param name="out_device_type"> The category of the hmd Device. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDGetDeviceCategory(UInt64 hmd_handle, ref NRDeviceCategory out_device_category);

            /// <summary>
            /// Check whether the current feature is supported.
            /// </summary>
            /// <param name="hmd_handle"> Handle of the out hmd.</param>
            /// <param name="feature"> Current feature. </param>
            /// <param name="out_is_supported"> Result of  whether the current feature is supported. </param>
            /// <returns></returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDIsFeatureSupported(UInt64 hmd_handle, NRSupportedFeature feature, ref bool out_is_supported);

            /// <summary> Nrhmd pause. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDPause(UInt64 hmd_handle);

            /// <summary> Nrhmd resume. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDResume(UInt64 hmd_handle);

            /// <summary> Nrhmd get eye pose from head. </summary>
            /// <param name="hmd_handle">         Handle of the hmd.</param>
            /// <param name="component">          The component.</param>
            /// <param name="out_component_pose_from_head"> [in,out] The out component pose from head.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDGetComponentPoseFromHead(UInt64 hmd_handle, NativeDevice component, ref NativeMat4f out_component_pose_from_head);

            /// <summary> Nrhmd get eye fov. </summary>
            /// <param name="hmd_handle">  Handle of the hmd.</param>
            /// <param name="component">   The component.</param>
            /// <param name="out_component_fov"> [in,out] Fov of the component.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDGetComponentFov(UInt64 hmd_handle, NativeDevice component, ref NativeFov4f out_component_fov);

            /// <summary> Nrhmd get camera intrinsic matrix. </summary>
            /// <param name="hmd_handle">           Handle of the hmd.</param>
            /// <param name="camera">               The camera.</param>
            /// <param name="out_intrinsic_matrix"> [in,out] The out intrinsic matrix.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDGetComponentIntrinsic(
                    UInt64 hmd_handle, NativeDevice camera, ref NativeMat3f out_intrinsic_matrix);

            /// <summary> Get the refresh rate of HMD display. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <param name="out_fresh_rate"> The refresh rate of display.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDGetComponentRefreshRate(
                    UInt64 hmd_handle, NativeDevice component, ref UInt32 out_fresh_rate);

            /// <summary> Nrhmd get camera distortion parameters. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <param name="camera">     The camera.</param>
            /// <param name="out_params"> A variable-length parameters list containing out parameters.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDGetComponentDistortion(
                    UInt64 hmd_handle, NativeDevice camera, ref NRDistortionParams out_params);

            /// <summary> Nrhmd get eye resolution. </summary>
            /// <param name="hmd_handle">         Handle of the hmd.</param>
            /// <param name="component">          The component.</param>
            /// <param name="out_component_resolution"> [in,out] Resolution of the component.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDGetComponentResolution(UInt64 hmd_handle, NativeDevice component, ref NativeResolution out_component_resolution);

            /// <summary> Nrhmd stop. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDStop(UInt64 hmd_handle);

            /// <summary> Nrhmd destroy. </summary>
            /// <param name="hmd_handle"> Handle of the hmd.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHMDDestroy(UInt64 hmd_handle);
        };
    }
}
