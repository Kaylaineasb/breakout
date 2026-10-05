using System.Collections;
using TMPro;
using UnityEngine;

// Tween simples: "pulo" de escala + flash de cor. Usa tempo não escalado (funciona na pausa).
public class UIPunch : MonoBehaviour
{
    [SerializeField] float scaleAmount = 1.3f;
    [SerializeField] float duration = 0.2f;
    [SerializeField] Color flashColor = new(1f, 0.84f, 0.04f);

    TMP_Text text;
    Color baseColor;
    Vector3 baseScale;
    Coroutine routine;

    void Awake()
    {
        baseScale = transform.localScale;
        if (TryGetComponent(out text)) baseColor = text.color;
    }

    void OnDisable()
    {
        routine = null;
        transform.localScale = baseScale;
        if (text != null) text.color = baseColor;
    }

    public void Play()
    {
        if (!isActiveAndEnabled) return;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(PunchRoutine());
    }

    IEnumerator PunchRoutine()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);

            // Sobe e volta (meia senoide)
            transform.localScale = baseScale * Mathf.Lerp(1f, scaleAmount, Mathf.Sin(p * Mathf.PI));
            if (text != null) text.color = Color.Lerp(flashColor, baseColor, p);
            yield return null;
        }

        transform.localScale = baseScale;
        if (text != null) text.color = baseColor;
        routine = null;
    }
}
