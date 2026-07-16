using UnityEngine;

public class AutoFade : MonoBehaviour
{
    [SerializeField]
    private GenericFader _genericFader;

    [SerializeField]
    private float _executeDelay;
    [SerializeField]
    private int _duration; 
    [SerializeField]
    private string _sceneName;

    [SerializeField]
    private bool _fadeIn = false;

    void Start()
    {
        Invoke(nameof(StartFade), _executeDelay);
    }

    private void StartFade()
    {
        if (_fadeIn)
        {
            _genericFader.StartFadeInAndLoad(_duration, _sceneName);
        }
        else
        {
            _genericFader.StartFadeOutAndLoad(_duration, _sceneName);
        }
    }
}
