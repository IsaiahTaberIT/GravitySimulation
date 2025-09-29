using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class UIHelper : MonoBehaviour
{
    Selectable target;
    public void SetTarget(Selectable s)
    {
        target = s;
    }

    public void ToggleInteractable(Toggle t)
    {
        target.interactable = t.isOn;
    }

    public void ToggleInteractableInverted(Toggle t)
    {
        target.interactable = !t.isOn;
    }

}
