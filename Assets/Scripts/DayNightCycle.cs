using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Directional Light")]
    public Light directionalLight;

    [Header("Rotation Settings")]
    public float dayLengthInSeconds = 360f;
    public float startAngle = 160f;

    [Header("Intensity Settings")]
    public float minIntensity = 0.10f;
    public float maxIntensity = 2f;

    private float time;

    void Update()
    {
        if (directionalLight == null) return;

        
        time += Time.deltaTime;
        float dayProgress = (time % dayLengthInSeconds) / dayLengthInSeconds;

        
        float rotationAngle = startAngle + dayProgress * 360f;
        directionalLight.transform.rotation = Quaternion.Euler(30f, rotationAngle, 0f);

        
        float intensityFactor = Mathf.Clamp01(Mathf.Sin(dayProgress * Mathf.PI * 2 + Mathf.PI / 2));
        directionalLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, intensityFactor);
    }
}
