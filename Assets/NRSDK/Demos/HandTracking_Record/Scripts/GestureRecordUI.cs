using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace NRKernal.NRExamples
{
    public class GestureRecordUI: MonoBehaviour
    {
        [SerializeField]
        Button m_RecordButton;
        [SerializeField]
        Dropdown m_GestureNameDropdown;
        [SerializeField]
        HandJointPoseRecorder m_HandJointPoseRecorder;

        private void Start()
        {
            m_RecordButton.onClick.AddListener(OnRecordButtonClick);
        }

        public string CurrentGestureName
        {
            get
            {
                int index = m_GestureNameDropdown.value;
                string gestureName = m_GestureNameDropdown.options[index].text;
                return gestureName;
            }
        }
        private void OnRecordButtonClick()
        {
            Toaster.Toast("Start Recording");
            RecordGesture();
        }

        private async Task RecordGesture()
        {
            m_RecordButton.enabled = false;

            var options = m_GestureNameDropdown.options;
            for(int i = 0; i< options.Count; ++i)
            {
                m_GestureNameDropdown.value = i;
                Toaster.Toast("6");
                await Task.Delay(1000);
                Toaster.Toast("5");
                await Task.Delay(1000);
                Toaster.Toast("4");
                await Task.Delay(1000);
                Toaster.Toast("3");
                await Task.Delay(1000);
                Toaster.Toast("2");
                await Task.Delay(1000);
                Toaster.Toast("1");
                await Task.Delay(1000);
                Toaster.Toast("Record");
                m_HandJointPoseRecorder.SaveHandJointsPoseData(CurrentGestureName);
                await Task.Delay(1000);
            }
            Toaster.Toast("Done!");
            m_RecordButton.enabled = true;
        }
    }
}
