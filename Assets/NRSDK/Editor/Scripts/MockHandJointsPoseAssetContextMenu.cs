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
    using UnityEditor;
    using UnityEngine;

    public class MockHandJointsPoseAssetContextMenu
    {
        [MenuItem("Assets/Create/NRSDK/MockHandJointsPoseAsset", false, 3)]
        public static void CreateMockHandJointsPoseAsset()
        {
            var asset = ScriptableObject.CreateInstance<MockHandJointsPoseAsset>();
            AssetDatabase.CreateAsset(asset, $"Assets/NRSDK/Resources/MockHandData/MockHandJointsPoseAsset.asset");
        }

    }
}
