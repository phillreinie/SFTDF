using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightEmitter2D : MonoBehaviour
{
    public Light2D light2D;

    [Header("Optional: Follow another transform")]
    public Transform follow;

    [Header("Optional: Power gating (buildings)")]
    public bool requirePoweredBuilding = false;

    private BuildingRuntime _runtime;

    private void Awake()
    {
        if (light2D == null) light2D = GetComponentInChildren<Light2D>();
        if (follow == null) follow = transform;

        if (requirePoweredBuilding)
            _runtime = GetComponent<BuildingRuntime>();
    }

    private void LateUpdate()
    {
        if (!RunStateGate.IsPlaying()) return;
        if (light2D == null || follow == null) return;

        // position
        light2D.transform.position = follow.position;

        // optional power gating
        if (requirePoweredBuilding && _runtime != null)
        {
            bool on =  _runtime.isPowered;
            light2D.gameObject.SetActive(on);
        }
    }
}