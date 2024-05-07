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

    /// <summary> Ammend the order of script. </summary>
    public class ScriptOrder : Attribute
    {
        /// <summary> The order. </summary>
        public int order;

        /// <summary> Constructor. </summary>
        /// <param name="order"> The order.</param>
        public ScriptOrder(int order)
        {
            this.order = order;
        }
    }
}
