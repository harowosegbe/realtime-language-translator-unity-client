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
    /// <summary> Holds information about Xreal SDK version info. </summary>
    public class NRVersionInfo
    {
        private static readonly string sUnityPackageVersion = "20240226104956";

        /// <summary> Gets the version. </summary>
        /// <returns> The version. </returns>
        public static string GetVersion()
        {
            return NativeAPI.GetVersion();
        }

        public static string GetNRSDKPackageVersion()
        {
            return sUnityPackageVersion;
        }
    }
}
