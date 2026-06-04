using System.Collections;
using UnityEngine;

public class MessageTrigger : MonoBehaviour
{
    public CanvasGroup messageCanvas;
    public float displayDuration = 2f;
    public float fadeDuration = 1.5f;

    private Coroutine _fade;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (_fade != null) StopCoroutine(_fade);
        _fade = StartCoroutine(ShowAndFade());
    }

    IEnumerator ShowAndFade()
    {
        messageCanvas.alpha = 1f;
        yield return new WaitForSeconds(displayDuration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            messageCanvas.alpha = Mathf.SmoothStep(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        messageCanvas.alpha = 0f;
    }
}
