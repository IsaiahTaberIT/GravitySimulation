using UnityEngine;

public class RgbSliderControllerUI : MonoBehaviour
{
    [SerializeField] private RgbSliderUI _RedSlider;
    [SerializeField] private RgbSliderUI _GreenSlider;
    [SerializeField] private RgbSliderUI _BlueSlider;

    private Color m_Color;
    
    public Color Color
    {
        get { return m_Color; }

        set
        {
            m_Color = value;
            ApplyColors();
            UpdateSliders();
        }
    }


    public void UpdateSliders()
    {
        _RedSlider.Value = (Mathf.InverseLerp(0, 1, m_Color.r));
        _GreenSlider.Value = (Mathf.InverseLerp(0, 1, m_Color.g));
        _BlueSlider.Value = (Mathf.InverseLerp(0, 1, m_Color.b));

    }


    private void ApplyColors()
    {
        Color c = m_Color;
        _RedSlider.Gradient.SetColors(new Color(0, c.g, c.b), new Color(1, c.g, c.b));
        _GreenSlider.Gradient.SetColors(new Color(c.r, 0, c.b), new Color(c.r, 1, c.b));
        _BlueSlider.Gradient.SetColors(new Color(c.r, c.g, 0), new Color(c.r, c.g, 1));

    }
}
