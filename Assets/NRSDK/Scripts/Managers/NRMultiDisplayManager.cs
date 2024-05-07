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

    /// <summary> Manager for multi-displays. </summary>
    [HelpURL("https://developer.xreal.com/develop/unity/customize-phone-controller")]
    [ScriptOrder(NativeConstants.NRVIRTUALDISPLAY_ORDER)]
    public class NRMultiDisplayManager : SingletonBehaviour<NRMultiDisplayManager>
    {
        /// <summary> The default virtual displayer. </summary>
        [SerializeField] GameObject m_DefaultVirtualDisplayer;
        private NRVirtualDisplayer m_VirtualDisplayer;

        new void Awake()
        {
            base.Awake();
            if (isDirty) return;
            m_VirtualDisplayer = FindObjectOfType<NRVirtualDisplayer>();
            // Use the customise virtualdisplay if find one.
            if (m_VirtualDisplayer != null)
            {
                return;
            }

            Debug.Log("[NRMultiDisplayManager] Awake: Create NRVirtualDisplayer.");
            // Use the default virtual display if can not find one.
#if UNITY_EDITOR
            var inst = Instantiate(m_DefaultVirtualDisplayer);
            m_VirtualDisplayer = inst.GetComponent<NRVirtualDisplayer>();
#else
            var virtualDisplayer = new GameObject("NRVirtualDisplayer").AddComponent<NRVirtualDisplayer>();
            GameObject.DontDestroyOnLoad(virtualDisplayer.gameObject);
#endif
        }

        new void OnDestroy()
        {
            if (isDirty) return;
            base.OnDestroy();
            Debug.Log("[NRMultiDisplayManager] OnDestroy: Destroy NRVirtualDisplayer.");
            if (m_VirtualDisplayer != null)
            {
                GameObject.DestroyImmediate(m_VirtualDisplayer.gameObject);
            }
        }
    }
}
