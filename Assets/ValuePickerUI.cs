using System;
using UnityEngine;
using UnityEngine.EventSystems;

using UnityEngine.UI;
using UnityEngine.UIElements;
public class ValuePickerUI : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public float scale;
    public ColorPickerUI ColorPicker;
    public CanvasRenderer Knob;
    public CanvasRenderer Output;
    public ComputeShader ValuePickerShader;
    public RenderTexture ValuePickerTexture;
    public float Power;
    private Color _Hue = Color.blue;
    [SerializeField] private float Value;
    [SerializeField] private float Saturation;
    private RectTransform MyRectTransform;
    private Rect MyRect;
    private Color NewColor;


    public void SetHue(Color hue)
    {
        _Hue = hue;
        UpdateColor();
        ColorPicker.Color = NewColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Execute();
    }

    public void Repair(float saturation, float value)
    {
        UpdateBounds();

        Vector2 outputpos;

        outputpos.x = Mathf.Lerp(MyRect.min.x, MyRect.max.x,1- saturation) ;
        outputpos.y = Mathf.Lerp(MyRect.min.y, MyRect.max.y, value);

        



      //  outputpos += MyRect.center;
      //  outputpos -= MyRect.position;

        Knob.transform.position = outputpos;

        UpdateColor();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (MyRectTransform == null)
        {
            MyRectTransform = GetComponent<RectTransform>();

        }
       



        //  scalefactor = Camera.main.pixelHeight / scaler.referenceResolution.y;

     

        Gizmos.DrawSphere(MyRectTransform.rect.max + (Vector2)MyRectTransform.transform.position, 10);
        Gizmos.DrawSphere(MyRectTransform.rect.center + (Vector2)MyRectTransform.transform.position, 10);
        Gizmos.DrawSphere(new Vector2(MyRectTransform.rect.width / 2,0) + MyRectTransform.rect.center + (Vector2)MyRectTransform.transform.position, 10);
        Gizmos.DrawSphere(MyRectTransform.rect.position + (Vector2)MyRectTransform.transform.position, 10);
        Gizmos.color = Color.blue;
        Gizmos.color = Logic.LerpColor(Color.blue, Color.white, 0.75f);

        Gizmos.DrawSphere(MyRectTransform.rect.min + (Vector2)MyRectTransform.transform.position, 10);

        Debug.Log(GravityController.ScaleFactor);
        Gizmos.color = Logic.LerpColor(Color.blue, Color.red, 0.75f);

        Logic.DrawRectTransform(MyRectTransform, GravityController.ScaleFactor);

        Gizmos.color = Logic.LerpColor(Color.blue, Color.green, 0.75f);

        Rect r = MyRectTransform.RectFromRectTransform(GravityController.ScaleFactor);

        Logic.DrawBox(r);

        if (r.Contains(Input.mousePosition))
        {
            Gizmos.color = Color.green;

        }
        else
        {
            Gizmos.color = Color.magenta;

        }


        Gizmos.DrawSphere(Input.mousePosition, 10);
    }


    private void UpdateBounds()
    {

        MyRectTransform = GetComponent<RectTransform>();

        MyRect = MyRectTransform.RectFromRectTransform(GravityController.ScaleFactor);
    }

    public void Execute()
    {
    
        if (MyRect.Contains(Input.mousePosition))
        {
            // Debug.Log("mouse");
            Knob.transform.position = Input.mousePosition;
        }
        else
        {
            Vector2 outputpos;

            outputpos = Logic.MaxVector(MyRect.min, Input.mousePosition);
            outputpos = Logic.MinVector(MyRect.max, outputpos);

            Knob.transform.position = outputpos;

        }

        UpdateBounds();
        UpdateColor();

        ColorPicker.UpdateSliders();
        ColorPicker.Color = NewColor;
    }





    void UpdateVisuals()
    {
        if (ValuePickerTexture == null)
        {
            ValuePickerTexture = new(256, 256, 0, RenderTextureFormat.ARGBFloat);
            ValuePickerTexture.enableRandomWrite = true;
            ValuePickerTexture.Create();
        }
        int kernel = ValuePickerShader.FindKernel("CSMain");
        ValuePickerShader.SetFloat("Power", Power);
        ValuePickerShader.SetVector("Hue", _Hue);
        ValuePickerShader.SetTexture(kernel, "Result", ValuePickerTexture);
        ValuePickerShader.Dispatch(kernel, 32, 32, 1);
    }
    
    private void OnValidate()
    {
        if (TryGetComponent<RawImage>(out RawImage MyImage))
        {
            UpdateVisuals();

            MyImage.texture = ValuePickerTexture;
        }

    }


    /*
            Texture2D tex2d = new Texture2D(ValuePickerTexture.width, ValuePickerTexture.height,TextureFormat.RGBAFloat,false, true);
            Graphics.CopyTexture(ValuePickerTexture, tex2d);


           


            Sprite newSprite = Sprite.Create(tex2d, new Rect(0, 0, ValuePickerTexture.width, ValuePickerTexture.height), new Vector2(0.5f, 0.5f), 100f);


     */

    void UpdateColor()
    {
        UpdateVisuals();
        Saturation = Mathf.InverseLerp(MyRect.min.x, MyRect.max.x, Knob.transform.position.x);
        Value = 1 - Mathf.InverseLerp(MyRect.min.y, MyRect.max.y, Knob.transform.position.y);
        NewColor = Logic.LerpColor(_Hue, Color.white, Saturation);
        NewColor = Logic.LerpColor(NewColor, Color.black, Value);
        Output.SetColor(NewColor);
        ColorPicker.UpdateSliders();
    }

    public void OnDrag(PointerEventData eventData)
    {
        
        
        Execute();
    }
}
