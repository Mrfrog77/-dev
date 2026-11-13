using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class kazanma_koşulu : MonoBehaviour
{
    [SerializeField] TMP_Text winText;
    [SerializeField] string winMessage = "Kazandın!";
    [SerializeField, Min(1f)] float winFontSize = 64f;

    bool hasWon;

    void Awake()
    {
        Collider2D goalCollider = GetComponent<Collider2D>();
        if (goalCollider == null)
        {
            Debug.LogError("Kazanma objesine bir Collider2D ekleyin.", this);
            enabled = false;
            return;
        }

        goalCollider.isTrigger = true;

        if (winText == null)
            winText = CreateWinText();

        if (winText == null)
        {
            enabled = false;
            return;
        }

        winText.text = winMessage;
        winText.fontSize = winFontSize;
        winText.alignment = TextAlignmentOptions.Center;
        winText.color = Color.yellow;
        winText.raycastTarget = false;
        winText.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasWon || other.GetComponentInParent<araç_gidişi>() == null)
            return;

        hasWon = true;
        winText.gameObject.SetActive(true);
    }

    TMP_Text CreateWinText()
    {
        TMP_FontAsset font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null)
        {
            Debug.LogError("Kazandın yazısı için LiberationSans SDF TMP fontu bulunamadı.", this);
            return null;
        }

        GameObject canvasObject = new GameObject(
            "Kazanma Mesaji Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject textObject = new GameObject(
            "Kazanma Mesaji",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvas.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TMP_Text text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        return text;
    }
}
