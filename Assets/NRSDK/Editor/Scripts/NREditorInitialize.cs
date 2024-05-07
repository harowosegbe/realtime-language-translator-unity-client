/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Threading.Tasks;
using System;

namespace NRKernal
{
    [InitializeOnLoad]
    static class NREditorInitialize
    {
        public const string ConfigPath = "Assets/NRLibraryStripConfig.asset";
        static NREditorInitialize()
        {
            if (!Application.isBatchMode)
                DelayCreateLibraryConfigIfNotExist();
        }

        private static async void DelayCreateLibraryConfigIfNotExist()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            if (!File.Exists(ConfigPath))
            {
                var config = ScriptableObject.CreateInstance<NRLibraryStripConfig>();
                AssetDatabase.CreateAsset(config, ConfigPath);
            }
        }
    }
}