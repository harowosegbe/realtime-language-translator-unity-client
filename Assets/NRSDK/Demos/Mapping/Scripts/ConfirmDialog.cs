using NRKernal;
using NRKernal.NRExamples;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class ConfirmDialog : MonoBehaviour
{
    [SerializeField]
    private Text m_GuideText;

    [SerializeField]
    private Button m_ConfirmButton;

    [SerializeField]
    private GameObject m_Panel;



    private void Awake()
    {
        instance = this;
    }
    private static ConfirmDialog instance;
    public static ConfirmDialog Instance => instance;

    public void Show()
    {
        m_Panel.SetActive(true);
    }

    public async Task WaitUntilClosed()
    {
        m_WaitCloseDialogTask = new TaskCompletionSource<bool>();
        await m_WaitCloseDialogTask.Task;
    }


    private TaskCompletionSource<bool> m_WaitCloseDialogTask = null;
    public void CloseDialog()
    {
        this.m_Panel.SetActive(false);

        if(m_WaitCloseDialogTask != null)
        {
            m_WaitCloseDialogTask.SetResult(true);
        }
    }
}
