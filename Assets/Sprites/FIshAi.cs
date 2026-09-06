using UnityEngine;

public class FishAI : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 2f;

    // 먹이가 없을 때 이동할 목표를 바꾸는 시간
    public float wanderChangeTime = 3f;

    [Header("활동범위")]
    public GameObject[] habitats;

    [Header("체력")]
    public float maxHealth = 100f;
    public float health = 100f;

    // 활동범위 안에서 초당 감소
    public float normalHealthLoss = 4f;

    // 활동범위 밖에서 최대 체력의 10% 감소
    [Range(0f, 1f)]
    public float dangerHealthLossPercent = 0.1f;

    [Header("먹이")]
    public GameObject[] foodPrefabs;

    // 먹이에 닿았다고 판단하는 거리
    public float eatingDistance = 0.5f;

    // 먹었을 때 회복하는 체력
    public float foodHealAmount = 15f;

    [Header("번식")]
    public GameObject fishPrefab;

    [Range(0f, 1f)]
    public float reproductionChance = 0.2f;

    // 현재 추적 중인 먹이
    private GameObject nearestFood;

    // 먹이가 없을 때 이동할 위치
    private Vector2 wanderTarget;

    private float wanderTimer;


    // ==================================================
    // 시작
    // ==================================================

    void Start()
    {
        health = maxHealth;

        // 처음부터 랜덤한 곳으로 이동
        SetNewWanderTarget();
    }


    // ==================================================
    // 업데이트
    // ==================================================

    void Update()
    {
        // ------------------------------------------
        // 활동범위 밖
        // ------------------------------------------

        if (!IsInsideHabitat())
        {
            ReleaseCurrentFood();

            MoveBackToHabitat();
        }

        // ------------------------------------------
        // 활동범위 안
        // ------------------------------------------

        else
        {
            // 먹이 찾기
            FindNearestFood();

            // --------------------------------------
            // 먹이가 있으면 먹이를 추적
            // --------------------------------------

            if (nearestFood != null)
            {
                MoveToFood();

                CheckFoodDistance();
            }

            // --------------------------------------
            // 먹이가 없으면 자유롭게 이동
            // --------------------------------------

            else
            {
                Wander();
            }
        }

        // 체력 감소
        UpdateHealth();
    }


    // ==================================================
    // 먹이가 없을 때 자유롭게 이동
    // ==================================================

    void Wander()
    {
        wanderTimer -= Time.deltaTime;

        // 일정 시간이 지나면 새로운 위치 선택
        if (wanderTimer <= 0f)
        {
            SetNewWanderTarget();
        }

        // 목표 위치로 이동
        transform.position = Vector2.MoveTowards(
            transform.position,
            wanderTarget,
            moveSpeed * Time.deltaTime
        );

        // 목표 위치에 도착하면 새로운 위치 선택
        if (Vector2.Distance(
            transform.position,
            wanderTarget) < 0.1f)
        {
            SetNewWanderTarget();
        }
    }


    // ==================================================
    // 새로운 랜덤 이동 위치 선택
    // ==================================================

    void SetNewWanderTarget()
    {
        GameObject habitat = FindNearestHabitat();

        if (habitat == null)
        {
            return;
        }

        SpriteRenderer sprite =
            habitat.GetComponent<SpriteRenderer>();

        if (sprite == null)
        {
            return;
        }

        Bounds bounds = sprite.bounds;

        float randomX = Random.Range(
            bounds.min.x,
            bounds.max.x
        );

        float randomY = Random.Range(
            bounds.min.y,
            bounds.max.y
        );

        wanderTarget = new Vector2(
            randomX,
            randomY
        );

        wanderTimer = wanderChangeTime;
    }


    // ==================================================
    // 활동범위로 돌아가기
    // ==================================================

    void MoveBackToHabitat()
    {
        GameObject nearestHabitat =
            FindNearestHabitat();

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

        Vector2 targetPosition =
            new Vector2(
                targetX,
                targetY
            );

        transform.position =
            Vector2.MoveTowards(
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
        if (habitats == null ||
            habitats.Length == 0)
        {
            return null;
        }

        GameObject nearestHabitat = null;

        float nearestDistance =
            Mathf.Infinity;

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

            Bounds bounds =
                sprite.bounds;

            Vector3 closestPoint =
                bounds.ClosestPoint(
                    transform.position
                );

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
        // ------------------------------------------
        // 현재 먹이를 계속 추적 중인지 확인
        // ------------------------------------------

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


        // ------------------------------------------
        // 먹이 설정 확인
        // ------------------------------------------

        if (foodPrefabs == null ||
            foodPrefabs.Length == 0)
        {
            return;
        }


        // ------------------------------------------
        // Food 태그를 가진 모든 오브젝트 찾기
        // ------------------------------------------

        GameObject[] foods =
            GameObject.FindGameObjectsWithTag(
                "Food"
            );

        GameObject bestFood = null;

        float nearestDistance =
            Mathf.Infinity;


        // ------------------------------------------
        // 먹이 검사
        // ------------------------------------------

        foreach (GameObject food in foods)
        {
            if (food == null)
            {
                continue;
            }

            // 먹을 수 있는 종류인지 확인
            if (!IsFoodTypeAllowed(food))
            {
                continue;
            }

            // 서식지 밖의 먹이는 무시
            if (!IsPositionInsideHabitat(
                food.transform.position))
            {
                continue;
            }

            FoodTarget foodTarget =
                food.GetComponent<FoodTarget>();

            if (foodTarget == null)
            {
                continue;
            }

            // 다른 물고기가 추적 중이면 무시
            if (foodTarget.IsTargeted())
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
                bestFood = food;
            }
        }


        // ------------------------------------------
        // 먹이 선점
        // ------------------------------------------

        if (bestFood != null)
        {
            FoodTarget target =
                bestFood.GetComponent<FoodTarget>();

            if (target != null &&
                target.TryClaim(this))
            {
                nearestFood = bestFood;
            }
        }
    }


    // ==================================================
    // 먹이 종류 확인
    // ==================================================

    bool IsFoodTypeAllowed(GameObject food)
    {
        foreach (GameObject foodPrefab in foodPrefabs)
        {
            if (foodPrefab == null)
            {
                continue;
            }

            string prefabName =
                foodPrefab.name;

            if (food.name == prefabName ||
                food.name.StartsWith(
                    prefabName + "("))
            {
                return true;
            }
        }

        return false;
    }


    // ==================================================
    // 먹이에게 이동
    // ==================================================

    void MoveToFood()
    {
        if (nearestFood == null)
        {
            return;
        }

        transform.position =
            Vector2.MoveTowards(
                transform.position,
                nearestFood.transform.position,
                moveSpeed * Time.deltaTime
            );
    }


    // ==================================================
    // 먹이와의 거리 확인
    // ==================================================

    void CheckFoodDistance()
    {
        if (nearestFood == null)
        {
            return;
        }

        float distance =
            Vector2.Distance(
                transform.position,
                nearestFood.transform.position
            );

        if (distance <= eatingDistance)
        {
            EatFood(nearestFood);
        }
    }


    // ==================================================
    // 먹기
    // ==================================================

    void EatFood(GameObject food)
    {
        if (food == null)
        {
            return;
        }

        FoodTarget target =
            food.GetComponent<FoodTarget>();

        if (target != null)
        {
            target.Release(this);
        }

        Destroy(food);

        nearestFood = null;

        // 체력 회복
        health += foodHealAmount;

        health = Mathf.Min(
            health,
            maxHealth
        );

        // 번식
        Reproduce();

        // 먹은 뒤 다시 랜덤 이동 시작
        SetNewWanderTarget();
    }


    // ==================================================
    // 현재 먹이 추적 포기
    // ==================================================

    void ReleaseCurrentFood()
    {
        if (nearestFood == null)
        {
            return;
        }

        FoodTarget target =
            nearestFood.GetComponent<FoodTarget>();

        if (target != null)
        {
            target.Release(this);
        }

        nearestFood = null;
    }


    // ==================================================
    // 활동범위 안에 있는지 확인
    // ==================================================

    bool IsInsideHabitat()
    {
        return IsPositionInsideHabitat(
            transform.position
        );
    }


    bool IsPositionInsideHabitat(
        Vector2 position)
    {
        if (habitats == null ||
            habitats.Length == 0)
        {
            return false;
        }

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
    // 체력 감소
    // ==================================================

    void UpdateHealth()
    {
        if (IsInsideHabitat())
        {
            health -=
                normalHealthLoss *
                Time.deltaTime;
        }
        else
        {
            float dangerDamage =
                maxHealth *
                dangerHealthLossPercent;

            health -=
                dangerDamage *
                Time.deltaTime;
        }

        if (health <= 0f)
        {
            ReleaseCurrentFood();

            Destroy(gameObject);
        }
    }


    // ==================================================
    // 번식
    // ==================================================

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
            // 부모와 겹치지 않도록 약간 떨어진 위치
            Vector2 offset =
                Random.insideUnitCircle * 0.5f;

            Vector3 spawnPosition =
                transform.position +
                new Vector3(
                    offset.x,
                    offset.y,
                    0f
                );


            // 새 물고기 생성
            GameObject newFishObject =
                Instantiate(
                    fishPrefab,
                    spawnPosition,
                    Quaternion.identity
                );


            // 새 물고기의 FishAI 가져오기
            FishAI newFish =
                newFishObject.GetComponent<FishAI>();


            if (newFish != null)
            {
                // 부모와 같은 서식지
                newFish.habitats = habitats;

                // 부모와 같은 먹이
                newFish.foodPrefabs =
                    foodPrefabs;

                // 부모와 같은 번식 대상
                newFish.fishPrefab =
                    fishPrefab;
            }

            Debug.Log("번식 성공!");
        }
    }
}