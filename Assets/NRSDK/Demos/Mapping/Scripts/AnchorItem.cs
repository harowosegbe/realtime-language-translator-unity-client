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
    using NRKernal.NRExamples;
    using System;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    /// <summary> An anchor item. </summary>
    public class AnchorItem : MonoBehaviour, IPointerClickHandler
    {
        /// <summary> The key. </summary>
        public string key;
        /// <summary> The on anchor item click. </summary>
        public Action<string, GameObject> OnAnchorItemClick;
        /// <summary> The anchor panel. </summary>
        [SerializeField]
        private GameObject canvas;
        [SerializeField]
        private Text anchorUUID;
        [SerializeField]
        private Text anchorState;
        [SerializeField]
        private Button remapBtn;
        [SerializeField]
        private ObserveRange m_ObserveRange;
        private NRWorldAnchor m_NRWorldAnchor;
        private Material m_Material;

        #region properties
        public float ObserveAngle => m_ObserveRange.Angle;
        public NREstimateDistance ObserveDistance => m_ObserveRange.Distance;
        #endregion

        void Start()
        {
            if (TryGetComponent(out m_NRWorldAnchor))
            {
                if (canvas != null)
                    canvas.SetActive(true);
                if (anchorUUID != null)
                    anchorUUID.text = m_NRWorldAnchor.UUID;
                m_Material = GetComponentInChildren<Renderer>()?.material;
                if (m_Material != null)
                {
                    m_NRWorldAnchor.OnTrackingChanged += (NRWorldAnchor worldAnchor, TrackingState state) =>
                    {
                        switch (state)
                        {
                            case TrackingState.Tracking:
                                m_Material.color = Color.green;
                                break;
                            case TrackingState.Paused:
                                m_Material.color = Color.white;
                                break;
                            case TrackingState.Stopped:
                                m_Material.color = Color.red;
                                break;
                        }
                    };
                    m_NRWorldAnchor.OnAnchorStateChanged += AnchorItem_OnAnchorStateChanged;
                }
            }
        }

        private void Update()
        {
            if (m_NRWorldAnchor == null)
            {
                return;
            }
#if UNITY_EDITOR
            mockAnchorState();
#endif

            updateRemapButtonState();

        }

        public void Save()
        {
            if (m_NRWorldAnchor != null)
            {
                m_NRWorldAnchor.SaveAnchor();
            }
        }

        public void Erase()
        {
            if (m_NRWorldAnchor != null)
                m_NRWorldAnchor.EraseAnchor();
        }

        public void Destory()
        {
            if (m_NRWorldAnchor != null)
            {
                m_NRWorldAnchor.DestroyAnchor();
                if (m_NRWorldAnchor == MapQualityIndicator.CurrentAnchor)
                {
                    MapQualityIndicator.InterruptMappingGuide();
                }
            }
        }

        public void EnableRemapButton(bool enable)
        {
            remapBtn.gameObject.SetActive(enable);
        }

        public void Remap()
        {
            if (m_NRWorldAnchor != null)
            {
                if (m_NRWorldAnchor.Remap())
                {
                    MapQualityIndicator.SetCurrentAnchor(m_NRWorldAnchor);
                }
            }
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            OnAnchorItemClick?.Invoke(key, gameObject);
        }


        private void updateRemapButtonState()
        {
            if (NRWorldAnchorStore.Instance.IsRemapping)
            {
                EnableRemapButton(false);
            }
            else
            {
                if (m_MappingState == MappingState.MAPPING_STATE_REQUEST || m_MappingState == MappingState.MAPPING_STATE_REMAP_FAILURE)
                {
                    EnableRemapButton(true);
                }
                else
                {
                    EnableRemapButton(false);
                }
            }
        }

        private MappingState m_MappingState = MappingState.MAPPING_STATE_UNKNOWN;
        private void AnchorItem_OnAnchorStateChanged(NRWorldAnchor anchor, MappingState state)
        {
            m_MappingState = state;
            anchorState.text = $"{state}";
        }

        #region mock
        private void mockAnchorState()
        {
            if (m_NRWorldAnchor == null)
            {
                return;
            }

            if (Input.GetKey(KeyCode.U))
            {
                m_NRWorldAnchor.CurrentAnchorState = NRAnchorState.NR_ANCHOR_STATE_UNKNOWN;
            }
            else if (Input.GetKey(KeyCode.I))
            {
                if (NRWorldAnchorStore.Instance.CreatingAnchorHandle == m_NRWorldAnchor.AnchorHandle ||
                    NRWorldAnchorStore.Instance.RemappingAnchorHandle == m_NRWorldAnchor.AnchorHandle)
                {
                    m_NRWorldAnchor.CurrentAnchorState = NRAnchorState.NR_ANCHOR_STATE_MAPPING;
                }
            }
            else if (Input.GetKey(KeyCode.O))
            {
                if (NRWorldAnchorStore.Instance.CreatingAnchorHandle == m_NRWorldAnchor.AnchorHandle ||
                    NRWorldAnchorStore.Instance.RemappingAnchorHandle == m_NRWorldAnchor.AnchorHandle)
                {
                    m_NRWorldAnchor.CurrentAnchorState = NRAnchorState.NR_ANCHOR_STATE_SUCCESS;
                }
            }
            else if (Input.GetKey(KeyCode.P))
            {
                m_NRWorldAnchor.CurrentAnchorState = NRAnchorState.NR_ANCHOR_STATE_REQUEST;
            }
            else if (Input.GetKey(KeyCode.J))
            {
                if (NRWorldAnchorStore.Instance.CreatingAnchorHandle == m_NRWorldAnchor.AnchorHandle ||
                    NRWorldAnchorStore.Instance.RemappingAnchorHandle == m_NRWorldAnchor.AnchorHandle)
                {
                    m_NRWorldAnchor.CurrentAnchorState = NRAnchorState.NR_ANCHOR_STATE_FAILURE;
                }
            }
        }

        [SerializeField]
        private ConfirmDialog m_GuideDialog;
        internal async Task ShowGuide()
        {
            m_GuideDialog.Show();
            await m_GuideDialog.WaitUntilClosed();
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ObserveRange
    {
        /// <summary>
        /// The angle between the leftmost ray and the rightmost ray
        /// casted from the anchor to the observer in degree.
        /// </summary>
        public float Angle;
        /// <summary>
        /// The maximum distance between the observer and the anchor.
        /// </summary>
        public NREstimateDistance Distance;
    }
}
