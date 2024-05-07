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
    using UnityEngine;
    using UnityEngine.Rendering;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using AOT;

    public class NRMetrics : SingletonBehaviour<NRMetrics>
    {
        // statistics of single frame
        internal class FrameStats
        {
            public int frameIdx;
            public ulong updateTime;
            public ulong l_preRenderTime;
            public ulong l_postRenderTime;
            public ulong r_preRenderTime;
            public ulong r_postRenderTime;
            public ulong onGUITime;
            public ulong endOfFrameTime;
            public ulong endOfFrameInRenderThreadTime;
            public ulong presentTime;
            public bool isFinish = false;

            public uint framePresentCount = 0;
            public uint extraFrameCount = 0;
            public uint earlyFrameCount = 0;
            public uint droppedFrameCount = 0;
            public ulong frameLatency = 0;

            public FrameStats(int frameIdx)
            {
                this.frameIdx = frameIdx;
            }

            public void Reset()
            {
                isFinish = false;
            }

            public void FeedNativeMetricsStats(NativeMetrics nativeMetrics)
            {
        #if !UNITY_EDITOR
                    framePresentCount = nativeMetrics.GetCurrFramePresentCount();
                    extraFrameCount = nativeMetrics.GetExtendedFrameCount();
                    earlyFrameCount = nativeMetrics.GetEarlyFrameCount();
                    droppedFrameCount = nativeMetrics.GetDroppedFrameCount();
                    frameLatency = nativeMetrics.GetAppFrameLatency();

        #endif
            }

            override public string ToString()
            {
                return string.Format("FrameStats: frNum={0}, Prd={1}, UpdPrd={2}, Pre2RdEnd={3}, Upd2RdEnd={4}, Upd2Pre={5}, l_Pre2Post={6}, rPre2Post={7}, lPreToRPost={8}, postToEnd={9}, endToRdEnd={10}, FPC={11}, EFC={12}, early={13}, drop={14}, sdkPrd={15}",
                    frameIdx, presentTime - l_preRenderTime, presentTime - updateTime, 
                    endOfFrameInRenderThreadTime - l_preRenderTime,
                    endOfFrameInRenderThreadTime - updateTime,
                    l_preRenderTime - updateTime, 
                    l_postRenderTime - l_preRenderTime, 
                    r_postRenderTime - r_preRenderTime, r_postRenderTime - l_preRenderTime,
                    endOfFrameTime - r_postRenderTime,
                    endOfFrameInRenderThreadTime - endOfFrameTime,
                    framePresentCount, extraFrameCount, earlyFrameCount, droppedFrameCount, frameLatency);
            }
        }

        // multi frames metrics
        internal class FrameMetrics
        {
            private NativeMetrics m_NativeMetrics;
            private float m_Duration = 1;
            private float m_BegTime = 0;

            // sum statistics in the period of duration
            private uint m_FrameNum = 0;
            private ulong m_UpdateToPresentSum = 0;
            private ulong m_PreToPresentSum = 0;
            private ulong m_PostToPresentSum = 0;

            private uint m_FramePresentCountSum = 0;
            private float m_NativeTimeStamp = 0;
            private uint m_ExtraFrameCountSum = 0;
            private uint m_EarlyFrameCountSum = 0;
            private uint m_DroppedFrameCountSum = 0;
            private ulong m_SdkFrameLatencySum = 0;


            // sum statistics all the time
            private uint m_FrameNumAll = 0;
            private ulong m_UpdateToPresentSumAll = 0;
            private ulong m_PreToPresentSumAll = 0;
            private ulong m_PostToPresentSumAll = 0;

            private uint m_FramePresentCountSumAll = 0;
            private uint m_ExtraFrameCountSumAll = 0;
            private uint m_EarlyFrameCountSumAll = 0;
            private uint m_DroppedFrameCountSumAll = 0;
            private ulong m_SdkFrameLatencySumAll = 0;


            // average or summary statistics in the period of duration
            public ulong avgUpdateToPresentT = 0;
            public ulong avgPreToPresentT = 0;
            public ulong avgPostToPresentT = 0;

            public float avrFramePresentCount = 0;
            public float avgExtraFrameCount = 0;
            public uint sumEarlyFrameCount = 0;
            public uint sumDroppedFrameCount = 0;
            public ulong avgSdkFrameLatency = 0;
            public uint presentFPS = 0;

            // average or summary statistics all the time
            public ulong avgUpdateToPresentTAll = 0;
            public ulong avgPreToPresentTAll = 0;
            public ulong avgPostToPresentTAll = 0;


            public float avrFramePresentCountAll = 0;
            public float avgExtraFrameCountAll = 0;
            public uint sumEarlyFrameCountAll = 0;
            public uint sumDroppedFrameCountAll = 0;
            public ulong avgSdkFrameLatencyAll = 0;


            public FrameMetrics(NativeMetrics nativeMetrics, float duration)
            {
                m_NativeMetrics = nativeMetrics;
                m_Duration = duration;
                m_BegTime = Time.realtimeSinceStartup;
                m_NativeTimeStamp = Time.realtimeSinceStartup;
            }

            /// <summary> Feed one frame stats. </summary>
            /// <param name="frameStats">        The frame stats.</param>
            /// <returns> Is the metrics duration full. </returns>
            internal bool FeedFrameStats(FrameStats frameStats, bool enableLog)
            {
                var curTime = Time.realtimeSinceStartup;
                if (frameStats.isFinish && frameStats.presentTime > frameStats.updateTime && frameStats.presentTime > frameStats.l_preRenderTime && frameStats.presentTime > frameStats.r_postRenderTime)
                {
                    m_FrameNum++;
                    m_FrameNumAll++;
                    var updateToPresent = frameStats.presentTime - frameStats.updateTime;
                    m_UpdateToPresentSum += updateToPresent;
                    m_UpdateToPresentSumAll += updateToPresent;

                    var preToPresent = frameStats.presentTime - frameStats.l_preRenderTime;
                    m_PreToPresentSum += preToPresent;
                    m_PreToPresentSumAll += preToPresent;
                    
                    var postToPresent = frameStats.presentTime - frameStats.r_postRenderTime;
                    m_PostToPresentSum += postToPresent;
                    m_PostToPresentSumAll += postToPresent;

                    m_FramePresentCountSum += frameStats.framePresentCount;
                    m_FramePresentCountSumAll += frameStats.framePresentCount;

                    m_ExtraFrameCountSum += frameStats.extraFrameCount;
                    m_ExtraFrameCountSumAll += frameStats.extraFrameCount;

        #if !UNITY_EDITOR
                    if (curTime - m_NativeTimeStamp > 0.5f)
                    {
                        m_NativeTimeStamp = curTime;

                        var earlyFrameCount = m_NativeMetrics.GetEarlyFrameCount();
                        m_EarlyFrameCountSum += earlyFrameCount;
                        m_EarlyFrameCountSumAll += earlyFrameCount;

                        var droppedFrameCount = m_NativeMetrics.GetDroppedFrameCount();
                        m_DroppedFrameCountSum += droppedFrameCount;
                        m_DroppedFrameCountSumAll += droppedFrameCount;
                    }
        #endif

                    m_SdkFrameLatencySum += frameStats.frameLatency;
                    m_SdkFrameLatencySumAll += frameStats.frameLatency;
                }

                bool isFull = curTime - m_BegTime >= m_Duration;
                if (isFull)
                {
                    try
                    {
                        SaveDurationMetrics();
                    }
                    catch (Exception ex)
                    {
                        NRDebugger.Error("[NRMetrics] Exception: {0}", ex.Message);
                    }
                    if (enableLog)
                        NRDebugger.Info(ToString());

                    Reset(false);
                }
                return isFull;
            }

            override public string ToString()
            {
                var duration = Time.realtimeSinceStartup - m_BegTime;
                return string.Format("FrameMetrics: FPS={0}, frNum={1}, UpdPrd={2}, prd={3}, postPrd={4}, sdkPrd={5}, FPC={6}, EFC={7}, early={8}, drop={9}, presentFPS={19}\n frNumA={10}, UpdPrdA={11}, prdA={12}, postPrdA={13}, sdkPrdA={14}, FPCA={15}, EFCA={16}, earlyA={17}, dropA={18}",
                    (int)(m_FrameNum/duration), m_FrameNum, avgUpdateToPresentT, avgPreToPresentT, avgPostToPresentT, avgSdkFrameLatency, avrFramePresentCount, avgExtraFrameCount, sumEarlyFrameCount, sumDroppedFrameCount,
                    m_FrameNumAll, avgUpdateToPresentTAll, avgPreToPresentTAll, avgPostToPresentTAll, avgSdkFrameLatencyAll, avrFramePresentCountAll, avgExtraFrameCountAll, sumEarlyFrameCountAll, sumDroppedFrameCountAll, presentFPS);
            }

            private void SaveDurationMetrics()
            {
                if (m_FrameNum <= 0 || m_FrameNumAll <= 0)
                    return;

                // average or summary statistics in the period of duration
                avgUpdateToPresentT = m_UpdateToPresentSum / m_FrameNum;
                avgPreToPresentT = m_PreToPresentSum / m_FrameNum;
                avgPostToPresentT = m_PostToPresentSum / m_FrameNum;

                avrFramePresentCount = (float)m_FramePresentCountSum / m_FrameNum;
                avgExtraFrameCount = (float)m_ExtraFrameCountSum / m_FrameNum;
                sumEarlyFrameCount = m_EarlyFrameCountSum;
                sumDroppedFrameCount = m_DroppedFrameCountSum;
                avgSdkFrameLatency = m_SdkFrameLatencySum / m_FrameNum;
                presentFPS = m_NativeMetrics.GetPresentFps();
            
                // average or summary statistics all the time
                avgUpdateToPresentTAll = m_UpdateToPresentSumAll / m_FrameNumAll;
                avgPreToPresentTAll = m_PreToPresentSumAll / m_FrameNumAll;
                avgPostToPresentTAll = m_PostToPresentSumAll / m_FrameNumAll;

                avrFramePresentCountAll = (float)m_FramePresentCountSumAll / m_FrameNumAll;
                avgExtraFrameCountAll = (float)m_ExtraFrameCountSumAll / m_FrameNumAll;
                sumEarlyFrameCountAll = m_EarlyFrameCountSumAll;
                sumDroppedFrameCountAll = m_DroppedFrameCountSumAll;
                avgSdkFrameLatencyAll = m_SdkFrameLatencySumAll / m_FrameNumAll;
            }

            internal void Reset(bool resetAll = true)
            {
                m_BegTime = Time.realtimeSinceStartup;
                m_NativeTimeStamp = Time.realtimeSinceStartup;
                m_FrameNum = 0;

                m_UpdateToPresentSum = 0;
                m_PreToPresentSum = 0;
                m_PostToPresentSum = 0;

                m_FramePresentCountSum = 0;
                m_ExtraFrameCountSum = 0;
                m_EarlyFrameCountSum = 0;
                m_DroppedFrameCountSum = 0;
                m_SdkFrameLatencySum = 0;

                if (resetAll)
                {
                    m_UpdateToPresentSumAll = 0;
                    m_PreToPresentSumAll = 0;
                    m_PostToPresentSumAll = 0;

                    m_FramePresentCountSumAll = 0;
                    m_ExtraFrameCountSumAll = 0;
                    m_EarlyFrameCountSumAll = 0;
                    m_DroppedFrameCountSumAll = 0;
                    m_SdkFrameLatencySumAll = 0;
                    m_FrameNumAll = 0;
                }
            }
        }

        [SerializeField]
        bool m_EnableLog = true;

        private Dictionary<int, FrameStats> m_DicFrameStats = new Dictionary<int, FrameStats>();
        private List<FrameStats> m_CacheFrameStats = new List<FrameStats>();

        private FrameMetrics m_FrameMetrics;
        internal FrameMetrics frameMetrics { get { return m_FrameMetrics; } }

        private NativeMetrics m_NativeMetrics;


        /// <summary> Renders the event delegate described by eventID. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        private delegate void RenderEventDelegate(int eventID);
        /// <summary> Handle of the render thread. </summary>
        private static RenderEventDelegate RenderThreadHandle = new RenderEventDelegate(RunOnRenderThread);
        /// <summary> The render thread handle pointer. </summary>
        private static IntPtr RenderThreadHandlePtr = Marshal.GetFunctionPointerForDelegate(RenderThreadHandle);
        
        private const int k_EndOfFrameEvent = 0x0001;
        private const int k_BaseFactor = 1000000000;

        private void Start()
        {
            NRDebugger.Info("[NRMetrics] Start");

#if !UNITY_EDITOR
            m_NativeMetrics = new NativeMetrics();

#if USING_XR_SDK
            m_NativeMetrics.Create(NativeXRPlugin.GetMetricsHandle());
#else
            m_NativeMetrics.Create();
            m_NativeMetrics.Start();
#endif

#endif

#if UNITY_EDITOR || USING_XR_SDK
            StartCoroutine(EndOfFrameCoroutine());
#else
            NRSessionManager.Instance.NRSwapChainMan.onBeforeSubmitFrameInMainThread += OnBeforeSubmitFrameInMainThread;
#endif

            m_FrameMetrics = new FrameMetrics(m_NativeMetrics, 1.0f);

            if (GraphicsSettings.currentRenderPipeline == null)
            {
                Camera.onPreRender += OnCameraPreRender;
                Camera.onPostRender += OnCameraPostRender;
            }
            else
            {
                RenderPipelineManager.beginCameraRendering += OnBeginCameraRender;
                RenderPipelineManager.endCameraRendering += OnPostCameraRender;
            }

            NRKernalUpdater.OnEnterUpdate += OnEnterUpdate;
        }

        override protected void OnDestroy()
        {
            NRDebugger.Info("[NRMetrics] OnDestroy");
            m_NativeMetrics?.Stop();
            m_NativeMetrics?.Destroy();
            m_NativeMetrics = null;


            if (GraphicsSettings.currentRenderPipeline == null)
            {
                Camera.onPreRender -= OnCameraPreRender;
                Camera.onPostRender -= OnCameraPostRender;
            }
            else
            {
                RenderPipelineManager.beginCameraRendering -= OnBeginCameraRender;
                RenderPipelineManager.endCameraRendering -= OnPostCameraRender;
            }
            NRKernalUpdater.OnEnterUpdate -= OnEnterUpdate;

            base.OnDestroy();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                this.Pause();
            }
            else
            {
                this.Resume();
            }
        }

        public void Pause()
        {
            m_NativeMetrics?.Pause();
        }

        public void Resume()
        {
            m_NativeMetrics?.Resume();

            m_FrameMetrics?.Reset();
        }

        FrameStats GetFrameStats(int frameIdx)
        {
            FrameStats frameStats = null;
            if (m_DicFrameStats.TryGetValue(frameIdx, out frameStats))
            {
                return frameStats;
            }

            if (m_CacheFrameStats.Count > 0)
            {
                frameStats = m_CacheFrameStats[0];
                m_CacheFrameStats.RemoveAt(0);
                frameStats.frameIdx = frameIdx;
                // NRDebugger.Info("[NRMetrics] GetFrameStats from cache: {0}, dicLen={1}", frameStats.frameIdx, m_DicFrameStats.Count);
            }
            else
            {
                frameStats = new FrameStats(frameIdx);
                // NRDebugger.Info("[NRMetrics] GetFrameStats new: {0}, dicLen={1}", frameStats.frameIdx, m_DicFrameStats.Count);
            }

            m_DicFrameStats.Add(frameIdx, frameStats);
            return frameStats;
        }

        void OnEnterUpdate()
        {
            var updateTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
            // NRDebugger.Info("[NRMetrics] OnEnterUpdate: FrameCount={0}, updateTime={1}", Time.frameCount, updateTime);
            
            var frameStats = GetFrameStats(Time.frameCount);
            frameStats.updateTime = updateTime;
        }


        void OnCameraPreRender(Camera camera)
        {
            OnCameraPreRenderImp(camera);
        }

        void OnBeginCameraRender(ScriptableRenderContext context, Camera camera)
        {
            OnCameraPreRenderImp(camera);
        }

        void OnCameraPreRenderImp(Camera cam)
        {
#if UNITY_EDITOR || USING_XR_SDK
            if (cam.name == "CenterCamera")
            {
                CheckFrameFinish();

                var frameStats = GetFrameStats(Time.frameCount);
                var l_preRenderTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
                frameStats.l_preRenderTime = l_preRenderTime;
                frameStats.presentTime = NRFrame.CurrentPoseTimeStamp;
                // NRDebugger.Info("[NRMetrics] OnCameraPreRender: FrameCount={0}, center preRenderTime={1}, presentTime={2}", Time.frameCount, l_preRenderTime, frameStats.presentTime);
            }
#else
            if (cam.name == "LeftCamera")
            {
                CheckFrameFinish();

                var frameStats = GetFrameStats(Time.frameCount);
                var l_preRenderTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
                frameStats.l_preRenderTime = l_preRenderTime;
                frameStats.presentTime = NRFrame.CurrentPoseTimeStamp;
                // NRDebugger.Info("[NRMetrics] OnCameraPreRender: FrameCount={0}, l_preRenderTime={1}, presentTime={2}", Time.frameCount, l_preRenderTime, frameStats.presentTime);
            }
            else if (cam.name == "RightCamera")
            {
                var frameStats = GetFrameStats(Time.frameCount);
                var r_preRenderTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
                frameStats.r_preRenderTime = r_preRenderTime;
                // NRDebugger.Info("[NRMetrics] OnCameraPreRender: FrameCount={0}, r_preRenderTime={1}", Time.frameCount, r_preRenderTime);
            }
#endif
        }

        void CheckFrameFinish()
        {
            foreach (var kv in m_DicFrameStats)
            {
                var frameStats = kv.Value;
                if (frameStats.isFinish)
                {
                    if (NRFrame.SessionStatus == SessionState.Running)
                    {
                        frameStats.FeedNativeMetricsStats(m_NativeMetrics);
                        // NRDebugger.Info(frameStats.ToString());
                        m_FrameMetrics?.FeedFrameStats(frameStats, m_EnableLog);
                    }

                    // recycle frameStats
                    m_DicFrameStats.Remove(kv.Key);
                    frameStats.Reset();
                    m_CacheFrameStats.Add(frameStats);

                    break;
                }
            }
        }

        void OnCameraPostRender(Camera camera)
        {
            OnCameraPostRenderImp(camera);
        }

        void OnPostCameraRender(ScriptableRenderContext context, Camera camera)
        {
            OnCameraPostRenderImp(camera);
        }

        void OnCameraPostRenderImp(Camera cam)
        {
#if UNITY_EDITOR || USING_XR_SDK
            if (cam.name == "CenterCamera")
            {
                var frameStats = GetFrameStats(Time.frameCount);
                var l_postRenderTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
                frameStats.l_postRenderTime = l_postRenderTime;
                frameStats.r_preRenderTime = l_postRenderTime;
                frameStats.r_postRenderTime = l_postRenderTime;
                // NRDebugger.Info("[NRMetrics] OnCameraPostRender: FrameCount={0}, center postRenderTime={1}", Time.frameCount, l_postRenderTime);
            }
#else
            if (cam.name == "LeftCamera")
            {
                var frameStats = GetFrameStats(Time.frameCount);
                var l_postRenderTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
                frameStats.l_postRenderTime = l_postRenderTime;
                // NRDebugger.Info("[NRMetrics] OnCameraPostRender: FrameCount={0}, l_postRenderTime={1}", Time.frameCount, l_postRenderTime);
            }
            else if (cam.name == "RightCamera")
            {
                var frameStats = GetFrameStats(Time.frameCount);
                var r_postRenderTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
                frameStats.r_postRenderTime = r_postRenderTime;
                // NRDebugger.Info("[NRMetrics] OnCameraPostRender: FrameCount={0}, r_postRenderTime={1}", Time.frameCount, r_postRenderTime);
            }
#endif
        }

        void OnGUI()
        {
            var frameStats = GetFrameStats(Time.frameCount);
            var onGUITime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
            frameStats.onGUITime = onGUITime;
            // NRDebugger.Info("[NRMetrics] OnGUI: FrameCount={0}, curTime={1}", Time.frameCount, onGUITime);            
        }

        /// <summary> The coroutine. </summary>
        /// <returns> An IEnumerator. </returns>
        private IEnumerator EndOfFrameCoroutine()
        {
            WaitForEndOfFrame delay = new WaitForEndOfFrame();
            yield return delay;

            while (true)
            {
                yield return delay;
                OnBeforeSubmitFrameInMainThread();
            }
        }

        void OnBeforeSubmitFrameInMainThread()
        {
            var frameStats = GetFrameStats(Time.frameCount);
            var endOfFrameTime = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
            frameStats.endOfFrameTime = endOfFrameTime;
            // NRDebugger.Info("[NRMetrics] EndOfFrame: FrameCount={0}, endOfFrameTime={1}", Time.frameCount, endOfFrameTime);

            var frameIdx = Time.frameCount;
            int virtualEventId = k_BaseFactor * k_EndOfFrameEvent + frameIdx;
            GL.IssuePluginEvent(RenderThreadHandlePtr, virtualEventId);
        }

        /// <summary> Executes the 'on render thread' operation. </summary>
        /// <param name="eventID"> Identifier for the event.</param>
        [MonoPInvokeCallback(typeof(RenderEventDelegate))]
        private static void RunOnRenderThread(int eventID)
        {
            var realEventId = eventID / k_BaseFactor;
            if (realEventId == k_EndOfFrameEvent)
            {
                var frameIdx = eventID % k_BaseFactor;
                // UnityEngine.Debug.LogFormat("[NRMetrics] RunOnRenderThread : eventID={0}, FrameCount={1}, frameIdx={2}", eventID, Time.frameCount, frameIdx);
                NRMetrics.Instance.OnFrameFinish(frameIdx);
            }
        }

        void OnFrameFinish(int frameIdx)
        {
            var frameStats = GetFrameStats(frameIdx);
            var time = NRSessionManager.Instance.TrackingSubSystem.GetHMDTimeNanos();
            frameStats.endOfFrameInRenderThreadTime = time;
            frameStats.isFinish = true;
            // NRDebugger.Info("[NRMetrics] OnFrameFinish: FrameCount={0}, endOfFrameTime={1}", frameIdx, time);
        }

        public void ResetMetrics()
        {
            m_FrameMetrics?.Reset();
        }
    }
}
