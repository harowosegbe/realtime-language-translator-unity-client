/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

#if USING_XR_MANAGEMENT && USING_XR_SDK_XREAL
#define USING_XR_SDK
#endif

namespace NRKernal
{
    using System;
    using System.Runtime.InteropServices;
    using AOT;

    internal delegate void OnNativeErrorCallback(NativeResult result, string funcName, bool needthrowerror);
    internal delegate void OnGfxThreadStartCallback(UInt64 renderingHandle);
    internal delegate void OnGfxThreadSubmitCallback(UInt64 frameHandle);
    internal delegate void OnGfxThreadPopulateFrameCallback();
    internal delegate void OnDisplaySubSystemStartCallback(bool start);

    /// <summary> A controller for handling native glasses. </summary>
    internal partial class NativeXRPlugin
    {
#if USING_XR_SDK
        internal static void RegistEventCallback(OnNativeErrorCallback onNativeError, OnGfxThreadStartCallback onStartCallback, OnGfxThreadPopulateFrameCallback onPopulateFrameCallback, OnGfxThreadSubmitCallback onSubmitCallback)
        {
            NativeApi.RegistEventCallback(onNativeError, onStartCallback, onPopulateFrameCallback, onSubmitCallback);
        }

        internal static void RegistDisplaySubSystemEventCallback(OnDisplaySubSystemStartCallback onStartCallback)
        {
            NativeApi.RegistDisplaySubSystemEventCallback(onStartCallback);
        }

        internal static void SetLogLevel(int logLevel)
        {
            NativeApi.SetLogLevel(logLevel);
        }

        internal static void SetMonoMode(bool monoMode)
        {
            NativeApi.SetMonoMode(monoMode);
        }

        internal static void PopulateFrameHandle(UInt64 frameHandle)
        {
            NativeApi.PopulateFrameHandle(frameHandle);
        }

        // multiPassEye: 0-None, 1-LeftEye, 2-RightEye
        internal static IntPtr[] CreateDisplayTextures(int texNum, int texWidth, int texHeight, int texArrayLength, UInt32 multiPassEye = 0)
        {
            if (!NativeApi.CreateDisplayTextures((UInt32)texNum, (UInt32)texWidth, (UInt32)texHeight, (UInt32)texArrayLength, multiPassEye))
            {
                NRDebugger.Error("CreateDisplayTextures failed");
                return null;
            }
            IntPtr[] rst = new IntPtr[texNum];
            for (UInt32 i = 0; i < texNum; i++)
            {
                rst[i] = NativeApi.AcquireCreatedTexture(i, multiPassEye);
            }
            return rst;
        }

        internal static bool PopulateDisplayTexture(IntPtr idTexture, UInt32 multiPassEye = 0)
        {
            return NativeApi.PopulateDisplayTexture((IntPtr)idTexture, multiPassEye);
        }

        internal static LostTrackingReason GetLostTrackingReason()
        {
            return NativeApi.GetLostTrackingReason();
        }

        internal static UInt64 GetFramePresentTime()
        {
            return NativeApi.GetFramePresentTime();
        }

        internal static int GetPerceptionID()
        {
            return NativeApi.GetPerceptionID();
        }

        internal static UInt64 GetPerceptionHandle()
        {
            return NativeApi.GetPerceptionHandle();
        }

        internal static UInt64 GetPerceptionGroupHandle()
        {
            return NativeApi.GetPerceptionGroupHandle();
        }

        internal static void SwitchTrackingType(TrackingType type)
        {
            NativeApi.SwitchTrackingType(type);
        }

        internal static UInt64 GetHeadTrackingHandle()
        {
            return NativeApi.GetHeadTrackingHandle();
        }

        internal static UInt64 GetHMDHandle()
        {
            return NativeApi.GetHMDHandle();
        }

        internal static UInt64 GetMetricsHandle()
        {
            return NativeApi.GetMetricsHandle();
        }

        internal static UInt64 GetDisplayHandle()
        {
            return NativeApi.GetDisplayHandle();
        }

        internal static UInt64 GetRenderHandle()
        {
            return NativeApi.GetRenderHandle();
        }

        internal static void SetTargetFrameRate(int targetFrameRate)
        {
            NativeApi.SetTargetFrameRate(targetFrameRate);
        }
        
        internal static int GetTargetFrameRate()
        {
            return NativeApi.GetTargetFrameRate();
        }
        
        internal static void WakeUpNextFrame()
        {
            NativeApi.WakeUpNextFrame();
        }

        private partial struct NativeApi
        {
            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public extern static void RegistEventCallback(OnNativeErrorCallback onNativeError, OnGfxThreadStartCallback onStartCallback, OnGfxThreadPopulateFrameCallback onPopulateFrameCallback, OnGfxThreadSubmitCallback onSubmitCallback);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public extern static void RegistDisplaySubSystemEventCallback(OnDisplaySubSystemStartCallback onStartCallback);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern void SetLogLevel(int logLevel);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern void SetMonoMode(bool monoMode);


            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public extern static void PopulateFrameHandle(UInt64 frameHandle);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public extern static bool CreateDisplayTextures(UInt32 texNum, UInt32 texWidth, UInt32 texHeight, UInt32 texArrayLength, UInt32 multiPassEye);
            
            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public extern static IntPtr AcquireCreatedTexture(UInt32 texIdx, UInt32 multiPassEye);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern bool PopulateDisplayTexture(IntPtr idTexture, UInt32 multiPassEye);



            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern LostTrackingReason GetLostTrackingReason();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetFramePresentTime();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern int GetPerceptionID();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetPerceptionHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetPerceptionGroupHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern void SwitchTrackingType(TrackingType type);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetHeadTrackingHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetHMDHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetMetricsHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetDisplayHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern UInt64 GetRenderHandle();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern void SetTargetFrameRate(int targetFrameRate);

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern int GetTargetFrameRate();

            [DllImport(NativeConstants.NRNativeXRPlugin, CharSet = CharSet.Auto)]
            public static extern void WakeUpNextFrame();
        }
#endif
    }
}
