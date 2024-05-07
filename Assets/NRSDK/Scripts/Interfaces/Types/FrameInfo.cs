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


    public enum NRFrameFlags
    {
        NR_FRAME_CHANGED_FOCUS_PLANE = 1 << 0,
        NR_FRAME_CHANGED_NONE = 0
    };

    public enum NRTextureType
    {
        NONE = -1,
        NR_TEXTURE_2D = 0,
        NR_TEXTURE_2D_ARRAY,
        NR_TEXTURE_TYPE_NUM
    };

    public enum FrameRateMode
    {
        NR_FPS_30 = 1,
        NR_UNLIMITED = 2
    };

    public struct SwitchModeFrameInfo
    {
        [MarshalAs(UnmanagedType.Bool)]
        public bool flag;
        [MarshalAs(UnmanagedType.SysInt)]
        public IntPtr renderTexture;
    }

    public class FrameInfo
    {
        public NativeMat4f headPose;
        public NativeVector3f focusPosition;
        public NativeVector3f normalPosition;
        ///Time for the frame to present on screen
        public UInt64 presentTime;
        /// Bitfield representing NRFrameChanged fields changed last frame.  Combination of #NRFrameChanged.
        public NRFrameFlags changeFlag;
        public NRTextureType textureType;
        // local cache for frameHandle
        public UInt64 frameHandle;

        public void Set(NativeMat4f p, Vector3 focuspos, Vector3 normal, UInt64 timestamp, NRFrameFlags flag, NRTextureType texturetype, UInt64 frameHandle)
        {
            this.headPose = p;
            this.focusPosition = new NativeVector3f(focuspos);
            this.normalPosition = new NativeVector3f(normal);
            this.presentTime = timestamp;
            this.changeFlag = flag;
            this.textureType = texturetype;
            this.frameHandle = frameHandle;
        }

        public override string ToString()
        {
            return string.Format("headPose:{0}, presentTime:{1} changeFlag:{2}, frameHandle:{3}",
                headPose, presentTime, changeFlag, frameHandle);
        }
    }
}
