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

    public class ThermalLog : AndroidJavaProxy
    {
        public ThermalLog() : base("com.nreal.nrealapp.IThermalLog")
        {
            NRDebugger.Info("[ThermalLog]: new ThermalLog");
        }

        void OnThermalLog(int status)
        {
            ThermalMgr.CurStatus = status;
            NRDebugger.Info("[ThermalLog]: status={0}", status);
        }
    }
}