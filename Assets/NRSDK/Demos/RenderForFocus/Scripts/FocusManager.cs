using NRKernal;
using UnityEngine;
using UnityEngine.UI;

public class FocusManager : MonoBehaviour
{
    private Transform m_HeadTransfrom;
    private Vector3 m_FocusPosition;
    RaycastHit hitResult;
    private FocusItem currentFocusItem;
    public bool adjustFocusPlaneNorm = true;
    public bool isFocusInViewSpace
    {
        get;
        set;
    } = true;

    void Start()
    {
        m_HeadTransfrom = NRSessionManager.Instance.CenterCameraAnchor;

        //NRInput.ReticleVisualActive = false;
        //NRInput.LaserVisualActive = false;
    }

    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
    void Update()
    {
        if (Physics.Raycast(new Ray(m_HeadTransfrom.position, m_HeadTransfrom.forward), out hitResult, 100))
        {
            m_FocusPosition = isFocusInViewSpace ? m_HeadTransfrom.InverseTransformPoint(hitResult.point) : hitResult.point;

            var item = hitResult.collider.GetComponent<FocusItem>();
            if (item != null && currentFocusItem != item)
            {
                currentFocusItem?.OnOut();

                currentFocusItem = item;
                currentFocusItem.OnEnter();
            }

            Vector3 normal = isFocusInViewSpace ? m_HeadTransfrom.InverseTransformDirection(hitResult.normal) : hitResult.normal;
            if (adjustFocusPlaneNorm)
                NRFrame.SetFocusPlane(m_FocusPosition, normal, isFocusInViewSpace ? NRReferenceSpaceType.NR_REFERENCE_SPACE_VIEW : NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL);
            else
                NRFrame.SetFocusPlane(m_FocusPosition, -Vector3.forward, isFocusInViewSpace ? NRReferenceSpaceType.NR_REFERENCE_SPACE_VIEW : NRReferenceSpaceType.NR_REFERENCE_SPACE_GLOBAL);
        }
        else
        {
            currentFocusItem?.OnOut();
            currentFocusItem = null;
        }

        // if (Time.frameCount % 100 == 0 || stopwatch.ElapsedMilliseconds >= 20)
        // {
        //     Debug.Log("time cost a frame:" + stopwatch.ElapsedMilliseconds);
        // }
        // stopwatch.Reset();
        // stopwatch.Start();
    }

    void OnDrawGizmos()
    {
        if (hitResult.collider != null)
        {
            Gizmos.DrawSphere(hitResult.point, 0.1f);
            Gizmos.DrawLine(m_HeadTransfrom.position, hitResult.point);
        }
    }
}
