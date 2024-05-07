﻿/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal
{
    /// <summary> Trackable plane type. </summary>
    public enum TrackablePlaneType
    {
        /// <summary>
        /// HORIZONTAL trackable plane.
        /// </summary>
        HORIZONTAL = 0,

        /// <summary>
        /// VERTICAL trackable plane.
        /// </summary>
        VERTICAL = 1,

        /// <summary>
        /// INVALID trackable plane.
        /// </summary>
        INVALID = 2
    }

    /// <summary> Trackable plane's finding mode. </summary>
    public enum TrackablePlaneFindingMode
    {
        /// <summary>
        /// Disable plane detection.
        /// </summary>
        DISABLE = 0,

        /// <summary>
        ///  Enable plane detection.
        /// </summary>
        HORIZONTAL = 1,

        /// <summary>
        ///  Enable plane detection.
        /// </summary>
        VERTICAL = 2,

        /// <summary>
        ///  Enable plane detection.
        /// </summary>
        BOTH = 3,
    }
}
