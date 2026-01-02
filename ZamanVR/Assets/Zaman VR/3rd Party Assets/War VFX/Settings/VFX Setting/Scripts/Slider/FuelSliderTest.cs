using UnityEngine;
using UnityEngine.UI;

public class FuelSliderTest : MonoBehaviour
{
    [Header("UI")]
    public Slider fuelSlider;
    public Image fuelFill;

    [Header("Fuel")]
    [Range(0f, 1f)]
    public float fuel = 1f;

    public float consumeSpeed = 0.25f;

    [Header("Low Fuel")]
    public float lowFuelThreshold = 0.2f;
    public Color normalColor = Color.green;
    public Color lowFuelColor = Color.red;
    public float pulseSpeed = 4f;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // Consume while thrusting
        if (Input.GetKey(KeyCode.Space))
            fuel -= consumeSpeed * Time.deltaTime;

        fuel = Mathf.Clamp01(fuel);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (fuelSlider)
            fuelSlider.value = fuel;

        if (!fuelFill) return;

        if (fuel <= lowFuelThreshold)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            fuelFill.color = Color.Lerp(normalColor, lowFuelColor, t);
        }
        else
        {
            fuelFill.color = normalColor;
        }
    }

    // --------------------
    // Orb API
    // --------------------

    public void Gain(float amount)
    {
        fuel = Mathf.Clamp01(fuel + amount);
        UpdateUI();
    }

    public bool HasFuel(float amount)
    {
        return fuel >= amount;
    }
}