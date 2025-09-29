using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;

public class ToolTipUpdater : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsInside;

    public string TitleText;

    public string BodyText;

    public ToolTipController Controller;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Controller.TitleText = TitleText;
        Controller.BodyText = BodyText;
     
        IsInside = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        IsInside = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (Controller == null)
        {
            Controller = FindAnyObjectByType<ToolTipController>();
            if (Controller == null)
            {
                Debug.LogWarning("There is no Tooltip In The Scene", gameObject);
                return;
            }
        }

        if (IsInside)
        {
            Controller.AddHoverProgress();
        }
    }
}
