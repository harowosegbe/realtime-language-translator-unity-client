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
    using System.Runtime.InteropServices;

    public partial class NativeAPI
    {
        private static UInt64 m_ApiHandler;
        public static UInt64 ApiHandler
        {
            get { return m_ApiHandler; }
        }

#if UNITY_ANDROID
        /// <summary> Create NRAPI on android platform. </summary>
        internal static void Create(IntPtr unityActivity)
        {
            if (m_ApiHandler != 0)
                return;

            UInt64 apiHandler = 0;
            NativeResult result = NativeApi.NRAPICreate(unityActivity, ref apiHandler);
            NativeErrorListener.Check(result, m_ApiHandler, "NRAPICreate", true);
            m_ApiHandler = apiHandler;

        }
#else
        /// <summary> Create NRAPI on none-android platform. </summary>
        internal static void Create()
        {
            if (m_ApiHandler != 0)
                return;

            UInt64 apiHandler = 0;
            NativeApi.NRAPICreate(ref apiHandler);
            m_ApiHandler = apiHandler;
        }
#endif

        /// <summary> Get the version information of SDK. </summary>
        /// <returns> The version. </returns>
        internal static string GetVersion()
        {
            NRVersion version = new NRVersion();
            NativeApi.NRGetVersion(m_ApiHandler, ref version);
            return version.ToString();
        }

        /// <summary> Start NRAPI. </summary>
        internal static void Start()
        {
            if (m_ApiHandler != 0)
            {
                NativeApi.NRAPIStart(m_ApiHandler);
            }
        }

        /// <summary> Stop NRAPI. </summary>
        internal static void Stop()
        {
            if (m_ApiHandler != 0)
            {
                NativeApi.NRAPIStop(m_ApiHandler);
            }
        }

        /// <summary> Destroy NRAPI. </summary>
        internal static void Destroy()
        {
            if (m_ApiHandler != 0)
            {
                NativeApi.NRAPIDestroy(m_ApiHandler);
                m_ApiHandler = 0;
            }
        }

        private partial struct NativeApi
        {
#if UNITY_ANDROID
            /// <summary> Create API object. </summary>
            [DllImport(NativeConstants.NRNativeLibrary)]
            internal static extern NativeResult NRAPICreate(IntPtr android_activity, ref UInt64 out_api_handle);
#else
            [DllImport(NativeConstants.NRNativeLibrary)]
            internal static extern NativeResult NRAPICreate(ref UInt64 out_api_handle);
#endif

            /// <summary> Get the version information of SDK. </summary>
            /// <param name="out_version"> [in,out] The version information as NRVersion.</param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            internal static extern NativeResult NRGetVersion(UInt64 api_handle, ref NRVersion out_version);

            /// <summary> Start API object. </summary>
            /// <param name="api_handle"> The Handle of API object.</param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            internal static extern NativeResult NRAPIStart(UInt64 api_handle);

            /// <summary> Stop API ojbect. </summary>
            /// <param name="api_handle"> The Handle of API object.</param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            internal static extern NativeResult NRAPIStop(UInt64 api_handle);

            /// <summary>Release memory used by the API object. </summary>
            /// <param name="api_handle"> The Handle of API object.</param>
            /// <returns> The result of operation. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            internal static extern NativeResult NRAPIDestroy(UInt64 api_handle);
        };
    }
}
