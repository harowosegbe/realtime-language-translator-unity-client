/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/                  
* 
*****************************************************************************/

using System.Runtime.InteropServices;

namespace NRKernal
{
    /// <summary> Values that represent nr camera models. </summary>
    public enum NRCameraModel
    {
        /// <summary> An enum constant representing the nr camera model radial option. </summary>
        NR_CAMERA_MODEL_RADIAL = 1,
        /// <summary> An enum constant representing the nr camera model fisheye option. </summary>
        NR_CAMERA_MODEL_FISHEYE = 2,
        /// <summary> An enum constant representing the nr camera model fisheye624 option. </summary>
        NR_CAMERA_MODEL_FISHEYE624 = 3,
    }

    /// <summary>
    ///     if camera_model == NR_CAMERA_MODEL_RADIAL,the first 4 value of distortParams is:
    /// // radial_k1、radial_k2、radial_r1、radial_r2. // else if camera_model ==
    /// NR_CAMERA_MODEL_FISHEYE: // fisheye_k1、fisheye_k2、fisheye_k3、fisheye_k4. </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NRDistortionParams
    {
        /// <summary> The camera model. </summary>
        [MarshalAs(UnmanagedType.I4)]
        public NRCameraModel cameraModel;
        /// <summary> The first distort parameters. </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public float[] distortParams;

        /// <summary> Convert this object into a string representation. </summary>
        /// <returns> A string that represents this object. </returns>
        public override string ToString()
        {
            return string.Format("cameraModel:{0} distortParams0:{1} distortParams1:{2} distortParams2:{3} distortParams3:{4} distortParams4:{5} distortParams5:{6} distortParams6:{7}",
                cameraModel, distortParams[0], distortParams[1], distortParams[2], distortParams[3], distortParams[4], distortParams[5], distortParams[6]);
        }
    }
}
