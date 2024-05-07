using System;
using System.Collections.Generic;
using UnityEngine;

namespace NRKernal.NRExamples
{
    public class HandJointPoseRecorder: MonoBehaviour
    {
        static string k_HandJointPoseDataStoreDir => $"{Application.persistentDataPath}/HandJointPose";

        private void Awake()
        {
            CreateGestureStoreDir();
        }


        public void SaveHandJointsPoseData(string gestureName)
        {
            string fileName = $"{k_HandJointPoseDataStoreDir}/{gestureName}.json";

            var hand = NRInput.Hands.GetHand(HandEnum.RightHand);
            var handState = hand.GetHandState();
            var headPose = NRFrame.HeadPose;
            string json = JsonUtility.ToJson(new HandJointsPoseData(headPose, handState.jointsPoseDict));
            System.IO.File.WriteAllText(fileName, json);            
        }

        private void CreateGestureStoreDir()
        {
            try
            {
                if (!System.IO.Directory.Exists(k_HandJointPoseDataStoreDir))
                {
                    System.IO.Directory.CreateDirectory(k_HandJointPoseDataStoreDir);
                }
            }
            catch (System.Exception e)
            {
                NRDebugger.Error(e);
                throw e;
            }
        }
    }


}
