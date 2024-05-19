# Unity 3D Client for Real-Time Audio Transcription

This repository contains a Unity 3D client that connects to a real-time audio transcription server using the Unity Socket.IO client. The transcribed text is then displayed on XREAL (formerly Nreal) augmented reality glasses using the XREAL SDK.

## Features

- **Real-Time Transcription**: Receives real-time transcriptions from the server.
- **Socket.IO Integration**: Uses Socket.IO for WebSocket communication with the server.
- **AR Display**: Displays transcribed text on XREAL AR glasses using the XREAL SDK.

## Prerequisites

Before you begin, ensure you have met the following requirements:

- **Unity**: Unity 2019.4 or later installed.
- **XREAL SDK**: Download and install the [XREAL SDK](https://xreal.gitbook.io/nrsdk).
- **Socket.IO for Unity**: Import the [Socket.IO for Unity](https://github.com/itisnajim/SocketIOUnity) package into your project.

## Getting Started

### 1. Clone the Repository

```bash
git https://github.com/harowosegbe/realtime-language-translator-unity-client.git
cd realtime-language-translator-unity-client
```

### 2. Open the Project in Unity
- Launch Unity Hub.
- Click on the "Open" button.
- Navigate to the cloned repository folder and select it.

### 3. Import Required Packages

    1. ***Import XREAL SDK***:
        - Download the XREAL SDK from the XREAL developer portal.
        - Import the SDK into your Unity project.

    2. ***Import Socket.IO for Unity***:
        - Download the Socket.IO package from the UnitySocketIO GitHub.
        - Import the package into your Unity project.

### 4. Configure the Socket.IO Client
- Open the /Realtime Language Translator/Script/Manager.cs file.

- Configure the server URL:

```csharp
[SerializeField] private string m_APIBaseURL = "http://your.server.url:3000";
```

### 5. Configure XREAL SDK
- Follow the XREAL SDK documentation to set up your scene for AR display.
- Create a UI element (e.g., TextMeshPro text) to display the transcriptions.
- Ensure the UI element is positioned correctly in the AR space.

### 6. Run the Project
- Connect your XREAL glasses to your development environment.
- Press the "Play" button in Unity to start the application.
- The client should now connect to the server and display real-time transcriptions on the AR glasses. 

## Folder Structure

```bash
realtime-language-translator-unity-client/Assets
├── Realtime Language Translator/
│   ├── Scripts/
│   │   ├── Manager.cs      # Handles Socket.IO communication
│   │   ├── TMPDropDownHelper.cs      # Handles TextMeshPro dropdown for XREAL Canvas
│   │   ├── WavUtility.cs # Manages converting audio clip to byte
│   ├── Scene/
│   │   ├── Main.unity       # Main scene for the application
│   ├── ...
├── ProjectSettings/
├── README.md
└── ...

```
