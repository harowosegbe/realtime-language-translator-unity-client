/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NRKernal.NRExamples;

namespace NRKernal.NRExamples
{
    public class FocusDistPanel : MonoBehaviour
    {
        [SerializeField]
        private Toggle m_FollowHeadToggle;
        [SerializeField]
        private CameraSmoothFollow m_FollowComp;

        [SerializeField]
        private Text m_TxtFocusDist;
        [SerializeField]
        private Text m_TxtFocusPlaneNorm;
        [SerializeField]
        private Text m_TxtFocusSpaceType;
        [SerializeField]
        private Slider m_DistanceAdjSlider;
        [SerializeField]
        private Toggle m_AutoFucusToggle;
        [SerializeField]
        private Toggle m_AutoFocusPlaneNormToggle;
        [SerializeField]
        private Toggle m_FocusInViewSpaceToggle;
        public FocusManager m_FocusMan;


        [SerializeField]
        private Text m_TxtTargetFPS;
        [SerializeField]
        private Button m_BtnTargetFPS;
        private int m_TargetFPS = 30;

        [SerializeField]
        private Button m_BtnPlaneDist;
        [SerializeField]
        private Transform m_ControlledPlane;
        private int m_PlaneDistType = 0;

        [SerializeField]
        GameObject m_FocusAnchorGO;

        [SerializeField]
        private Button m_BtnTransport;

        public Transform rootTran;
        
        void Start()
        {
            if (m_FocusMan == null)
            {
                m_FocusMan = GameObject.FindObjectOfType<FocusManager>();
            }

            m_FollowHeadToggle.isOn = true;
            m_FollowHeadToggle.onValueChanged.AddListener(OnToggleFollowHead);

            float default_distance = 1.4f;
            m_TxtFocusDist.text = default_distance.ToString("F2");

            m_DistanceAdjSlider.maxValue = 100.0f;
            m_DistanceAdjSlider.minValue = 1.0f;
            m_DistanceAdjSlider.value = default_distance;
            m_DistanceAdjSlider.onValueChanged.AddListener(OnSlideValueChange);

            m_AutoFucusToggle.isOn = m_FocusMan != null;
            m_AutoFucusToggle.onValueChanged.AddListener(OnToggleChanged);

            m_AutoFocusPlaneNormToggle.isOn = m_FocusMan != null ? m_FocusMan.adjustFocusPlaneNorm : false;
            m_AutoFocusPlaneNormToggle.onValueChanged.AddListener(OnToggleFocusPlaneNormChanged);

            m_FocusInViewSpaceToggle.isOn = m_FocusMan != null ? m_FocusMan.isFocusInViewSpace : true;
            m_FocusInViewSpaceToggle.onValueChanged.AddListener(OnToggleFocusSpaceTypeChanged);

            /// targetFPS
            m_TargetFPS = 30;
            NRFrame.targetFrameRate = m_TargetFPS;

            m_TxtTargetFPS.text = m_TargetFPS.ToString();
            m_BtnTargetFPS.onClick.AddListener(OnBtnTargetFPS);

            m_BtnPlaneDist.onClick.AddListener(OnBtnPlaneDist);
            
            m_BtnTransport.onClick.AddListener(OnBtnTransport);
        }

        void OnToggleFollowHead(bool isOn)
        {
            if (m_FollowComp != null)
                m_FollowComp.enabled = isOn;
        }

        void OnToggleChanged(bool isOn)
        {
            if (m_FocusMan != null)
            {
                m_FocusMan.enabled = isOn;
            }
        }

        void OnToggleFocusPlaneNormChanged(bool isOn)
        {
            if (m_FocusMan != null)
            {
                m_FocusMan.adjustFocusPlaneNorm = isOn;
            }
        }

        void OnToggleFocusSpaceTypeChanged(bool isOn)
        {
            if (m_FocusMan != null)
            {
                m_FocusMan.isFocusInViewSpace = isOn;
            }
            m_DistanceAdjSlider.enabled = isOn;
        }

        void OnSlideValueChange(float val)
        {
            NRFrame.SetFocusDistance(val);
            m_TxtFocusDist.text = val.ToString("F2");
        }

        void OnBtnTargetFPS()
        {
            if (m_TargetFPS == 60)
                m_TargetFPS = 10;
            else if (m_TargetFPS == 10)
                m_TargetFPS = 15;
            else if (m_TargetFPS == 15)
                m_TargetFPS = 30;
            else if (m_TargetFPS == 30)
                m_TargetFPS = 60;
            else
                m_TargetFPS = 30;

            NRFrame.targetFrameRate = m_TargetFPS;
            m_TxtTargetFPS.text = m_TargetFPS.ToString();
        }

        void OnBtnPlaneDist()
        {
            m_PlaneDistType = (m_PlaneDistType + 1) % 3;

            float dist = 2.5f;
            if (m_PlaneDistType == 0)
                dist = 2.5f;
            else if (m_PlaneDistType == 1)
                dist = 4f;
            else if (m_PlaneDistType == 2)
                dist = 10f;
            
            m_ControlledPlane.position = new Vector3(0, 0, dist);
        }

        private bool isOrigPos = true;
        void OnBtnTransport()
        {
            rootTran.position = isOrigPos ? new Vector3(0, 0, 10000) : Vector3.zero;
            
            isOrigPos = !isOrigPos;
        }

        private void Update()
        {
            var swapChainMan = NRSessionManager.Instance.NRSwapChainMan;
            var focusPoint = swapChainMan.FocusPoint;
            var focusPlaneNorm = swapChainMan.FocusPlaneNorm;
            var spaceType = swapChainMan.FocusPlaneSpaceType;

            m_TxtFocusDist.text = focusPoint.ToString("F2");
            m_TxtFocusPlaneNorm.text = focusPlaneNorm.ToString("F2");
            m_TxtFocusSpaceType.text = spaceType == NRReferenceSpaceType.NR_REFERENCE_SPACE_VIEW ? "View" : "Global";

            if (m_FocusAnchorGO != null)
            {
                if (spaceType == NRReferenceSpaceType.NR_REFERENCE_SPACE_VIEW)
                {
                    m_FocusAnchorGO.transform.position = NRSessionManager.Instance.CenterCameraAnchor.TransformPoint(focusPoint);
                }
                else
                    m_FocusAnchorGO.transform.position = focusPoint;
            }
        }
    }
}
