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
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class NRProjectConfig : ScriptableObject
    {
        public List<NRDeviceCategory> targetDevices = new List<NRDeviceCategory>
        {
            NRDeviceCategory.REALITY,
            NRDeviceCategory.VISION,
        };

        /// <summary> whether to support to run in multi-resume mode. </summary>
        public bool supportMultiResume = true;

        public string GetTargetDeviceTypesDesc()
        {
            string devices = string.Empty;
            foreach (var device in targetDevices)
            {
                if (devices != string.Empty)
                    devices += "|";
                if (device == NRDeviceCategory.REALITY)
                    devices += $"{(int)device}|NrealLight";
                else if (device == NRDeviceCategory.VISION)
                    devices += $"{(int)device}|NrealAir";
            }
            return devices;
        }
    }
}
