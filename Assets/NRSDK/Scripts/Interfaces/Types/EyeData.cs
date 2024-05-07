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
    using UnityEngine;

    /// <summary> Eye pose data. </summary>
    public struct EyePoseData
    {
        /// <summary> Left eye pose. </summary>
        public Pose LEyePose;

        /// <summary> Right eye pose. </summary>
        public Pose REyePose;

        /// <summary> Center eye pose. </summary>
        public Pose CEyePose;

        /// <summary> RGB eye pose. </summary>
        public Pose RGBEyePose;
    }

    /// <summary> Eye project matrix. </summary>
    public struct EyeProjectMatrixData
    {
        /// <summary> Left display project matrix. </summary>
        public Matrix4x4 LEyeMatrix;

        /// <summary> Right display project matrix. </summary>
        public Matrix4x4 REyeMatrix;

        /// <summary> Center display project matrix. </summary>
        public Matrix4x4 CEyeMatrix;

        /// <summary> RGB camera project matrix. </summary>
        public Matrix4x4 RGBEyeMatrix;
    }
}
