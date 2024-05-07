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
    using UnityEngine;
    using System.Runtime.InteropServices;

    /// <summary> Callback, called when the nr display resolution. </summary>
    /// <param name="width">  The width.</param>
    /// <param name="height"> The height.</param>
    public delegate void NRDisplayResolutionCallback(int width, int height);

    /// <summary> HMD Eye offset Native API . </summary>
    internal partial class NativeDisplay
    {
        /// <summary> Handle of the multi display. </summary>
        private UInt64 m_DisplayHandle;

        /// <summary> Creates a new bool. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Create(UInt64 displayHandle = 0)
        {
            NRDebugger.Info("[NativeDisplay] Create: displayHandle={0}", displayHandle);
            if (displayHandle == 0)
            {
                NativeResult result = NativeApi.NRDisplayCreate(ref displayHandle);
                NativeErrorListener.Check(result, this, "Create", true);
            }

            NRDebugger.Debug("[NativeDisplay] Created: displayHandle={0}", displayHandle);
            m_DisplayHandle = displayHandle;
            return m_DisplayHandle != 0;
        }

        /// <summary> Starts this object. </summary>
        public void Start()
        {
            NativeResult result = NativeApi.NRDisplayStart(m_DisplayHandle);
            NativeErrorListener.Check(result, this, "Start", true);
        }

        /// <summary> Listen main screen resolution changed. </summary>
        /// <param name="callback"> The callback.</param>
        public void ListenMainScrResolutionChanged(NRDisplayResolutionCallback callback)
        {
            NativeResult result = NativeApi.NRDisplaySetMainDisplayResolutionCallback(m_DisplayHandle, callback);
            NativeErrorListener.Check(result, this, "ListenMainScrResolutionChanged");
        }

        /// <summary> Stops this object. </summary>
        public void Stop()
        {
            NativeResult result = NativeApi.NRDisplayStop(m_DisplayHandle);
            NativeErrorListener.Check(result, this, "Stop");
        }

        /// <summary> Updates the home screen texture described by rendertexture. </summary>
        /// <param name="rendertexture"> The rendertexture.</param>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool UpdateHomeScreenTexture(IntPtr rendertexture)
        {
            NativeResult result = NativeApi.NRDisplaySetMainDisplayTexture(m_DisplayHandle, rendertexture);
            NativeErrorListener.Check(result, this, "UpdateHomeScreenTexture");
            return result == NativeResult.Success;
        }

        /// <summary> Pauses this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Pause()
        {
            NativeResult result = NativeApi.NRDisplayPause(m_DisplayHandle);
            NativeErrorListener.Check(result, this, "Pause");
            return result == NativeResult.Success;
        }

        /// <summary> Resumes this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Resume()
        {
            NativeResult result = NativeApi.NRDisplayResume(m_DisplayHandle);
            NativeErrorListener.Check(result, this, "Resume");
            return result == NativeResult.Success;
        }

        /// <summary> Destroys this object. </summary>
        /// <returns> True if it succeeds, false if it fails. </returns>
        public bool Destroy()
        {
            NativeResult result = NativeApi.NRDisplayDestroy(m_DisplayHandle);
            NativeErrorListener.Check(result, this, "Destroy");
            m_DisplayHandle = 0;
            return result == NativeResult.Success;
        }

        /// <summary> A native api. </summary>
        private struct NativeApi
        {
            /// <summary> Nr display create. </summary>
            /// <param name="out_display_handle"> [in,out] Handle of the out display.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayCreate(ref UInt64 out_display_handle);

            /// <summary> Callback, called when the nr display set main display resolution. </summary>
            /// <param name="display_handle">      The display handle.</param>
            /// <param name="resolution_callback"> The resolution callback.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplaySetMainDisplayResolutionCallback(UInt64 display_handle,
                NRDisplayResolutionCallback resolution_callback);

            /// <summary> Nr display start. </summary>
            /// <param name="display_handle"> The display handle.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayStart(UInt64 display_handle);

            /// <summary> Nr display stop. </summary>
            /// <param name="display_handle"> The display handle.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayStop(UInt64 display_handle);

            /// <summary> Nr display pause. </summary>
            /// <param name="display_handle"> The display handle.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayPause(UInt64 display_handle);

            /// <summary> Nr display resume. </summary>
            /// <param name="display_handle"> The display handle.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayResume(UInt64 display_handle);

            /// <summary> Nr display set main display texture. </summary>
            /// <param name="display_handle">     The display handle.</param>
            /// <param name="controller_texture"> The controller texture.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplaySetMainDisplayTexture(UInt64 display_handle,
                IntPtr controller_texture);

            /// <summary> Nr display destroy. </summary>
            /// <param name="display_handle"> The display handle.</param>
            /// <returns> A NativeResult. </returns>
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRDisplayDestroy(UInt64 display_handle);
        };
    }
}
