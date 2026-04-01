using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ActionContext
{
    public bool IsDone;

    public ESkillType SkillType;
    public ESkillTrigger SkillTrigger;
    public int TriggerConditionValue;

    public ESkillSource SkillSource;
    public ESkillStatusType StatusType;

    public bool IsCritical;
    public float CriticalValueRate;
    public float Value;
    public ESkillSource TargetSource;

    public List<TargetPair> TargetUnits;

    public SkillTableData SkillData;
}

public struct TargetPair
{
    public bool isCharacter;
    public int position;

    public TargetPair(bool isCharacter, int position)
    {
        this.isCharacter = isCharacter;
        this.position = position;
    }
}

public class Skill
{
    public ActionContext Context;
    public Unit CasterUnit { get; private set; }
    //public int UnitSpeed = 0;
    //public bool IsCharacter = true;
    //// 전투 관련 스탯
    //public bool HasBleed = false;
    //public bool HasParalyse = false;
    //public GameObject thisObject;
    //public int position;
    //public float MaxHP;

    public SkillTableData SkillData;
    //  public int ID;
    //  public ECardSkillType SkillType;
    //  public ECardSkillSource SkillSource;
    //  public float EffectValue;
    //  public float UpgradeEffectValue;
    //  public ECardSkillStatusType StatusType;
    //  public ECardSkillTargetType TargetType;
    //  public int TargetCount;
    //  public int HitCount;
    //  public string CardText;
    //  public int NextSkillID;
    //  public string Sound;

    public CardTableData CasterCard;
    //    public int ID;
    //    public string Name;
    //    public ECardType CardType;
    //    public ECardRarity CardRarity;
    //    public int Cost;
    //    public string CardText;
    //    public string Texture;
    //    public int SkillID;
    public Skill(Unit caster, SkillTableData SkillData, CardTableData CasterCard)
    {
        CasterUnit = caster;
        this.SkillData = SkillData;
        this.CasterCard = CasterCard;
        
        Context = new ActionContext();
        Context.SkillData = SkillData;
        Context.IsCritical = false;
        Context.Value = 0;
        Context.TargetUnits = new List<TargetPair>();
        Context.StatusType = ESkillStatusType.E_NONE;
        Context.SkillType = ESkillType.E_DEFAULT;
        Context.SkillSource = ESkillSource.E_NONE;
        Context.StatusType = SkillData.StatusType;
        Context.SkillType = SkillData.SkillType;
        Context.SkillSource = SkillData.SkillSource;
    }

    public void Initialize() { }
}

public class UnitSkill
{
    public Unit CasterUnit { get; private set; }
    public SkillTableData SkillData;
    public CardTableData CasterCard;

    public UnitSkill(Unit caster, SkillTableData SkillData, CardTableData CasterCard)
    {
        CasterUnit = caster;
        this.SkillData = SkillData;
        this.CasterCard = CasterCard;
    }

    public void Initialize() { }
}