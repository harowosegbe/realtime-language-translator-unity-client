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
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary> 6-dof Head Tracking's Native API . </summary>
    public class NativeHeadTracking
    {
        /// <summary> The native interface. </summary>
        private NativeInterface m_NativeInterface;
        /// <summary> Gets the handle of the tracking. </summary>
        /// <value> The tracking handle. </value>
        public UInt64 TrackingHandle
        {
            get
            {
                return m_NativeInterface.PerceptionHandle;
            }
        }

        /// <summary> Handle of the head tracking. </summary>
        private UInt64 m_HeadTrackingHandle = 0;
        /// <summary> Gets the handle of the head tracking. </summary>
        /// <value> The head tracking handle. </value>
        public UInt64 HeadTrackingHandle
        {
            get
            {
                return m_HeadTrackingHandle;
            }
        }

        /// <summary> Constructor. </summary>
        /// <param name="nativeInterface"> The native interface.</param>
        public NativeHeadTracking(NativeInterface nativeInterface)
        {
            m_NativeInterface = nativeInterface;
        }

        /// <summary> Creates a new bool. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Create(UInt64 headTrackingHandle = 0)
        {
            if (m_NativeInterface.PerceptionHandle == 0)
            {
                return false;
            }

            NRDebugger.Info("[NativeHeadTracking] Create: TrackingHandle={0}, headTrackingHandle={1}", m_NativeInterface.PerceptionHandle, headTrackingHandle);
            if (headTrackingHandle == 0)
            {
                var result = NativeApi.NRHeadTrackingCreate(m_NativeInterface.PerceptionHandle, ref headTrackingHandle);
                NativeErrorListener.Check(result, this, "Create");
            }
            m_HeadTrackingHandle = headTrackingHandle;
            return m_HeadTrackingHandle != 0;
        }

        /// <summary> Gets head pose. </summary>
        /// <param name="pose">      [in,out] The pose.</param>
        /// <param name="timestamp"> (Optional) The timestamp.</param>
        /// <param name="predict">   (Optional) The predict.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool GetHeadPose(ref Pose pose, UInt64 timestamp)
        {
            if (m_HeadTrackingHandle == 0)
            {
                pose = Pose.identity;
                return false;
            }
            UInt64 headPoseHandle = 0;
            var acquireTrackingPoseResult = NativeApi.NRHeadTrackingAcquireHeadPose(m_NativeInterface.PerceptionHandle, m_HeadTrackingHandle, timestamp, ref headPoseHandle);
            if (acquireTrackingPoseResult != NativeResult.Success)
            {
                return false;
            }
            NativeMat4f headpos_native = new NativeMat4f(Matrix4x4.identity);
            var getPoseResult = NativeApi.NRHeadPoseGetPose(m_NativeInterface.PerceptionHandle, headPoseHandle, ref headpos_native);
            if (getPoseResult != NativeResult.Success)
            {
                return false;
            }
            ConversionUtility.ApiPoseToUnityPose(headpos_native, out pose);
            NativeApi.NRHeadPoseDestroy(m_NativeInterface.PerceptionHandle, headPoseHandle);
            return (acquireTrackingPoseResult == NativeResult.Success) && (getPoseResult == NativeResult.Success);
        }

        public bool GetHeadPoseRecommend(ref Pose pose)
        {
            if (m_HeadTrackingHandle == 0)
            {
                pose = Pose.identity;
                return false;
            }
            ulong recommend_time = m_NativeInterface.NativePerception.GetHMDTimeNanos();
            ulong predict_time = 0;
            var result = NativeApi.NRHeadTrackingGetRecommendPredictTime(m_NativeInterface.PerceptionHandle, m_HeadTrackingHandle, ref predict_time);
            if (result != NativeResult.Success)
            {
                return false;
            }
            recommend_time += predict_time;

            return GetHeadPose(ref pose, recommend_time);
        }

        public void Recenter()
        {
            if (m_HeadTrackingHandle == 0)
            {
                return;
            }
            var result = NativeApi.NRHeadTrackingRecenter(m_NativeInterface.PerceptionHandle, m_HeadTrackingHandle);
            NativeErrorListener.Check(result, this, "Recenter");
        }

        public bool GetFramePresentHeadPose(ref Pose pose, ref LostTrackingReason lostReason, ref UInt64 timestamp)
        {
            if (m_HeadTrackingHandle == 0 || m_NativeInterface.PerceptionHandle == 0)
            {
                NRDebugger.Error("[NativeTrack] GetFramePresentHeadPose: trackingHandle is zero");
                pose = Pose.identity;
                timestamp = 0;
                return false;
            }
            UInt64 headPoseHandle = 0;
            m_NativeInterface.NativeRenderring.GetFramePresentTime(ref timestamp);
            var acquireTrackingPoseResult = NativeApi.NRHeadTrackingAcquireHeadPose(m_NativeInterface.PerceptionHandle, m_HeadTrackingHandle, timestamp, ref headPoseHandle);
            if (acquireTrackingPoseResult != NativeResult.Success)
            {
                lostReason = LostTrackingReason.PRE_INITIALIZING;
                NRDebugger.Info("[NativeTrack] GetFramePresentHeadPose PRE_INITIALIZING");
                return true;
            }

            var trackReasonRst = NativeApi.NRHeadPoseGetTrackingReason(m_NativeInterface.PerceptionHandle, headPoseHandle, ref lostReason);
            NativeErrorListener.Check(trackReasonRst, this, "GetFramePresentHeadPose-LostReason");
            // NRDebugger.Info("[NativeTrack] GetFramePresentHeadPose: trackReasonRst={0}, lost_tracking_reason={1}", trackReasonRst, lostReason);

            NativeMat4f headpos_native = new NativeMat4f(Matrix4x4.identity);
            var getPoseResult = NativeApi.NRHeadPoseGetPose(m_NativeInterface.PerceptionHandle, headPoseHandle, ref headpos_native);
            NativeErrorListener.Check(getPoseResult, this, "GetFramePresentHeadPose");

            ConversionUtility.ApiPoseToUnityPose(headpos_native, out pose);
            if (float.IsNaN(pose.position.x) || float.IsNaN(pose.position.y) || float.IsNaN(pose.position.z) ||
                float.IsNaN(pose.rotation.x) || float.IsNaN(pose.rotation.y) || float.IsNaN(pose.rotation.z) || float.IsNaN(pose.rotation.w))
            {
                NRDebugger.Error("[NativeTrack] GetFramePresentHeadPose invalid: lostReason={0}, unityPose=\n{1}\nnativePose=\n{2}", lostReason, pose.ToString("F6"), headpos_native.ToString());
            }
            NativeApi.NRHeadPoseDestroy(m_NativeInterface.PerceptionHandle, headPoseHandle);
            return true;
        }

        public bool GetFramePresentTimeByCount(uint count, ref UInt64 timeStamp)
        {
            var refreshRate = NRDevice.Subsystem.NativeHMD.GetRefreshRate();
            var vsyncLen = 1000000000 / refreshRate;
            ulong presentTime = 0;
            m_NativeInterface.NativeRenderring.GetFramePresentTime(ref presentTime);
            timeStamp = presentTime + vsyncLen * count;
            NRDebugger.Info("[NativeTrack] GetFramePresentTimeByCount: count={0}, vsyncLen={1}, time={2}=>{3}", count, vsyncLen, presentTime, timeStamp);
            return true;
        }

        /// <summary> Destroys this object. </summary>
        public void Destroy()
        {
            if (m_HeadTrackingHandle == 0)
            {
                return;
            }
            var result = NativeApi.NRHeadTrackingDestroy(m_NativeInterface.PerceptionHandle, m_HeadTrackingHandle);
            m_HeadTrackingHandle = 0;
            NativeErrorListener.Check(result, this, "Destroy");
        }

        public partial struct NativeApi
        {
            /// <summary> Create and initialize the head perception object. </summary>
            /// <param name="perception_handle"> The handle of perception system object. </param>
            /// <param name="out_head_tracking_handle"> [in,out] The handle of head perception object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingCreate(UInt64 perception_handle,
                ref UInt64 out_head_tracking_handle);

            /// <summary> Get the recommend time in nano seconds used to predict head movement. </summary>
            /// <param name="perception_handle">  The handle of perception object. </param>
            /// <param name="head_tracking_handle"> The handle of head_tracking object. </param>
            /// <param name="out_predict_time_nanos"> [in,out] The recommend time used to predict head movement. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingGetRecommendPredictTime(
                UInt64 perception_handle, UInt64 head_tracking_handle, ref UInt64 out_predict_time_nanos);

            /// <summary> Get the head_tracking pose at hmd_time_nanos. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="head_tracking_handle"> The handle of head_tracking object </param>
            /// <param name="hmd_time_nanos"> The time to get the head pose. </param>
            /// <param name="out_tracking_pose_handle"> [in,out] The head_pose handle stores the related head pose information. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingAcquireHeadPose(UInt64 perception_handle,
                UInt64 head_tracking_handle, UInt64 hmd_time_nanos, ref UInt64 out_tracking_pose_handle);

            /// <summary> Recenter the head orientation. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="head_tracking_handle"> The handle of head_tracking object </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingRecenter(UInt64 perception_handle,
                UInt64 head_tracking_handle);

            /// <summary> Get the pose stored in head_pose object. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="tracking_pose_handle"> The handle of head_pose object. </param>
            /// <param name="out_pose"> [in,out] The pose data stored in head_pose object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadPoseGetPose(UInt64 perception_handle,
                UInt64 tracking_pose_handle, ref NativeMat4f out_pose);

            /// <summary> Get the tracking reason stored in head_pose object. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="tracking_pose_handle"> The handle of head_pose object. </param>
            /// <param name="out_tracking_reason"> [in,out] Tracking reason indicates
            /// the state of perception system at the time of acquiring head pose. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadPoseGetTrackingReason(UInt64 perception_handle,
                UInt64 tracking_pose_handle, ref LostTrackingReason out_tracking_reason);

            /// <summary> Release memory used by the head pose object. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="tracking_pose_handle"> The handle of head_pose object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadPoseDestroy(UInt64 perception_handle, UInt64 tracking_pose_handle);

            /// <summary> Release memory used by the head_tracking object. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="head_tracking_handle"> The handle of head_tracking object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRHeadTrackingDestroy(UInt64 perception_handle, UInt64 head_tracking_handle);
        };
    }
}
