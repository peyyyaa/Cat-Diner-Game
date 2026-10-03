using UnityEngine;

// Put this on an empty object named Effects in the Diner scene.
// Any script can call Effects.Float(...) or Effects.Pop(...).
public class Effects : MonoBehaviour
{
    public static Effects Instance;

    // Drag the FloatingText prefab here.
    public FloatingText FloatingTextPrefab;

    void Awake()
    {
        Instance = this;
    }

    // Shows a short text that rises and fades, e.g. "+PHP 72".
    public static void Float(Vector3 position, string text, Color color)
    {
        if (Instance == null || Instance.FloatingTextPrefab == null)
        {
            return;
        }

        FloatingText floating = Instantiate(Instance.FloatingTextPrefab, position, Quaternion.identity);
        floating.Show(text, color);
    }

    // Makes something appear with a little bounce.
    public static void Pop(GameObject target)
    {
        if (target != null)
        {
            target.AddComponent<PopIn>();
        }
    }
}
