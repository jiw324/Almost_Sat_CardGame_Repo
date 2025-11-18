using UnityEngine;

public class IconIdleBehavior : MonoBehaviour
{
    [SerializeField] private float amplitude = 1f;
    [SerializeField] private float frequency = 1f;
    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        float newY = startY + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
