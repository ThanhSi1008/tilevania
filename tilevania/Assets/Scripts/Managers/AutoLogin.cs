using System.Collections;
using UnityEngine;

public class AutoLogin : MonoBehaviour
{
    [SerializeField] private GameObject loadingOverlay;

    void Start()
    {
        StartCoroutine(AutoLoginRoutine());
    }

    private IEnumerator AutoLoginRoutine()
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.HasToken())
        {
            yield break;
        }

        SetLoading(true);
        yield return AuthManager.Instance.ValidateTokenCoroutine(success =>
        {
            // No-op here; result can be used by UI to show state if wired.
        });
        SetLoading(false);
    }

    private void SetLoading(bool state)
    {
        if (loadingOverlay != null)
        {
            loadingOverlay.SetActive(state);
        }
    }
}

