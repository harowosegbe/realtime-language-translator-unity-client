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
    /// <summary> Device Tracking State. </summary>
    public enum TrackingState
    {
        /// <summary>
        /// TRACKING means the object is being tracked and its state is valid.
        /// </summary>
        Tracking = 0,

        /// <summary>
        /// PAUSED indicates that NRSDK has paused tracking, 
        /// and the related data is not accurate.  
        /// </summary>
        Paused = 1,

        /// <summary>
        /// STOPPED means that NRSDK has stopped tracking, and will never resume tracking. 
        /// </summary>
        Stopped = 2
    }
}
