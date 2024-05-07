/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using UnityEngine;

namespace NRKernal.NRExamples
{
    public class SetFocusPlaneTool : MonoBehaviour
    {
        private Transform m_HeadTransfrom;
        private Vector3 m_FocusPosition;
        RaycastHit hitResult;
        private const float MinFocusDistance = 0.3f;
        private const float MaxDistance = 100;

        void Update()
        {
            if (m_HeadTransfrom == null)
            {
                m_HeadTransfrom = NRSessionManager.Instance.CenterCameraAnchor;
                if (m_HeadTransfrom == null)
                    return;
            }

            if (Physics.Raycast(new Ray(m_HeadTransfrom.position + m_HeadTransfrom.forward * MinFocusDistance, m_HeadTransfrom.forward), out hitResult, MaxDistance))
            {
                m_FocusPosition = m_HeadTransfrom.InverseTransformPoint(hitResult.point);
                NRFrame.SetFocusDistance(m_FocusPosition.magnitude);
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (hitResult.collider != null)
            {
                Gizmos.DrawSphere(hitResult.point, 0.1f);
                Gizmos.DrawLine(m_HeadTransfrom.position, hitResult.point);
            }
        }
#endif
    }
}
