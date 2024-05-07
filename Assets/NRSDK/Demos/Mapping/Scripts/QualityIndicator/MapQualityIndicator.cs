/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/
namespace NRKernal.NRExamples
{
    using NRKernal.Persistence;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    /// <summary>
    /// 反应Anchor 建图质量的视觉元素
    /// </summary>
    public class MapQualityIndicator : SingletonBehaviour<MapQualityIndicator>
    {
        [SerializeField]
        private IndicatorSettings m_Settings;


        #region public
        public static IndicatorSettings Settings => Instance.m_Settings;
        public static NRWorldAnchor CurrentAnchor => Instance.m_currentWorldAnchor;
        public static void AddStateChangeListener(Action<NRWorldAnchor, MappingState> action)
        {
            Instance.OnMappingStateChanged += action;
        }
        public static void RemoveStateChangeListener(Action<NRWorldAnchor, MappingState> action)
        {
            Instance.OnMappingStateChanged -= action;
        }

        public static void SetCurrentAnchor(NRWorldAnchor anchor, bool isRemapping = true)
        {
            NRDebugger.Info($"[{nameof(MapQualityIndicator)}] {nameof(SetCurrentAnchor)} {anchor.UUID} {isRemapping}");
            if (Instance.m_currentWorldAnchor == anchor)
            {
                return;
            }
            Instance.setCurrentAnchor(anchor, isRemapping);
        }

        public static void ShowMappingGuide()
        {
            Instance.showMappingGuide();
        }

        public static void InterruptMappingGuide()
        {
            Instance.interruptMappingGuide();
        }
        public static void FinishMappingGuide()
        {
            Instance.finishMappingGuide();
        }
        #endregion


        #region properties
        private List<MapQualityBar> bars
        {
            get
            {
                if (m_bars == null || m_bars.Count != Settings.barCount)
                {
                    m_bars = initalizeBars();
                }
                return m_bars;
            }
        }

        /// <summary>
        /// 用于评估建图质量的pose参数
        /// </summary>
        private Pose estimatePose
        {
            get
            {
                return NRFrame.HeadPose;
            }
        }
        #endregion

        private event Action<NRWorldAnchor, MappingState> OnMappingStateChanged;

        private NRWorldAnchor m_currentWorldAnchor;
        private float nextEstimateTime;
        private List<MapQualityBar> m_bars;
        [SerializeField]
        private GameObject m_qualityBarPrefab;

        /// <summary> 环形的朝向 </summary>
        private Vector3 m_ringForward;

        /// <summary>anchor 到第一个bar的朝向 </summary>
        private Vector3 m_startDir;
        /// <summary>
        /// 是否是重建
        /// </summary>
        private bool m_isRemapping;

        protected override void Awake()
        {
            base.Awake();
            NRKernalUpdater.OnUpdate += OnUpdate;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            NRKernalUpdater.OnUpdate -= OnUpdate;
        }

        void OnUpdate()
        {
            if (m_currentWorldAnchor == null)
            {
                return;
            }

            if (Time.time > nextEstimateTime)
            {
                if (m_currentWorldAnchor.CurrentAnchorState != NRAnchorState.NR_ANCHOR_STATE_MAPPING)
                {
                    return;
                }

                int index = computeBarIndex();

                if (index < 0 || index >= m_bars.Count)
                {
                    return;
                }

                var visitedBar = bars[index];
                visitedBar.IsVisited = true;
                //更新建图质量
                NREstimateQuality quality = NRWorldAnchorStore.Instance.UpdateMapQuality(m_currentWorldAnchor, estimatePose);
                visitedBar.QualityState = quality;
                nextEstimateTime = Time.time + Settings.estimateIntervalSeconds;

                Debug.DrawLine(m_currentWorldAnchor.transform.position, visitedBar.transform.position, Color.black, 1);
                Debug.DrawLine(m_currentWorldAnchor.transform.position, estimatePose.position, Color.red, 1);

                NRDebugger.Info($"[{this.GetType()}] Count Of Good Bars for {m_currentWorldAnchor.UUID}: {this.CountOfGoodBars}");
            }
        }

        private void setCurrentAnchor(NRWorldAnchor anchor, bool isRemapping)
        {
            //清除上一个anchor的状态
            interruptMappingGuide();

            m_isRemapping = isRemapping;
            m_currentWorldAnchor = anchor;
            m_currentWorldAnchor.OnAnchorStateChanged += MapQualityIndicator_OnAnchorStateChanged;
        }

        private void MapQualityIndicator_OnAnchorStateChanged(NRWorldAnchor anchor, MappingState state)
        {
            OnMappingStateChanged?.Invoke(anchor, state);
        }

        private void showMappingGuide()
        {
            if (m_currentWorldAnchor == null)
            {
                NRDebugger.Warning($"[{nameof(MapQualityIndicator)}] {nameof(ShowMappingGuide)} anchor is null!");
                return;
            }

            NRDebugger.Info($"[{nameof(MapQualityIndicator)}] {nameof(ShowMappingGuide)} {m_currentWorldAnchor.UUID}");

            recycleBars();
            placeQualityBars();
            nextEstimateTime = Time.time + Settings.estimateIntervalSeconds;
        }

