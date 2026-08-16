using UnityEngine;

public class PhytoplanktonLife : MonoBehaviour
{
    public float lifeTime = 30f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}