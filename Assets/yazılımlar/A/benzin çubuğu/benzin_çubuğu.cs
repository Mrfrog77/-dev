using UnityEngine;

public class benzin_çubuğu : MonoBehaviour
{
    [SerializeField, Min(0f)] float drainDuration = 8f;

    RectTransform rectTransform;
    float maxWidth;
    float currentWidth;
    float drainMultiplier = 1f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("Benzin çubuğu için RectTransform bileşeni gerekli.", this);
            return;
        }

        Canvas.ForceUpdateCanvases();
        maxWidth = rectTransform.rect.width;
        currentWidth = maxWidth;
    }

    void Update()
    {
        if (rectTransform == null || maxWidth <= 0f)
            return;

        if (drainDuration > 0f)
            currentWidth = Mathf.MoveTowards(currentWidth, 0f, maxWidth / drainDuration * drainMultiplier * Time.deltaTime);

        ApplyWidth();
    }

    public void SetDrainMultiplier(float multiplier)
    {
        drainMultiplier = Mathf.Max(0f, multiplier);
    }

    public void AddFuelPercent(float percent)
    {
        if (rectTransform == null || maxWidth <= 0f)
        {
            Debug.LogError("Benzin çubuğu henüz başlatılmadı veya genişliği sıfır.", this);
            return;
        }

        currentWidth = Mathf.Min(maxWidth, currentWidth + maxWidth * Mathf.Max(0f, percent));
        ApplyWidth();
    }

    void ApplyWidth()
    {
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, currentWidth);
    }
}
