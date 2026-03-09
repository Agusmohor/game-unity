using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;
    [SerializeField] private bool showTogglePrompt = true;

    [Header("Battery")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainRate = 8f;
    [SerializeField] private float currentBattery = 100f;
    [SerializeField] [Range(0.01f, 1f)] private float lowBatteryThreshold = 0.2f;
    [SerializeField] private bool showLowBatteryMessage = true;

    [Header("Light Settings")]
    [SerializeField] private Light flashlightLight;
    [SerializeField] private float lightIntensity = 2.5f;
    [SerializeField] private float lightRange = 20f;

    [Header("Messages")]
    [SerializeField] private string lowBatteryMessage = "Bateria baja";
    [SerializeField] private string emptyBatteryMessage = "Sin bateria";

    private bool isOn;
    private bool lowBatteryMessageShown;
    private bool emptyBatteryMessageShown;

    public float CurrentBattery => currentBattery;
    public float BatteryPercent => maxBattery <= 0f ? 0f : currentBattery / maxBattery;
    public bool IsOn => isOn;

    private void Awake()
    {
        if (flashlightLight == null)
        {
            flashlightLight = GetComponentInChildren<Light>();
        }

        if (flashlightLight != null)
        {
            flashlightLight.type = LightType.Spot;
        }

        maxBattery = Mathf.Max(0.01f, maxBattery);
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        ApplyLightSettings();
        SetFlashlight(false);
    }

    private void Start()
    {
        BindToHudIfAvailable();
    }

    private void OnEnable()
    {
        BindToHudIfAvailable();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleFlashlight();
        }

        if (!isOn)
        {
            return;
        }

        currentBattery -= drainRate * Time.deltaTime;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        float batteryPercent = BatteryPercent;
        if (showLowBatteryMessage && !lowBatteryMessageShown && batteryPercent > 0f && batteryPercent <= lowBatteryThreshold)
        {
            lowBatteryMessageShown = true;
            GameHUD.Instance?.ShowStatusMessage(lowBatteryMessage, 1.3f);
        }

        if (currentBattery <= 0f)
        {
            if (!emptyBatteryMessageShown)
            {
                emptyBatteryMessageShown = true;
                GameHUD.Instance?.ShowStatusMessage(emptyBatteryMessage, 1.4f);
            }

            SetFlashlight(false);
        }
    }

    public void ToggleFlashlight()
    {
        if (isOn)
        {
            SetFlashlight(false);
            return;
        }

        if (currentBattery <= 0f)
        {
            if (!emptyBatteryMessageShown)
            {
                emptyBatteryMessageShown = true;
                GameHUD.Instance?.ShowStatusMessage(emptyBatteryMessage, 1.4f);
            }
            return;
        }

        SetFlashlight(true);
    }

    public void SetFlashlight(bool value)
    {
        isOn = value;
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isOn;
        }
    }

    public void SetBattery(float value)
    {
        currentBattery = Mathf.Clamp(value, 0f, maxBattery);

        if (currentBattery > 0f)
        {
            emptyBatteryMessageShown = false;
        }

        if (BatteryPercent > lowBatteryThreshold)
        {
            lowBatteryMessageShown = false;
        }

        if (currentBattery <= 0f && isOn)
        {
            SetFlashlight(false);
        }
    }

    public void RefillBattery()
    {
        SetBattery(maxBattery);
    }

    public string GetTogglePrompt()
    {
        if (!showTogglePrompt)
        {
            return "";
        }

        if (isOn)
        {
            return "[F] Apagar linterna";
        }

        if (currentBattery <= 0f)
        {
            return "";
        }

        return "[F] Encender linterna";
    }

    private void OnValidate()
    {
        maxBattery = Mathf.Max(0.01f, maxBattery);
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);
        drainRate = Mathf.Max(0f, drainRate);
        lowBatteryThreshold = Mathf.Clamp01(lowBatteryThreshold);
        lightIntensity = Mathf.Max(0f, lightIntensity);
        lightRange = Mathf.Max(0f, lightRange);

        ApplyLightSettings();
    }

    private void ApplyLightSettings()
    {
        if (flashlightLight == null)
        {
            return;
        }

        flashlightLight.intensity = lightIntensity;
        flashlightLight.range = lightRange;
    }

    private void BindToHudIfAvailable()
    {
        if (GameHUD.Instance != null)
        {
            GameHUD.Instance.BindFlashlight(this);
        }
    }
}
