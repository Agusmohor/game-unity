using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FlashlightHUD : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private Image flashlightIcon;
    [SerializeField] private Image batteryFillImage;
    [SerializeField] private TMP_Text batteryPercentText;
    [SerializeField] private CanvasGroup lowBatteryWarningGroup;
    [SerializeField] private TMP_Text lowBatteryWarningText;

    [Header("Visual")]
    [SerializeField] [Range(0.01f, 1f)] private float lowBatteryThreshold = 0.2f;
    [SerializeField] private string lowBatteryText = "Bateria baja";
    [SerializeField] private Color iconOnColor = Color.white;
    [SerializeField] private Color iconOffColor = new Color(0.55f, 0.55f, 0.55f, 0.95f);
    [SerializeField] private Color batteryNormalColor = new Color(0.78f, 0.86f, 0.98f, 1f);
    [SerializeField] private Color batteryLowColor = new Color(1f, 0.3f, 0.2f, 1f);
    [SerializeField] private float warningBlinkSpeed = 4f;

    private FlashlightController controller;

    private void Awake()
    {
        if (lowBatteryWarningText != null)
        {
            lowBatteryWarningText.text = lowBatteryText;
        }

        if (lowBatteryWarningGroup != null)
        {
            lowBatteryWarningGroup.alpha = 0f;
            lowBatteryWarningGroup.interactable = false;
            lowBatteryWarningGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (controller == null)
        {
            return;
        }

        float batteryPercent = Mathf.Clamp01(controller.BatteryPercent);

        if (batteryFillImage != null)
        {
            batteryFillImage.fillAmount = batteryPercent;
            batteryFillImage.color = batteryPercent <= lowBatteryThreshold ? batteryLowColor : batteryNormalColor;
        }

        if (batteryPercentText != null)
        {
            batteryPercentText.text = Mathf.CeilToInt(batteryPercent * 100f) + "%";
        }

        if (flashlightIcon != null)
        {
            flashlightIcon.color = controller.IsOn ? iconOnColor : iconOffColor;
        }

        bool showWarning = controller.IsOn && batteryPercent > 0f && batteryPercent <= lowBatteryThreshold;
        if (lowBatteryWarningGroup != null)
        {
            if (showWarning)
            {
                float blink = 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * warningBlinkSpeed));
                lowBatteryWarningGroup.alpha = blink;
            }
            else
            {
                lowBatteryWarningGroup.alpha = 0f;
            }
        }
    }

    public void SetController(FlashlightController newController)
    {
        controller = newController;
    }
}
