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

    public interface ITrackingDataProvider : ILifecycle
    {
        bool GetHeadPose(ref Pose pose, UInt64 timestamp);

        ulong GetHMDTimeNanos();

        bool GetFramePresentHeadPose(ref Pose pose, ref LostTrackingReason lostReason, ref UInt64 timeStamp);

        bool GetFramePresentTimeByCount(uint count, ref UInt64 timestamp);

        bool InitTrackingType(TrackingType type);

        bool SwitchTrackingType(TrackingType type);

        void Recenter();
    }
}
