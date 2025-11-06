using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class uiFader : MonoBehaviour
{
    [SerializeField] private float inDuration = 0.15f;
    [SerializeField] private float outDuration = 0.10f;

    private CanvasGroup cg;
    private Coroutine co;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    public void show()
    {
        gameObject.SetActive(true);
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(fade(1f, inDuration, true));
    }

    public void hide()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(fade(0f, outDuration, false));
    }

    System.Collections.IEnumerator fade(float target, float dur, bool enable)
    {
        float start = cg.alpha, t = 0f;
        cg.blocksRaycasts = enable;
        cg.interactable = enable;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, dur);
            cg.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }
        cg.alpha = target;
        if (!enable)
        {
            cg.blocksRaycasts = false;
            cg.interactable = false;
            gameObject.SetActive(false);
        }
    }
}
