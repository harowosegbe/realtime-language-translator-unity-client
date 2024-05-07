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
    /// <summary> The reason of HMD untracked. </summary>
    public enum LostTrackingReason
    {
        /// <summary> Pre initializing. </summary>
        PRE_INITIALIZING = -1,

        /// <summary> None. </summary>
        NONE = 0,

        /// <summary> Initializing. </summary>
        INITIALIZING = 1,

        /// <summary> Move too fast. </summary>
        EXCESSIVE_MOTION = 2,

        /// <summary> Feature point deficiency. </summary>
        INSUFFICIENT_FEATURES = 3,

        /// <summary> Reposition. </summary>
        RELOCALIZING = 4,

        /// <summary> Reposition. </summary>
        ENTER_VRMODE = 5,
    }
}
