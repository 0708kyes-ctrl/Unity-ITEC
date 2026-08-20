using UnityEngine;

public class FoodTarget : MonoBehaviour
{
    public FishAI targetFish;

    public bool IsTargeted()
    {
        return targetFish != null;
    }

    public void SetTarget(FishAI fish)
    {
        targetFish = fish;
    }

    public void ClearTarget(FishAI fish)
    {
        if (targetFish == fish)
        {
            targetFish = null;
        }
    }
}