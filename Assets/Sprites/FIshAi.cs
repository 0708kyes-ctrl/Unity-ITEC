using UnityEngine;

public class FishAI : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 2f;

    [Header("서식지")]
    public GameObject[] habitats;

    [Header("체력")]
    public float maxHealth = 100f;
    public float health = 100f;

    public float normalHealthLoss = 4f;

    // 서식지 밖에서 최대 체력의 10%
    public float dangerHealthLossPercent = 0.1f;

    [Header("먹이")]
    public GameObject foodPrefab;

    [Header("번식")]
    [Range(0f, 1f)]
    public float reproductionChance = 0.2f;

    private Vector2 targetPosition;

    void Start()
    {
        health = maxHealth;

        ChooseNewTarget();
    }

    void Update()
    {
        MoveFish();
        UpdateHealth();
    }

    // -------------------------
    // 이동
    // -------------------------

    void MoveFish()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, targetPosition) < 0.2f)
        {
            ChooseNewTarget();
        }
    }

    // -------------------------
    // 체력
    // -------------------------

    void UpdateHealth()
    {
        if (IsInsideHabitat())
        {
            // 정상 상태
            health -= normalHealthLoss * Time.deltaTime;
        }
        else
        {
            // 서식지 밖
            float dangerDamage =
                maxHealth * dangerHealthLossPercent;

            health -= dangerDamage * Time.deltaTime;
        }

        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }

    // -------------------------
    // 랜덤 목표 선택
    // -------------------------

    void ChooseNewTarget()
    {
        if (habitats == null || habitats.Length == 0)
        {
            return;
        }

        // 지정된 Background 중 하나 선택
        GameObject habitat =
            habitats[Random.Range(0, habitats.Length)];

        SpriteRenderer sprite =
            habitat.GetComponent<SpriteRenderer>();

        if (sprite == null)
        {
            return;
        }

        Bounds bounds = sprite.bounds;

        float randomX =
            Random.Range(bounds.min.x, bounds.max.x);

        float randomY =
            Random.Range(bounds.min.y, bounds.max.y);

        targetPosition = new Vector2(
            randomX,
            randomY
        );
    }

    // -------------------------
    // 서식지 확인
    // -------------------------

    bool IsInsideHabitat()
    {
        if (habitats == null || habitats.Length == 0)
        {
            return false;
        }

        Vector2 position = transform.position;

        foreach (GameObject habitat in habitats)
        {
            SpriteRenderer sprite =
                habitat.GetComponent<SpriteRenderer>();

            if (sprite == null)
            {
                continue;
            }

            if (sprite.bounds.Contains(position))
            {
                return true;
            }
        }

        return false;
    }

    // -------------------------
    // 먹이 먹기
    // -------------------------

    void OnTriggerEnter2D(Collider2D other)
    {
        if (foodPrefab == null)
        {
            return;
        }

        if (other.gameObject.name == foodPrefab.name ||
            other.gameObject.name.Contains(foodPrefab.name))
        {
            EatFood(other.gameObject);
        }
    }

    void EatFood(GameObject food)
    {
        // 먹이 삭제
        Destroy(food);

        // 20% 확률로 번식
        if (Random.value <= reproductionChance)
        {
            Reproduce();
        }
    }

    // -------------------------
    // 번식
    // -------------------------

    void Reproduce()
    {
        Instantiate(
            gameObject,
            transform.position,
            Quaternion.identity
        );
    }
}