using UnityEngine;
using UnityEngine.UI;

public class ProcessingBarUI : MonoBehaviour
{
    public Image fillImage;

    void Awake()
    {
        SetProgress(0f);
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        SetProgress(0f);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetProgress(float value)
    {
        fillImage.fillAmount = Mathf.Clamp01(value);
    }
}
