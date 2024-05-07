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
    using System;
    using System.Collections.Generic;
    using UnityEngine;
#if USING_XR_SDK
    using UnityEngine.XR;
    using Unity.XR.NRSDK;
#endif

    /// <summary>
    /// Holds information about NR Device's pose in the world coordinate, trackables, etc.. Through
    /// this class, application can get the information of current frame. It contains session status,
    /// lost tracking reason, device pose, trackables, etc. </summary>
    public class NRFrame
    {
        /// <summary> Get the tracking state of HMD. </summary>
        /// <value> The session status. </value>
        public static SessionState SessionStatus
        {
            get
            {
                return NRSessionManager.Instance.SessionState;
            }
        }

        /// <summary> Get the lost tracking reason of HMD. </summary>
        /// <value> The lost tracking reason. </value>
        public static LostTrackingReason LostTrackingReason
        {
            get
            {
                return NRSessionManager.Instance.LostTrackingReason;
            }
        }

        /// <summary> If sdk is running on xrplugin framework. </summary>
        /// <value> The value. </value>
        public static bool IsXR
        {
            get
            {
#if USING_XR_SDK
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary> If sdk is running in mono mode. </summary>
        /// <value> If sdk is running in mono mode. </value>
        public static bool MonoMode
        {
            get
            {
                return NRDevice.Instance.MonoMode;
            }
        }

        /// <summary> The head pose. </summary>
        private static Pose m_HeadPose;

        /// <summary> Get the pose of device in unity world coordinate. </summary>
        /// <value> Pose of device. </value>
        public static Pose HeadPose
        {
            get
            {
                return m_HeadPose;
            }
        }

        public static bool isHeadPoseReady { get; private set; } = false;

        /// <summary> Gets head pose by recommend timestamp. </summary>
        /// <param name="pose">      [in,out] The pose.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        [Obsolete("Use 'GetFramePresentHeadPose' instead.")]
        public static bool GetHeadPoseRecommend(ref Pose pose)
        {
            if (SessionStatus == SessionState.Running)
            {
                return NRSessionManager.Instance.NativeAPI.NativeHeadTracking.GetHeadPoseRecommend(ref pose);
            }
            return false;
        }

        /// <summary> Gets head pose by timestamp. </summary>
        /// <param name="pose">      [in,out] The pose.</param>
        /// <param name="timestamp"> The timestamp.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public static bool GetHeadPoseByTime(ref Pose pose, UInt64 timestamp)
        {
            if (SessionStatus == SessionState.Running)
            {
                return NRSessionManager.Instance.TrackingSubSystem.GetHeadPose(ref pose, timestamp);
            }
            return false;
        }

        /// <summary>
        /// Get the pose information when the current frame display on the screen.
        /// </summary>
        /// <param name="pose"></param>
        /// <param name="timestamp">current timestamp to the pose.</param>
        /// <returns></returns>
        public static bool GetFramePresentHeadPose(ref Pose pose, ref LostTrackingReason lostReason, ref UInt64 timestamp)
        {
            if (NRSessionManager.Instance.IsRunning)
            {
                isHeadPoseReady = NRSessionManager.Instance.TrackingSubSystem.GetFramePresentHeadPose(ref pose, ref lostReason, ref timestamp);
                return isHeadPoseReady;
            }
            return false;
        }

        /// <summary> Has we got the eye position from head? </summary>
        private static bool m_HasGetEyePoseFrameHead = false;

        /// <summary> The eye position from head. </summary>
        private static EyePoseData m_EyePoseFromHead;

        /// <summary> Get the offset position between eye and head. </summary>
        /// <value> The eye pose from head. </value>
        public static EyePoseData EyePoseFromHead
        {
            get
            {
                if (m_HasGetEyePoseFrameHead)
                    return m_EyePoseFromHead;

                if (SessionStatus == SessionState.Running)
                {
                    m_EyePoseFromHead.LEyePose = NRDevice.Subsystem.GetDevicePoseFromHead(NativeDevice.LEFT_DISPLAY);
                    m_EyePoseFromHead.REyePose = NRDevice.Subsystem.GetDevicePoseFromHead(NativeDevice.RIGHT_DISPLAY);
                    m_EyePoseFromHead.CEyePose = NRDevice.Subsystem.GetDevicePoseFromHead(NativeDevice.HEAD_CENTER);
                    m_EyePoseFromHead.RGBEyePose = NRDevice.Subsystem.GetDevicePoseFromHead(NativeDevice.RGB_CAMERA);
                    m_HasGetEyePoseFrameHead = true;
                }
                return m_EyePoseFromHead;
            }
        }

        /// <summary> Get the offset position between device and head. </summary>
        /// <value> The device pose from head. </value>
        public static Pose GetDevicePoseFromHead(NativeDevice device)
        {
            return NRDevice.Subsystem.GetDevicePoseFromHead(device);
        }

        /// <summary> Get the project matrix of camera in unity. </summary>
        /// <param name="result"> [out] True to result.</param>
        /// <param name="znear">  The znear.</param>
        /// <param name="zfar">   The zfar.</param>
        /// <returns> project matrix of camera. </returns>
        public static EyeProjectMatrixData GetEyeProjectMatrix(out bool result, float znear, float zfar)
        {
            return NRDevice.Subsystem.GetEyeProjectMatrix(out result, znear, zfar);
        }

        /// <summary> Get the intrinsic matrix of device. </summary>
        /// <returns> The device intrinsic matrix. </returns>
        public static NativeMat3f GetDeviceIntrinsicMatrix(NativeDevice device)
        {
            return NRDevice.Subsystem.GetDeviceIntrinsicMatrix(device);
        }

        /// <summary> Get the distortion param of device. </summary>
        /// <returns> The device intrinsic matrix. </returns>
        public static NRDistortionParams GetDeviceDistortion(NativeDevice device)
        {
            return NRDevice.Subsystem.GetDeviceDistortion(device);
        }

        /// <summary> Get the intrinsic matrix of rgb camera. </summary>
        /// <returns> The RGB camera intrinsic matrix. </returns>
        public static NativeMat3f GetRGBCameraIntrinsicMatrix()
        {
            return GetDeviceIntrinsicMatrix(NativeDevice.RGB_CAMERA);
        }

        /// <summary> Get the Distortion of rgbcamera. </summary>
        /// <returns> The RGB camera distortion. </returns>
        public static NRDistortionParams GetRGBCameraDistortion()
        {
            return GetDeviceDistortion(NativeDevice.RGB_CAMERA);
        }

        /// <summary> Gets the resolution of device. </summary>
        /// <param name="eye"> device index.</param>
        /// <returns> The device resolution. </returns>
        public static NativeResolution GetDeviceResolution(NativeDevice device)
        {
            return NRDevice.Subsystem.GetDeviceResolution(device);
        }

        /// <summary> Gets device fov. </summary>
        /// <param name="eye">         The display index.</param>
        /// <param name="fov"> [in,out] The out device fov.</param>
        /// <returns> A NativeResult. </returns>
        public static void GetEyeFov(NativeDevice eye, ref NativeFov4f fov)
        {
            NRDevice.Subsystem.GetEyeFov(eye, ref fov);
        }

        private static UInt64 m_CurrentPoseTimeStamp = 0;
        public static UInt64 CurrentPoseTimeStamp
        {
            get
            {
                return m_CurrentPoseTimeStamp;
            }
        }

#if USING_XR_SDK
        static internal List<XRNodeState> m_NodeStates = new List<XRNodeState>();
        static internal bool GetNodePoseData(List<XRNodeState> nodeStates, XRNode node, out Pose resultPose)
        {
            resultPose = Pose.identity;
            for (int i = 0; i < nodeStates.Count; i++)
            {
                var nodeState = nodeStates[i];
                if (nodeState.nodeType == node)
                {
                    bool rst = nodeState.TryGetPosition(out resultPose.position);
                    rst = rst && nodeState.TryGetRotation(out resultPose.rotation);
                    return rst;
                }
            }
            return false;
        }
#endif

        internal static void ResetHeadPose()
        {
            m_CurrentPoseTimeStamp = 0;
            m_HeadPose = Pose.identity;
        }

        internal static void OnPreUpdate(ref LostTrackingReason lostTrackReason)
        {
#if USING_XR_SDK && !UNITY_EDITOR
            Pose pose = Pose.identity;
            InputTracking.GetNodeStates(m_NodeStates);
            GetNodePoseData(m_NodeStates, XRNode.Head, out pose);

            lostTrackReason = NativeXRPlugin.GetLostTrackingReason();
            var timeStamp = NativeXRPlugin.GetFramePresentTime();

            if (NRDebugger.logLevel <= LogLevel.Debug)
            {
                var rotEuler = pose.rotation.eulerAngles;
                NRDebugger.Info("[NRFrame] OnPreUpdate-XR: LostTrackReason={0}, pose={1}-{6}, PresentTime={2}, lastHeadPose={3}, lastPresentTime={4}, frameHandle={5}",
                    lostTrackReason, pose.ToString("F6"), timeStamp, m_HeadPose.ToString("F6"), m_CurrentPoseTimeStamp, NRSessionManager.Instance.NRSwapChainMan.FrameHandle, rotEuler.ToString("F6"));
            }

            m_HeadPose = pose;
            m_CurrentPoseTimeStamp = timeStamp;
#else
            Pose pose = Pose.identity;
            LostTrackingReason lostReason = LostTrackingReason.NONE;
            ulong timeStamp = 0;
            bool result = GetFramePresentHeadPose(ref pose, ref lostReason, ref timeStamp);
            if (result)
            {
                lostTrackReason = lostReason;
                if (lostReason != LostTrackingReason.INITIALIZING)
                {
                    m_HeadPose = pose;
                    m_CurrentPoseTimeStamp = timeStamp;
                }
                else
                {
                    NRDebugger.Info("[NRFrame] OnPreUpdate: LostTrackReason={0}, pose={1}, PresentTime={2}, lastHeadPose={3}, lastPresentTime={4}, frameHandle={5}",
                        lostReason, pose.ToString("F4"), timeStamp, m_HeadPose.ToString("F4"), m_CurrentPoseTimeStamp, NRSessionManager.Instance.NRSwapChainMan.FrameHandle);
                }
            }
            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[NRFrame] OnPreUpdate: result={2}, LostTrackReason={3}, presentTime={4}, pos={0}, headPos={1}, ", pose.ToString("F4"), m_HeadPose.ToString("F4"), result, LostTrackingReason, m_CurrentPoseTimeStamp);
#endif
        }

        /// <summary> Get the list of trackables with specified filter. </summary>
        /// <typeparam name="T"> Generic type parameter.</typeparam>
        /// <param name="trackables"> A list where the returned trackable is stored. The previous values
        ///                           will be cleared.</param>
        /// <param name="filter">     Query filter.</param>
        public static void GetTrackables<T>(List<T> trackables, NRTrackableQueryFilter filter) where T : NRTrackable
        {
            if (SessionStatus != SessionState.Running)
            {
                return;
            }
            trackables.Clear();
            NRSessionManager.Instance.TrackableFactory.GetTrackables<T>(trackables, filter);
        }

        public static Matrix4x4 GetWorldMatrixFromUnityToNative()
        {
            var hmdPoseTracker = NRSessionManager.Instance.NRHMDPoseTracker;
            if (hmdPoseTracker == null)
            {
                return Matrix4x4.identity;
            }
            else
            {
                return hmdPoseTracker.GetWorldOffsetMatrixFromNative();
            }
        }

        /// <summary> Set a plane in camera or global space that acts as the focal plane of the Scene for this frame. </summary>
        /// <param name="point"> The position of the focal point.</param>
        /// <param name="normal"> The normal of the plane being viewed at the focal point.</param>
        /// <param name="spaceType"> Space type of the plane. While NR_REFERENCE_SPACE_VIEW means that the plane is relative to the Camera, NR_REFERENCE_SPACE_GLOBAL means that the plane is in world space.</param>
        public static void SetFocusPlane(Vector3 point, Vector3 normal, NRReferenceSpaceType spaceType = NRReferenceSpaceType.NR_REFERENCE_SPACE_VIEW)
        {
            // NRDebugger.Info("SetFocusPlane: point={0}, normal={1}", point.ToString("F2"), normal.ToString("F2"));

#if USING_XR_SDK
            if (NRDevice.XRDisplaySubsystem != null && NRDevice.XRDisplaySubsystem.running)
                NRDevice.XRDisplaySubsystem?.SetFocusPlane(point, normal, Vector3.zero);
#endif
            NRSessionManager.Instance.NRSwapChainMan?.SetFocusPlane(point, normal, spaceType);
        }

        /// <summary> Set a focus point in camera space that acts as the focal point of the Scene for this frame. </summary>
        /// <param name="distance"> The distance of the focal point relative to camera.</param>
        public static void SetFocusDistance(float distance)
        {
            // NRDebugger.Info("SetFocusDistance: distance={0}", distance);
            Vector3 point = new Vector3(0, 0, distance);
            Vector3 normal = -Vector3.forward;
#if USING_XR_SDK
            if (NRDevice.XRDisplaySubsystem != null && NRDevice.XRDisplaySubsystem.running)
                NRDevice.XRDisplaySubsystem?.SetFocusPlane(point, normal, Vector3.zero);
#endif
            NRSessionManager.Instance.NRSwapChainMan?.SetFocusPlane(point, normal, NRReferenceSpaceType.NR_REFERENCE_SPACE_VIEW);
        }

#if USING_XR_SDK
        internal static TSubsystem CreateXRSubsystem<TDescriptor, TSubsystem>(string id)
            where TDescriptor : UnityEngine.ISubsystemDescriptor
            where TSubsystem : class, UnityEngine.ISubsystem
        {
            List<TDescriptor> descriptors = new List<TDescriptor>();
            SubsystemManager.GetSubsystemDescriptors<TDescriptor>(descriptors);

            UnityEngine.ISubsystem subsys = null;
            if (descriptors.Count > 0)
            {
                foreach (var descriptor in descriptors)
                {
                    if (String.Compare(descriptor.id, id, true) == 0)
                    {
                        subsys = descriptor.Create();
                    }
                }
            }
            return subsys as TSubsystem;
        }
#endif

        internal static TSubsystem CreateSubsystem<TDescriptor, TSubsystem>(string id)
            where TDescriptor : NRKernal.ISubsystemDescriptor
            where TSubsystem : class, NRKernal.ISubsystem
        {
            List<TDescriptor> descriptors = new List<TDescriptor>();
            NRSubsystemManager.GetSubsystemDescriptors(descriptors);

            NRKernal.ISubsystem subsys = null;
            foreach (var descriptor in descriptors)
            {
                if (descriptor.id.Equals(id))
                {
                    subsys = descriptor.Create();
                }
            }
            return subsys as TSubsystem;
        }

        internal static void DestroySubsystem<TDescriptor, TSubsystem>(string id)
            where TDescriptor : NRKernal.ISubsystemDescriptor
            where TSubsystem : class, NRKernal.ISubsystem
        {
            List<TDescriptor> descriptors = new List<TDescriptor>();
            NRSubsystemManager.GetSubsystemDescriptors(descriptors);

            foreach (var descriptor in descriptors)
            {
                if (descriptor.id.Equals(id))
                {
                    descriptor.Destroy();
                }
            }
        }

        public static int targetFrameRate
        {
            get
            {
#if USING_XR_SDK && !UNITY_EDITOR
                return NativeXRPlugin.GetTargetFrameRate();
#else
                return Application.targetFrameRate;
#endif
            }
            set
            {
#if USING_XR_SDK && !UNITY_EDITOR
                NativeXRPlugin.SetTargetFrameRate(value);
#else
                Application.targetFrameRate = value;
#endif
            }
        }
        
        /// <summary> Gets a value indicating whether this object is linear color space. </summary>
        /// <value> True if this object is linear color space, false if not. </value>
        public static bool isLinearColorSpace
        {
            get
            {
                return QualitySettings.activeColorSpace == ColorSpace.Linear;
            }
        }

        public static void Destroy()
        {
            isHeadPoseReady = false;
            m_HasGetEyePoseFrameHead = false;
            m_CurrentPoseTimeStamp = 0;
            m_HeadPose = Pose.identity;
        }

        public static bool isXRRenderMultiview
        {
            get
            {
                // return false;
#if USING_XR_SDK
                NRSettings setting = NRXRLoader.GetSettings();
                return setting.m_StereoRenderingModeAndroid == NRSettings.StereoRenderingModeAndroid.Multiview;
#else
                return false;
#endif
            }
        }
    }
}
