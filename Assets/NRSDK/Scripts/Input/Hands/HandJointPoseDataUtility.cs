namespace NRKernal
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class HandJointPoseDataUtility
    {


        private static MockHandJointsPoseAsset s_HandDataAsset;
        public static MockHandJointsPoseAsset HandDataAsset
        {
            get
            {
                if (s_HandDataAsset == null)
                {
                    s_HandDataAsset = Resources.Load<MockHandJointsPoseAsset>(MockHandJointsPoseAsset.AssetPath);
                }
                if (s_HandDataAsset == null)
                {
                    NRDebugger.Error($"[{nameof(HandJointPoseDataUtility)}] No HandJointsPoseAsset Found!");
                }
                return s_HandDataAsset;
            }
        }

        /// <summary>
        /// Convert json string to the provided hand joint pose dictionary
        /// </summary>
        /// <param name="jsonStr"></param>
        /// <param name="dict"></param>
        public static void JsonToDict(HandGesture handGesture, Dictionary<HandJointID, Pose> dict)
        {
            if (dict == null)
                return;

            if (HandDataAsset.TryGetHandDataJson(handGesture, out HandJointsPoseData jointsArrayData))
            {
                jointsArrayData.WriteDictionary(dict);
            }
        }
    }

    [Serializable]
    public class HandJointsPoseData
    {
        public List<SingleHandJointInfo> jointInfoArray;
        public Pose headPose;

        public HandJointsPoseData(Pose headPose, Dictionary<HandJointID, Pose> jointDict)
        {
            this.headPose = headPose;
            jointInfoArray = new List<SingleHandJointInfo>();
            foreach (var kv in jointDict)
            {
                var pose = kv.Value;
                var poseFromHead = new Pose(pose.position - headPose.position, Quaternion.Inverse(headPose.rotation) * pose.rotation);
                jointInfoArray.Add(new SingleHandJointInfo(kv.Key, poseFromHead));
            }
        }

        public void WriteDictionary(Dictionary<HandJointID, Pose> jointPoseDict)
        {
            jointPoseDict.Clear();
            for (int i = 0; i < jointInfoArray.Count; ++i)
            {
                var item = jointInfoArray[i];
                jointPoseDict.Add(item.jointID, item.jointPose);
            }
        }
    }

    [Serializable]
    public class SingleHandJointInfo
    {
        public HandJointID jointID;
        public Pose jointPose;

        public SingleHandJointInfo(HandJointID jointID, Pose jointPose)
        {
            this.jointID = jointID;
            this.jointPose = jointPose;
        }
    }
}