using UnityEngine;

public class BuildingDebugLabel : MonoBehaviour
{
    public BuildingRuntime runtime;
    public Vector3 offset = new Vector3(0f, 0.6f, 0f);

    private void Reset()
    {
        runtime = GetComponent<BuildingRuntime>();
    }

    private void OnGUI()
    {
        if (runtime == null || runtime.data == null) return;

        var cam = Camera.main;
        if (cam == null) return;

        Vector3 w = runtime.transform.position + offset;
        Vector3 s = cam.WorldToScreenPoint(w);
        if (s.z < 0) return;

        // GUI y is inverted
        float x = s.x;
        float y = Screen.height - s.y;

        string text = $"P:{(runtime.isPowered ? "Y" : "N")}  S:{(runtime.isStorageLinked ? "Y" : "N")}";
        GUI.Label(new Rect(x - 40, y - 10, 120, 20), text);
    }
}