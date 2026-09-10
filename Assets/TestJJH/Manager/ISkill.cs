using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkill
{
    Unit CasterUnit { get; }

    void Initialize();
    void Enter();
    void Execute();
    void End();
}

public class CardSkill : ISkill
{
    public Unit CasterUnit { get; private set; }
    public CardSkillTableData SkillData;
    public CardTableData CasterCard;

    public CardSkill(Unit caster, CardSkillTableData SkillData, CardTableData CasterCard)
    {
        CasterUnit = caster;
        this.SkillData = SkillData;
        this.CasterCard = CasterCard;
    }

    public void Initialize() { }
    public void Enter() { /* 카드 스킬 시작 로직 */ }
    public void Execute() { /* 실행 로직 */ }
    public void End() { /* 종료 로직 */ }
}

public class UnitSkill : ISkill
{
    public Unit CasterUnit { get; private set; }
    public CardSkillTableData SkillData;
    public CardTableData CasterCard;

    public UnitSkill(Unit caster, CardSkillTableData SkillData, CardTableData CasterCard)
    {
        CasterUnit = caster;
        this.SkillData = SkillData;
        this.CasterCard = CasterCard;
    }

    public void Initialize() { }
    public void Enter() { /* 유닛 스킬 시작 로직 */ }
    public void Execute() { /* 실행 로직 */ }
    public void End() { /* 종료 로직 */ }
}