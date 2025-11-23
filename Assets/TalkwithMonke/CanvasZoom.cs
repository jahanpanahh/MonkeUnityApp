using UnityEngine;
using System.Collections;

public class CanvasZoom : MonoBehaviour
{
    public float startScale = 0.5f;
    public float zoomDuration = 2f;

    void Start()
    {
        StartCoroutine(ZoomInAnimation());
    }

    IEnumerator ZoomInAnimation()
    {
        transform.localScale = Vector3.one * startScale;

        float elapsed = 0f;
        Vector3 targetScale = Vector3.one;
        Vector3 initialScale = transform.localScale;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            // Smooth curve
            t = t * t * (3f - 2f * t);

            transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
    }
}
