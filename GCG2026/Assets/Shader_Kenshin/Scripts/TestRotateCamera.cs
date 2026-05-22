using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [SerializeField]
    private Vector3 _center = Vector3.zero;

    [SerializeField]
    private Vector3 _axis = Vector3.up;

    [SerializeField]
    private float _rotate = 2;

    [SerializeField]
    private bool _isMoving = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isMoving) return;

        transform.RotateAround(
            _center, _axis, 360 / _rotate * Time.deltaTime);
    }
}
