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
    using System;
    using System.Collections.Generic;


    /// <summary> A controller provider factory. </summary>
    internal static class ControllerProviderFactory
    {
        private static Dictionary<Type, ControllerProviderBase> m_ControllerProviderDict = new Dictionary<Type, ControllerProviderBase>();

        /// <summary> Type of the android controller provider. </summary>
        public static Type controllerProviderType
        {
            get
            {
#if UNITY_EDITOR
                return typeof(EditorControllerProvider);
#else
                return typeof(NRControllerProvider);
#endif
            }
        }

        /// <summary> Get controller provider. </summary>
        /// <param name="providerType"> Type of the provider.</param>
        /// <returns> The cached controller provider. </returns>
        internal static ControllerProviderBase GetControllerProvider(Type providerType)
        {
            if (providerType != null)
            {
                if (m_ControllerProviderDict.ContainsKey(providerType))
                {
                    return m_ControllerProviderDict[providerType];
                }
            }
            return null;
        }

        /// <summary> Create controller provider. </summary>
        /// <param name="providerType"> Type of the provider.</param>
        /// <param name="states">       The states.</param>
        /// <returns> The new controller provider. </returns>
        internal static ControllerProviderBase CreateControllerProvider(Type providerType, ControllerState[] states)
        {
            if (providerType != null)
            {
                object parserObj = Activator.CreateInstance(providerType, new object[] { states });
                if (parserObj is ControllerProviderBase)
                {
                    var controllerProvider = parserObj as ControllerProviderBase;
                    m_ControllerProviderDict.Add(providerType, controllerProvider);
                    return controllerProvider;
                }
            }
            return null;
        }
    }

}