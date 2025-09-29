using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class RgbSliderUI : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField] private RectTransform _BackgroundRect;
    [SerializeField] private RectTransform _HandleRect;
    [SerializeField] private float m_Value ;
    [SerializeField] private TMP_InputField _TextMeshProUGUI;
    [SerializeField] private bool AllowEdits = false;
    public void SetAllowEdits(bool allowEdits)
    {
        AllowEdits = allowEdits;
    }




    public void SetSliderValue(TMP_InputField valuefield)
    {
        if (valuefield.text == "" || valuefield.text == null)
        {
            return;
        }

        if (!AllowEdits)
        {
            return;
        }

        if (float.TryParse(valuefield.text, out float value))
        {
        


            value = Mathf.Clamp(value,0,255);

            Value = value / 255f;
            valuefield.text = value.ToString();

        }
        else
        {
            Value = 0;
        }

        
        UpdateColor();
    }

    private void OnValidate()
    {
        UpdateVisuals();
    }


    public float Value 
    {
        get { return m_Value; }

        set
        {
            m_Value = value;
            //Debug.Log("Visuals");

            UpdateVisuals();

        }
    }


    [SerializeField] private ColorPickerUI ColorPicker;
    public GradientUI Gradient;
    public Color OutputColor;


    public void UpdateColor()
    {
        OutputColor = Gradient.ColorAtTime(Value);
        ColorPicker.UpdateSliders();
        ColorPicker.Color = OutputColor;
        ColorPicker.UpdatePickers();

    }

    void UpdateTextDisplay()
    {
        if (_TextMeshProUGUI != null && !AllowEdits)
        {
            _TextMeshProUGUI.text = Mathf.CeilToInt(Value * 255).ToString();
        }
    }




    void UpdateVisuals()
    {
        float scalefactor = Mathf.Max(Camera.main.pixelWidth, Camera.main.pixelHeight) / 1024f;

        Rect r = _BackgroundRect.RectFromRectTransform(scalefactor);

        float initx = r.center.x;
        float width = r.width / 2;
        Vector2 newpos = Input.mousePosition;
        newpos.x = Mathf.Lerp(initx - width, initx + width, Value);
        newpos.y = _BackgroundRect.position.y;
        _HandleRect.position = newpos;
        UpdateTextDisplay();
    }


    public void OnDrag(PointerEventData eventData)
    {
        float scalefactor = Mathf.Max(Camera.main.pixelWidth, Camera.main.pixelHeight) / 1024f;
        Rect r = _BackgroundRect.RectFromRectTransform(scalefactor);
        float initx = r.center.x;
        float width = r.width / 2;
        Value = Mathf.InverseLerp(initx - width, initx + width, Input.mousePosition.x);

        UpdateVisuals();
        UpdateColor();



    }

    public void OnPointerDown(PointerEventData eventData)
    {
        float scalefactor = Mathf.Max(Camera.main.pixelWidth, Camera.main.pixelHeight) / 1024f;
        Rect r = _BackgroundRect.RectFromRectTransform(scalefactor);
        float initx = r.center.x;
        float width = r.width / 2;
        Value = Mathf.InverseLerp(initx - width, initx + width, Input.mousePosition.x);

        UpdateVisuals();
        UpdateColor();
    }
}
