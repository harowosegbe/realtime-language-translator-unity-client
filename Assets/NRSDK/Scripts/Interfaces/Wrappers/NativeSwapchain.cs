/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* NRSDK is distributed in the hope that it will be useful                                                              
*                                                                                                                                                           
* https://www.xreal.com/         
* 
*****************************************************************************/

namespace NRKernal
{
    using System;
    using UnityEngine;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine.Assertions;

    /// <summary> Swapchain Native API. </summary>
    internal partial class NativeSwapchain
    {
        private UInt64 m_RenderHandler;
        private UInt64 RenderHandler
        {
            get { return m_RenderHandler; }
        }

        public NativeSwapchain(UInt64 renderHandler)
        {
            m_RenderHandler = renderHandler;
        }

        #region swap chain
        internal int GetRecommandBufferCount(UInt64 swapchainHandler)
        {
            int buffer_count = 0;
            NativeApi.NRSwapchainGetRecommendBufferCount(RenderHandler, swapchainHandler, ref buffer_count);
            return buffer_count;
        }

        internal UInt64 CreateSwapchain(UInt64 bufferspecHandler)
        {
            UInt64 swapchainHandler = 0;
            var result = NativeApi.NRSwapchainCreateEx(RenderHandler, bufferspecHandler, ref swapchainHandler);
            Assert.IsTrue(result == NativeResult.Success);

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[NativeSwapchain] CreateSwapchain: RenderHandler={0}, bufferspechandler={1}, swapchainHandler={2}", RenderHandler, bufferspecHandler, swapchainHandler);
            return swapchainHandler;
        }

        internal UInt64 CreateSwapchainAndroidSurface(UInt64 bufferspecHandler, ref IntPtr androidSurface)
        {
            UInt64 swapchainHandler = 0;
            var result = NativeApi.NRSwapchainCreateAndroidSurface(RenderHandler, bufferspecHandler, ref swapchainHandler, ref androidSurface);
            Assert.IsTrue(result == NativeResult.Success);
            //NRDebugger.Info("[NativeSwapchain] CreateSwapchainAndroidSurface: RenderHandler={0}, bufferspechandler={1}, swapchainHandler={2}, surface={3}", RenderHandler, bufferspechandler, swapchainHandler,);
            return swapchainHandler;
        }

        internal void UpdateExternalSurface(UInt64 swapchainHandle, int transformCount, NativeTransform[] transforms, NativeDevice[] targetEyes,
            Int64 timestamp, int frameIndex)
        {
            //NRDebugger.Info("[NativeSwapchain] UpdateExternalSurface");
            NativeApi.NRSwapchainUpdateExternalSurface(RenderHandler, swapchainHandle, transformCount, transforms, targetEyes, timestamp, frameIndex);
        }

        internal void SetSwapChainBuffers(UInt64 swapchainHandle, IntPtr[] bufferHandler)
        {
            StringBuilder st = new StringBuilder();
            for (int i = 0; i < bufferHandler.Length; i++)
            {
                st.Append(bufferHandler[i] + " ");
            }

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[NativeSwapchain] SetSwapChainBuffers: swapChain={0}, ids=[{1}]", swapchainHandle, st.ToString());
            NativeApi.NRSwapchainSetBuffers(RenderHandler, swapchainHandle, bufferHandler.Length, bufferHandler);
        }

        internal void DestroySwapChain(UInt64 swapchainHandler)
        {
            //NRDebugger.Info("[NativeSwapchain] DestroySwapChain: {0}.", swapchainHandler);
            var result = NativeApi.NRSwapchainDestroy(RenderHandler, swapchainHandler);
            Assert.IsTrue(result == NativeResult.Success);
        }
        #endregion

