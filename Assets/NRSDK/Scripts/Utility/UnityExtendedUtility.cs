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
    using System;
    using System.IO;

    public class UnityExtendedUtility
    {
        public static RenderTexture CreateRenderTexture(int width, int height, int depth = 24, RenderTextureFormat format = RenderTextureFormat.ARGB32, bool usequaAnti = true)
        {
            // Fixed UNITY_2018_2 editor preview effect for video capture and photo capture.
#if UNITY_2018_2 && UNITY_EDITOR
            var rt = new RenderTexture(width, height, depth, format, NRFrame.isLinearColorSpace ? RenderTextureReadWrite.Linear : RenderTextureReadWrite.Default);
#else
            var rt = new RenderTexture(width, height, depth, format, NRFrame.isLinearColorSpace ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Default);
#endif
            rt.wrapMode = TextureWrapMode.Clamp;
            if (QualitySettings.antiAliasing > 0 && usequaAnti)
            {
                rt.antiAliasing = QualitySettings.antiAliasing;
            }
            else
            {
                rt.antiAliasing = 1;
            }

            rt.Create();
            return rt;
        }

        public static bool SaveTextureAsPNG(RenderTexture renderTexture)
        {
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            
            return SaveTextureAsPNG(texture);
        }

        public static bool SaveTextureAsPNG(Texture2D texture)
        {
            try
            {
                string filename = string.Format("Xreal_Shot_{0}.png", NRTools.GetTimeStamp().ToString());
                string path = string.Format("{0}/XrealShots", Application.persistentDataPath);
                string filePath = string.Format("{0}/{1}", path, filename);

                byte[] bytes = texture.EncodeToPNG();
                NRDebugger.Info("SaveTextureAsPNG: {0}Kb was saved to [{1}]",  bytes.Length / 1024, filePath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                File.WriteAllBytes(string.Format("{0}/{1}", path, filename), bytes);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
