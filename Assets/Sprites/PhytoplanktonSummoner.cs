using UnityEngine;

public class PhytoplanktonSummoner : MonoBehaviour
{
    public GameObject phytoplanktonPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 3f;

    [Range(0f, 1f)]
    public float spawnChance = 0.3f;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        InvokeRepeating(nameof(TrySpawnPhytoplankton), 0f, spawnInterval);
    }

    void TrySpawnPhytoplankton()
    {
        // 소환 확률
        if (Random.value > spawnChance)
        {
            return;
        }

        // 객체의 실제 월드 영역
        Bounds bounds = spriteRenderer.bounds;

        // 객체 전체 영역에서 랜덤한 위치
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);

        Vector3 spawnPosition = new Vector3(
            randomX,
            randomY,
            transform.position.z
        );

        Instantiate(
            phytoplanktonPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }
}