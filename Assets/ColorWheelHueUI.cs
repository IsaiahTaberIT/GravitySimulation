using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ColorWheelHueUI : MonoBehaviour , IPointerDownHandler, IDragHandler
{
    public RectTransform Self;
    public CanvasRenderer Knob;
   // public CanvasRenderer Output;
    public ComputeShader ColorPickerShader;
    public RenderTexture ColorPickerTexture;
    public ValuePickerUI ValuePicker;
    public float RadiusTweakMultiplyer = 1.0f;
    public float Radius = 1;
    public float Power = 1;

    Vector2 RedDir = Quaternion.AngleAxis(0,new Vector3(0,0,1)) * Vector2.up;
    Vector2 GreenDir = Quaternion.AngleAxis(120, new Vector3(0, 0, 1)) * Vector2.up;
    Vector2 BlueDir = Quaternion.AngleAxis(240, new Vector3(0, 0, 1)) * Vector2.up;

    Vector2 Dir = Vector2.zero;


    void powPreserveSign(ref float v, float p)
    {
        float sign = Mathf.Sign(v);
        v = Mathf.Abs(v);
        v = Mathf.Pow(v, p) * sign;

    }


    public void Repair(float angle)
    {
        //Debug.Log("hi");
        Dir = Quaternion.AngleAxis(angle,new Vector3(0,0,1)) * Vector2.up;
        DetermineHue(-angle + 180+360);
    }

    public void RenderColorWheel()
    {
        if (ColorPickerTexture == null)
        {
            ColorPickerTexture = new(256, 256, 0);
            ColorPickerTexture.enableRandomWrite = true;
            ColorPickerTexture.Create();
        }

        int kernel = ColorPickerShader.FindKernel("CSMain");
        ColorPickerShader.SetFloat("Power", Power);
        ColorPickerShader.SetTexture(kernel, "Result", ColorPickerTexture);
        ColorPickerShader.Dispatch(kernel, 32, 32, 1);
    }

    private void OnDrawGizmos()
    {
       
        Gizmos.color = Color.red;
        if (Self != null)
        {
            Logic.DrawBox(Self.RectFromRectTransform(GravityController.ScaleFactor));

        }
    }

    private void DetermineHue(float angle)
    {
        angle %= 360;


        Radius = Self.RectFromRectTransform(GravityController.ScaleFactor).width / 2f * RadiusTweakMultiplyer;

       // Debug.Log(GravityController.ScaleFactor);

        Knob.transform.position = transform.position + (Vector3)Dir.normalized * (Radius);

        Vector3 Euler = Knob.transform.eulerAngles;
        Euler.z = -angle + 90;
        Knob.transform.eulerAngles = Euler;

        Color Red = new Color(1, 0, 0);
        Color Yellow = new Color(1, 1, 0);
        Color Green = new Color(0, 1, 0);
        Color Cyan = new Color(0, 1, 1);
        Color Blue = new Color(0, 0, 1);
        Color Magenta = new Color(1, 0, 1);

        Color[] Colors = { Red, Yellow, Green, Cyan, Blue, Magenta };

        float t;
        angle /= 60;
        int lowindex = Mathf.FloorToInt(angle);
        int Highindex = Mathf.CeilToInt(angle) % 6;
        t = angle % 1f;


        Color NewColor = Logic.LerpColor(Colors[lowindex], Colors[Highindex], t);


        RenderColorWheel();

        ValuePicker.SetHue(NewColor);
    }

    private void OnValidate()
    {
        if (TryGetComponent<RawImage>(out RawImage MyImage))
        {
            RenderColorWheel();

            MyImage.texture = ColorPickerTexture;
        }

    }

    void Execute()
    {

        Vector3 StartingColor = Vector3.right;

        Dir = Input.mousePosition - transform.position;
        // Dir /= 200;

        float angle = Vector2.SignedAngle(Dir, Vector2.up) + 180;

        DetermineHue(angle);

    }

    private void Start()
    {
        Self = GetComponent<RectTransform>();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        Execute();
    }

    public void OnDrag(PointerEventData eventData)
    {
        Execute();

    }
}
