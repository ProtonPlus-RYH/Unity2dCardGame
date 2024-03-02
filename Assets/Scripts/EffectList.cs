using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectTarget
{
    duelist, LP, MaxLP, SP, MaxSP, MP, MaxMP, handCard, handZone, deckCard, deckZone, graveCard, graveZone, fieldCard, fieldZone, holdingCard
}

public enum JudgeType
{
    valueGreaterThan, valueLessThan, valueEqualTo, valueNotEqualTo, cardTypeIs, cardTypeIsNot
}

public enum EffectType
{
    nullEffect, numChange, atkChange, ifQuickChange, ifActivableChange, ifNegatedChange, useCountINTurnChange, useCountInDuelChange, limitAdd, delayEffect
}

public enum BuffLast
{
    turnLast, turnLast_opponent, actionLast, cardLast, eternal
}

public enum SelectionType
{
    selectTF, selectCard, selectZone, selectAction, selectMovementWithCancel
}

public enum SolveTarget
{
    self, opponent, both
}

public class EffectList// : MonoBehaviour
{
    public SolveTarget getOpponent(SolveTarget input)
    {
        SolveTarget result = SolveTarget.opponent;
        if(input == SolveTarget.opponent)
        {
            result = SolveTarget.self;
        }
        return result;
    }

