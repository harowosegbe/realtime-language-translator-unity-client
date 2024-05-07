/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/
namespace NRKernal.Persistence
{
    public enum MappingState
    {
        MAPPING_STATE_UNKNOWN = 0,
        MAPPING_STATE_REQUEST = 1,
        MAPPING_STATE_MAPPING = 2,
        MAPPING_STATE_SUCCESS = 4,
        MAPPING_STATE_NEW_FAILURE = 5,
        MAPPING_STATE_REMAP_FAILURE = 6
    }

}
