using System;
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
    using System.Runtime.InteropServices;
    using UnityEngine;

    public enum NRSwapchainCreateFlags
    {
        NR_SWAPCHAIN_CREATE_FLAGS_PROTECT_TEXTURE = 0x1,
        NR_SWAPCHAIN_CREATE_FLAGS_STATIC_TEXTURE = 0x2,
        NR_SWAPCHAIN_CREATE_FLAGS_OES_TEXTURE = 0x4,
        NR_SWAPCHAIN_CREATE_FLAGS_NONE = 0
    };

    public enum NRTextureFormat
    {
        NR_TEXTURE_FORMAT_COLOR_RGB8 = 0,
        NR_TEXTURE_FORMAT_COLOR_SRGB8 = 1,
        // Equivalent to GL_RGBA8. 
        NR_TEXTURE_FORMAT_COLOR_RGBA8 = 2,
        NR_TEXTURE_FORMAT_COLOR_SRGB8_ALPHA8 = 3,
        // Equivalent to GL_ALPHA.
        NR_TEXTURE_FORMAT_COLOR_A8 = 4,
        NR_TEXTURE_FORMAT_DEPTH_16 = 5,
        NR_TEXTURE_FORMAT_DEPTH_24 = 6,
        NR_TEXTURE_FORMAT_DEPTH_32 = 7,
        NR_TEXTURE_FORMAT_DEPTH_24_STENCIL_8 = 8,
        NR_TEXTURE_FORMAT_DEPTH_32_STENCIL_8 = 9,
    };
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeRectf
    {
        /// <summary> The X coordinate. </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float bottom;
        /// <summary> The Y coordinate. </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float left;
        /// <summary> The Z coordinate. </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float top;
        /// <summary> The width. </summary>
        [MarshalAs(UnmanagedType.R4)]
        public float right;

        public NativeRectf(Rect rect)
        {
            left = rect.xMin;
            bottom = rect.yMin;
            right = rect.xMax;
            top = rect.yMax;
        }

        public NativeRectf(float left, float bottom, float right, float top)
        {
            this.left = left;
            this.bottom = bottom;
            this.right = right;
            this.top = top;
        }

        public override string ToString()
        {
            return string.Format("bottom:{0} left:{1} top:{2} right:{3}", bottom, left, top, right);
        }
    }

    public enum NRExternalSurfaceFlags
    {
        NONE = 0,
        /// Create the underlying BufferQueue in synchronous mode, allowing multiple buffers to be
        /// queued instead of always replacing the last buffer.  Buffers are retired in order, and
        /// the producer may block until a new buffer is available.
        NR_EXTERNAL_SURFACE_FLAG_SYNCHRONOUS = 0x1,

        /// Indicates that the compositor should acquire the most recent buffer whose presentation
        /// timestamp is not greater than the expected display time of the final composited frame.
        /// Together with FLAG_SYNCHRONOUS, this flag is suitable for video surfaces where several
        /// frames can be queued ahead of time.
        NR_EXTERNAL_SURFACE_FLAG_USE_TIMESTAMPS = 0x2,
    }

    public enum NRViewportType
    {
        NR_VIEWPORT_PROJECTION = 0,
        NR_VIEWPORT_QUAD = 1,
    }

    public enum NRCompositionFlags
    {
        NR_COMPOSITION_FLAG_CORRECT_CHROMATIC_ABERRATION = 0x1,
        NR_COMPOSITION_FLAG_USE_TEXTURE_SOURCE_ALPHA = 0x2,
        NR_COMPOSITION_FLAG_UNPREMULTIPLIED_ALPHA = 0x4,
    };

    public enum NRReferenceSpaceType
    {
        //The original position and orientation of global space is inited as tracking system inited 
        //and never changed.
        NR_REFERENCE_SPACE_GLOBAL = 0,
        //The original position and orientation of view space is the same as head pose 
        //and changed as the head move.
        NR_REFERENCE_SPACE_VIEW = 1,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NRApiRequirements
    {
        public UInt32 min_major;
        public UInt32 min_minor;
        public UInt32 min_patch;

        public UInt32 max_major;
        public UInt32 max_minor;
        public UInt32 max_patch;    
    }
}