    public SelectionType selectingType;//存储执行的选择类型
    public int selectTimes = 0;
    public void DoSelection(SolveTarget solveTarget, SelectionType selectionType, int selectQuantity)
    {
        Player player = EffectTransformer.Instance.getUserByPhase();
        Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            selectingType = selectionType;
            selectTimes = selectQuantity;
            EffectTransformer.Instance.ifPaused = true;
            EffectTransformer.Instance.ifDoingSelection = true;
            switch (selectionType)
            {
                case SelectionType.selectMovementWithCancel:
                    EffectTransformer.Instance.AskingDialog_Title.text = "请选择前进或者后退";
                    EffectTransformer.Instance.AskingDialog.SetActive(true);
                    BattleManager_Single.Instance.MoveForwardClick += MoveForwardWithSelection;
                    BattleManager_Single.Instance.MoveBackClick += MoveBackWithSelection;
                    BattleManager_Single.Instance.CancelClick += SelectionEnd_Event;
                    break;
            }
        }
    }

    public EffectTarget judgingTarget;//存储执行的判断目标
    public JudgeType judgingType;//存储执行的判断种类
    public void DoJudge(SolveTarget solveTarget, EffectTarget effectTarget, JudgeType judgeType, int judgingReference)
    {
        Player player = EffectTransformer.Instance.getUserByPhase();
        Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            judgingTarget = effectTarget;
            judgingType = judgeType;
            switch (effectTarget)//可能性太多了要用到再加
            {
                case EffectTarget.fieldCard:
                    bool ifOpponentUsedCard = false;
                    if (player.opponent.fieldZone.cardCount() != 0)
                    {
                        ifOpponentUsedCard = true;
                    }
                    if (solveTarget == SolveTarget.opponent && !ifOpponentUsedCard){EffectTransformer.Instance.judgeResult = false;}//对手没卡直接返回false
                    else{
                        switch (judgeType)
                        {
                            case JudgeType.cardTypeIs://1为攻击，0为行动
                                if (judgingReference == 1)
                                {
                                    if (solveTarget == SolveTarget.opponent)
                                    {
                                        if (player.opponent.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card.GetType() == typeof(AttackCard))
                                        {
                                            EffectTransformer.Instance.judgeResult = true;
                                        }else
                                        {
                                            EffectTransformer.Instance.judgeResult = false;
                                        }
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }
        }
        else
        {
            Debug.Log("判断被无效");
        }
    }

    public void GiveBuff(SolveTarget solveTarget, EffectTarget effectTarget, EffectType effectType, int effectReference, List<BuffLast> buffLast, List<int> lastReference, bool ifJudged)
    {

        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                Buff buff = new Buff(solveTarget, effectTarget, effectType, effectReference, buffLast, lastReference);
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    player.AddBuff(buff);
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    player.opponent.AddBuff(buff);
                }
                    BattleManager_Single.Instance.buffListInGame.Add(buff);
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }


    #region common effects

    public void Move(SolveTarget solveTarget, bool ifUseStamina, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (ifUseStamina)
                {
                    if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                    {
                        BattleManager_Single.Instance.ChangeSP(player, System.Math.Abs(num) * (-1));
                    }
                    if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                    {
                        BattleManager_Single.Instance.ChangeSP(player.opponent, System.Math.Abs(num) * (-1));
                    }
                }
                BattleManager_Single.Instance.ChangeDistance(num);
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }


    public void DeclareAttackByCard(bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            var attackCard = playerCard as AttackCard;
            int damage = attackCard.attackPower_current;
            int distance = attackCard.distance_current;
            if (!playerCard.ifNegated)
            {
                //距离计算
                if (distance >= BattleManager_Single.Instance.distanceInGame)
                {
                    //伤害计算
                    DealDamage(SolveTarget.opponent, damage, false);
                }
                else
                {
                    Debug.Log("逃出攻击范围，攻击不生效");
                    Negate(SolveTarget.self, false);
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void DeclareAttackByEffect(SolveTarget attacker, int distance, int damage, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                //距离计算
                if (distance <= BattleManager_Single.Instance.distanceInGame)
                {
                    //伤害计算
                    DealDamage(getOpponent(attacker), damage, false);//攻击者来看的对手受伤
                }
                else
                {
                    Debug.Log("逃出攻击范围，攻击不生效");
                    Negate(attacker, false);//效果让对方进行攻击并挥空会无效对方此时正在发动的卡的效果，之后考虑要不要改
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void DealDamage(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeLP(player, num * (-1));
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeLP(player.opponent, num * (-1));
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void Cure(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeLP(player, num);
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeLP(player.opponent, num);
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void DecreaseSP(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeSP(player, num*(-1));
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeSP(player.opponent, num*(-1));
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void RestoreSP(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeSP(player, num);
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeSP(player.opponent, num);
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void DecreaseMP(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeMP(player, num * (-1));
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeMP(player.opponent, num * (-1));
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void RestoreMP(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeMP(player, num);
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.ChangeMP(player.opponent, num);
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void PayCost()
    {
        Player player = EffectTransformer.Instance.getUserByPhase();
        Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        int spCost = playerCard.staminaCost_current;
        int mpCost = playerCard.manaCost_current;
        if (!playerCard.ifNegated)
        {
            DecreaseSP(SolveTarget.self, spCost, false);
            DecreaseMP(SolveTarget.self, mpCost, false);
        }
        else
        {
            Debug.Log("被无效");
        }
    }

    public void DrawCard(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.DrawCard(player, num);
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.DrawCard(player.opponent, num);
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void Negate(SolveTarget solveTarget, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            bool ifOpponentCardExists = false;
            if (player.opponent.fieldZone.cardCount() != 0)
            {
                ifOpponentCardExists = true;
            }

            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.opponent && ifOpponentCardExists || solveTarget == SolveTarget.both && ifOpponentCardExists)
                {
                    int queueNum = player.opponent.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card.SetIfNegated(true);
                }
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    int queueNum = playerCard.SetIfNegated(true);
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void CannotBeCountered(bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (EffectTransformer.Instance.processPhase == SolvingProcess.activationDeclare)
                {
                    EffectTransformer.Instance.ifCanBeCountered = false;
                }
                else
                {
                    Debug.Log("已跳过时点");
                }
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    public void CounterDelayed(bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                EffectTransformer.Instance.ifCounterDelayed = true;
            }
            else
            {
                Debug.Log("被无效");
            }
        }
    }

    #endregion

    #region Effects with Selection

    public void SelectionEnd_Event(object sender, EventArgs e)
    {
        SelectionEnd();
    }
    public void SelectionEnd()
    {
        switch (selectingType)
        {
            case SelectionType.selectMovementWithCancel:
                EffectTransformer.Instance.AskingDialog.SetActive(false);
                BattleManager_Single.Instance.MoveForwardClick -= MoveForwardWithSelection;
                BattleManager_Single.Instance.MoveBackClick -= MoveBackWithSelection;
                BattleManager_Single.Instance.CancelClick -= SelectionEnd_Event;
                break;
        }
        EffectTransformer.Instance.selectSolve();
    }

    public void MoveForwardWithSelection(object sender, EventArgs e)
    {
        BattleManager_Single.Instance.ChangeDistance(-1);
        selectTimes--;
        if (selectTimes == 0)
        {
            SelectionEnd();
        }
    }
    public void MoveBackWithSelection(object sender, EventArgs e)
    {
        BattleManager_Single.Instance.ChangeDistance(1);
        selectTimes--;
        if (selectTimes == 0)
        {
            SelectionEnd();
        }
    }

    #endregion
}
