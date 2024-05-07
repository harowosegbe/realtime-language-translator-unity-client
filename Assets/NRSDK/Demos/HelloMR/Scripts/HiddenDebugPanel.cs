﻿/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;

namespace NRKernal.NRExamples
{
    /// <summary> Panel for editing the hidden debug. </summary>
    public class HiddenDebugPanel : MonoBehaviour
    {
        /// <summary> The buttons root. </summary>
        public Transform m_ButtonsRoot;
        /// <summary> The buttons. </summary>
        private UserDefineButton[] Buttons;

        /// <summary> Starts this object. </summary>
        void Start()
        {
            Buttons = gameObject.GetComponentsInChildren<UserDefineButton>(true);
            m_ButtonsRoot.gameObject.SetActive(false);

            foreach (var item in Buttons)
            {
                item.OnClick += OnItemTriggerEvent;
            }
        }

        /// <summary> Executes the 'item trigger event' action. </summary>
        /// <param name="key"> The key.</param>
        private void OnItemTriggerEvent(string key)
        {
            if (key.Equals("InvisibleBtn"))
            {
                m_ButtonsRoot.gameObject.SetActive(!m_ButtonsRoot.gameObject.activeInHierarchy);
            }
            else if (CanSceneLoaded(key))
            {
                SceneManager.LoadScene(key);
            }
        }

        /// <summary> Determine if we can scene loaded. </summary>
        /// <param name="name"> The name.</param>
        /// <returns> True if we can scene loaded, false if not. </returns>
        private bool CanSceneLoaded(string name)
        {
            return (SceneUtility.GetBuildIndexByScenePath(name) != -1) &&
                !SceneManager.GetActiveScene().name.Equals(name);
        }
    }
}
