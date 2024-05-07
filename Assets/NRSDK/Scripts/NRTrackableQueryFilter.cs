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
    /// <summary> A filter for trackable queries. </summary>
    public enum NRTrackableQueryFilter
    {
        /// <summary>
        /// Indicates available trackables.
        /// </summary>
        All,

        /// <summary>
        /// Indicates new trackables detected in the current NRSDK Frame.
        /// </summary>
        New,
    }
}
