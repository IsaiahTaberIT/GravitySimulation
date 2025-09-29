using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static UnityEngine.EventSystems.PointerEventData;
public class ClickDetectionUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
{
    public UnityEvent<PointerEventData> OnDragging;
    public UnityEvent<PointerEventData> OnScrolling;


    [SerializeField] private DownUI Down;
    [SerializeField] private UpUI Up;

    [SerializeField] private bool M_HoldingAny = false;
    public bool HoldingAny { get { return M_HoldingAny; } private set { M_HoldingAny = value; } }

    [SerializeField] private bool M_HoldingRight = false;
    public bool HoldingRight { get { return M_HoldingRight; } private set { M_HoldingRight = value; } }

    [SerializeField] private bool M_HoldingLeft = false;
    public bool HoldingLeft { get { return M_HoldingLeft; } private set { M_HoldingLeft = value;  } }

    [SerializeField] private bool M_HoldingMiddle = false;
    public bool HoldingMiddle { get { return M_HoldingMiddle; } private set { M_HoldingMiddle = value; } }

    [SerializeField] private bool M_Dragging = false;
    public bool Dragging { get { return M_Dragging; } private set { M_Dragging = value; } }







    [System.Serializable]
    private class DownUI
    {
        public UnityEvent OnClickDown;
        public UnityEvent OnRightDown;
        public UnityEvent OnLeftDown;
        public UnityEvent OnMiddleDown;
    }


    [System.Serializable]
    private class UpUI
    {
        public UnityEvent OnClickUp;
        public UnityEvent OnRightUp;
        public UnityEvent OnLeftUp;
        public UnityEvent OnMiddleUp;
    }


 
    public void OnPointerDown(PointerEventData eventData)
    {
        HoldingAny = true;

        Down.OnClickDown.Invoke();

        switch (eventData.button)
        {
            case InputButton.Left:
                Down.OnLeftDown.Invoke();
                HoldingLeft = true;
                break;
            case InputButton.Right:
                Down.OnRightDown.Invoke();
                HoldingRight = true;
                break;
            case InputButton.Middle:
                Down.OnMiddleDown.Invoke();
                HoldingMiddle = true;
                break;
            default:
                break;

        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Up.OnClickUp.Invoke();

        switch (eventData.button)
        {
            case InputButton.Left:
                Up.OnLeftUp.Invoke();
                HoldingLeft = false;
                break;
            case InputButton.Right:
                Up.OnRightUp.Invoke();
                HoldingRight = false;
                break;
            case InputButton.Middle:
                Up.OnMiddleUp.Invoke();
                HoldingMiddle = false;
                break;
            default:
                break;
        }


        if (!HoldingLeft && !HoldingRight && !HoldingMiddle)
        {
            HoldingAny = false;
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragging.Invoke(eventData);
    }

    public void OnScroll(PointerEventData eventData)
    {
        OnScrolling.Invoke(eventData);
    }
}
