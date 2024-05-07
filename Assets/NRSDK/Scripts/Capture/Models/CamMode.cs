/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal.Record
{
    /// <summary> The camera mode of capture. </summary>
    public enum CamMode
    {
        /// <summary>
        /// Resource is not in use.
        /// </summary>
        None = 0,

        /// <summary>
        /// Resource is in Photo Mode.
        /// </summary>
        PhotoMode = 1,

        /// <summary>
        /// Resource is in Video Mode.
        /// </summary>
        VideoMode = 2
    }
    /// <summary> The side of capture. </summary>
    public enum CaptureSide
    {
        Single = 0,
        Both = 1,
    }
    
}
