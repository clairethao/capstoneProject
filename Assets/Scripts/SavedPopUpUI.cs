using System.Collections;
using UnityEngine;

public class SavedPopupUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public IEnumerator ShowPopup()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            canvasGroup.alpha = t;
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * 3f;
            canvasGroup.alpha = t;
            yield return null;
        }
    }
}