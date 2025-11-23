using UnityEngine;
using System.Collections;

public class CameraZoom : MonoBehaviour
{
    public float startOrthographicSize = 10f;
    public float endOrthographicSize = 6f;
    public float zoomDuration = 2f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            StartCoroutine(ZoomInAnimation());
        }
    }

    IEnumerator ZoomInAnimation()
    {
        mainCamera.orthographicSize = startOrthographicSize;

        float elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            // Smooth curve for natural animation
            t = t * t * (3f - 2f * t);

            mainCamera.orthographicSize = Mathf.Lerp(startOrthographicSize, endOrthographicSize, t);
            yield return null;
        }

        mainCamera.orthographicSize = endOrthographicSize;
    }
}
