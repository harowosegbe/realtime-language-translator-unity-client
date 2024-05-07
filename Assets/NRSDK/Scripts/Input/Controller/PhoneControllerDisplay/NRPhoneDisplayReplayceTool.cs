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

    public class NRPhoneDisplayReplayceTool : MonoBehaviour
    {
        public virtual NRPhoneScreenProviderBase CreatePhoneScreenProvider()
        {
            return new NRDefaultPhoneScreenProvider();
        }
    }
}
