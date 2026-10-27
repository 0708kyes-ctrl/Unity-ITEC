
using UnityEngine;

public class FishAI : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 2f;
    public float wanderChangeTime = 3f;

    [Header("방향 전환")]
    // 원본 이미지가 왼쪽을 향하므로 false
    public bool spriteFacesRight = false;

    private Vector3 originalScale;
    private float lastX;

    [Header("활동범위")]
    public GameObject[] habitats;

    [Header("체력")]
    public float maxHealth = 100f;
    public float health = 100f;
    public float normalHealthLoss = 4f;

    [Range(0f, 1f)]
    public float dangerHealthLossPercent = 0.1f;

    [Header("먹이")]
    public GameObject[] foodPrefabs;
    public float eatingDistance = 0.5f;
    public float foodHealAmount = 15f;

    [Header("번식")]
    public GameObject fishPrefab;

    [Range(0f, 1f)]
    public float reproductionChance = 0.2f;

    private GameObject nearestFood;
    private Vector2 wanderTarget;
    private float wanderTimer;

    void Start()
    {
        health = maxHealth;

        originalScale = transform.localScale;
        lastX = transform.position.x;

        SetNewWanderTarget();
    }

    void Update()
    {
        if (!IsInsideHabitat())
        {
            ReleaseCurrentFood();
            MoveBackToHabitat();
        }
        else
        {
            FindNearestFood();

            if (nearestFood != null)
            {
                MoveToFood();
                CheckFoodDistance();
            }
            else
            {
                Wander();
            }
        }

        // 이동 방향에 따라 물고기 좌우 반전
        UpdateFacingDirection();

        // 체력 감소
        UpdateHealth();
    }

    // 이동 방향에 따라 좌우 반전
    void UpdateFacingDirection()
    {
        float direction = transform.position.x - lastX;

        if (direction > 0.001f)
        {
            // 오른쪽으로 이동
            Vector3 scale = originalScale;

            scale.x = Mathf.Abs(originalScale.x) *
                      (spriteFacesRight ? 1f : -1f);

            transform.localScale = scale;
        }
        else if (direction < -0.001f)
        {
            // 왼쪽으로 이동
            Vector3 scale = originalScale;

            scale.x = Mathf.Abs(originalScale.x) *
                      (spriteFacesRight ? -1f : 1f);

            transform.localScale = scale;
        }

        lastX = transform.position.x;
    }

    // 먹이가 없을 때 자유롭게 이동
    void Wander()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            SetNewWanderTarget();
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            wanderTarget,
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(
            transform.position,
            wanderTarget) < 0.1f)
        {
            SetNewWanderTarget();
        }
    }

    // 새로운 랜덤 이동 위치 선택
    void SetNewWanderTarget()
    {
        GameObject habitat = FindNearestHabitat();

        if (habitat == null) return;

        SpriteRenderer sprite =
            habitat.GetComponent<SpriteRenderer>();

        if (sprite == null) return;

        Bounds bounds = sprite.bounds;

        float randomX = Random.Range(
            bounds.min.x,
            bounds.max.x
        );

        float randomY = Random.Range(
            bounds.min.y,
            bounds.max.y
        );

        wanderTarget = new Vector2(randomX, randomY);
        wanderTimer = wanderChangeTime;
    }

    // 활동범위로 돌아가기
    void MoveBackToHabitat()
    {
        GameObject nearestHabitat = FindNearestHabitat();

        if (nearestHabitat == null) return;

        SpriteRenderer sprite =
            nearestHabitat.GetComponent<SpriteRenderer>();

        if (sprite == null) return;

        Bounds bounds = sprite.bounds;

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

        Vector2 targetPosition = new Vector2(targetX, targetY);

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }

    // 가장 가까운 활동범위 찾기
    GameObject FindNearestHabitat()
    {
        if (habitats == null || habitats.Length == 0)
            return null;

        GameObject nearestHabitat = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject habitat in habitats)
        {
            if (habitat == null) continue;

            SpriteRenderer sprite =
                habitat.GetComponent<SpriteRenderer>();

            if (sprite == null) continue;

            Bounds bounds = sprite.bounds;

            Vector3 closestPoint =
                bounds.ClosestPoint(transform.position);

            float distance = Vector2.Distance(
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

    // 가장 가까운 먹이 찾기
    void FindNearestFood()
    {
        if (nearestFood != null)
        {
            FoodTarget currentTarget =
                nearestFood.GetComponent<FoodTarget>();

            if (currentTarget != null &&
                currentTarget.IsClaimedBy(this))
            {
                return;
            }

            nearestFood = null;
        }

        if (foodPrefabs == null || foodPrefabs.Length == 0)
            return;

        GameObject[] foods =
            GameObject.FindGameObjectsWithTag("Food");

        GameObject bestFood = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject food in foods)
        {
            if (food == null) continue;

            if (!IsFoodTypeAllowed(food)) continue;

            if (!IsPositionInsideHabitat(food.transform.position))
                continue;

            FoodTarget foodTarget =
                food.GetComponent<FoodTarget>();

            if (foodTarget == null) continue;

            if (foodTarget.IsTargeted()) continue;

            float distance = Vector2.Distance(
                transform.position,
                food.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                bestFood = food;
            }
        }

        if (bestFood != null)
        {
            FoodTarget target =
                bestFood.GetComponent<FoodTarget>();

            if (target != null && target.TryClaim(this))
            {
                nearestFood = bestFood;
            }
        }
    }

    // 먹이 종류 확인
    bool IsFoodTypeAllowed(GameObject food)
    {
        foreach (GameObject foodPrefab in foodPrefabs)
        {
            if (foodPrefab == null) continue;

            string prefabName = foodPrefab.name;

            if (food.name == prefabName ||
                food.name.StartsWith(prefabName + "("))
            {
                return true;
            }
        }

        return false;
    }

    // 먹이에게 이동
    void MoveToFood()
    {
        if (nearestFood == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            nearestFood.transform.position,
            moveSpeed * Time.deltaTime
        );
    }

    // 먹이와의 거리 확인
    void CheckFoodDistance()
    {
        if (nearestFood == null) return;

        float distance = Vector2.Distance(
            transform.position,
            nearestFood.transform.position
        );

        if (distance <= eatingDistance)
        {
            EatFood(nearestFood);
        }
    }

    // 먹기
    void EatFood(GameObject food)
    {
        if (food == null) return;

        FoodTarget target = food.GetComponent<FoodTarget>();

        if (target != null)
        {
            target.Release(this);
        }

        Destroy(food);
        nearestFood = null;

        health += foodHealAmount;
        health = Mathf.Min(health, maxHealth);

        Reproduce();
        SetNewWanderTarget();
    }

    // 현재 먹이 추적 포기
    void ReleaseCurrentFood()
    {
        if (nearestFood == null) return;

        FoodTarget target =
            nearestFood.GetComponent<FoodTarget>();

        if (target != null)
        {
            target.Release(this);
        }

        nearestFood = null;
    }

    // 활동범위 안에 있는지 확인
    bool IsInsideHabitat()
    {
        return IsPositionInsideHabitat(transform.position);
    }

    bool IsPositionInsideHabitat(Vector2 position)
    {
        if (habitats == null || habitats.Length == 0)
            return false;

        foreach (GameObject habitat in habitats)
        {
            if (habitat == null) continue;

            SpriteRenderer sprite =
                habitat.GetComponent<SpriteRenderer>();

            if (sprite == null) continue;

            if (sprite.bounds.Contains(position))
            {
                return true;
            }
        }

        return false;
    }

    // 체력 감소
    void UpdateHealth()
    {
        if (IsInsideHabitat())
        {
            health -= normalHealthLoss * Time.deltaTime;
        }
        else
        {
            float dangerDamage =
                maxHealth * dangerHealthLossPercent;

            health -= dangerDamage * Time.deltaTime;
        }

        if (health <= 0f)
        {
            ReleaseCurrentFood();
            Destroy(gameObject);
        }
    }

    // 번식
    void Reproduce()
    {
        if (fishPrefab == null)
        {
            Debug.LogWarning(
                "Fish Prefab이 설정되지 않았습니다."
            );

            return;
        }

        if (Random.value <= reproductionChance)
        {
            Vector2 offset =
                Random.insideUnitCircle * 0.5f;

            Vector3 spawnPosition =
                transform.position +
                new Vector3(offset.x, offset.y, 0f);

            GameObject newFishObject = Instantiate(
                fishPrefab,
                spawnPosition,
                Quaternion.identity
            );

            FishAI newFish =
                newFishObject.GetComponent<FishAI>();

            if (newFish != null)
            {
                newFish.habitats = habitats;
                newFish.foodPrefabs = foodPrefabs;
                newFish.fishPrefab = fishPrefab;
            }

            Debug.Log("번식 성공!");
        }
    }
}
