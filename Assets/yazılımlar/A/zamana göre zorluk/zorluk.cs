using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class zorluk : MonoBehaviour
{
    [SerializeField, Min(1f)] float durationToMaxDifficulty = 60f;
    [SerializeField, Min(1f)] float maxDrainMultiplier = 4f;
    [SerializeField, Range(0.01f, 0.5f)] float initialMovementRange = 0.08f;
    [SerializeField, Min(0f)] float restartDelay = 2f;
    [SerializeField] benzin_çubuğu fuelBar;
    [SerializeField] düğme button;
    [SerializeField] araç_gidişi vehicle;

    RectTransform fuelBarRect;
    float elapsedTime;
    bool gameOverStarted;

    void Start()
    {
        if (fuelBar == null || button == null)
        {
            Debug.LogError("Zorluk için benzin çubuğunu ve düğmeyi Inspector'dan atayın.", this);
            enabled = false;
            return;
        }

        fuelBarRect = fuelBar.GetComponent<RectTransform>();
        if (fuelBarRect == null)
        {
            Debug.LogError("Benzin çubuğunda RectTransform bileşeni bulunamadı.", fuelBar);
            enabled = false;
            return;
        }

        if (vehicle == null)
            vehicle = FindFirstObjectByType<araç_gidişi>();

        ApplyDifficulty(0f);
    }

    void Update()
    {
        if (gameOverStarted)
            return;

        if (fuelBarRect.rect.width <= 0f)
        {
            gameOverStarted = true;
            button.SetInteractable(false);
            if (vehicle == null)
                vehicle = FindFirstObjectByType<araç_gidişi>();

            if (vehicle != null)
                vehicle.SetMovementEnabled(false);
            else
                Debug.LogWarning("Benzin bitti ancak sahnede araç denetleyicisi bulunamadı.", this);

            Debug.Log("Benzin bitti. Oyun yeniden başlatılacak.", this);
            StartCoroutine(RestartAfterDelay());
            return;
        }

        elapsedTime += Time.deltaTime;
        float difficulty = Mathf.Clamp01(elapsedTime / durationToMaxDifficulty);
        ApplyDifficulty(difficulty);
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay);

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex < 0)
        {
            Debug.LogError("Aktif sahne Build Settings içine eklenmemiş; oyun yeniden başlatılamadı.", this);
            yield break;
        }

        SceneManager.LoadScene(sceneIndex);
    }

    void ApplyDifficulty(float difficulty)
    {
        fuelBar.SetDrainMultiplier(Mathf.Lerp(1f, maxDrainMultiplier, difficulty));
        button.SetMovementRange(Mathf.Lerp(initialMovementRange, 1f, difficulty));
    }
}
