namespace NRKernal
{
    using UnityEngine;

    /// <summary> The FPS counter. </summary>
    public class FPSCounter : MonoBehaviour
    {
        /// <summary> The range of time, in seconds, to collect frames for fps calculation. </summary>
        public float timeRange = 3;

        /// <summary> Gets or sets the average FPS. </summary>
        /// <value> The average FPS. </value>
        public int AverageFPS { get; private set; }
        /// <summary> Gets or sets the highest FPS. </summary>
        /// <value> The highest FPS. </value>
        public int HighestFPS { get; private set; }
        /// <summary> Gets or sets the lowest FPS. </summary>
        /// <value> The lowest FPS. </value>
        public int LowestFPS { get; private set; }

        int frameCounter = 0;
        float lastTimeStamp = 0;
        float maxFrameTime = 0;
        float minFrameTime = 1000;

        /// <summary> Updates this object. </summary>
        void Update()
        {
            ProfileCurFrame();
            CaculateFPS();
        }

        /// <summary> Profile current frame. </summary>
        void ProfileCurFrame()
        {
            var curFrameTime = Time.unscaledDeltaTime;
            if (curFrameTime > maxFrameTime)
                maxFrameTime = curFrameTime;
            if (curFrameTime < minFrameTime)
                minFrameTime = curFrameTime;


            frameCounter++;
        }

        /// <summary> Caculate the fps. </summary>
        void CaculateFPS()
        {
            var curTime = Time.realtimeSinceStartup;
            if (curTime - lastTimeStamp > timeRange && frameCounter > 0)
            {
                AverageFPS = (int)(frameCounter / (curTime - lastTimeStamp));
                HighestFPS = (int)(1 / minFrameTime);
                LowestFPS = (int)(1 / maxFrameTime);

                lastTimeStamp = curTime;
                // NRDebugger.Info("AverageFPS: {0}", AverageFPS);
                Reset();
            }

        }

        /// <summary> Reset. </summary>
        private void Reset()
        {
            frameCounter = 0;
            maxFrameTime = 0;
            minFrameTime = 1000;
        }
    }
}