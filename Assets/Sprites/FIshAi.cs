using UnityEngine;

public class FishAI : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 2f;

    [Header("수심 범위")]
    public float minDepth = -3f;
    public float maxDepth = 3f;

    [Header("체력")]
    public float health = 100f;
    public float normalHealthLoss = 1f;
    public float dangerHealthLoss = 5f;

    private Vector2 targetPosition;

    private SpriteRenderer background;

    private float minX;
    private float maxX;

    void Start()
    {
        background = GameObject.Find("Background")
            .GetComponent<SpriteRenderer>();

        Bounds bounds = background.bounds;

        minX = bounds.min.x;
        maxX = bounds.max.x;

        ChooseNewTarget();
    }

    void Update()
    {
        MoveFish();
        UpdateHealth();
    }

    void MoveFish()
    {
        bool outsideDepth =
            transform.position.y < minDepth ||
            transform.position.y > maxDepth;

        if (outsideDepth)
        {
            float targetY = Mathf.Clamp(
                transform.position.y,
                minDepth,
                maxDepth
            );

            targetPosition = new Vector2(
                transform.position.x,
                targetY
            );
        }

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

    void UpdateHealth()
    {
        bool outsideDepth =
            transform.position.y < minDepth ||
            transform.position.y > maxDepth;

        if (outsideDepth)
        {
            health -= dangerHealthLoss * Time.deltaTime;
        }
        else
        {
            health -= normalHealthLoss * Time.deltaTime;
        }

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    void ChooseNewTarget()
    {
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minDepth, maxDepth);

        targetPosition = new Vector2(
            randomX,
            randomY
        );
    }
}