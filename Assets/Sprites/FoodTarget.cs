using UnityEngine;

public class FoodTarget : MonoBehaviour
{
    private FishAI targetFish;

    public bool IsTargeted()
    {
        return targetFish != null;
    }

    public bool TryClaim(FishAI fish)
    {
        if (targetFish != null && targetFish != fish)
        {
            return false;
        }

        targetFish = fish;
        return true;
    }

    public void Release(FishAI fish)
    {
        if (targetFish == fish)
        {
            targetFish = null;
        }
    }

    public bool IsClaimedBy(FishAI fish)
    {
        return targetFish == fish;
    }
}