        #region buffer spec
        internal UInt64 CreateBufferSpec(BufferSpec spec)
        {
            UInt64 bufferSpecsHandler = 0;
            //NRDebugger.Info("[NativeSwapchain] CreateBufferSpec.");
            NativeApi.NRBufferSpecCreate(RenderHandler, ref bufferSpecsHandler);
            NativeApi.NRBufferSpecSetSize(RenderHandler, bufferSpecsHandler, spec.size);
            NativeApi.NRBufferSpecSetTextureFormat(RenderHandler, bufferSpecsHandler, spec.colorFormat);
            NativeApi.NRBufferSpecSetSamples(RenderHandler, bufferSpecsHandler, spec.samples);
            try
            {
                NativeApi.NRBufferSpecSetExternalSurfaceFlag(RenderHandler, bufferSpecsHandler, spec.surfaceFlag);
                NativeApi.NRBufferSpecSetExternalSurfaceHolder(RenderHandler, bufferSpecsHandler, 2);
            }
            catch (Exception ex)
            {
                NRDebugger.Warning("[NativeSwapchain] CreateBufferSpec: {0}", ex.Message);
            }

            NativeApi.NRBufferSpecSetCreateFlags(RenderHandler, bufferSpecsHandler, spec.createFlag);
            if (spec.useTextureArray)
                NativeApi.NRBufferSpecSetMultiviewLayers(RenderHandler, bufferSpecsHandler, 2);
            return bufferSpecsHandler;
        }

        internal void DestroyBufferSpec(UInt64 bufferspechandler)
        {
            //NRDebugger.Info("[NativeSwapchain] DestroyBufferSpec.");
            NativeApi.NRBufferSpecDestroy(RenderHandler, bufferspechandler);
        }
        #endregion

        #region view port
        public UInt64 CreateBufferViewport()
        {
            UInt64 viewportHandler = 0;
            NativeResult result = NativeApi.NRBufferViewportCreate(RenderHandler, ref viewportHandler);
            NativeErrorListener.Check(result, this, "CreateBufferViewport");

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[NativeSwapchain] CreateBufferViewport: RenderHandler={0}, viewportHandler={1}.", RenderHandler, viewportHandler);
            return viewportHandler;
        }

        internal void PopulateBufferViewportData(UInt64 viewportHandler, UInt64 swapchainHandler,
            ref NativeRectf sourceUV, NativeDevice targetDisplay,
            NRViewportType viewportType, NRReferenceSpaceType spaceType, ref NativeTransform nativePose, Vector2 quadSize, int perceptionId)
        {
            if (viewportHandler == 0)
                return;

            // NRDebugger.Info("[NativeSwapchain] PopulateBufferViewportData: RenderHandler={0}, viewportHandler={1}, swapchainHandle={2}.", RenderHandler, viewportHandler, swapchainHandler);
            NativeApi.NRBufferViewportSetSourceUV(RenderHandler, viewportHandler, ref sourceUV);
            NativeApi.NRBufferViewportSetTargetComponent(RenderHandler, viewportHandler, targetDisplay);
            NativeApi.NRBufferViewportSetSwapchain(RenderHandler, viewportHandler, swapchainHandler);
            NativeApi.NRBufferViewportSetType(RenderHandler, viewportHandler, viewportType);
            // NativeApi.NRBufferViewportSetFlags(RenderHandler, viewportHandler, NRCompositionFlags);

            // NRDebugger.Info("[NativeSwapchain] PopulateBufferViewportData quadSize={0}\n{1}", nativePose.ToString(), quadSize.ToString("F4"), nativePose.ToString());
            NativeApi.NRBufferViewportSetTransform(RenderHandler, viewportHandler, spaceType, ref nativePose);

            NativeApi.NRBufferViewportSetQuadSize(RenderHandler, viewportHandler, quadSize.x, quadSize.y);
            NativeApi.NRBufferViewportSetPerceptionId(RenderHandler, viewportHandler, perceptionId);
        }

