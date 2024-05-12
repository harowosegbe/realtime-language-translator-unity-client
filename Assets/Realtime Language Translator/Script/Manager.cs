
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NRKernal;
using NRKernal.Record;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using UnityEngine.Events;



public class Manager : MonoBehaviour
{

    NRVideoCapture m_VideoCapture = null;
    NRAudioCapture m_AudioCapture = null;

    [SerializeField] private RawImage m_Preview = null;
    [SerializeField] private TextMeshProUGUI m_DisplayText = null;
    [SerializeField] private AudioSource m_Audio = null;
    AudioClip m_CurrentAudio = null;

    [SerializeField] private List<string> m_Translations = new List<string>();
    [SerializeField] private int m_CurrentSendIndex = 0;

    [SerializeField] private string m_APIBaseURL = "";

    public string VideoSavePath
    {
        get
        {
            string timeStamp = Time.time.ToString().Replace(".", "").Replace(":", "");
            string filename = string.Format("Xreal_Record_{0}.mp4", timeStamp);
            return Path.Combine(Application.persistentDataPath, filename);
        }
    }

    public class TranslationResponse
    {
        public string error;
        public string success;
        public string translation;
    }
 
    // Start is called before the first frame update
    void Start()
    {
        StartRecording();
        UpdateTextDisplay();
        // NRInput.AddClickListener(ControllerHandEnum.)
    }

    void CreateVideoCapture()
    {
        NRVideoCapture.CreateAsync(false,  (NRVideoCapture videoCapture)=>{
            m_VideoCapture = videoCapture;
        });

        m_AudioCapture = NRAudioCapture.Create();
        m_AudioCapture.OnAudioData += OnAudioDataReceived;
    }

    void StartRecording(){
        if (m_VideoCapture == null){
            CreateVideoCapture();
        }

        CameraParameters cameraParameters = new()
        {
            cameraResolutionHeight = 200,
            cameraResolutionWidth = 200,
            hologramOpacity = 0.0f,
            frameRate = 24,
            blendMode = BlendMode.Blend,
            // Set audio state, audio record needs the permission of "android.permission.RECORD_AUDIO",
            // Add it to your "AndroidManifest.xml" file in "Assets/Plugin".
            audioState = NRVideoCapture.AudioState.MicAudio,
            camMode = CamMode.None
        };
        
        StartCoroutine(AudioInput(10));
        // m_VideoCapture.StartVideoModeAsync(cameraParameters, OnStartedVideoCaptureMode);
        // m_AudioCapture.StartAudioModeAsync(cameraParameters, OnEndedAudioCaptureMode);
    }

    private void OnEndedAudioCaptureMode(NRAudioCapture.AudioCaptureResult result)
    {
        // throw new NotImplementedException();
        if (!result.success)
        {
            // NRDebugger.Info("Started Audio Capture Mode faild!");
            return;
        }

        m_AudioCapture.StartRecordingAsync(VideoSavePath, onAudioStarted );
    
    }

    private void onAudioStarted(NRAudioCapture.AudioCaptureResult result)
    {
        
    }

    void OnStartedVideoCaptureMode(NRVideoCapture.VideoCaptureResult result)
    {
        if (!result.success)
        {
            // NRDebugger.Info("Started Audio Capture Mode faild!");
            return;
        }

        // m_VideoCapture.GetContext().StartCapture();

        StartCoroutine(AudioInput(5));        
    }

    private void UpdateTextDisplay (){

        m_DisplayText.text = "";

        foreach (var item in m_Translations)
        {
            m_DisplayText.text += item + " ";
        }

    }

    IEnumerator AudioInput(int loopTime = 10){

        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        //button to trigger listening
        
        m_CurrentAudio = Microphone.Start(null, false, loopTime, 44100);

        //filtering

        yield return new WaitForSeconds(10);

        // Stop recording
        Microphone.End(null);

        // Convert audio clip to WAV format
        byte[] audioData = WavUtility.FromAudioClip(m_CurrentAudio);

        // async 
        //send to API
        // StartCoroutine(SendAudioData(m_CurrentAudio));
        StartCoroutine(SendAudioData(audioData, (text)=>{

            TranslationResponse response = JsonUtility.FromJson<TranslationResponse>(text);

            if (!String.IsNullOrEmpty(response.translation))
            {
                m_Translations.Add(response.translation);
                UpdateTextDisplay();
            }
            
        }));

        m_CurrentSendIndex++;
        // StartCoroutine(AudioInput(5));
    }

    IEnumerator SendAudioData(byte[] audioData, UnityAction<string> callback)
    {

        // Encode audio data as base64 string
        string base64Audio = System.Convert.ToBase64String(audioData);

        // Create JSON object with audio data
        string json = "{\"audio\": \"" + base64Audio + "\"}";

        // Replace with your server URL
        string serverURL = m_APIBaseURL + "/v1/translation";

        using (UnityWebRequest www = UnityWebRequest.Put(serverURL, Encoding.UTF8.GetBytes(json)))
        {
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "application/json");

            // Send request and wait for response
            yield return www.SendWebRequest();

            // Check for errors
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error sending audio data: " + www.error);
            }
            else
            {
                // Parse response from server (if any)
                string responseText = www.downloadHandler.text;
                Debug.Log("Server response: " + responseText);
                callback.Invoke(responseText);
            }
        }
    }



    private void OnAudioDataReceived(IntPtr data, uint size)
    {
        // throw new NotImplementedException();
        print("Data Recieved");
    }

}
