using UnityEngine;

public class FoodTarget : MonoBehaviour
{
    // 현재 이 먹이를 추적하고 있는 물고기
    private FishAI targetFish;

    // 다른 물고기가 추적 중인지 확인
    public bool IsTargeted()
    {
        // 추적하던 물고기가 삭제되었다면
        // 다시 추적할 수 있도록 함
        if (targetFish == null)
        {
            targetFish = null;
            return false;
        }

        return true;
    }

    // 먹이를 차지하려고 시도
    public bool TryClaim(FishAI fish)
    {
        // 이미 다른 물고기가 추적 중
        if (targetFish != null && targetFish != fish)
        {
            return false;
        }

        targetFish = fish;

        return true;
    }

    // 추적 해제
    public void Release(FishAI fish)
    {
        if (targetFish == fish)
        {
            targetFish = null;
        }
    }

    // 특정 물고기가 이 먹이를 추적하고 있는지
    public bool IsClaimedBy(FishAI fish)
    {
        return targetFish == fish;
    }
}