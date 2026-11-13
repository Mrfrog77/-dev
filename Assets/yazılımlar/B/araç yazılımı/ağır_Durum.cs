using UnityEngine;
using UnityEngine.UI;

public class ağır_Durum : MonoBehaviour
{
    [SerializeField] araç_gidişi vehicle;
    [SerializeField, Min(0f)] float speedDecrease = 100f;

    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null || vehicle == null)
        {
            Debug.LogError("Ağır durum düğmesine Button ve araç referansı atayın.", this);
            enabled = false;
            return;
        }

        button.onClick.AddListener(DecreaseSpeed);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(DecreaseSpeed);
    }

    void DecreaseSpeed()
    {
        vehicle.AdjustMotorSpeed(-speedDecrease);
    }
}
