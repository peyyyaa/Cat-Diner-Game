using TMPro;
using UnityEngine;

// Put this on the FloatingText prefab (a 3D TextMeshPro text).
// It floats upward, fades out, then removes itself.
public class FloatingText : MonoBehaviour
{
    public TMP_Text Label;
    public float RiseSpeed = 0.8f;   // units per second
    public float Lifetime = 1.2f;    // seconds before it disappears

    float age = 0f;
    Color color = Color.white;

    public void Show(string text, Color startColor)
    {
        if (Label == null)
        {
            Label = GetComponent<TMP_Text>();
        }
        Label.text = text;
        color = startColor;
        Label.color = startColor;
    }

    void Update()
    {
        // unscaledDeltaTime keeps it moving even when the game is frozen at closing time.
        float step = Time.unscaledDeltaTime;
        age += step;

        transform.position += Vector3.up * RiseSpeed * step;

        // Fade out: alpha goes from 1 to 0 over the lifetime.
        Color faded = color;
        faded.a = 1f - (age / Lifetime);
        Label.color = faded;

        if (age >= Lifetime)
        {
            Destroy(gameObject);
        }
    }
}
