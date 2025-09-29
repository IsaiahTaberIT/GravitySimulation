using UnityEngine;
using static ColorPickerUI;
using UnityEngine.UI;

using UnityEngine.EventSystems;

[ExecuteAlways]
public class ColorPickerInterfaceUI : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private ColorPickerUI colorPickerUI;
    [SerializeField] private RawImage MyImage;
    [SerializeField] private IColorPickerable pickerable;

    public MonoBehaviour ColorPickerable;

    public void UpdateDisplayColor()
    {
        if (MyImage != null && pickerable != null)
        {
            MyImage.color = pickerable.Color;
        }
    }


    private void AssignColors()
    {
        if (MyImage != null)
        {
            MyImage.color = colorPickerUI.Color;
        }

        if (pickerable != null) {
            pickerable.Color = colorPickerUI.Color;

        }
      
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Start()
    {
        Validate();
        UpdateDisplayColor();

    }


    private void OnValidate()
    {

        Validate();
      
    }
    /// <summary>
    /// makes sure the object dragged in from the inspector implements IColorPickerable
    /// because unity doesn't doesnt expose interface instances in the inspector for reasons that elude me
    /// </summary>
    private void Validate()
    {

        if (ColorPickerable == null)
        {
            pickerable = null;
            return;
        }

        if (MyImage == null)
        {
            MyImage = GetComponent<RawImage>();
        }

        if(ColorPickerable.TryGetComponent<IColorPickerable>(out IColorPickerable p))
        {
            pickerable = p;
        }
        else
        {
            Debug.LogWarning(ColorPickerable.name + " Is Not IColorPickerable");
            ColorPickerable = null;

            if (pickerable != null && (pickerable is MonoBehaviour m))
            {
                ColorPickerable = m;
            }

        }
    }

    void Unsubscribe()
    {
        colorPickerUI.OnColorUpdated -= AssignColors;
        colorPickerUI.OnClose -= Unsubscribe;
    }


    private void TryPass()
    {
        //ensure that you dont subscribe multiple times
        Unsubscribe();

        if (ColorPickerable != null)
        {
            colorPickerUI.OnColorUpdated += AssignColors;
            colorPickerUI.OnClose += Unsubscribe;
            colorPickerUI.OpenColorPicker();

            if(pickerable != null)
            {
                colorPickerUI.Color = pickerable.Color;
                colorPickerUI.UpdateVisuals();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        TryPass();
    }

}
