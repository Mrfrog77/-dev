using UnityEngine;
using UnityEngine.UI;

public class hafif_Durum : MonoBehaviour
{
    [SerializeField] araç_gidişi vehicle;
    [SerializeField, Min(0f)] float speedIncrease = 100f;

    Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null || vehicle == null)
        {
            Debug.LogError("Hafif durum düğmesine Button ve araç referansı atayın.", this);
            enabled = false;
            return;
        }

        button.onClick.AddListener(IncreaseSpeed);
    }

    void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(IncreaseSpeed);
    }

    void IncreaseSpeed()
    {
        vehicle.AdjustMotorSpeed(speedIncrease);
    }
}
