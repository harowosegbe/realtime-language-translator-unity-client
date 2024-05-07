
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
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ThermalMgr
    {
        private static int _curStatus;
        public static int CurStatus
        {
            set
            {
                _curStatus = value;
            }
            get
            {
                return _curStatus;
            }
        }

        static private bool m_Inited = false;
        public static void Init()
        {
            if (m_Inited)
                return;

            NRDebugger.Info("[ThermalLog]: ThermalMgr Init");
            var powerMgr = new AndroidJavaObject("com.nreal.nrealapp.PowerMgr");
    #if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var unityActivity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            powerMgr.Call("init", unityActivity, new ThermalLog());
    #endif
            m_Inited = true;
        }
    }
}