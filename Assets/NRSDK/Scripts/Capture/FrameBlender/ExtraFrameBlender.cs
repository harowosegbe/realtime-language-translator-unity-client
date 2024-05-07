/****************************************************************************
* Copyright 2019 Xreal Techonology Limited. All rights reserved.
*                                                                                                                                                          
* This file is part of NRSDK.                                                                                                          
*                                                                                                                                                           
* https://www.xreal.com/        
* 
*****************************************************************************/

namespace NRKernal.Record
{
    using UnityEngine;

    /// <summary> A frame blender. </summary>
    public class ExtraFrameBlender : BlenderBase
    {
        /// <summary> Target camera. </summary>
        protected Camera m_TargetCamera;
        /// <summary> The encoder. </summary>
        protected IEncoder m_Encoder;
        /// <summary> The blend material. </summary>
        private Material m_BackGroundMat;
        private NRBackGroundRender m_NRBackGroundRender;
        private NRCameraInitializer m_DeviceParamInitializer;

        private CaptureSide m_CaputreSide;
        private RenderTexture m_BlendTexture;
        private RenderTexture m_BlendTextureLeft;
        private RenderTexture m_BlendTextureRight;
        /// <summary> Gets or sets the blend texture. </summary>
        /// <value> The blend texture. </value>
        public override RenderTexture BlendTexture
        {
            get
            {
                return m_BlendTexture;
            }
        }

        /// <summary> Initializes this object. </summary>
        /// <param name="camera">  The camera.</param>
        /// <param name="encoder"> The encoder.</param>
        /// <param name="param">   The parameter.</param>
        public override void Init(Camera camera, IEncoder encoder, CameraParameters param)
        {
            base.Init(camera, encoder, param);

            Width = param.cameraResolutionWidth;
            Height = param.cameraResolutionHeight;
            m_TargetCamera = camera;
            m_Encoder = encoder;
            BlendMode = param.blendMode;
            m_CaputreSide = param.captureSide;

            m_NRBackGroundRender = m_TargetCamera.gameObject.GetComponent<NRBackGroundRender>();
            if (m_NRBackGroundRender == null)
            {
                m_NRBackGroundRender = m_TargetCamera.gameObject.AddComponent<NRBackGroundRender>();
            }
            m_NRBackGroundRender.enabled = false;
            m_DeviceParamInitializer = camera.gameObject.GetComponent<NRCameraInitializer>();

            m_TargetCamera.enabled = false;
            m_BlendTexture = UnityExtendedUtility.CreateRenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            m_TargetCamera.targetTexture = m_BlendTexture;
            if(m_CaputreSide == CaptureSide.Both)
            {
                m_BlendTextureLeft = UnityExtendedUtility.CreateRenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
                m_BlendTextureRight = UnityExtendedUtility.CreateRenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            }
        }

        /// <summary> Executes the 'frame' action. </summary>
        /// <param name="frame"> The frame.</param>
        public override void OnFrame(UniversalTextureFrame frame)
        {
            base.OnFrame(frame);

            if (!m_DeviceParamInitializer.IsInitialized)
            {
                return;
            }

            if (m_BackGroundMat == null)
            {
                m_BackGroundMat = CreatBlendMaterial(BlendMode, frame.textureType);
                m_NRBackGroundRender.SetMaterial(m_BackGroundMat);
            }

            bool isyuv = frame.textureType == TextureType.YUV;
            const string MainTextureStr = "_MainTex";
            const string UTextureStr = "_UTex";
            const string VTextureStr = "_VTex";

            switch (BlendMode)
            {
                case BlendMode.VirtualOnly:
                    m_NRBackGroundRender.enabled = false;
                    CameraRenderToTarget();
                    break;
                case BlendMode.RGBOnly:
                case BlendMode.Blend:
                case BlendMode.WidescreenBlend:
                    if (isyuv)
                    {
                        m_BackGroundMat.SetTexture(MainTextureStr, frame.textures[0]);
                        m_BackGroundMat.SetTexture(UTextureStr, frame.textures[1]);
                        m_BackGroundMat.SetTexture(VTextureStr, frame.textures[2]);
                    }
                    else
                    {
                        m_BackGroundMat.SetTexture(MainTextureStr, frame.textures[0]);
                    }
                    m_NRBackGroundRender.enabled = true;
                    CameraRenderToTarget();
                    break;
                default:
                    m_NRBackGroundRender.enabled = false;
                    break;
            }

            // Commit frame                
            m_Encoder.Commit(BlendTexture, frame.timeStamp);
            FrameCount++;
        }
        private void CameraRenderToTarget()
        {
            if (m_CaputreSide == CaptureSide.Single)
            {
                m_TargetCamera.targetTexture = m_BlendTexture;
                m_TargetCamera.Render();
            }
            else if (m_CaputreSide == CaptureSide.Both)
            {
                var pos = m_TargetCamera.transform.position;
                var rotation = m_TargetCamera.transform.rotation;
                var mat = m_TargetCamera.projectionMatrix;

                var originLCam = NRSessionManager.Instance.NRHMDPoseTracker.leftCamera;
                var originRCam = NRSessionManager.Instance.NRHMDPoseTracker.rightCamera;

                m_TargetCamera.transform.position = originLCam.transform.position;
                m_TargetCamera.transform.rotation = originLCam.transform.rotation;
                m_TargetCamera.projectionMatrix = originLCam.projectionMatrix;
                m_TargetCamera.targetTexture = m_BlendTextureLeft;
                m_TargetCamera.Render();

                m_TargetCamera.transform.position = originRCam.transform.position;
                m_TargetCamera.transform.rotation = originRCam.transform.rotation;
                m_TargetCamera.projectionMatrix = originRCam.projectionMatrix;
                m_TargetCamera.targetTexture = m_BlendTextureRight;
                m_TargetCamera.Render();

                m_TargetCamera.transform.position = pos;
                m_TargetCamera.transform.rotation = rotation;
                m_TargetCamera.projectionMatrix = mat;

                MergeRenderTextures(m_BlendTextureLeft, m_BlendTextureRight, m_BlendTexture);
            }
        }
        private void MergeRenderTextures(Texture leftSrc, Texture rightSrc, RenderTexture target)
        {
            //Set the RTT in order to render to it
            Graphics.SetRenderTarget(target);

            //Setup 2D matrix in range 0..1, so nobody needs to care about sized
            GL.LoadPixelMatrix(0, 1, 1, 0);

            //Then clear & draw the texture to fill the entire RTT.
            GL.Clear(true, true, new Color(0, 0, 0, 0));

            Graphics.DrawTexture(new Rect(0, 0, 0.5f, 1.0f), leftSrc);
            Graphics.DrawTexture(new Rect(0.5f, 0, 0.5f, 1.0f), rightSrc);
        }
        private Material CreatBlendMaterial(BlendMode mode, TextureType texturetype)
        {
            string shader_name;
            shader_name = "Record/Shaders/NRBackground{0}";
            shader_name = string.Format(shader_name, texturetype == TextureType.RGB ? "" : "YUV");
            return new Material(Resources.Load<Shader>(shader_name));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged
        /// resources. </summary>
        public override void Dispose()
        {
            base.Dispose();

            m_BlendTexture?.Release();
            m_BlendTexture = null;
            m_BlendTextureLeft?.Release();
            m_BlendTextureLeft = null;
            m_BlendTextureRight?.Release();
            m_BlendTextureRight = null;
        }
    }
}
