using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class baştan_başlama : MonoBehaviour
{
    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Sahneyi yeniden başlatmak için bu nesnede Button bileşeni gerekli.", this);
            enabled = false;
            return;
        }

        button.onClick.AddListener(RestartScene);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(RestartScene);
    }

    void RestartScene()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (sceneIndex < 0)
        {
            Debug.LogError("Aktif sahne Build Settings içine eklenmemiş; sahne yeniden yüklenemedi.", this);
            return;
        }

        SceneManager.LoadScene(sceneIndex);
    }
}
