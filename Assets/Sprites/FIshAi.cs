using UnityEngine;

public class FishAI : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 2f;

    [Header("활동범위")]
    public GameObject[] habitats;

    [Header("체력")]
    public float maxHealth = 100f;
    public float health = 100f;

    // 활동범위 안에서 초당 감소하는 체력
    public float normalHealthLoss = 4f;

    // 활동범위 밖에서 최대 체력의 10% 감소
    [Range(0f, 1f)]
    public float dangerHealthLossPercent = 0.1f;

    [Header("먹이")]
    public GameObject foodPrefab;

    // 이 거리 안에 들어오면 먹이를 먹음
    public float eatingDistance = 0.5f;

    [Header("번식")]
    [Range(0f, 1f)]
    public float reproductionChance = 0.2f;

    private GameObject nearestFood;


    void Start()
    {
        health = maxHealth;
    }


    void Update()
    {
        // 활동범위 밖인가?
        if (!IsInsideHabitat())
        {
            // 활동범위로 돌아감
            MoveBackToHabitat();
        }
        else
        {
            // 활동범위 안에서는 가장 가까운 먹이를 찾음
            FindNearestFood();

            // 먹이가 있다면 추격
            if (nearestFood != null)
            {
                MoveToFood();

                // 먹이에 닿았는지 확인
                CheckFoodDistance();
            }
        }

        // 체력 감소
        UpdateHealth();
    }


    // ==================================================
    // 활동범위로 돌아가기
    // ==================================================

    void MoveBackToHabitat()
    {
        GameObject nearestHabitat = FindNearestHabitat();

        if (nearestHabitat == null)
        {
            return;
        }

        SpriteRenderer sprite =
            nearestHabitat.GetComponent<SpriteRenderer>();

        if (sprite == null)
        {
            return;
        }

        Bounds bounds = sprite.bounds;

        // 현재 위치에서 가장 가까운 활동범위 안쪽 위치
        float targetX = Mathf.Clamp(
            transform.position.x,
            bounds.min.x,
            bounds.max.x
        );

        float targetY = Mathf.Clamp(
            transform.position.y,
            bounds.min.y,
            bounds.max.y
        );

        Vector2 targetPosition = new Vector2(
            targetX,
            targetY
        );

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }


    // ==================================================
    // 가장 가까운 활동범위 찾기
    // ==================================================

    GameObject FindNearestHabitat()
    {
        if (habitats == null || habitats.Length == 0)
        {
            return null;
        }

        GameObject nearestHabitat = null;

        float nearestDistance = Mathf.Infinity;

        foreach (GameObject habitat in habitats)
        {
            if (habitat == null)
            {
                continue;
            }

            SpriteRenderer sprite =
                habitat.GetComponent<SpriteRenderer>();

            if (sprite == null)
            {
                continue;
            }

            Bounds bounds = sprite.bounds;

            Vector3 closestPoint =
                bounds.ClosestPoint(transform.position);

            float distance =
                Vector2.Distance(
                    transform.position,
                    closestPoint
                );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestHabitat = habitat;
            }
        }

        return nearestHabitat;
    }


    // ==================================================
    // 가장 가까운 먹이 찾기
    // ==================================================

    void FindNearestFood()
    {
        nearestFood = null;

        if (foodPrefab == null)
        {
            return;
        }

        // Food 태그를 가진 모든 먹이 찾기
        GameObject[] foods =
            GameObject.FindGameObjectsWithTag("Food");

        float nearestDistance = Mathf.Infinity;

        foreach (GameObject food in foods)
        {
            if (food == null)
            {
                continue;
            }

            float distance =
                Vector2.Distance(
                    transform.position,
                    food.transform.position
                );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestFood = food;
            }
        }
    }


    // ==================================================
    // 먹이 추격
    // ==================================================

    void MoveToFood()
    {
        if (nearestFood == null)
        {
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            nearestFood.transform.position,
            moveSpeed * Time.deltaTime
        );
    }


    // ==================================================
    // 먹이에 닿았는지 확인
    // ==================================================

    void CheckFoodDistance()
    {
        if (nearestFood == null)
        {
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            nearestFood.transform.position
        );

        if (distance <= eatingDistance)
        {
            EatFood(nearestFood);
        }
    }


    // ==================================================
    // 활동범위 확인
    // ==================================================

    bool IsInsideHabitat()
    {
        if (habitats == null || habitats.Length == 0)
        {
            return false;
        }

        Vector2 position = transform.position;

        foreach (GameObject habitat in habitats)
        {
            if (habitat == null)
            {
                continue;
            }

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


    // ==================================================
    // 체력
    // ==================================================

    void UpdateHealth()
    {
        if (IsInsideHabitat())
        {
            // 활동범위 안
            health -= normalHealthLoss * Time.deltaTime;
        }
        else
        {
            // 활동범위 밖
            float dangerDamage =
                maxHealth * dangerHealthLossPercent;

            health -= dangerDamage * Time.deltaTime;
        }

        // 체력이 0 이하라면 죽음
        if (health <= 0f)
        {
            Destroy(gameObject);
        }
    }


    // ==================================================
    // 먹이 먹기
    // ==================================================

    void EatFood(GameObject food)
    {
        if (food == null)
        {
            return;
        }

        // 먹이 삭제
        Destroy(food);

        // 현재 추적하던 먹이 초기화
        nearestFood = null;

        // 20% 확률로 번식
        if (Random.value <= reproductionChance)
        {
            Reproduce();
        }
    }


    // ==================================================
    // 번식
    // ==================================================

    void Reproduce()
    {
        // 현재 물고기의 위치에서 1마리 추가
        Instantiate(
            gameObject,
            transform.position,
            Quaternion.identity
        );
    }
}