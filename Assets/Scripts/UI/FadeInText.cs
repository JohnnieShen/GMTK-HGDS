using UnityEngine;
using System.Collections;

public class FadeInOutTextGroup : MonoBehaviour
{
    public CanvasGroup group;
    public float fadeDuration = 1.5f;
    public float displayDuration = 2.0f;

    void Start()
    {
        StartCoroutine(FadeSequence());
    }

    IEnumerator FadeSequence()
    {
        // Fade in
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        group.alpha = 1f;

        // Wait
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        group.alpha = 0f;

        // Optional: disable the GameObject afterwards
        gameObject.SetActive(false);
    }
}
