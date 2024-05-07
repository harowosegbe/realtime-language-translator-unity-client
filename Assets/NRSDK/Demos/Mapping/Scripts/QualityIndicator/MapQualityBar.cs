/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal.Persistence
{
    using System;
    using UnityEngine;

    public class MapQualityBar : MonoBehaviour
    {
        [SerializeField]
        private new Renderer renderer;

        [Header("Quality Colors")]
        public Color initColor = Color.white;
        public Color inSufficientColor = Color.red;
        public Color sufficientColor = Color.yellow;
        public Color goodColor = Color.green;

        private const string _varColor = "_Color";

        #region Animator
        [SerializeField]
        private Animator animator;
        // Hash values for variables defined by MapQualityBarAnimator.
        private static readonly int _paramQuality = Animator.StringToHash("Quality");
        private static readonly int _paramIsVisited = Animator.StringToHash("IsVisited");
        private static readonly int _paramColorCurve = Animator.StringToHash("ColorCurve");

        // Hash values for states defined by MapQualityBarAnimator.
        private static readonly int _stateLow = Animator.StringToHash("Base Layer.Low");
        private static readonly int _stateMedium = Animator.StringToHash("Base Layer.Medium");
        private static readonly int _stateHigh = Animator.StringToHash("Base Layer.High");

        private bool m_isVisited;
        [SerializeField]
        private NREstimateQuality m_qualityState;
        private float m_alpha = 1.0f;

        /// <summary>
        /// Gets or sets a value indicating whether this map quality bar has been visited.
        /// </summary>
        public bool IsVisited
        {
            get
            {
                return m_isVisited;
            }

            set
            {
                m_isVisited = value;
                animator.SetBool(_paramIsVisited, m_isVisited);
            }
        }
        public NREstimateQuality QualityState
        {
            get
            {
                return m_qualityState;
            }

            set
            {
                if (m_qualityState != value)
                {
                    m_qualityState = value;
                    animator.SetInteger(_paramQuality, (int)m_qualityState);
                }
            }
        }

        #endregion

        public void Recycle()
        {
            gameObject.SetActive(false);
            IsVisited = false;
            QualityState = NREstimateQuality.NR_ANCHOR_QUALITY_INSUFFICIENT;
        }

        public void SetAlpha(float alpha)
        {
            m_alpha = alpha;
        }

        private void Update()
        {

#if UNITY_EDITOR
            mockStateChange();
#endif

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            float colorCurve = animator.GetFloat(_paramColorCurve);
            Color color = initColor;
            if (stateInfo.fullPathHash == _stateLow)
            {
                color = Color.Lerp(initColor, inSufficientColor, colorCurve);
            }
            else if (stateInfo.fullPathHash == _stateMedium)
            {
                color = Color.Lerp(initColor, sufficientColor, colorCurve);
            }
            else if (stateInfo.fullPathHash == _stateHigh)
            {
                color = Color.Lerp(initColor, goodColor, colorCurve);
            }

            color.a = m_alpha;
            renderer.material.SetColor(_varColor, color);
        }

        private void mockStateChange()
        {
            if (IsVisited)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    QualityState =(NREstimateQuality.NR_ANCHOR_QUALITY_INSUFFICIENT);
                }
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    QualityState = (NREstimateQuality.NR_ANCHOR_QUALITY_SUFFICIENT);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    QualityState = (NREstimateQuality.NR_ANCHOR_QUALITY_GOOD);
                }
            }
        }
    }
}
