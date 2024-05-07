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
    using System.Collections.Generic;
    using UnityEngine;
    public class MockHandJointsPoseAsset : ScriptableObject
    {
        [SerializeField]
        List<GestureNameJointsPoseDataPair> jointsPoseDataPairs;

        internal static string AssetPath = $"MockHandData/MockHandJointsPoseAsset";



        public bool TryGetHandDataJson(HandGesture gestureName, out HandJointsPoseData jointsArrayData)
        {
            for(int i = 0; i < jointsPoseDataPairs.Count; i++)
            {
                if (jointsPoseDataPairs[i].GestureName == gestureName)
                {
                    string jsonData = jointsPoseDataPairs[i].JointsPoseFile.text;
                    jointsArrayData = JsonUtility.FromJson<HandJointsPoseData>(jsonData);
                    return true;
                }
            }
            jointsArrayData = null;
            return false;
        }
    }

    [System.Serializable]
    public class GestureNameJointsPoseDataPair
    {
        public HandGesture GestureName;
        public TextAsset JointsPoseFile;
    }
}
