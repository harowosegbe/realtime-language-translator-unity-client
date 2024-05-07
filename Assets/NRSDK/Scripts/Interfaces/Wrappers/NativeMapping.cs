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
    using System.Text;
    using UnityEngine;

    /// <summary> Mapping Native API. </summary>
    public class NativeMapping
    {
        /// <summary> The native interface. </summary>
        private NativeInterface m_NativeInterface;
        /// <summary> The native tracking handle. </summary>
        private UInt64 PerceptionHandle
        {
            get
            {
                return m_NativeInterface.PerceptionHandle;
            }
        }

        /// <summary> Constructor. </summary>
        /// <param name="nativeInterface"> The native interface.</param>
        public NativeMapping(NativeInterface nativeInterface)
        {
            m_NativeInterface = nativeInterface;
        }

        /// <summary> Add an anchor. </summary>
        /// <param name="pose"> The anchor pose.</param>
        /// <returns> The new anchor handle. </returns>
        public UInt64 AddAnchor(Pose pose)
        {
            UInt64 anchorHandle = 0;
            ConversionUtility.UnityPoseToApiPose(pose, out NativeMat4f nativePose);
            var result = NativeApi.NRPerceptionAcquireNewAnchor(PerceptionHandle, ref nativePose, ref anchorHandle);
            NRDebugger.Info("[NativeMapping] NRPerceptionAcquireNewAnchor: {0}", result);
            NativeErrorListener.Check(result, this, "AddAnchor");
            return anchorHandle;
        }

        /// <summary> Save an anchor to file. </summary>
        /// <param name="path"> The anchor file path.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool SaveAnchor(UInt64 anchor_handle, string path)
        {
            var buff = Encoding.UTF8.GetBytes(path);
            NRDebugger.Info("[NativeMapping] NRTrackableAnchorSave: before ");
            var result = NativeApi.NRTrackableAnchorSave(PerceptionHandle, anchor_handle, buff, (uint)buff.Length);
            NRDebugger.Info("[NativeMapping] NRTrackableAnchorSave: {0}", result);
            NativeErrorListener.Check(result, this, "SaveAnchor");
            return result == NativeResult.Success;
        }

        /// <summary> Load an anchor from file. </summary>
        /// <param name="path"> The anchor file path.</param>
        /// <returns> The new anchor handle. </returns>
        public UInt64 LoadAnchor(string path)
        {
            UInt64 anchorHandle = 0;
            var buff = Encoding.UTF8.GetBytes(path);
            var result = NativeApi.NRPerceptionLoadAnchor(PerceptionHandle, buff, (uint)buff.Length, ref anchorHandle);
            NRDebugger.Info("[NativeMapping] NRPerceptionLoadAnchor: {0}", result);
            NativeErrorListener.Check(result, this, "LoadAnchor");
            return anchorHandle;
        }

        /// <summary> Create an anchor list. </summary>
        /// <returns> The new anchor list handle. </returns>
        public UInt64 CreateAnchorList()
        {
            UInt64 anchorlisthandle = 0;
            var result = NativeApi.NRPerceptionObjectListCreate(PerceptionHandle, ref anchorlisthandle);
            NRDebugger.Debug("[NativeMapping] NRPerceptionObjectListCreate: {0}", result);
            NativeErrorListener.Check(result, this, "CreateAnchorList");
            return anchorlisthandle;
        }

        /// <summary> Updates the anchor described by anchorlisthandle. </summary>
        /// <param name="anchorlisthandle"> The anchorlisthandle.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool UpdateAnchor(UInt64 anchorlisthandle)
        {
            var result = NativeApi.NRPerceptionUpdateTrackables(PerceptionHandle, TrackableType.TRACKABLE_ANCHOR, anchorlisthandle);
            NRDebugger.Debug("[NativeMapping] NRPerceptionUpdateTrackables: {0}", result);
            NativeErrorListener.Check(result, this, "UpdateAnchor");
            return result == NativeResult.Success;
        }

        /// <summary> Destroys the anchor list described by anchorlisthandle. </summary>
        /// <param name="anchorlisthandle"> The anchorlisthandle.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool DestroyAnchorList(UInt64 anchorlisthandle)
        {
            var result = NativeApi.NRPerceptionObjectListDestroy(PerceptionHandle, anchorlisthandle);
            NRDebugger.Debug("[NativeMapping] NRPerceptionObjectListDestroy: {0}", result);
            NativeErrorListener.Check(result, this, "DestroyAnchorList");
            return result == NativeResult.Success;
        }

        /// <summary> Gets anchor list size. </summary>
        /// <param name="perception_object_list_handle"> Handle of the anchor list.</param>
        /// <returns> The anchor list size. </returns>
        public uint GetAnchorListSize(UInt64 perception_object_list_handle)
        {
            uint size = 0;
            var result = NativeApi.NRPerceptionObjectListGetSize(PerceptionHandle, perception_object_list_handle, ref size);
            NRDebugger.Debug("[NativeMapping] NRPerceptionObjectListGetSize: {0} Result: {1}", size, result);
            NativeErrorListener.Check(result, this, "GetAnchorListSize");
            return size;
        }

        /// <summary> Acquires the item. </summary>
        /// <param name="perception_object_list_handle"> Handle of the anchor list.</param>
        /// <param name="index">              Zero-based index of the.</param>
        /// <returns> An UInt64. </returns>
        public UInt64 AcquireItem(UInt64 perception_object_list_handle, int index)
        {
            UInt64 anchorHandle = 0;
            var result = NativeApi.NRPerceptionObjectListAcquireItem(PerceptionHandle, perception_object_list_handle, index, ref anchorHandle);
            NRDebugger.Debug("[NativeMapping] NRPerceptionObjectListAcquireItem: {0}", result);
            NativeErrorListener.Check(result, this, "AcquireItem");
            return anchorHandle;
        }

        /// <summary> Gets tracking state. </summary>
        /// <param name="anchor_handle"> Handle of the anchor.</param>
        /// <returns> The tracking state. </returns>
        public TrackingState GetTrackingState(UInt64 anchor_handle)
        {
            TrackingState trackingState = TrackingState.Stopped;
            var result = NativeApi.NRTrackableGetTrackingState(PerceptionHandle, anchor_handle, ref trackingState);
            NRDebugger.Debug("[NativeMapping] NRTrackableGetTrackingState: {0}", result);
            NativeErrorListener.Check(result, this, "GetTrackingState");
            return trackingState;
        }

        /// <summary> Gets anchor universally unique identifier. </summary>
        /// <param name="anchor_handle"> Handle of the anchor.</param>
        /// <returns> The anchor native identifier. </returns>
        public string GetAnchorUUID(UInt64 anchor_handle)
        {
            uint len = 16;
            byte[] buffer = new byte[len];
            var result = NativeApi.NRTrackableAnchorGetUUID(PerceptionHandle, anchor_handle, buffer, ref len);
            Guid guid = new Guid(buffer);
            string uuid = guid.ToString();
            NRDebugger.Info("[NativeMapping] NRTrackableAnchorGetUUID: {0}", result);
            NativeErrorListener.Check(result, this, "GetAnchorUUID");
            return uuid;
        }

        /// <summary>
        /// Estimate Feature Map Quality for an Anchor
        /// </summary>
        /// <param name="anchor_handle"> handle of the anchor </param>
        /// <param name="pose">eye pose</param>
        /// <returns></returns>
        public NREstimateQuality EstimateMapQuality(UInt64 anchor_handle, Pose pose)
        {
            NREstimateQuality quality = NREstimateQuality.NR_ANCHOR_QUALITY_INSUFFICIENT;
            ConversionUtility.UnityPoseToApiPose(pose, out var apiPose);
            var result = NativeApi.NRTrackableAnchorEstimateGetQuality(PerceptionHandle, anchor_handle, ref apiPose, ref quality);
            NRDebugger.Info("[NativeMapping] EstimateAnchorQuality: {0}", result);
            NativeErrorListener.Check(result, this, "EstimateAnchorQuality");
            return quality;
        }

        public bool GetEstimateAngleRange(UInt64 anchor_handle, ref float angle)
        {
            var result = NativeApi.NRTrackableAnchorEstimateGetAngle(PerceptionHandle, anchor_handle, ref angle);
            NRDebugger.Info($"[NativeMapping] GetEstimateAngleRange: {result} {angle}");
            NativeErrorListener.Check(result, this, "GetEstimateAngleRange");
            return result == NativeResult.Success;
        }

        public bool SetEstimateAngleRange(UInt64 anchor_handle, float angle)
        {
            var result = NativeApi.NRTrackableAnchorEstimateSetAngle(PerceptionHandle, anchor_handle, angle);
            NRDebugger.Info($"[NativeMapping] SetEstimateAngleRange: {result} {angle}");
            NativeErrorListener.Check(result, this, "SetEstimateAngleRange");
            return result == NativeResult.Success;
        }


        public bool GetEstimateDistanceRange(UInt64 anchor_handle, ref NREstimateDistance distance)
        {
            var result = NativeApi.NRTrackableAnchorEstimateGetDistance(PerceptionHandle, anchor_handle, ref distance);
            NRDebugger.Info($"[NativeMapping] GetEstimateDistanceRange: {result} {distance}");
            NativeErrorListener.Check(result, this, "GetEstimateDistanceRange");
            return result == NativeResult.Success;
        }

        public bool SetEstimateDistanceRange(UInt64 anchor_handle, NREstimateDistance distance)
        {
            var result = NativeApi.NRTrackableAnchorEstimateSetDistance(PerceptionHandle, anchor_handle, distance);
            NRDebugger.Info($"[NativeMapping] SetEstimateDistanceRange: {result} {distance}");
            NativeErrorListener.Check(result, this, "SetEstimateDistanceRange");
            return result == NativeResult.Success;
        }

        /// <summary>
        /// Get Anchor State
        /// </summary>
        /// <param name="anchor_handle"></param>
        /// <returns>The state of the anchor</returns>
        public NRAnchorState GetAnchorState(UInt64 anchor_handle)
        {
            NRAnchorState state = NRAnchorState.NR_ANCHOR_STATE_UNKNOWN;
            var result = NativeApi.NRTrackableAnchorGetState(PerceptionHandle, anchor_handle, ref state);
            NRDebugger.Info($"[NativeMapping] GetAnchorState: {result} {state}");
            NativeErrorListener.Check(result, this, "GetAnchorState");
            return state;
        }

        /// <summary>
        /// Rebuild the feature map of the anchor
        /// </summary>
        /// <param name="anchor_handle"></param>
        /// <returns></returns>
        public bool Remap(UInt64 anchor_handle)
        {
            NRDebugger.Debug("[NativeMapping] before Remap");
            var result = NativeApi.NRTrackableAnchorRemap(PerceptionHandle, anchor_handle);
            NRDebugger.Info("[NativeMapping] Remap: {0}", result);
            NativeErrorListener.Check(result, this, "Remap");
            return result == NativeResult.Success;
        }

        /// <summary> Gets anchor pose. </summary>
        /// <param name="anchor_handle"> Handle of the anchor.</param>
        /// <returns> The anchor pose. </returns>
        public Pose GetAnchorPose(UInt64 anchor_handle)
        {
            NativeMat4f nativePose = NativeMat4f.identity; 
            NRDebugger.Debug($"[NativeMapping] before NRTrackableAnchorGetPose: {anchor_handle} ");
            var result = NativeApi.NRTrackableAnchorGetPose(PerceptionHandle, anchor_handle, ref nativePose);
            NRDebugger.Debug($"[NativeMapping] NRTrackableAnchorGetPose: {result} ");
            NativeErrorListener.Check(result, this, "GetAnchorPose");
            ConversionUtility.ApiPoseToUnityPose(nativePose, out Pose unitypose);
            NRDebugger.Debug($"[NativeMapping] GetAnchorPose: {unitypose} ");
            return unitypose;
        }

        /// <summary> Destroys the anchor described by anchor_handle. </summary>
        /// <param name="anchor_handle"> Handle of the anchor.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool DestroyAnchor(UInt64 anchor_handle)
        {
            var result = NativeApi.NRTrackableAnchorDestroy(PerceptionHandle, anchor_handle);
            NRDebugger.Info("[NativeMapping] NRTrackableAnchorDestroy: {0}", result);
            NativeErrorListener.Check(result, this, "DestroyAnchor");
            return result == NativeResult.Success;
        }

        /// <summary> A native api. </summary>
        private struct NativeApi
        {
            /// <summary> Acquire the handle of a new anchor with its pose. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="pose">              The pose of the acquired anchor. </param>
            /// <param name="out_anchor_handle"> The handle of the acquired anchor. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionAcquireNewAnchor(
                UInt64 perception_handle, ref NativeMat4f pose, ref UInt64 out_anchor_handle);

            /// <summary> Save an anchor object to a local file. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="anchor_handle">     The handle of the anchor object. </param>
            /// <param name="file_path">         The path of the file to save the anchor to. </param>
            /// <param name="file_path_size">    The size of the file path. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorSave(
                UInt64 perception_handle, UInt64 anchor_handle, byte[] file_path, uint file_path_size);

            /// <summary> Load an anchor object from a local file. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="file_path">         The path of the file to save the anchor to. </param>
            /// <param name="file_path_size">    The size of the file path. </param>
            /// <param name="out_anchor_handle"> The output handle for the anchor loaded. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionLoadAnchor(
                UInt64 perception_handle, byte[] file_path, uint file_path_size, ref UInt64 out_anchor_handle);

            /// <summary> Create an empty perception object list. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="out_perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionObjectListCreate(
                UInt64 perception_handle, ref UInt64 out_perception_object_list_handle);

            /// <summary> Get the newly updated trackable object of specified kind of trackable types. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="trackable_type"> The trackable type. </param>
            /// <param name="perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionUpdateTrackables(
                UInt64 perception_handle, TrackableType trackable_type, UInt64 perception_object_list_handle);

            /// <summary> Release memory used by the perception object list object. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionObjectListDestroy(
                UInt64 perception_handle, UInt64 perception_object_list_handle);

            /// <summary> Get the perception object list size. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <param name="out_list_size"> The size of perception_object_list. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionObjectListGetSize(
                UInt64 perception_handle, UInt64 perception_object_list_handle, ref uint out_list_size);

            /// <summary> Get the element of perception object list by index. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <param name="index"> Index of elements of perception object list. </param>
            /// <param name="out_perception_object"> The perception object element in the list. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionObjectListAcquireItem(
                UInt64 perception_handle, UInt64 perception_object_list_handle, int index, ref UInt64 out_perception_object);

            /// <summary> Get the trackable state of trackable object. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="anchor_handle">     The handle of the anchor. </param>
            /// <param name="out_tracking_state"> Tracking state of the anchor. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableGetTrackingState(UInt64 perception_handle,
                UInt64 anchor_handle, ref TrackingState out_tracking_state);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorEstimateGetDistance(UInt64 perception_handle,
                                              UInt64 anchor_handle,
                                              ref NREstimateDistance distance);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorEstimateSetDistance(UInt64 perception_handle,
                                              UInt64 anchor_handle,
                                              NREstimateDistance distance);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorEstimateGetAngle(UInt64 perception_handle,
                                           UInt64 trackable_handle,
                                           ref float observe_angle); 

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorEstimateSetAngle(UInt64 perception_handle,
                                           UInt64 trackable_handle,
                                           float observe_angle);

            /// <summary>
            /// Get quality of estimate, given camera pose and anchor ID
            /// </summary>
            /// <param name="perception_handle"></param>
            /// <param name="trackable_handle"></param>
            /// <param name="pose"></param>
            /// <param name="out_anchor_quality"></param>
            /// <returns></returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorEstimateGetQuality(UInt64 perception_handle,
                UInt64 trackable_handle, ref NativeMat4f pose, ref NREstimateQuality out_anchor_quality);

            /// <summary>
            /// Reset an anchor object
            /// </summary>
            /// <param name="perception_handle"></param>
            /// <param name="trackable_handle"></param>
            /// <returns></returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorRemap(UInt64 perception_handle, UInt64 trackable_handle);

            /// <summary>
            /// 
            /// </summary>
            /// <param name="perception_handle"></param>
            /// <param name="trackable_handle"></param>
            /// <param name="out_anchor_state"></param>
            /// <returns></returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorGetState(UInt64 perception_handle, UInt64 trackable_handle, ref NRAnchorState out_anchor_state);

            /// <summary> Get the UUID of an anchor object. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="anchor_handle">     The handle of the anchor object. </param>
            /// <param name="out_buffer">        The output buffer where the UUID will be stored. </param>
            /// <param name="out_buffer_size">   The size of the content in the output buffer. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorGetUUID(
                UInt64 perception_handle, UInt64 anchor_handle, byte[] out_buffer, ref uint out_buffer_size);

            /// <summary> Get the pose of an anchor object. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="anchor_handle">     The handle of the anchor object. </param>
            /// <param name="out_pose">          The pose of the anchor object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorGetPose(
                UInt64 perception_handle, UInt64 anchor_handle, ref NativeMat4f out_pose);

            /// <summary> Destroy an anchor object </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="anchor_handle">     The handle of the anchor object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableAnchorDestroy(
                UInt64 perception_handle, UInt64 anchor_handle);
        }
    }
}
