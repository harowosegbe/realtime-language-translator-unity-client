using NRKernal;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace NRKernal.NRExamples
{
    public class Toaster : MonoBehaviour
    {
        [SerializeField]
        private Text text;

        private void Awake()
        {
            instance = this;
        }
        private static Toaster instance;
        public static Toaster Instance => instance;


        public static void Toast(string message, int duration = 2000)
        {
            if (instance == null)
            {
                NRDebugger.Error($"No instance found, please create a gameobject with [Toaster] Component in the scene first, message: {message}");
                return;
            }
            Instance.InternalToast(message, duration);
        }

        private CancellationTokenSource toastCancellation;
        private async void InternalToast(string msg, int duration)
        {
            if (toastCancellation != null)
            {
                toastCancellation.Cancel();
                toastCancellation.Dispose();
            }
            toastCancellation = new CancellationTokenSource();

            var headpose = NRFrame.HeadPose;
            this.transform.position = headpose.position + headpose.forward * 3;
            this.transform.rotation = headpose.rotation;
            text.text = msg;

            try
            {
                await Task.Delay(duration, toastCancellation.Token);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            if (!toastCancellation.Token.IsCancellationRequested)
            {
                text.text = string.Empty;
            }
        }

        private void OnDestroy()
        {
            if(toastCancellation != null)
            {
                toastCancellation.Cancel();
            }
        }
    }
}