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
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    /// <summary> 6-dof Trackable's Native API . </summary>
    internal partial class NativeTrackable
    {
        /// <summary> The native interface. </summary>
        private NativeInterface m_NativeInterface;

        /// <summary> Constructor. </summary>
        /// <param name="nativeInterface"> The native interface.</param>
        public NativeTrackable(NativeInterface nativeInterface)
        {
            m_NativeInterface = nativeInterface;
        }

        public bool UpdateTrackables(TrackableType trackable_type, List<UInt64> trackables)
        {
            if (m_NativeInterface == null || m_NativeInterface.PerceptionHandle == 0)
            {
                return false;
            }

            trackables.Clear();
            UInt64 trackable_list_handle = 0;
            var create_result = NativeApi.NRPerceptionObjectListCreate(m_NativeInterface.PerceptionHandle, ref trackable_list_handle);
            var update_result = NativeApi.NRPerceptionUpdateTrackables(m_NativeInterface.PerceptionHandle, trackable_type, trackable_list_handle);
            uint list_size = 0;
            var getsize_result = NativeApi.NRPerceptionObjectListGetSize(m_NativeInterface.PerceptionHandle, trackable_list_handle, ref list_size);
            for (int i = 0; i < list_size; i++)
            {
                UInt64 trackable_handle = 0;
                var acquireitem_result = NativeApi.NRPerceptionObjectListAcquireItem(m_NativeInterface.PerceptionHandle, trackable_list_handle, i, ref trackable_handle);
                if (acquireitem_result == NativeResult.Success) trackables.Add(trackable_handle);
            }
            var destroy_result = NativeApi.NRPerceptionObjectListDestroy(m_NativeInterface.PerceptionHandle, trackable_list_handle);
            return (create_result == NativeResult.Success) && (update_result == NativeResult.Success)
                && (getsize_result == NativeResult.Success) && (destroy_result == NativeResult.Success);
        }

        /// <summary> Gets an identify. </summary>
        /// <param name="trackable_handle"> Handle of the trackable.</param>
        /// <returns> The identify. </returns>
        public UInt32 GetIdentify(UInt64 trackable_handle)
        {
            if (m_NativeInterface.PerceptionHandle == 0)
            {
                return 0;
            }
            UInt32 identify = NativeConstants.IllegalInt;
            NativeApi.NRTrackableGetIdentifier(m_NativeInterface.PerceptionHandle, trackable_handle, ref identify);
            return identify;
        }

        /// <summary> Gets trackable type. </summary>
        /// <param name="trackable_handle"> Handle of the trackable.</param>
        /// <returns> The trackable type. </returns>
        public TrackableType GetTrackableType(UInt64 trackable_handle)
        {
            if (m_NativeInterface.PerceptionHandle == 0)
            {
                return TrackableType.TRACKABLE_BASE;
            }
            TrackableType trackble_type = TrackableType.TRACKABLE_BASE;
            NativeApi.NRTrackableGetType(m_NativeInterface.PerceptionHandle, trackable_handle, ref trackble_type);
            return trackble_type;
        }

        /// <summary> Gets tracking state. </summary>
        /// <param name="trackable_handle"> Handle of the trackable.</param>
        /// <returns> The tracking state. </returns>
        public TrackingState GetTrackingState(UInt64 trackable_handle)
        {
            if (m_NativeInterface.PerceptionHandle == 0)
            {
                return TrackingState.Stopped;
            }
            TrackingState status = TrackingState.Stopped;
            NativeApi.NRTrackableGetTrackingState(m_NativeInterface.PerceptionHandle, trackable_handle, ref status);
            return status;
        }

        private partial struct NativeApi
        {
            /// <summary> Create an empty perception object list. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="out_perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionObjectListCreate(
                UInt64 perception_handle, ref UInt64 out_perception_object_list_handle);

            /// <summary> Release memory used by the perception object list object. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionObjectListDestroy(
                UInt64 perception_handle, UInt64 perception_object_list_handle);

            /// <summary> Get the newly updated trackable object of specified kind of trackable types. </summary>
            /// <param name="perception_handle"> The handle of the perception object. </param>
            /// <param name="trackable_type"> The trackable type. </param>
            /// <param name="perception_object_list_handle"> The handle of perception-object-list object. </param>
            /// <returns> The result of the operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionUpdateTrackables(
                UInt64 perception_handle, TrackableType trackable_type, UInt64 perception_object_list_handle);

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

            /// <summary> Nr trackable get identifier. </summary>
            /// <param name="session_handle">   Handle of the session.</param>
            /// <param name="trackable_handle"> Handle of the trackable.</param>
            /// <param name="out_identifier">   [in,out] Identifier for the out.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableGetIdentifier(UInt64 session_handle,
                UInt64 trackable_handle, ref UInt32 out_identifier);

            /// <summary> Nr trackable get type. </summary>
            /// <param name="session_handle">     Handle of the session.</param>
            /// <param name="trackable_handle">   Handle of the trackable.</param>
            /// <param name="out_trackable_type"> [in,out] Type of the out trackable.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableGetType(UInt64 session_handle,
                UInt64 trackable_handle, ref TrackableType out_trackable_type);

            /// <summary> Nr trackable get tracking state. </summary>
            /// <param name="session_handle">     Handle of the session.</param>
            /// <param name="trackable_handle">   Handle of the trackable.</param>
            /// <param name="out_tracking_state"> [in,out] State of the out tracking.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRTrackableGetTrackingState(UInt64 session_handle,
                UInt64 trackable_handle, ref TrackingState out_tracking_state);
        };
    }
}
