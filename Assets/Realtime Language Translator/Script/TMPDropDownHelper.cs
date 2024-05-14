using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TMPDropDownHelper : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Dropdown m_DropDown = null;

    private void OnValidate(){
        if (m_DropDown == null){
            m_DropDown = GetComponent<TMP_Dropdown>();
        }
    }
    // Start is called before the first frame update
    public void OnPointerClick(PointerEventData eventData)
    {
        RemoveUnityCanvas();
    }

    private void RemoveUnityCanvas()
    {
        var raycasttarget = gameObject.GetComponentsInChildren<GraphicRaycaster>();
        foreach (var item in raycasttarget)
        {
            GameObject.Destroy(item);
        }

        var canvas = gameObject.GetComponentsInChildren<Canvas>();
        foreach (var item in canvas)
        {
            GameObject.Destroy(item);
        }
    }
}
