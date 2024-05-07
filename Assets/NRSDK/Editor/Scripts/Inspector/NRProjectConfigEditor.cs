/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/
using UnityEngine;
using UnityEditor;
using NRKernal;

[CustomEditor(typeof(NRProjectConfig))]
public class NRProjectConfigEditor : Editor
{
	override public void OnInspectorGUI()
	{
		NRProjectConfig projectConfig = (NRProjectConfig)target;
		DrawUniqueProjectConfig(projectConfig);
		EditorGUILayout.Space();
	}

	public static void DrawUniqueProjectConfig(NRProjectConfig projectConfig)
    {
		//Target Devices properties
		EditorGUILayout.LabelField("Target Devices", EditorStyles.boldLabel);
		EditorGUILayout.BeginVertical(EditorStyles.helpBox);
		bool modify = false;
		foreach (NRDeviceCategory deviceCategory in System.Enum.GetValues(typeof(NRDeviceCategory)))
		{
			if (deviceCategory == NRDeviceCategory.INVALID)
				continue;
			
			bool curSupport = projectConfig.targetDevices.Contains(deviceCategory);
			bool newSupport = curSupport;
			NREditorUtility.BoolField(projectConfig, ObjectNames.NicifyVariableName(deviceCategory.ToString()), ref newSupport, ref modify);

			if (newSupport && !curSupport)
			{
				projectConfig.targetDevices.Add(deviceCategory);
			}
			else if (curSupport && !newSupport)
			{
				projectConfig.targetDevices.Remove(deviceCategory);
			}
		}
		
		EditorGUILayout.Space();
		NREditorUtility.BoolField(projectConfig, "supportMultiResume", ref projectConfig.supportMultiResume, ref modify);
		
		
		EditorGUILayout.EndVertical();

		if (modify)
		{
			NRProjectConfigHelper.CommitProjectConfig(projectConfig);
		}
	}
}