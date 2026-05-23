using UnityEngine;
using UnityEngine.UI;

public class ProximityFade : MonoBehaviour
{
    public CanvasGroup whiteOverlay;
    public float fadeStartDistance = 5f;
    public float fadeEndDistance = 1f;
    public float fadeSpeed = 3f;

    private DoorTrigger[] _doors;

    void Start()
    {
        _doors = FindObjectsByType<DoorTrigger>(FindObjectsSortMode.None);
    }

    void Update()
    {
        float closest = GetClosestDoorDistance();
        float targetAlpha = Mathf.InverseLerp(fadeStartDistance, fadeEndDistance, closest);
        if (whiteOverlay != null)
            whiteOverlay.alpha = Mathf.Lerp(whiteOverlay.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }

    float GetClosestDoorDistance()
    {
        float min = Mathf.Infinity;
        foreach (var door in _doors)
            min = Mathf.Min(min, Vector3.Distance(transform.position, door.transform.position));
        return min;
    }
}
