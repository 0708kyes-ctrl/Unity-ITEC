using UnityEngine;

public class PhytoplanktonSummoner : MonoBehaviour
{
    public GameObject phytoplanktonPrefab;

    public float spawnInterval = 1f;

    void Start()
    {
        InvokeRepeating("SpawnPhytoplankton", 0f, spawnInterval);
    }

    void SpawnPhytoplankton()
    {
        float y;

        // 3개 구역 중 하나 선택
        int zone = Random.Range(0, 10);

        if (zone <= 4)
        {
            // 표층: 위에서부터 0 ~ 20
            y = Random.Range(0f, 20f);
        }
        else if (zone <= 7)
        {
            // 수온약층: 위에서부터 20 ~ 45
            y = Random.Range(20f, 45f);
        }
        else
        {
            // 심해층: 위에서부터 45 ~ 90
            y = Random.Range(45f, 90f);
        }
        // Summoner 기준 위치
        Vector3 spawnPosition = transform.position;

        // Y 위치
        spawnPosition.y += y;

        // X 위치를 -50 ~ +50 사이에서 랜덤
        spawnPosition.x += Random.Range(-50f, 50f);

        Instantiate(
            phytoplanktonPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }
}