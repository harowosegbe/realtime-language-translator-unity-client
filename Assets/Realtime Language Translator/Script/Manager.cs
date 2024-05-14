
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
    NRAudioCapture m_AudioCapture = null;
    NRVideoCapture m_VideoCapture = null;

    [Header("UI Objects")]
    [SerializeField] private TextMeshProUGUI m_DisplayText = null;
    [SerializeField] private TMP_Dropdown m_FromLangDropdown = null;
    [SerializeField] private TMP_Dropdown m_ToLangDropdown = null;
    [SerializeField] private Button m_StartButton = null;

    [Header("Audio")]
    [SerializeField] private AudioSource m_Audio = null;
    AudioClip m_CurrentAudio = null;
    [SerializeField] private List<string> m_SupportedLanguages = new();

    [SerializeField] private List<string> m_Translations = new();
    [SerializeField] private int m_CurrentSendIndex = 0;

    [SerializeField] private string m_APIBaseURL = "";
    [SerializeField] private int m_ListenDuration = 5;
    [SerializeField] private bool m_SystemListenState = false;

    public class TranslationResponse
    {
        public string error;
        public string success;
        public string translation;
    }

    [System.Serializable]
    public class AudioTranslateData
    {
        public string audio;
        public string fromLang;
        public string toLang;
    }

    string m_DefaultMicDevice = null;
 
    // Start is called before the first frame update
    void Start()
    {
        // StartRecording();
        UpdateUIDisplays();

        ControllerType controllerType = NRInput.GetControllerType();
        ControllerHandEnum domainHand = NRInput.DomainHand;

        if (m_StartButton)
        {
            m_StartButton.onClick.AddListener(OnStartButtonClicked);
        }

        m_FromLangDropdown.options.Clear();
        m_ToLangDropdown.options.Clear();

        foreach (var item in m_SupportedLanguages)
        {
            m_FromLangDropdown.options.Add(
                new TMP_Dropdown.OptionData(item)
            );

            m_ToLangDropdown.options.Add(
                new TMP_Dropdown.OptionData(item)
            );
        }

        m_FromLangDropdown.value = 1;

        // m_DefaultMicDevice = Microphone.devices[0];
        
    }

    private void OnStartButtonClicked()
    {
        m_SystemListenState = !m_SystemListenState;

        if (m_SystemListenState)
        {
            StartRecording();
        }
        else{
            StopRecording();
        }

        UpdateUIDisplays();

    }

    void CreateAudioCapture()
    {
        NRVideoCapture.CreateAsync(false, (video)=>{
            m_VideoCapture = video;
        });

        m_AudioCapture = NRAudioCapture.Create();
        m_AudioCapture.OnAudioData += OnAudioDataReceived;
    }

    void StartRecording(){
        if (m_AudioCapture == null || m_VideoCapture == null)
        {
            CreateAudioCapture();
        }

        StartCoroutine(AudioInput(m_ListenDuration));
    }

    void StopRecording(){

        Microphone.End(m_DefaultMicDevice);
        StopAllCoroutines();
        EndAndSendAudioData();
        
    }

    private void UpdateUIDisplays (){

        m_DisplayText.text = "";

        var latestTranslations = m_Translations.TakeLast(5);

        foreach (var item in latestTranslations)
        {
            m_DisplayText.text += item + " ";
        }

        var btnText = m_StartButton.GetComponentInChildren<TextMeshProUGUI>();

        if (!btnText)
        {
            return;
        }

        if (m_SystemListenState)
        {
            btnText.text = "Stop Listening";
            m_StartButton.image.color = Color.red;
        }
        else{
            btnText.text = "Start Listening";
            m_StartButton.image.color = Color.green;
        }

    }

    IEnumerator AudioInput(int loopTime = 10){

        if (!Microphone.IsRecording(m_DefaultMicDevice))
        {
            m_CurrentAudio = Microphone.Start(m_DefaultMicDevice, true, loopTime, 44100);
        }
        
        while (Microphone.IsRecording(m_DefaultMicDevice))
        {
            yield return new WaitForSeconds(loopTime); // Wait for loopTime seconds
            EndAndSendAudioData();
        }

    }

    private void EndAndSendAudioData(){

        int translationIndex = m_CurrentSendIndex;

        // Convert audio clip to WAV format
        byte[] audioData = WavUtility.FromAudioClip(m_CurrentAudio);

        //Send Audio to API
        StartCoroutine(SendAudioData(audioData, (text)=>{
            HandleResponseText(text, translationIndex);
        }));
    }

    private void HandleResponseText (string text, int index){
        TranslationResponse response = JsonUtility.FromJson<TranslationResponse>(text);

        if (!String.IsNullOrEmpty(response.translation))
        {
            // m_Translations.Insert(index, response.translation);
            m_Translations.Add(response.translation);
            UpdateUIDisplays();
        }
    }

    IEnumerator SendAudioData(byte[] audioData, UnityAction<string> callback)
    {

        // Encode audio data as base64 string
        string base64Audio = System.Convert.ToBase64String(audioData);
        
        AudioTranslateData audioDataJson = new (){
            audio = base64Audio,
            fromLang = m_FromLangDropdown.options[m_FromLangDropdown.value].text,
            toLang = m_ToLangDropdown.options[m_ToLangDropdown.value].text
        };

        string json = JsonUtility.ToJson(audioDataJson);

        // Create JSON object with audio data
        // string json = "{\"audio\": \"" + base64Audio + "\", \"lang\": \"" + m_FromLangDropdown.value + "\"}";

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
