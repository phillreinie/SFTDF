using UnityEngine;
using UnityEngine.SceneManagement;

public class RunOverManager : MonoBehaviour
{
    private bool _ended;

    private void OnEnable()
    {
        Health.OnAnyDeath += HandleAnyDeath; // we’ll add this static event in Health (below)
    }

    private void OnDisable()
    {
        Health.OnAnyDeath -= HandleAnyDeath;
    }

    private void HandleAnyDeath(Health h)
    {
        if (_ended || h == null) return;

        // Lose
        if (h.GetComponentInParent<CoreMarker>() != null)
        {
            _ended = true;
            Restart();
            return;
        }

        // Win
        if (h.GetComponentInParent<EndSpawnerMarker>() != null)
        {
            _ended = true;
            Restart();
            return;
        }
    }

    private void Restart()
    {
        var scene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(scene);
    }
}