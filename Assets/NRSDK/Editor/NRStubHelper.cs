﻿/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

using UnityEngine;
using UnityEditor;
using System;

public class NRStubHelper : ScriptableObject
{
	// Stub helper class to locate XReal Utilities Path through Unity Editor API.
	public static bool IsInsideUnityPackage()
	{
		var so = ScriptableObject.CreateInstance(typeof(NRStubHelper));
		var script = MonoScript.FromScriptableObject(so);
		string assetPath = AssetDatabase.GetAssetPath(script);
		if (assetPath.StartsWith("Packages\\", StringComparison.InvariantCultureIgnoreCase) ||
			assetPath.StartsWith("Packages/", StringComparison.InvariantCultureIgnoreCase))
			return true;
		return false;
	}
}
