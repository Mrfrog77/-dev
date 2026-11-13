using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class düğme : MonoBehaviour
{
    [SerializeField] benzin_çubuğu fuelBar;
    [SerializeField, Range(0f, 1f)] float fillPercentPerClick = 0.03f;
    [SerializeField, Range(0f, 1f)] float doublePressChance = 0.25f;
    [SerializeField, Min(0f)] float doublePressInterval = 1f;
    [SerializeField] Color doublePressColor = new Color(0.55f, 0.15f, 0.8f);

    Button button;
    RectTransform buttonRect;
    RectTransform parentRect;
    Graphic buttonGraphic;
    Coroutine bounceRoutine;
    Vector3 originalScale;
    Color normalGraphicColor;
    ColorBlock normalButtonColors;
    bool doublePressRequired;
    int pressesInSequence;
    float lastPressTime;
    float movementRange = 0.08f;

    void Start()
    {
        button = GetComponent<Button>();
        buttonRect = GetComponent<RectTransform>();
        parentRect = transform.parent as RectTransform;

        if (button == null || buttonRect == null || parentRect == null)
        {
            Debug.LogError("Düğme bir UI Button olmalı ve RectTransform altında bulunmalı.", this);
            enabled = false;
            return;
        }

        if (fuelBar == null)
        {
            Debug.LogError("Benzin çubuğunu Inspector'dan Fuel Bar alanına atayın.", this);
            enabled = false;
            return;
        }

        buttonGraphic = button.targetGraphic;
        if (buttonGraphic == null)
        {
            Debug.LogError("Düğmenin rengini değiştirmek için Button bileşeninde Target Graphic atayın.", this);
            enabled = false;
            return;
        }

        originalScale = buttonRect.localScale;
        normalGraphicColor = buttonGraphic.color;
        normalButtonColors = button.colors;
        button.onClick.AddListener(HandleClick);
        SetDoublePressRequired(Random.value < doublePressChance);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);

        if (bounceRoutine != null)
            StopCoroutine(bounceRoutine);
    }

    void HandleClick()
    {
        Debug.Log("basıldı", this);
        PlayBounce();

        if (doublePressRequired)
        {
            if (pressesInSequence == 0 || Time.unscaledTime - lastPressTime > doublePressInterval)
                pressesInSequence = 0;

            pressesInSequence++;
            lastPressTime = Time.unscaledTime;

            if (pressesInSequence < 2)
                return;
        }

        fuelBar.AddFuelPercent(fillPercentPerClick);
        MoveButton();
        SetDoublePressRequired(Random.value < doublePressChance);
    }

    void SetDoublePressRequired(bool required)
    {
        doublePressRequired = required;
        pressesInSequence = 0;

        ColorBlock colors = normalButtonColors;
        if (required)
        {
            colors.normalColor = doublePressColor;
            colors.highlightedColor = Color.Lerp(doublePressColor, Color.white, 0.2f);
            colors.pressedColor = Color.Lerp(doublePressColor, Color.black, 0.2f);
            colors.selectedColor = doublePressColor;
        }

        button.colors = colors;
        buttonGraphic.color = required ? doublePressColor : normalGraphicColor;
    }

    public void SetMovementRange(float range)
    {
        movementRange = Mathf.Clamp01(range);
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
    }

    void PlayBounce()
    {
        if (bounceRoutine != null)
        {
            StopCoroutine(bounceRoutine);
            buttonRect.localScale = originalScale;
        }

        bounceRoutine = StartCoroutine(Bounce());
    }

    IEnumerator Bounce()
    {
        yield return AnimateScale(originalScale, originalScale * 0.88f, 0.06f);
        yield return AnimateScale(originalScale * 0.88f, originalScale * 1.12f, 0.1f);
        yield return AnimateScale(originalScale * 1.12f, originalScale, 0.12f);
        buttonRect.localScale = originalScale;
        bounceRoutine = null;
    }

    IEnumerator AnimateScale(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            buttonRect.localScale = Vector3.Lerp(from, to, progress);
            yield return null;
        }

        buttonRect.localScale = to;
    }

    void MoveButton()
    {
        Rect bounds = parentRect.rect;
        float buttonWidth = buttonRect.rect.width * Mathf.Abs(originalScale.x);
        float buttonHeight = buttonRect.rect.height * Mathf.Abs(originalScale.y);
        float minX = bounds.xMin + buttonWidth * buttonRect.pivot.x;
        float maxX = bounds.xMax - buttonWidth * (1f - buttonRect.pivot.x);
        float minY = bounds.yMin + buttonHeight * buttonRect.pivot.y;
        float maxY = bounds.yMax - buttonHeight * (1f - buttonRect.pivot.y);

        Vector3 position = buttonRect.localPosition;
        float rangeX = (maxX - minX) * movementRange * 0.5f;
        float rangeY = (maxY - minY) * movementRange * 0.5f;
        float moveMinX = minX <= maxX ? Mathf.Max(minX, position.x - rangeX) : bounds.center.x;
        float moveMaxX = minX <= maxX ? Mathf.Min(maxX, position.x + rangeX) : bounds.center.x;
        float moveMinY = minY <= maxY ? Mathf.Max(minY, position.y - rangeY) : bounds.center.y;
        float moveMaxY = minY <= maxY ? Mathf.Min(maxY, position.y + rangeY) : bounds.center.y;

        float x = Random.Range(moveMinX, moveMaxX);
        float y = Random.Range(moveMinY, moveMaxY);

        if (Mathf.Approximately(x, position.x) && Mathf.Approximately(y, position.y))
        {
            if (moveMaxX > moveMinX && (moveMaxX - moveMinX) >= (moveMaxY - moveMinY))
                x = Mathf.Approximately(position.x, moveMinX) ? moveMaxX : moveMinX;
            else if (moveMaxY > moveMinY)
                y = Mathf.Approximately(position.y, moveMinY) ? moveMaxY : moveMinY;
            else
            {
                Debug.LogWarning("Düğmenin taşınabileceği boş alan yok.", this);
                return;
            }
        }

        buttonRect.localPosition = new Vector3(x, y, position.z);
    }
}
