using UnityEngine;

// Makes a UI element gently float up and down (for the menu logo).
// Put it on the Logo image.
public class UIBob : MonoBehaviour
{
    public float Height = 12f;   // how far it moves, in pixels
    public float Speed = 1.5f;   // how fast it bobs

    RectTransform rect;
    Vector2 startPosition;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        startPosition = rect.anchoredPosition;
    }

    void Update()
    {
        // Mathf.Sin goes smoothly between -1 and 1 over time.
        float offset = Mathf.Sin(Time.unscaledTime * Speed) * Height;
        rect.anchoredPosition = startPosition + new Vector2(0f, offset);
    }
}
