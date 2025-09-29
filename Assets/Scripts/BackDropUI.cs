using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class BackDropUI : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public CameraManager manager;
    public bool MiddleMouse;
    public bool LeftMouse;

    public void OnPointerDown(PointerEventData eventData)
    {
      //  Debug.Log("UI Element Clicked!");

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            manager.StartDrag();
            MiddleMouse = true;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LeftMouse = true;
        }


        //Debug.Log("UI Element OnPointerDown!");

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //  Debug.Log("UI Element Clicked!");

        if (eventData.button == PointerEventData.InputButton.Middle)
        {
            manager.StartDrag();
            MiddleMouse = false;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LeftMouse = false;
        }



    }

}

