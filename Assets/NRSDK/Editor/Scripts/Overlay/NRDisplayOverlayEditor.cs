/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal.Experimental
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(NRDisplayOverlay))]
    public class NRDisplayOverlayEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            NRDisplayOverlay overlay = (NRDisplayOverlay)target;
            if (overlay == null)
            {
                return;
            }

            overlay.targetDisplay = (NativeDevice)EditorGUILayout.EnumPopup(new GUIContent("Target Display", "Which display this overlay should render to."), overlay.targetDisplay);
        }
    }
}
