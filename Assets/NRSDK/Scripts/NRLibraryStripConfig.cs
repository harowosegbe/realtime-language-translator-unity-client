/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NRKernal
{
    [Serializable]
    public class NRLibraryStripConfig : ScriptableObject
    {
        [SerializeField]
        public bool StripBaseApi = false;
        [SerializeField]
        public bool StripDualAgentTracking = false;

        [SerializeField]
        public bool StripHandTracking = false;

        [SerializeField]
        public bool StripImageTracking = false;
        [SerializeField]
        public bool StripMeshing = false;

        [SerializeField]
        public bool StripSpatialAnchor = false;
    }
}