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

    /// <summary>
    /// Metrics's Native API.
    /// </summary>
    internal class NativeMetrics
    {
        private UInt64 m_MetricsHandle = 0;

        /// <summary> Create this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Create(UInt64 metricsHandle = 0)
        {
            NRDebugger.Info("[NativeMetrics] Create: metricsHandle={0}", metricsHandle);
            if (metricsHandle == 0)
            {
                NativeResult result = NativeApi.NRMetricsCreate(ref metricsHandle);
                NativeErrorListener.Check(result, this, "Create");
            }

            m_MetricsHandle = metricsHandle;
            return m_MetricsHandle != 0;
        }

        /// <summary> Start this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Start()
        {
            var result = NativeApi.NRMetricsStart(m_MetricsHandle);
            NativeErrorListener.Check(result, this, "Start");
            return result == NativeResult.Success;
        }

        /// <summary> Get the number of times the current frame has been displayed on the HMD screen. </summary>
        /// <returns> The number of times the current frame has been displayed on the HMD screen.. </returns>
        public uint GetCurrFramePresentCount()
        {
            if (m_MetricsHandle == 0)
            {
                return 0u;
            }
            uint frameCount = 1;
            var result = NativeApi.NRMetricsGetCurrFramePresentCount(m_MetricsHandle, ref frameCount);
            NativeErrorListener.Check(result, this, "GetCurrFramePresentCount");
            return frameCount;
        }

        /// <summary>Get the number of extended frame count. This indicates the number of extended frame count used when predict frame present time. </summary>
        /// <returns> The number of extended frame count. </returns>
        public uint GetExtendedFrameCount()
        {
            if (m_MetricsHandle == 0)
            {
                return 0u;
            }
            uint extraFrameCount = 0;
            var result = NativeApi.NRMetricsGetExtendedFrameCount(m_MetricsHandle, ref extraFrameCount);
            NativeErrorListener.Check(result, this, "GetExtendedFrameCount");
            return extraFrameCount;
        }

        public uint GetEarlyFrameCount()
        {
            if (m_MetricsHandle == 0)
            {
                return 0u;
            }
            uint frameCount = 0;
            var result = NativeApi.NRMetricsGetEarlyFrameCount(m_MetricsHandle, ref frameCount);
            NativeErrorListener.Check(result, this, "GetEarlyFrameCount");
            return frameCount;
        }

        public uint GetDroppedFrameCount()
        {
            if (m_MetricsHandle == 0)
            {
                return 0u;
            }
            uint frameCount = 0;
            var result = NativeApi.NRMetricsGetDroppedFrameCount(m_MetricsHandle, ref frameCount);
            NativeErrorListener.Check(result, this, "GetDroppedFrameCount");
            return frameCount;
        }

        public ulong GetAppFrameLatency()
        {
            if (m_MetricsHandle == 0)
            {
                return 0uL;
            }
            ulong latency = 0;
            var result = NativeApi.NRMetricsGetAppFrameLatency(m_MetricsHandle, ref latency);
            NativeErrorListener.Check(result, this, "GetAppFrameLatency");
            return latency;
        }

        public uint GetPresentFps()
        {
            if (m_MetricsHandle == 0)
            {
                return 0;
            }
            uint presentFPS = 0;
            var result = NativeApi.NRMetricsGetPresentFps(m_MetricsHandle, ref presentFPS);
            NativeErrorListener.Check(result, this, "GetPresentFps");
            return presentFPS;
        }

        public bool Stop()
        {
            if (m_MetricsHandle == 0)
            {
                return false;
            }

            NativeResult result = NativeApi.NRMetricsStop(m_MetricsHandle);
            NativeErrorListener.Check(result, this, "Stop");
            return result == NativeResult.Success;
        }

        public bool Destroy()
        {
            if (m_MetricsHandle == 0)
            {
                return false;
            }

            NativeResult result = NativeApi.NRMetricsDestroy(m_MetricsHandle);
            NativeErrorListener.Check(result, this, "Destroy");
            m_MetricsHandle = 0;
            return result == NativeResult.Success;
        }

        /// <summary> Pauses this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Pause()
        {
            if (m_MetricsHandle == 0)
            {
                return false;
            }

            var result = NativeApi.NRMetricsPause(m_MetricsHandle);
            NativeErrorListener.Check(result, this, "Pause");
            return result == NativeResult.Success;
        }

        /// <summary> Resumes this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Resume()
        {
            if (m_MetricsHandle == 0)
            {
                return false;
            }

            var result = NativeApi.NRMetricsResume(m_MetricsHandle);
            NativeErrorListener.Check(result, this, "Resume");
            return result == NativeResult.Success;
        }

        private partial struct NativeApi
        {
            /// <summary> Create the Metrics object. </summary>
            /// <param name="out_metrics_handle"> [in,out] Handle of the metrics.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsCreate(ref UInt64 out_metrics_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsStart(UInt64 metrics_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsGetCurrFramePresentCount(UInt64 metrics_handle, ref uint frame_present_count);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsGetExtendedFrameCount(UInt64 metrics_handle, ref uint extended_frame_count);

            // [DllImport(NativeConstants.NRNativeLibrary)]
            // public static extern NativeResult NRMetricsGetTearedFrameCount(UInt64 metrics_handle, ref uint teared_frame_count);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsGetEarlyFrameCount(UInt64 metrics_handle, ref uint early_frame_count);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsGetDroppedFrameCount(UInt64 metrics_handle, ref uint dropped_frame_count);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsGetPresentFps(UInt64 metrics_handle, ref uint present_fps);

            // [DllImport(NativeConstants.NRNativeLibrary)]
            // public static extern NativeResult NRMetricsGetFrameCompositeTime(UInt64 metrics_handle, ref ulong frame_composite_time);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsGetAppFrameLatency(UInt64 metrics_handle, ref ulong app_frame_latency);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsStop(UInt64 metrics_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsDestroy(UInt64 metrics_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsPause(UInt64 metrics_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRMetricsResume(UInt64 metrics_handle);
        }
    }
}