        internal void Populate3DBufferViewportData(UInt64 viewportHandler, UInt64 swapchainHandler,
            ref NativeRectf sourceUV, NativeDevice targetDisplay, bool isExternalSurface,
            NRViewportType viewportType, NRReferenceSpaceType spaceType, ref NativeTransform nativePose, NativeFov4f fov, int textureArraySlice, int perceptionId)
        {
            if (viewportHandler == 0)
                return;

            // NRDebugger.Info("[NativeSwapchain] Populate3DBufferViewportData: RenderHandler={0}, viewportHandler={1}, swapchainHandle={2}.", RenderHandler, viewportHandler, swapchainHandler);
            NativeApi.NRBufferViewportSetSourceUV(RenderHandler, viewportHandler, ref sourceUV);
            NativeApi.NRBufferViewportSetTargetComponent(RenderHandler, viewportHandler, targetDisplay);
            NativeApi.NRBufferViewportSetSwapchain(RenderHandler, viewportHandler, swapchainHandler);
            NativeApi.NRBufferViewportSetType(RenderHandler, viewportHandler, viewportType);
            // NativeApi.NRBufferViewportSetFlags(RenderHandler, viewportHandler, NRCompositionFlags);

            // NRDebugger.Info("[NativeSwapchain] Populate3DBufferViewportData nativePose:{0}", nativePose.ToString());
            if (!isExternalSurface)
                NativeApi.NRBufferViewportSetTransform(RenderHandler, viewportHandler, spaceType, ref nativePose);

            NativeApi.NRBufferViewportSetSourceFov(RenderHandler, viewportHandler, ref fov);
            if (textureArraySlice >= 0)
                NativeApi.NRBufferViewportSetMultiviewLayer(RenderHandler, viewportHandler, textureArraySlice);
            NativeApi.NRBufferViewportSetPerceptionId(RenderHandler, viewportHandler, perceptionId);
        }

        internal void SetBufferViewportFocusPlane(UInt64 viewportHandler, NRReferenceSpaceType spaceType, Vector3 focusPoint, Vector3 focusPlaneNorm)
        {
            if (viewportHandler == 0)
                return;

            var planePoint = new NativeVector3f(focusPoint);
            var planeNorm = new NativeVector3f(focusPlaneNorm);
            // NRDebugger.Info("SetBufferViewportFocusPlane: type={0}, point={1}, norm={2}", spaceType, focusPoint.ToString("F4"), focusPlaneNorm.ToString("F4"));
            NativeApi.NRBufferViewportSetFocusPlane(RenderHandler, viewportHandler, spaceType, ref planePoint, ref planeNorm);
        }

        internal void SetBufferViewPort(UInt64 frameHandler, int viewportIndex, UInt64 viewportHandler)
        {
            if (viewportHandler == 0)
                return;

            // NRDebugger.Info("[NativeSwapchain] SetBufferViewPort: frameHandler={0}, viewportIndex={1}, viewportHandle={2}", frameHandler, viewportIndex, viewportHandler);
            NativeApi.NRFrameSetBufferViewport(RenderHandler, frameHandler, viewportIndex, viewportHandler);
        }

        internal void DestroyBufferViewPort(UInt64 viewportHandle)
        {
            if (viewportHandle == 0)
                return;

            NativeApi.NRBufferViewportDestroy(RenderHandler, viewportHandle);
        }

        internal int GetBufferViewportCount(UInt64 frameHandler)
        {
            int length = 0;
            NativeApi.NRFrameGetViewportCount(RenderHandler, frameHandler, ref length);
            return length;
        }
        #endregion

