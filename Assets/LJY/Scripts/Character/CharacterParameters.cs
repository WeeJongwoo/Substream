using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterParameter", menuName = "Character/Parameter")]
public class CharacterParameters : ScriptableObject
{
    [SerializeField] private float hp;
    [SerializeField] private float maxHP;
    [SerializeField] private float minHP;
    [SerializeField] private float atk;
    [SerializeField] private float def;
    [SerializeField] private float speed;
    [SerializeField] private float criticalRate;
    [SerializeField] private float criticalDamage;
    [SerializeField] private float aetherRecoveryPoint;

    public float HP { get => hp; set => hp = Mathf.Floor(value); }
    public float MaxHP { get => maxHP; set => maxHP = Mathf.Floor(value); }
    public float MinHP { get => minHP; set => minHP = Mathf.Floor(value); }
    public float ATK { get => atk; set => atk = Mathf.Floor(value); }
    public float DEF { get => def; set => def = Mathf.Floor(value); }
    public float Speed { get => speed; set => speed = Mathf.Floor(value); }
    public float CriticalRate { get => criticalRate; set => criticalRate = Mathf.Floor(value); }
    public float CriticalDamage { get => criticalDamage; set => criticalDamage = Mathf.Floor(value); }
    public float AetherRecoveryPoint { get => aetherRecoveryPoint; set => aetherRecoveryPoint = Mathf.Floor(value); }

    // 애니메이션이나 효과음 등 정보도 추가할 필요 있음
}
