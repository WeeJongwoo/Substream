using System.Collections.Generic;

public class UIFacade : IManagerFacade
{
    public readonly MasterManager MasterManager;

    public readonly CharacterUIManager CharacterUIManager;
    public readonly MonsterUIManager MonsterUIManager;
    public readonly CardUIManager CardUIManager;
    public readonly TurnUIManager TurnUIManager;

    private Queue<UIEvent> m_UIEventActionQueue;

    public bool EventQueueIsNotEmpty()
    {
        return m_UIEventActionQueue.Count > 0;
    }

    public void Execute()
    {
        m_UIEventActionQueue.Dequeue().Execute();
    }

    public UIFacade(MasterManager masterManager,
        CharacterUIManager characterUIManager,
        MonsterUIManager monsterUIManager,
        CardUIManager cardUIManager,
        TurnUIManager turnUIManager)
    {
        this.CharacterUIManager = characterUIManager;
        MonsterUIManager = monsterUIManager;
        CardUIManager = cardUIManager;
        TurnUIManager = turnUIManager;
        this.MasterManager = masterManager;

        m_UIEventActionQueue = new Queue<UIEvent>();
    }

    // 전투 관련
    public void ApplyDamage(bool castUnitIsCharacter, int castUnitPos, bool targetUnitIsCharacter, int targetUnitPos, float amount)
    {
        m_UIEventActionQueue.Enqueue(new UIDamageEvent(this, castUnitIsCharacter,  castUnitPos, targetUnitIsCharacter, targetUnitPos, (int) amount));
    }

    public void ApplyHeal(bool castUnitIsCharacter, int castUnitPos, bool targetUnitIsCharacter, int targetUnitPos, float amount)
    {
        m_UIEventActionQueue.Enqueue(new UIHealEvent(this, castUnitIsCharacter,  castUnitPos, targetUnitIsCharacter, targetUnitPos, (int) amount));
    }

    // 카드 관련
    public void DrawCard(int amount)
    {
        m_UIEventActionQueue.Enqueue(new UIDrawCardEvent(this, amount));
    }

    public void DiscardCard(ActionContext context)
    {

    }

    public void UnitDead(Unit unit)
    {
        m_UIEventActionQueue.Enqueue(new UIUnitDeathEvent(this, unit));
    }

    // 턴 관련
    public void EndTurn()
    {
        m_UIEventActionQueue.Enqueue(new UIEndTurnEvent(this, MasterManager));
    }

    // 상태 관련
    public void AddStatusEffect(bool targetIsCharacter, int targetPosition, ESkillStatusType effect, int duration, Unit caster)
    {

    }

    public bool IsAlive(Unit unit)
    {
        return true;
    }

    public Unit GetCurrentUnit()
    {
        return null;
    }
}