        #region frame
        /// <returns> Camera frames. </returns>
        internal void AcquireFrame(ref UInt64 frameHandler, UInt64[] swapchainHandlers, IntPtr[] renderbuffers)
        {
            NativeApi.NRRenderingAcquireFrame(RenderHandler, ref frameHandler);

            if (swapchainHandlers.Length > 0)
            {
                if (NRDebugger.logLevel <= LogLevel.Debug)
                    NRDebugger.Info("[NativeSwapchain] AcquireBuffers: num={0}, idx0={1}, idx1={2}, RenderHandler={3}, frameHandler={4}",
                        swapchainHandlers.Length, swapchainHandlers[0], (swapchainHandlers.Length > 1) ? swapchainHandlers[1] : swapchainHandlers[0], RenderHandler, frameHandler);

                NativeApi.NRFrameAcquireBuffers(RenderHandler, frameHandler, swapchainHandlers, swapchainHandlers.Length, renderbuffers);

                if (NRDebugger.logLevel <= LogLevel.Debug)
                    NRDebugger.Info("[NativeSwapchain] AcquireBuffers: cameraTex0={0} cameraTex1={1}", renderbuffers[0], (renderbuffers.Length > 1) ? renderbuffers[1] : renderbuffers[0]);
            }
        }

        internal void SubmitFrame(UInt64 frameHandle)
        {
            if (frameHandle == 0)
            {
                return;
            }

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("SubmitFrame: frameHandle={0}, curPresentTime={1}", frameHandle, NRFrame.CurrentPoseTimeStamp);
            var result = NativeApi.NRFrameSubmit(RenderHandler, frameHandle);
            UInt64 displayTime = 0, displayPrd = 0;
            NativeApi.NRRenderingWaitFrame(RenderHandler, ref displayTime, ref displayPrd);
            //NRDebugger.Info("SubmitFrame after wait: {0}", NRFrame.CurrentPoseTimeStamp);
            NativeErrorListener.Check(result, this, "SubmitFrame");
        }

        public void PopulateFrameInfo(UInt64 frameHandler, UInt64 presentTime)
        {
            if (RenderHandler == 0)
                return;

            if (NRDebugger.logLevel <= LogLevel.Debug)
                NRDebugger.Info("[NativeSwapchain] PopulateFrameInfo: frameHandler={0}, presentTime={1}", frameHandler, presentTime);

            NativeApi.NRFrameSetPresentTime(RenderHandler, frameHandler, presentTime);
        }

        public void SetRefreshScreen(bool refresh)
        {
            if (RenderHandler == 0)
                return;
            NativeApi.NRRenderingSetRefreshScreen(RenderHandler, refresh);
        }

        #endregion

