﻿/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal.Record
{
    /// <summary>
    /// Record video from the MR world. You can record a RGB only, Virtual only or Blended image
    /// through this class. </summary>
    public class NRRecordBehaviour : CaptureBehaviourBase
    {
        /// <summary> Sets out put path. </summary>
        /// <param name="path"> Full pathname of the file.</param>
        public void SetOutPutPath(string path)
        {
            var encoder = this.GetContext().GetEncoder();
            ((VideoEncoder)encoder).EncodeConfig.SetOutPutPath(path);
        }
    }
}
