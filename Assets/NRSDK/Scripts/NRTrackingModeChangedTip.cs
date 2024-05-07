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
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    using System.IO;
    using System.Collections.Generic;

    public class NRTrackingModeChangedTip : MonoBehaviour
    {
        [SerializeField]
        private GameObject m_PauseRendererGroup;
        [SerializeField]
        private Renderer m_FullScreenMask;
        [SerializeField]
        private AnimationCurve m_AnimationCurve;
        private Mesh m_Mesh;
        private List<Vector3> m_Verts = new List<Vector3>();
        private List<Vector2> m_UV = new List<Vector2>();
        private List<int> m_Tris = new List<int>();
        private Coroutine m_FadeInCoroutine;
        private Coroutine m_FadeOutCoroutine;

        private static NativeResolution resolution = new NativeResolution(1920, 1080);

        public static NRTrackingModeChangedTip Create()
        {
            NRTrackingModeChangedTip lostTrackingTip;
            var config = NRSessionManager.Instance.NRSessionBehaviour?.SessionConfig;
            if (config == null || config.TrackingModeChangeTipPrefab == null)
            {
                lostTrackingTip = GameObject.Instantiate(Resources.Load<NRTrackingModeChangedTip>("NRTrackingModeChangedTip"));
            }
            else
            {
                lostTrackingTip = GameObject.Instantiate(config.TrackingModeChangeTipPrefab);
            }
#if !UNITY_EDITOR
            resolution = NRFrame.GetDeviceResolution(NativeDevice.LEFT_DISPLAY);
#endif

            lostTrackingTip.Initialize();
            NRDebugger.Info("[NRTrackingModeChangedTip] Created");
            return lostTrackingTip;
        }

        private void Initialize()
        {
            if (m_Mesh == null)
            {
                m_Mesh = new Mesh();
                m_Mesh.Clear();
                m_Verts.Clear();
                m_UV.Clear();
                m_Tris.Clear();
                m_Verts.Add(new Vector3(-1, -1, 0));
                m_Verts.Add(new Vector3(-1, 1, 0));
                m_Verts.Add(new Vector3(1, 1, 0));
                m_Verts.Add(new Vector3(1, -1, 0));

                m_UV.Add(new Vector2(0, 0));
                m_UV.Add(new Vector2(0, 1));
                m_UV.Add(new Vector2(1, 1));
                m_UV.Add(new Vector2(1, 0));

                m_Tris.Add(0);
                m_Tris.Add(1);
                m_Tris.Add(2);
                m_Tris.Add(2);
                m_Tris.Add(3);
                m_Tris.Add(0);

                m_Mesh.SetVertices(m_Verts);
                m_Mesh.SetUVs(0, m_UV);
                m_Mesh.SetTriangles(m_Tris, 0);
                m_Mesh.UploadMeshData(false);

                m_FullScreenMask.gameObject.GetComponent<MeshFilter>().sharedMesh = m_Mesh;
            }

        }

        public void Show()
        {
            gameObject.SetActive(true);
            m_FullScreenMask.gameObject.SetActive(false);
            if (m_FadeInCoroutine != null)
            {
                NRKernalUpdater.Instance.StopCoroutine(m_FadeInCoroutine);
            }
            if (m_FadeOutCoroutine != null)
            {
                NRKernalUpdater.Instance.StopCoroutine(m_FadeOutCoroutine);
            }

            m_PauseRendererGroup.SetActive(true);
            m_FadeInCoroutine = StartCoroutine(FadeIn());

        }
        public void Hide()
        {
            if (m_FadeInCoroutine != null)
            {
                NRKernalUpdater.Instance.StopCoroutine(m_FadeInCoroutine);
            }
            if (m_FadeOutCoroutine != null)
            {
                NRKernalUpdater.Instance.StopCoroutine(m_FadeOutCoroutine);
            }
            m_FadeOutCoroutine = StartCoroutine(FadeOut());
        }

        public void SetMessage(string msg)
        {
            //m_Lable.text = msg;
        }


        void LateUpdate()
        {
            //避免Mesh被视椎体裁减
            var centerAnchor = NRSessionManager.Instance.CenterCameraAnchor;
            if (centerAnchor != null)
            {
                m_PauseRendererGroup.transform.position = centerAnchor.position + centerAnchor.forward * 3;
                m_PauseRendererGroup.transform.rotation = centerAnchor.rotation;
            }
        }


        void OnDestroy()
        {
            if (m_Mesh != null)
            {
                UnityEngine.Object.Destroy(m_Mesh);
                m_Mesh = null;
            }
        }

        private IEnumerator FadeIn()
        {
            NRSessionManager.Instance.NRSwapChainMan.SetRefreshScreen(false);
            yield return 0;
        }

        private IEnumerator FadeOut()
        {
            m_FullScreenMask.gameObject.SetActive(true);
            m_FullScreenMask.sharedMaterial.SetColor("_Color", new Color(1f, 1f, 1f, 0));
            yield return null;
            yield return null;
            yield return null;
            NRSessionManager.Instance.NRSwapChainMan.SetRefreshScreen(true);

            var FadeOutDuring = 1f;
            var TimeElapse = 0f;
            while (true)
            {
                float percent = TimeElapse / FadeOutDuring;
                percent = m_AnimationCurve.Evaluate(percent);
                m_FullScreenMask.sharedMaterial.SetColor("_Color", new Color(1f, 1f, 1f, percent));
                yield return null;

                TimeElapse += Time.deltaTime;
                if (TimeElapse >= FadeOutDuring)
                {
                    break;
                }
            }
            gameObject.SetActive(false);
        }
    }
}