        private struct NativeApi
        {
            #region buffer spec
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecCreate(UInt64 rendering_handle, ref UInt64 out_spec_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecDestroy(UInt64 rendering_handle, UInt64 spec_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecGetSize(UInt64 rendering_handle, UInt64 spec_handle, ref NativeResolution out_buffer_spec_size);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecSetSize(UInt64 rendering_handle, UInt64 spec_handle, NativeResolution buffer_spec_size);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecGetSamples(UInt64 rendering_handle, UInt64 spec_handle, ref int out_num_samples);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecSetSamples(UInt64 rendering_handle, UInt64 spec_handle, int num_samples);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecSetTextureFormat(UInt64 rendering_handle, UInt64 spec_handle, NRTextureFormat color_format);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecSetMultiviewLayers(UInt64 rendering_handle, UInt64 spec_handle, int num_layers);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecSetExternalSurfaceFlag(UInt64 rendering_handle, UInt64 spec_handle, int surface_flag);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecSetExternalSurfaceHolder(UInt64 rendering_handle, UInt64 spec_handle, int surface_holder_type);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecSetCreateFlags(UInt64 rendering_handle, UInt64 spec_handle, UInt64 create_flags);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferSpecSetUsageFlags(UInt64 rendering_handle, UInt64 spec_handle, UInt64 usage_flags);
            #endregion

            #region swap chain

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRSwapchainCreateEx(UInt64 rendering_handle, UInt64 spec_handle, ref UInt64 out_swapchain_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRSwapchainDestroy(UInt64 rendering_handle, UInt64 swapchain_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRSwapchainCreateAndroidSurface(UInt64 rendering_handle, UInt64 spec_handle, ref UInt64 out_swapchain_handle, ref IntPtr out_surface);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRSwapchainGetRecommendBufferCount(UInt64 rendering_handle, UInt64 swapchain_handle, ref int buffer_count);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRSwapchainSetBuffers(UInt64 rendering_handle, UInt64 swapchain_handle, int buffer_count, IntPtr[] color_buffers);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingAcquireFrame(UInt64 rendering_handle, ref UInt64 out_frame_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingSetRefreshScreen(UInt64 rendering_handle, bool refresh);
            #endregion

            #region view port
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportCreate(UInt64 rendering_handle, ref UInt64 out_viewport_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportDestroy(UInt64 rendering_handle, UInt64 viewport_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportGetSourceUV(UInt64 rendering_handle, UInt64 viewport_handle, ref NativeRectf out_uv_rect);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetSourceUV(UInt64 rendering_handle, UInt64 viewport_handle, ref NativeRectf uv_rect);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportGetTransform(UInt64 rendering_handle, UInt64 viewport_handle, ref NRReferenceSpaceType out_reference_space, ref NativeTransform out_transform);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetTransform(UInt64 rendering_handle, UInt64 viewport_handle, NRReferenceSpaceType reference_space, ref NativeTransform transform);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportGetTargetComponent(UInt64 rendering_handle, UInt64 viewport_handle, ref NativeDevice out_component);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetTargetComponent(UInt64 rendering_handle, UInt64 viewport_handle, NativeDevice component);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportGetSwapchain(UInt64 rendering_handle, UInt64 viewport_handle, ref UInt64 out_swapchain_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetSwapchain(UInt64 rendering_handle, UInt64 viewport_handle, UInt64 swapchain_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetType(UInt64 rendering_handle, UInt64 viewport_handle, NRViewportType viewport_type);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetFlags(UInt64 rendering_handle, UInt64 viewport_handle, UInt64 flags);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetFocusPlane(UInt64 rendering_handle, UInt64 viewport_handle, NRReferenceSpaceType reference_space, ref NativeVector3f plane_point, ref NativeVector3f plane_normal);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetQuadSize(UInt64 rendering_handle, UInt64 viewport_handle, float quad_size_w, float quad_size_h);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportGetSourceFov(UInt64 rendering_handle, UInt64 viewport_handle, ref NativeFov4f out_fov);
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetSourceFov(UInt64 rendering_handle, UInt64 viewport_handle, ref NativeFov4f fov);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportGetMultiviewLayer(UInt64 rendering_handle, UInt64 viewport_handle, ref int out_texture_layer_index);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetMultiviewLayer(UInt64 rendering_handle, UInt64 viewport_handle, int texture_layer_index);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRBufferViewportSetPerceptionId(UInt64 rendering_handle, UInt64 viewport_handle, int perception_id);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRSwapchainUpdateExternalSurface(UInt64 rendering_handle, UInt64 swapchain_handle, int transform_count, NativeTransform[] transforms, NativeDevice[] transform_eyes, Int64 present_time, int transform_index);
            #endregion

            #region NRFrame
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRFrameAcquireBuffers(UInt64 rendering_handle, UInt64 frame_handle, UInt64[] swapchain_handles, int swapchian_count, IntPtr[] out_render_buffers);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRFrameSetPresentTime(UInt64 rendering_handle, UInt64 frame_handle, UInt64 present_time);

            // fence
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRFrameSubmit(UInt64 rendering_handle, UInt64 frame_handle);

            //for thread sync
            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRRenderingWaitFrame(UInt64 rendering_handle, ref UInt64 out_display_time, ref UInt64 out_dispaly_period);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRFrameGetViewportCount(UInt64 rendering_handle, UInt64 frame_handle, ref int out_list_count);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRFrameGetBufferViewport(UInt64 rendering_handle, UInt64 frame_handle, int viewport_index, ref UInt64 out_viewport_handle);

            [DllImport(NativeConstants.NRNativeLibrary)]
            public static extern NativeResult NRFrameSetBufferViewport(UInt64 rendering_handle, UInt64 frame_handle, int viewport_index, UInt64 viewport_handle);

            #endregion
        };
    }
}