        private void interruptMappingGuide()
        {
            if (m_currentWorldAnchor != null)
            {
                NRDebugger.Info($"[{nameof(MapQualityIndicator)}] {nameof(interruptMappingGuide)} {m_currentWorldAnchor.UUID}");
                m_currentWorldAnchor.OnAnchorStateChanged -= MapQualityIndicator_OnAnchorStateChanged;
                m_currentWorldAnchor = null;
            }

            recycleBars();
        }

        private async void finishMappingGuide()
        {
            if (m_currentWorldAnchor != null)
            {
                m_currentWorldAnchor.OnAnchorStateChanged -= MapQualityIndicator_OnAnchorStateChanged;
                m_currentWorldAnchor = null;
            }

            await turnAllBarsGood();

            await Task.Delay(2000);

            recycleBars();
        }

        private List<MapQualityBar> initalizeBars()
        {
            var list = new List<MapQualityBar>();

            NRDebugger.Info($"[MapQualityIndicator] initalizeBars {Settings.barCount}");

            for (int i = 0; i < Settings.barCount; i++)
            {
                var bar = Instantiate(m_qualityBarPrefab, this.transform).GetComponent<MapQualityBar>();
                bar.gameObject.SetActive(false);
                list.Add(bar);
            }

            return list;
        }

        /// <summary>
        /// 围绕anchor放置qualitybar
        /// 摆放规则: z轴为eye的forward 也是ringForward
        /// 
        ///               ^z  0度
        ///                |
        ///                |
        ///    --------|-------->x  90度
        ///     o         |         o     
        ///       o       |       o        
        ///             o | o              
        ///             180度              
        /// </summary>
        private void placeQualityBars()
        {
            m_ringForward = (m_currentWorldAnchor.transform.position - estimatePose.position).normalized;
            m_ringForward.y = 0;
            Vector3 upDir = Vector3.up;

            float range = Settings.angleRange;
            int barCount = Settings.barCount;

            float startAngle = normalizeAngle(180 - range * 0.5f);
            float deltaAngle = range / (barCount - 1);
            m_startDir = Quaternion.AngleAxis(startAngle, upDir) * m_ringForward;

            Vector3 midPos = Vector3.Lerp(m_currentWorldAnchor.transform.position, estimatePose.position, 0.3f);          
            float y = midPos.y;
            Vector3 initalPos = m_currentWorldAnchor.transform.position;
            initalPos.y = y;
            float radius = Vector3.Distance(initalPos, midPos);

            for (int i = 0; i < barCount; ++i)
            {
                var bar = bars[i];
                bar.gameObject.SetActive(true);
                Quaternion deltaRotation = Quaternion.AngleAxis(normalizeAngle(startAngle + i * deltaAngle), upDir);
                bar.transform.position = initalPos + deltaRotation * m_ringForward * radius;
                bar.transform.up = m_currentWorldAnchor.transform.position - bar.transform.position;
            }
        }

        private int computeBarIndex()
        {
            Vector3 viewRay = getDirectionFromAnchorToEye();
            viewRay.y = 0;
            Vector3 startDir = m_startDir;
            startDir.y = 0;

            float signedAngleFromStartDir = normalizeAngle(Vector3.SignedAngle(viewRay, startDir, Vector3.down));
            float angleStep = Settings.angleRange / (Settings.barCount - 1);
            return Mathf.FloorToInt(signedAngleFromStartDir / angleStep);
        }

        private void recycleBars()
        {
            foreach (MapQualityBar bar in bars)
            {
                bar.Recycle();
            }
        }

        private async Task turnAllBarsGood()
        {
            foreach(MapQualityBar bar in bars)
            {
                if(bar.QualityState == NREstimateQuality.NR_ANCHOR_QUALITY_GOOD)
                {
                    continue;
                }

                bar.IsVisited = true;
                bar.QualityState = NREstimateQuality.NR_ANCHOR_QUALITY_GOOD;
                await Task.Delay(100);
            }
        }

        #region helpers
        /// <summary>
        /// 规范化角度值范围为[0,360)
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static float normalizeAngle(float angle)
        {
            float ret = angle % 360f;
            if (ret < 0)
            {
                ret += 360f;
            }
            return ret;
        }

        private Vector3 getDirectionFromAnchorToEye()
        {
            var centerEyePose = estimatePose;
            Vector3 viewRay = centerEyePose.position - m_currentWorldAnchor.transform.position;
            return viewRay.normalized;
        }

        #endregion

        #region debug
        private int CountOfGoodBars
        {
            get
            {
                int count = 0;
                for(int i= 0; i < m_bars.Count; i++)
                {
                    if (m_bars[i].QualityState == NREstimateQuality.NR_ANCHOR_QUALITY_GOOD)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
        #endregion


        [Serializable]
        public class IndicatorSettings
        {
            public bool showIndicator;

            public float angleRange;
            public int barAngleStep;
            /// <summary>
            /// Interval for estimating mapping quality in seconds
            /// </summary>
            public float estimateIntervalSeconds = 0.5f;

            public int barCount => Mathf.FloorToInt(angleRange / barAngleStep);

        }
    }

}
