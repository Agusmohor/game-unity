using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F;

    [Header("Battery")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainRate = 8f;
    [SerializeField] private float currentBattery = 100f;

    [Header("Light Settings")]
    [SerializeField] private Light flashlightLight;
    [SerializeField] private float lightIntensity = 2.5f;
    [SerializeField] private float lightRange = 20f;

    private bool isOn;

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

        if (currentBattery <= 0f)
        {
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
        if (currentBattery <= 0f && isOn)
        {
            SetFlashlight(false);
        }
    }

    public void RefillBattery()
    {
        SetBattery(maxBattery);
    }

    private void OnValidate()
    {
        maxBattery = Mathf.Max(0.01f, maxBattery);
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);
        drainRate = Mathf.Max(0f, drainRate);
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
}
