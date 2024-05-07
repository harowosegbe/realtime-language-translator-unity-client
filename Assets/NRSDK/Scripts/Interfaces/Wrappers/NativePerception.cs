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
    using System.Runtime.InteropServices;

    [Flags]
    public enum NRPerceptionFeature : ulong
    {
        NR_PERCEPTION_FEATURE_NONE = 0uL,
        NR_PERCEPTION_FEATURE_TRACKABLE_PLANE = 0x01uL,
        NR_PERCEPTION_FEATURE_TRACKABLE_IMAGE = 0x02uL,
        NR_PERCEPTION_FEATURE_TRACKABLE_ANCHOR = 0x04uL,
        NR_PERCEPTION_FEATURE_MESHING = 0x08uL,
        NR_PERCEPTION_FEATURE_HAND_TRACKING = 0x10uL,
    };

    internal partial class NativePerception
    {
        /// <summary> The native interface. </summary>
        private NativeInterface m_NativeInterface;
        /// <summary> Handle of the perception. </summary>
        private UInt64 m_PerceptionHandle;
        /// <summary> Handle of the perception group. </summary>
        private UInt64 m_PerceptionGroupHandle;

        /// <summary> Constructor. </summary>
        /// <param name="nativeInterface"> The native interface.</param>
        public NativePerception(NativeInterface nativeInterface)
        {
            m_NativeInterface = nativeInterface;
        }

        /// <summary> Creates a new bool. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Create(UInt64 perceptionGroupHandle = 0)
        {
            NRDebugger.Info("[NativePerception] Create: PerceptionHandle={0}", perceptionGroupHandle);
            if (perceptionGroupHandle == 0)
            {
                NativeResult result = NativeApi.NRPerceptionGroupCreate(ref perceptionGroupHandle);
                NativeErrorListener.Check(result, this, "GroupCreate");
            }
            m_PerceptionGroupHandle = perceptionGroupHandle;
            return m_PerceptionGroupHandle != 0;
        }

        /// <summary> Switch tracking type. </summary>
        public bool SwitchTrackingType(TrackingType type)
        {
#if USING_XR_SDK
            NRDebugger.Info("[NativePerception] Begin SwitchTrackingType XR: {0}", type);
            NativeXRPlugin.SwitchTrackingType(type);
            m_PerceptionHandle = NativeXRPlugin.GetPerceptionHandle();
            m_NativeInterface.PerceptionHandle = m_PerceptionHandle;
            m_NativeInterface.PerceptionId = NativeXRPlugin.GetPerceptionID();
            NRDebugger.Info($"[NativePerception] End SwitchTrackingType XR: {type} handle={m_PerceptionHandle}");
            return m_PerceptionHandle != 0;
#else
            NRDebugger.Info("[NativePerception] Begin SwitchTrackingType: {0}", type);
            int group_count = 0;
            NativeResult result = NativeApi.NRPerceptionGroupGetCount(m_PerceptionGroupHandle, ref group_count);
            NativeErrorListener.Check(result, this, "GroupGetCount");
            NRDebugger.Info("[NativePerception] SwitchTrackingType NRPerceptionGroupGetCount:{0}", group_count);
            int perception_id = 0;
            UInt64 perceptionHandle = 0;
            for (int i = 0; i < group_count; i++)
            {
                if (NativeApi.NRPerceptionGroupCheckPerceptionType(m_PerceptionGroupHandle, i, type))
                {
                    result = NativeApi.NRPerceptionGroupGetPerceptionId(m_PerceptionGroupHandle, i, ref perception_id);
                    NativeErrorListener.Check(result, this, "GetPerceptionId");
                    result = NativeApi.NRPerceptionCreate(perception_id, ref perceptionHandle);
                    NRDebugger.Info("[NativePerception] NRPerceptionCreate:{0} TrackingType:{1}", perceptionHandle, type);
                    NativeErrorListener.Check(result, this, "Create");
                    if (perceptionHandle == 0)
                        break;
                    if (m_PerceptionHandle != 0)
                    {
                        result = NativeApi.NRPerceptionDestroy(m_PerceptionHandle);
                        NativeErrorListener.Check(result, this, "Destroy");
                    }
                    m_PerceptionHandle = perceptionHandle;
                    m_NativeInterface.PerceptionHandle = m_PerceptionHandle;
                    m_NativeInterface.PerceptionId = perception_id;
                    NRDebugger.Info($"[NativePerception] End SwitchTrackingType: {type} perceptionHandle={perceptionHandle}");
                    return result == NativeResult.Success;
                }
            }
            return false;
#endif
        }

        /// <summary> Starts this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Start()
        {
            if (m_PerceptionHandle == 0)
            {
                return false;
            }
            NRDebugger.Info("[NativePerception] Start: {0}", m_PerceptionHandle);
            NativeResult result = NativeApi.NRPerceptionStart(m_PerceptionHandle);
            NativeErrorListener.Check(result, this, "Start");
            return result == NativeResult.Success;
        }

        /// <summary> Pauses this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Pause()
        {
            if (m_PerceptionHandle == 0)
            {
                return false;
            }
            NativeResult result = NativeApi.NRPerceptionPause(m_PerceptionHandle);
            NativeErrorListener.Check(result, this, "Pause");
            return result == NativeResult.Success;
        }

        /// <summary> Resumes this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Resume()
        {
            if (m_PerceptionHandle == 0)
            {
                return false;
            }
            NativeResult result = NativeApi.NRPerceptionResume(m_PerceptionHandle);
            NativeErrorListener.Check(result, this, "Resume");
            return result == NativeResult.Success;
        }

        /// <summary> Stops this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Stop()
        {
            if (m_PerceptionHandle == 0)
            {
                return false;
            }
            NativeResult result = NativeApi.NRPerceptionStop(m_PerceptionHandle);
            NativeErrorListener.Check(result, this, "Stop");
            return result == NativeResult.Success;
        }

        /// <summary> Destroys this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Destroy()
        {
            if (m_PerceptionHandle == 0)
            {
                return false;
            }
            NativeResult result = NativeApi.NRPerceptionDestroy(m_PerceptionHandle);
            NativeErrorListener.Check(result, this, "Destroy");
            m_PerceptionHandle = 0;
            m_NativeInterface.PerceptionHandle = m_PerceptionHandle;
            result = NativeApi.NRPerceptionGroupDestroy(m_PerceptionGroupHandle);
            NativeErrorListener.Check(result, this, "GroupDestroy");
            return result == NativeResult.Success;
        }

        /// <summary> Get time stamp of HMD system time. </summary>
        /// <returns> The time stamp in nanoseconds. </returns>
        public ulong GetHMDTimeNanos()
        {
            if (m_PerceptionHandle == 0)
            {
                return 0;
            }
            ulong timestamp = 0;
            NativeResult result = NativeApi.NRPerceptionGetHMDTimeNanos(m_PerceptionHandle, ref timestamp);
            NativeErrorListener.Check(result, this, "GetHMDTimeNanos");
            return timestamp;
        }

        public NRPerceptionFeature GetSupportedFeatures()
        {
            if (m_PerceptionHandle == 0)
            {
                return NRPerceptionFeature.NR_PERCEPTION_FEATURE_NONE;
            }
            NRPerceptionFeature feature = NRPerceptionFeature.NR_PERCEPTION_FEATURE_NONE;
            NativeResult result = NativeApi.NRPerceptionGetSupportedFeatures(m_PerceptionHandle, ref feature);
            NativeErrorListener.Check(result, this, "GetSupportedFeatures");
            return feature;
        }

        private partial struct NativeApi
        {
            /// <summary> Create the perception group object. </summary>
            /// <param name="out_perception_group_handle"> [in,out] The handle of perception group object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionGroupCreate(ref UInt64 out_perception_group_handle);

            /// <summary> Get the count of perception group. </summary>
            /// <param name="perception_group_handle"> The handle of perception group object. </param>
            /// <param name="out_group_count"> [in,out] The count of perception group. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionGroupGetCount(UInt64 perception_group_handle, ref int out_group_count);

            /// <summary> Check whether the perception system which the group_index
            /// and the perception_type reference to are the same. </summary>
            /// <param name="perception_group_handle"> The handle of perception group object. </param>
            /// <param name="group_index"> The index of perception group. </param>
            /// <param name="perception_type"> The type of perception system. </param>
            /// <returns> True if the perceptions are the same, false otherwise. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern bool NRPerceptionGroupCheckPerceptionType(UInt64 perception_group_handle,
                int group_index, TrackingType perception_type);

            /// <summary> Get the perception id of perception system. </summary>
            /// <param name="perception_group_handle"> The handle of perception group object. </param>
            /// <param name="group_index"> The index of perception group. </param>
            /// <param name="out_perception_id"> [in,out] The identifier of perception system. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionGroupGetPerceptionId(UInt64 perception_group_handle,
                int group_index, ref int out_perception_id);

            /// <summary> Get the description of perception system. </summary>
            /// <param name="perception_group_handle"> The handle of perception group object. </param>
            /// <param name="group_index"> The index of perception group. </param>
            /// <param name="out_description"> [in,out] The description of perception system. </param>
            /// <param name="out_description_length"> [in,out] The length of description. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionGroupGetDescription(UInt64 perception_group_handle,
                int group_index, ref IntPtr out_description, ref int out_description_length);

            /// <summary> Get the supported features of perception system. </summary>
            /// <param name="perception_group_handle"> The handle of perception group object. </param>
            /// <param name="group_index"> The index of perception group. </param>
            /// <param name="out_supported_features"> [in,out] The supported features of perception system. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionGroupGetSupportedFeatures(UInt64 perception_group_handle,
                int group_index, ref NRPerceptionFeature out_supported_features);

            /// <summary> Release memory used by the perception group object. </summary>
            /// <param name="perception_group_handle"> The handle of perception group object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionGroupDestroy(UInt64 perception_group_handle);

            /// <summary> Create the perception system object. </summary>
            /// <param name="perception_id"> The identifier of perception system. </param>
            /// <param name="out_perception_handle"> [in,out] The handle of perception system object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionCreate(int perception_id, ref UInt64 out_perception_handle);

            /// <summary> Release memory used by the perception system object. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionDestroy(UInt64 perception_handle);

            /// <summary> Start the perception system. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionStart(UInt64 perception_handle);

            /// <summary> Stop the perception system. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionStop(UInt64 perception_handle);

            /// <summary> Pause the perception system. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionPause(UInt64 perception_handle);

            /// <summary> Resume the perception system. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionResume(UInt64 perception_handle);

            /// <summary> Get time stamp of HMD system time. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="out_hmd_time_nanos"> The time stamp in nanoseconds. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionGetHMDTimeNanos(UInt64 perception_handle,
                ref UInt64 out_hmd_time_nanos);

            /// <summary> Get the supported features of perception object. </summary>
            /// <param name="perception_handle"> The handle of perception object. </param>
            /// <param name="out_supported_features"> [in,out] The supported features of perception system. </param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRPerceptionGetSupportedFeatures(UInt64 perception_handle,
                ref NRPerceptionFeature out_supported_features);
        };
    }
}
