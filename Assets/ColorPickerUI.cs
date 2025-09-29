using System;
using UnityEngine;
[System.Serializable]
public class ColorPickerUI : MonoBehaviour
{
    // they say "dont re-invent the wheel", after literally re-inventing the wheel... the Color wheel to be specific
    // I can say a few things:
    // 1. definitely don't reinvent the wheel if expediency is the goal.
    // 2. how in the world are you supposed to learn if you dont make this stuff yourself?
    // 3. I still vastly prefer trying to solve these problems myself. however now that ive done it i'd be
    // more than willing to import a library/package for this... but sometimes just building it yourself is the best option
    // like how unity's sliders only have a event for OnValueChanged so when you try and update their positions you automattically call the event.
    // after like an hour of messing around with it I spent 20 miniutes making my own slider and it works perfectly



    [SerializeField] private RgbSliderControllerUI SliderController;
    [SerializeField] private ValuePickerUI ValuePicker;
    [SerializeField] private ColorWheelHueUI HueWheel;

    public Action OnColorUpdated = () => { };
    public Action OnClose = () => { };
    public void OpenColorPicker()
    {
        gameObject.SetActive(true);
    }

    public void CloseColorPicker()
    {
        OnClose();
        gameObject.SetActive(false);
    }

    public interface IColorPickerable
    {
        public Color Color { get; set; }
    }

    public void UpdateVisuals()
    {
        UpdatePickers();
        UpdateSliders();
    }

    public void UpdatePickers()
    {
        Color.RGBToHSV(m_Color, out float hue, out float sat, out float value);

        ValuePicker.Repair(sat, value);

        if (hue > 0)
        {
            // + 180 to rotate 180 degress because of the offset I have + 360 to ensure no negatives
            HueWheel.Repair(hue * -360 + 540);
        }
    }

    public void UpdateSliders()
    {
       SliderController.Color = Color;
    }
 
    private Color m_Color;

    public Color Color
    {
        get { return m_Color; }

        set
        {
            m_Color = value;
            OnColorUpdated();
        }
    }




}
