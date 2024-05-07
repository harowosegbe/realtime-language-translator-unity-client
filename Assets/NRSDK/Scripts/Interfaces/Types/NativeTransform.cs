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
    using System.Runtime.InteropServices;
    using UnityEngine;

    /// <summary> A native pose. </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeTransform
    {
        [MarshalAs(UnmanagedType.Struct)]
        public NativeVector3f position;
        [MarshalAs(UnmanagedType.Struct)]
        public NativeVector4f rotation;

        /// <summary> Constructor. </summary>
        /// <param name="position"> The position of a pose.</param>
        /// <param name="rotation"> The rotation of a pose.</param>
        internal NativeTransform(Vector3 position, Quaternion rotation)
        {
            this.position = new NativeVector3f(position);
            this.rotation = new NativeVector4f(rotation.x, rotation.y, rotation.z, rotation.w);
        }

        /// <summary> Constructor. </summary>
        /// <param name="pose"> The pose.</param>
        internal NativeTransform(Pose pose)
        {
            this.position = new NativeVector3f(pose.position);
            this.rotation = new NativeVector4f(pose.rotation.x, pose.rotation.y, pose.rotation.z, pose.rotation.w);
        }

        /// <summary> Converts this object to an unity pose. </summary>
        /// <returns> This object as a Pose. </returns>
        internal Pose ToUnityPose()
        {
            return new Pose(position.ToUnityVector3(), 
                new Quaternion
                {
                    x = rotation.X,
                    y = rotation.Y,
                    z = rotation.Z,
                    w = rotation.W
                });
        }

        /// <summary> Convert this object into a string representation. </summary>
        /// <returns> A string that represents this object. </returns>
        public override string ToString()
        {
            return string.Format("position:[{0}], rotation:[{1}]", position.ToString(), rotation.ToString());
        }
    }
}
