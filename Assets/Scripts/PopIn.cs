using UnityEngine;

// Added by Effects.Pop(): the object grows from nothing, overshoots a little,
// then settles at its normal size. Removes itself when done.
public class PopIn : MonoBehaviour
{
    public float Duration = 0.25f;

    float age = 0f;
    Vector3 normalScale;

    void Start()
    {
        normalScale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        // unscaledDeltaTime: still animates when the game is frozen (e.g. the stars at closing time).
        age += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(age / Duration);  // 0 at the start, 1 at the end

        // First 70%: grow to 115%. Last 30%: shrink back to 100%.
        float size;
        if (t < 0.7f)
        {
            size = Mathf.Lerp(0f, 1.15f, t / 0.7f);
        }
        else
        {
            size = Mathf.Lerp(1.15f, 1f, (t - 0.7f) / 0.3f);
        }

        transform.localScale = normalScale * size;

        if (t >= 1f)
        {
            transform.localScale = normalScale;
            Destroy(this);  // removes only this script, not the object
        }
    }
}