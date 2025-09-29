using TMPro;
using UnityEngine;


[ExecuteAlways]
public class ToolTipController : MonoBehaviour
{
    [SerializeField] private CanvasGroup Group;
    [SerializeField] private float _AlphaZeroThreshold = 0.15f;
    [SerializeField] private float _TransitionTime = 0.2f;
    [SerializeField] private float _HoverProgress;
    [SerializeField] private float _ProgressMaxBuffer = 0.5f;


    public void UpdatePosition()
    {
        Vector3 newpos = Input.mousePosition;
        newpos.x += TitleUI.rectTransform.sizeDelta.x / 2f;
        transform.position = newpos;
    }
    public void AddHoverProgress()
    {
        _HoverProgress += Time.deltaTime * 2f;
    }


    private void Update()
    {
        _HoverProgress -= Time.deltaTime;


        if (Group != null)
        {
            Group.alpha = Mathf.InverseLerp(_AlphaZeroThreshold, _AlphaZeroThreshold + _TransitionTime, _HoverProgress);

        }
        else
        {
            Group = GetComponent<CanvasGroup>();
        }

        _HoverProgress = Mathf.Clamp(_HoverProgress, 0, _ProgressMaxBuffer + _AlphaZeroThreshold + _TransitionTime);
        //UpdatePosition();

    }
    

    [SerializeField] private string titleText;
    public string TitleText
    {
        get => titleText;
        set
        {
            titleText = value;
            SetTitleUIText();
        }
    }

    [SerializeField] private string bodyText;
    public string BodyText
    {
        get => bodyText;
        set
        {
            bodyText = value;
            SetBodyUIText();
        }
    }

    [SerializeField] TextMeshProUGUI TitleUI;
    [SerializeField] TextMeshProUGUI BodyUI;

    public void UpdateTextFromUI()
    {
        TitleText = titleText;
        BodyText = bodyText;
    }

    private void OnValidate()
    {
        UpdateTextFromUI();
    }

    void SetTitleUIText()
    {
        if (TitleUI != null)
        {
            TitleUI.text = TitleText;
        }
    }

    void SetBodyUIText()
    {
        if (TitleUI != null)
        {
            BodyUI.text = BodyText;
        }
    }

}
