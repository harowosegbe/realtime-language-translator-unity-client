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
    /// <summary>
    /// Features that the sdk might support.
    /// </summary>
    public enum NRDeviceType
    {
        None = 0,
        XrealLight = 1,
        XrealAir = 2,
        XrealAIR2_PRO = 3,
        XrealAIR2 = 4,
        XrealAIR2_ULTRA = 5,
        Xreal_HONOR_AIR = 1001,
    }

    public enum NRDeviceCategory
    {
        INVALID = 0,
        REALITY = 1,
        VISION = 2,
    }
}
