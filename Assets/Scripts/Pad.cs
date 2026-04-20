using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Pad : MonoBehaviour
{
    public int padId;
    public AudioSource audioSource;
    public Image padImage;
    public Color flashColor = Color.white;
    public float flashDuration = 0.1f;

    private Color originalColor;
    private Coroutine flashRoutine;

    void Start()
    {
        originalColor = padImage.color;
    }

    public void PlaySound()
    {
        if (audioSource != null)
            audioSource.Play();

        Flash();
    }

    public void Flash()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        padImage.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        padImage.color = originalColor;
        flashRoutine = null;
    }
}