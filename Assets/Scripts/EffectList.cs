using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum EffectTarget
{
    duelist, LP, MaxLP, SP, MaxSP, MP, MaxMP, CurrentDistance, handCard, handZone, deckCard, deckZone, graveCard, graveZone, fieldCard, fieldZone, holdingCard
}

public enum JudgeType
{
    valueGreaterThan, valueLessThan, valueEqualTo, valueNotEqualTo, cardTypeIs, ifDistanceChanged
}

public enum EffectType
{
    nullEffect, numChange, atkChange, ifQuickChange, ifActivableChange, ifNegatedChange, useCountINTurnChange, useCountInDuelChange, banCard, moveBeforeAttack, damageTargetChange
}

public enum BuffLast
{
    turnLast, turnLast_opponent, actionLast, attackLast_Self, cardLast, eternal
}

public enum SelectionType
{
    selectTF, selectCard, selectZone, selectAction, selectMovementWithCancel
}

public enum SolveTarget
{
    self, opponent, both
}

public class EffectList //: MonoSingleton<EffectList>
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

    public SelectionType selectingType;//�洢ִ�е�ѡ������
    public int selectTimes = 0;
    public void DoSelection(SolveTarget solveTarget, SelectionType selectionType, int selectQuantity, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
            if (!playerCard.ifNegated)
            {
                selectingType = selectionType;
                selectTimes = selectQuantity;
                EffectTransformer.Instance.ifPaused = true;
                EffectTransformer.Instance.ifDoingSelection = true;
                switch (selectionType)
                {
                    case SelectionType.selectMovementWithCancel:
                        BattleManager_Single.Instance.AskingDialog_Title.text = "��ѡ��ǰ�����ߺ���";
                        BattleManager_Single.Instance.AskingDialog.SetActive(true);
                        BattleManager_Single.Instance.MoveForwardClick += MoveForwardWithSelection;
                        BattleManager_Single.Instance.MoveBackClick += MoveBackWithSelection;
                        BattleManager_Single.Instance.CancelClick += SelectionEnd_Event;
                        break;
                    case SelectionType.selectTF://����judge�Ľ��
                        BattleManager_Single.Instance.AskingDialog_2_Title.text = "�Ƿ�ִ����һ��Ч��";
                        BattleManager_Single.Instance.AskingDialog_2.SetActive(true);
                        BattleManager_Single.Instance.CancelClick += SelectCancel;
                        BattleManager_Single.Instance.ConfirmClick += SelectConfirm;
                        break;
                }
            }
        }
    }

    public EffectTarget judgingTarget;//�洢ִ�е��ж�Ŀ��
    public JudgeType judgingType;//�洢ִ�е��ж�����
    public void DoJudge(SolveTarget solveTarget, EffectTarget effectTarget, JudgeType judgeType, int judgingReference)
    {
        Player player = EffectTransformer.Instance.getUserByPhase();
        Card playerCard = EffectTransformer.Instance.solvingCard;
        if (!playerCard.ifNegated)
        {
            judgingTarget = effectTarget;
            judgingType = judgeType;
            switch (effectTarget)//������̫����Ҫ�õ��ټ�
            {
                case EffectTarget.fieldCard:
                    bool ifOpponentUsedCard = false;
                    if (player.opponent.fieldZone.cardCount() != 0)
                    {
                        ifOpponentUsedCard = true;
                    }
                    if (solveTarget == SolveTarget.opponent && !ifOpponentUsedCard){EffectTransformer.Instance.judgeResult = false;}//����û��ֱ�ӷ���false
                    else{
                        switch (judgeType)
                        {
                            case JudgeType.cardTypeIs://1Ϊ������0Ϊ�ж�
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
                case EffectTarget.CurrentDistance:
                    switch (judgeType)
                    {
                        case JudgeType.ifDistanceChanged:
                            if (BattleManager_Single.Instance.distanceInGame == EffectTransformer.Instance.distanceWhileDeclareActivation)
                            {
                                EffectTransformer.Instance.judgeResult = false;
                            }
                            else
                            {
                                EffectTransformer.Instance.judgeResult = true;
                            }
                            break;
                    }
                    break;
            }
        }
        else
        {
            Debug.Log("�жϱ���Ч");
        }
    }

    public void GiveBuff(SolveTarget solveTarget, EffectTarget effectTarget, EffectType effectType, int effectReference, List<BuffLast> buffLast, List<int> lastReference, bool ifJudged)
    {

        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
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
                Debug.Log("����Ч");
            }
        }
    }


    #region common effects

    public void Move(SolveTarget solveTarget, bool ifUseStamina, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
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
                Debug.Log("����Ч");
            }
        }
    }


    public void DeclareAttackByCard(bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
            var attackCard = playerCard as AttackCard;
            int damage = attackCard.attackPower_current;
            int distance = attackCard.distance_current;
            if (!playerCard.ifNegated)
            {
                BattleManager_Single.Instance.AttackDeclare(player);
                //�������
                if (distance >= BattleManager_Single.Instance.distanceInGame)
                {
                    //�˺�����
                    DealDamage(SolveTarget.opponent, damage, false);
                }
                else
                {
                    Debug.Log("�ӳ�������Χ����������Ч");
                    Negate(SolveTarget.self, false);
                }
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    public void DeclareAttackByEffect(SolveTarget attacker, int distance, int damage, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
            if (!playerCard.ifNegated)
            {
                if (attacker == SolveTarget.self)
                {
                    BattleManager_Single.Instance.AttackDeclare(player);
                }
                else
                {
                    BattleManager_Single.Instance.AttackDeclare(player.opponent);
                }
                //�������
                if (distance <= BattleManager_Single.Instance.distanceInGame)
                {
                    //�˺�����
                    DealDamage(getOpponent(attacker), damage, false);//�����������Ķ�������
                }
                else
                {
                    Debug.Log("�ӳ�������Χ����������Ч");
                    Negate(attacker, false);//Ч���öԷ����й������ӿջ���Ч�Է���ʱ���ڷ����Ŀ���Ч����֮����Ҫ��Ҫ��
                }
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    public void DealDamage(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    if (player.damageTarget == EffectTarget.LP)
                    {
                        BattleManager_Single.Instance.ChangeLP(player, num * (-1));
                        player.DamageAppear(num);
                    }else if (player.damageTarget == EffectTarget.SP)
                    {
                        BattleManager_Single.Instance.ChangeSP(player, num * (-1));
                    }
                    else if (player.opponent.damageTarget == EffectTarget.MP)
                    {
                        BattleManager_Single.Instance.ChangeMP(player, num * (-1));
                    }
                    else
                    {
                        Debug.Log("�˺�Ŀ�겻�Ϸ�");
                    }
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    if (player.opponent.damageTarget == EffectTarget.LP)
                    {
                        BattleManager_Single.Instance.ChangeLP(player.opponent, num * (-1));
                        player.opponent.DamageAppear(num);
                    }
                    else if (player.opponent.damageTarget == EffectTarget.SP)
                    {
                        BattleManager_Single.Instance.ChangeSP(player.opponent, num * (-1));
                    }
                    else if (player.opponent.damageTarget == EffectTarget.MP)
                    {
                        BattleManager_Single.Instance.ChangeMP(player.opponent, num * (-1));
                    }
                    else
                    {
                        Debug.Log("�˺�Ŀ�겻�Ϸ�");
                    }
                }
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    public void Cure(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
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
                Debug.Log("����Ч");
            }
        }
    }

    public void DecreaseSP(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
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
                Debug.Log("����Ч");
            }
        }
    }

    public void RestoreSP(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
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
                Debug.Log("����Ч");
            }
        }
    }

    public void DecreaseMP(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
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
                Debug.Log("����Ч");
            }
        }
    }

    public void RestoreMP(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
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
                Debug.Log("����Ч");
            }
        }
    }

    public void PayCost()
    {
        Player player = EffectTransformer.Instance.getUserByPhase();
        Card playerCard = EffectTransformer.Instance.solvingCard;
        int spCost = playerCard.staminaCost_current;
        int mpCost = playerCard.manaCost_current;
        if (!playerCard.ifNegated)
        {
            DecreaseSP(SolveTarget.self, spCost, false);
            DecreaseMP(SolveTarget.self, mpCost, false);
        }
        else
        {
            Debug.Log("����Ч");
        }
    }

    public void DrawCard(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
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
                Debug.Log("����Ч");
            }
        }
    }

    public void ReturnCardToDeck(SolveTarget solveTarget, EffectTarget effectTarget, int effectTargetReference, bool ifBottom, bool ifShuffle, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
            if (!playerCard.ifNegated)
            {
                switch (effectTarget)
                {
                    case EffectTarget.fieldCard:
                        if (solveTarget == SolveTarget.self)
                        {
                            BattleManager_Single.Instance.FieldCardReturnToDeck(player, ifBottom, ifShuffle);
                        }
                        break;
                }
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    public void ReturnCardToHand(SolveTarget solveTarget, EffectTarget effectTarget, int effectTargetReference, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
            if (!playerCard.ifNegated)
            {
                switch (effectTarget)
                {
                    case EffectTarget.fieldCard:
                        if (solveTarget == SolveTarget.self)
                        {
                            BattleManager_Single.Instance.FieldCardReturnToHand(player);
                        }
                        break;
                }
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    public void Negate(SolveTarget solveTarget, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
            bool ifOpponentCardExists = false;
            if (player.opponent.fieldZone.cardCount() != 0)
            {
                ifOpponentCardExists = true;
            }

            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.opponent && ifOpponentCardExists || solveTarget == SolveTarget.both && ifOpponentCardExists)
                {
                    if (player == EffectTransformer.Instance.activingCard.holdingPlayer)
                    {
                        EffectTransformer.Instance.negatedKey_Counter = player.opponent.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card.SetIfNegated(true);
                    }else if (player == EffectTransformer.Instance.counterCard.holdingPlayer)
                    {
                        EffectTransformer.Instance.negatedKey_Activer = player.opponent.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card.SetIfNegated(true);
                    }
                }
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    if (player == EffectTransformer.Instance.activingCard.holdingPlayer)
                    {
                        EffectTransformer.Instance.negatedKey_Activer = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card.SetIfNegated(true);
                    }
                    else if (player == EffectTransformer.Instance.counterCard.holdingPlayer)
                    {
                        EffectTransformer.Instance.negatedKey_Counter = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card.SetIfNegated(true);
                    }
                }
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    public void CannotBeCountered(bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
            if (!playerCard.ifNegated)
            {
                if (EffectTransformer.Instance.processPhase == SolvingProcess.activationDeclare)
                {
                    EffectTransformer.Instance.ifCanBeCountered = false;
                }
                else
                {
                    Debug.Log("������ʱ��");
                }
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    public void CounterDelayed(bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = EffectTransformer.Instance.getUserByPhase();
            Card playerCard = EffectTransformer.Instance.solvingCard;
            if (!playerCard.ifNegated)
            {
                EffectTransformer.Instance.ifCounterDelayed = true;
            }
            else
            {
                Debug.Log("����Ч");
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
                BattleManager_Single.Instance.AskingDialog.SetActive(false);
                BattleManager_Single.Instance.MoveForwardClick -= MoveForwardWithSelection;
                BattleManager_Single.Instance.MoveBackClick -= MoveBackWithSelection;
                BattleManager_Single.Instance.CancelClick -= SelectionEnd_Event;
                break;
            case SelectionType.selectTF:
                BattleManager_Single.Instance.AskingDialog_2.SetActive(false);
                BattleManager_Single.Instance.ConfirmClick -= SelectConfirm;
                BattleManager_Single.Instance.CancelClick -= SelectCancel;
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

    public void SelectConfirm(object sender, EventArgs e)
    {
        EffectTransformer.Instance.judgeResult = true;
        SelectionEnd();
    }

    public void SelectCancel(object sender, EventArgs e)
    {
        EffectTransformer.Instance.judgeResult = false;
        SelectionEnd();
    }

    #endregion

    #region Trigger Effects

    public void Move_Trigger(object sender, EventArgs e)
    {

    }

    #endregion
}
