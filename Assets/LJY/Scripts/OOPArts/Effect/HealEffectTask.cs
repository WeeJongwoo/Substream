using UnityEngine;

/// <summary>
/// 지정된 대상을 1회 회복하는 효과
/// </summary>
public class HealEffectTask : IEffectTask
{
    private Unit _target;
    private int _healAmount;

    private bool _isInitialized = false;
    private float _timer = 0f;
    private float _delayTime;

    public HealEffectTask(Unit target, int healAmount, float delayTime = 0.5f)
    {
        _target = target;
        _healAmount = healAmount;
        _delayTime = delayTime;
    }

    public bool ExecuteAndCheckCompletion()
    {
        if (!_isInitialized) {
            // 회복 대상을 향한 Heal() 호출
            // 회복 파티클 호출 코드

            _isInitialized = true;
        }

        _timer += Time.deltaTime;
        if (_timer < _delayTime) {
            return false;
        }

        return true;
    }
}