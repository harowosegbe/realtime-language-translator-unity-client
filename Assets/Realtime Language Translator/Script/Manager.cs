
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
using SocketIOClient;


public class Manager : MonoBehaviour
{
    NRAudioCapture m_AudioCapture = null;
    NRVideoCapture m_VideoCapture = null;

    [Header("UI Objects")]
    [SerializeField] private TextMeshProUGUI m_DisplayText = null;
    [SerializeField] private TextMeshProUGUI m_Microphone = null;
    [SerializeField] private TMP_Dropdown m_FromLangDropdown = null;
    [SerializeField] private TMP_Dropdown m_ToLangDropdown = null;
    [SerializeField] private Button m_StartButton = null;

    [Header("Audio")]
    [SerializeField] private AudioSource m_Audio = null;
    AudioClip m_CurrentAudio = null;
    [SerializeField] private List<string> m_SupportedLanguages = new();

    [SerializeField] private List<string> m_Translations = new();
    [SerializeField] private int m_CurrentSendIndex = 0;
    private const int sampleRate = 16000;


    [SerializeField] private string m_APIBaseURL = "";
    [SerializeField] private int m_ListenDuration = 5;
    [SerializeField] private int m_ChirpListenDuration = 5;
    [SerializeField] private int m_MicLoopDuration = 5;
    [SerializeField] private bool m_SystemListenState = false;
    [SerializeField] private string m_AuthToken = "6OVhqV-?0l1f11T&+OJ9:0:2WR";
    [SerializeField] public SocketIOUnity socket;


    public class TranslationResponse
    {
        public string error;
        public string success;
        public string translation;
    }

    [System.Serializable]
    public class AudioTranslateData
    {
        public byte[] audio;
        public string fromLang;
        public string toLang;
        public string authToken;
    }

    string m_DefaultMicDevice = null;
    string m_TranslatedString = "";
 
    // Start is called before the first frame update
    void Start()
    {
        UpdateUIDisplays();

        ControllerType controllerType = NRInput.GetControllerType();
        ControllerHandEnum domainHand = NRInput.DomainHand;

        if (m_StartButton)
        {
            m_StartButton.onClick.AddListener(OnStartButtonClicked);
        }

        //Here we clear the language dropdown at start
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

        m_FromLangDropdown.value = 0;

        //Initialise the Microphone
        m_DefaultMicDevice = Microphone.devices[0];
        m_Microphone.text = "Mic: " + m_DefaultMicDevice;

        //Socket io Connect
        var uri = new Uri(m_APIBaseURL);       
        socket = new SocketIOUnity(uri);

        socket.OnConnected += OnSocketConnected;
        socket.On("error", OnSocketError);

        socket.On("transcription", response =>
        {
            string text = response.GetValue<string>();
            m_TranslatedString = text;
        });

        socket.Connect();

        //dropdown change
        m_FromLangDropdown.onValueChanged.AddListener(OnDropDownChange);
        m_ToLangDropdown.onValueChanged.AddListener(OnDropDownChange);

        //Start Display Update
        StartCoroutine(HandleResponseText());
        
    }

    // When the socket is connected we set the initial client configuration
    private void OnSocketConnected(object sender, EventArgs e)
    {
        UpdateLanguageConfig();
    }

    //If there is a socket error we reconnect again
    private void OnSocketError(SocketIOResponse response)
    {
        Debug.LogError("Socket error");
        UpdateLanguageConfig();
    }

    //When the language dropdown is changed we update the client on the websocket
    private void OnDropDownChange(int arg0)
    {
        UpdateLanguageConfig();

        if (arg0 == 1 || arg0 == 2 || arg0 == 3)
        {
            m_MicLoopDuration = m_ChirpListenDuration;
        }
        else{
            m_MicLoopDuration = 1;
        }
    }
    
    // We update the client configuration by emiting a socket event
    private void UpdateLanguageConfig(){

        if (socket.Connected)
        {
            AudioTranslateData audioDataJson = new (){
                fromLang = m_FromLangDropdown.options[m_FromLangDropdown.value].text,
                toLang = m_ToLangDropdown.options[m_ToLangDropdown.value].text,
                authToken = m_AuthToken
            };

            string json = JsonUtility.ToJson(audioDataJson);

            socket.EmitStringAsJSONAsync("config", json);
        }
    }

    //Function called when Start button is clicked
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

    // Function to create audio capture request
    void CreateAudioCapture()
    {
        NRVideoCapture.CreateAsync(false, (video)=>{
            m_VideoCapture = video;
        });

        m_AudioCapture = NRAudioCapture.Create();
    }
    
    // Function called to start recording, if audio or video capture is not present we create a new one and request permission
    void StartRecording(){
        if (m_AudioCapture == null || m_VideoCapture == null)
        {
            CreateAudioCapture();
        }

        StartCoroutine(AudioInput());
    }

    // Function called to stop recording
    void StopRecording(){

        Microphone.End(m_DefaultMicDevice);
        StopAllCoroutines();
        EndAndSendAudioData();
        
    }

    // Function to refresh UI displays
    private void UpdateUIDisplays (){

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

    // Function to start microphone capture
    IEnumerator AudioInput(){

        if (!Microphone.IsRecording(m_DefaultMicDevice))
        {
            m_CurrentAudio = Microphone.Start(m_DefaultMicDevice, true, m_MicLoopDuration, sampleRate);
        }
        
        while (Microphone.IsRecording(m_DefaultMicDevice))
        {
            EndAndSendAudioData();
            yield return new WaitForSeconds(m_MicLoopDuration); // Wait for loopTime seconds

        }

    }

    // Function to clip current microphone data and send request to the API server
    private void EndAndSendAudioData(){

        int translationIndex = m_CurrentSendIndex;

        // Convert audio clip to WAV format
        byte[] audioData = WavUtility.FromAudioClip(m_CurrentAudio);
        
        //Send Audio to Socket
        if (socket.Connected)
        {
            socket.EmitAsync("audio", audioData);
        }

        //Send Audio to API
        // StartCoroutine(SendAudioData(audioData, (text)=>{
        //     HandleResponseText(text, translationIndex);
        // }));
    }

    // Function to handle API response and update AR text display
    IEnumerator HandleResponseText (){

        while (true)
        {
            m_DisplayText.text = m_TranslatedString;
            yield return new WaitForSeconds(0.25f);
        }
    }

    // On application quite we dispose the socket
    void OnApplicationQuit()
    {
        socket.Dispose();
    }

}
