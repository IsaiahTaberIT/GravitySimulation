using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class GradientUI : MonoBehaviour
{
    [SerializeField] private Color StartColor;
    [SerializeField] private Color EndColor;
    [SerializeField] private float Angle;
    [SerializeField] private Shader GradientShader;
    [SerializeField] private Material material;
    [SerializeField] private CanvasRenderer MyRenderer;
    [SerializeField] private RawImage MyImage;

    public void SetColors(Color colorStart, Color colorEnd)
    {
        StartColor = colorStart;
        EndColor = colorEnd;
        UpdateMaterial();
    }



    public Color ColorAtTime(float t)
    {
       return Logic.LerpVector(StartColor, EndColor, t);
    }


    void UpdateMaterial()
    {
        if (MyRenderer == null && !gameObject.TryGetComponent<CanvasRenderer>(out MyRenderer))
        {
            return;
        }
        if (GradientShader == null)
        {
            GradientShader = Resources.Load<Shader>("Gradient");
        }
        if (GradientShader == null)
        {
            return;
        }

        if (material == null)
        {
            material = new(GradientShader);
        }

        material.SetFloat("_Angle", Angle);
        material.SetColor("_StartColor", StartColor);
        material.SetColor("_EndColor", EndColor);

        MyRenderer.materialCount = 1;
        MyRenderer.SetMaterial(material,0);

       


    }

    private void OnValidate()
    {
        UpdateMaterial();
     

        if (TryGetComponent<RawImage>(out RawImage MyImage))
        {
            MyImage.material = material;
        }


    }
}
