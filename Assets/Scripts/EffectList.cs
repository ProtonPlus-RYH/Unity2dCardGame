using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectTarget
{
    duelist, LP, MaxLP, SP, MaxSP, MP, MaxMP, handCard, handZone, deckCard, deckZone, graveCard, graveZone, fieldCard, fieldZone
}

public enum JudgeType
{
    valueGreaterThan, valueLessThan, valueEqualTo, valueNotEqualTo, cardTypeIs, cardTypeIsNot
}

public enum EffectType
{
    nullEffect, numChange, ifQuickChange, ifActivableChange, ifNegatedChange, useCountINTurnChange, useCountInDuelChange, limitAdd, delayEffect
}

public enum BuffLast
{
    turnLast, actionLast, cardLast, eternal
}

public enum SelectionType
{
    selectTF, selectCard, selectZone
}

public enum SolveTarget
{
    self, opponent, both
}

public class EffectList// : MonoBehaviour
{
    public Player getUserByPhase()
    {
        Player result = EffectTransformer.Instance.activingCard.holdingPlayer;
        switch (EffectTransformer.Instance.processPhase)
        {
            case SolvingProcess.counterDeclare:
                result = result.opponent;
                break;
            case SolvingProcess.counterResolve:
                result = result.opponent;
                break;
            case SolvingProcess.delayResolve:
                result = result.opponent;
                break;
        }
        return result;
    }

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
    public void DoSelection(SolveTarget solveTarget, SelectionType selectionType)
    {
        EffectTransformer.Instance.ifPaused = true;
        EffectTransformer.Instance.ifDoingSelection = true;
        selectingType = selectionType;
    }

    public EffectTarget judgingTarget;//�洢ִ�е��ж�Ŀ��
    public JudgeType judgingType;//�洢ִ�е��ж�����
    public void DoJudge(SolveTarget solveTarget, EffectTarget effectTarget, JudgeType judgeType, int judgingReference)
    {
        Player player = getUserByPhase();
        Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            //EffectTransformer.Instance.ifPaused = true;
            //EffectTransformer.Instance.ifDoingJudgement = true;
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
            }
        }
        else
        {
            Debug.Log("�жϱ���Ч");
        }
    }


    public void GiveBuff(SolveTarget solveTarget, EffectTarget effectTarget, EffectType effectType, int effectReference, List<BuffLast> buffLast, List<int> lastReference)
    {
        Player player = getUserByPhase();
        Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            switch (effectTarget)
            {
                case EffectTarget.duelist:
                    break;
                case EffectTarget.LP:
                    break;
                case EffectTarget.MaxLP:
                    break;
                case EffectTarget.SP:
                    break;
                case EffectTarget.MaxSP:
                    break;
                case EffectTarget.MP:
                    break;
                case EffectTarget.MaxMP:
                    break;
                case EffectTarget.deckCard:
                    break;
                case EffectTarget.handCard:
                    break;
                case EffectTarget.graveCard:
                    break;
                case EffectTarget.fieldCard:
                    break;
                case EffectTarget.handZone:
                    if(solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                    {
                        player.opponent.handZone.AddBuff(new Buff(solveTarget, effectTarget, effectType, effectReference, buffLast, lastReference));
                    }
                    break;
                /*default://������Ч��

                    EffectForZone(solveTarget, effectTarget, effectType, effectReference, buffLast, lastReference);

                    break;*/
            }
        }
    }

    /*public void EffectForZone(SolveTarget solveTarget, EffectTarget effectTarget, EffectType effectType, int effectReference, List<BuffLast> buffLast, List<int> lastReference)
    {
        Player player = getUserByPhase();
        Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            switch (effectTarget)
            {
                case EffectTarget.handZone:
                    switch (effectType)
                    {
                        case EffectType.ifQuickChange:
                            if(solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both) 
                            {
                                Buff buff = new Buff(player, solveTarget, effectTarget, effectType, effectReference, buffLast, lastReference);
                                player.opponent.handZoneTransform.gameObject.GetComponent<Zone>().AddBuff(buff);
                            }
                            break;
                    }
                    break;
                case EffectTarget.deckZone:

                    break;
                case EffectTarget.graveZone:

                    break;
                case EffectTarget.fieldZone:

                    break;
                default:
                    Debug.Log("��������");
                    break;
            }
        }
    }*/

    #region common effects

    public void MoveForward(SolveTarget solveTarget, bool ifUseStamina, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (ifUseStamina)
                {
                    if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                    {
                        BattleManager_Single.Instance.ChangeSP(player, num * (-1));
                    }
                    if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                    {
                        BattleManager_Single.Instance.ChangeSP(player.opponent, num * (-1));
                    }
                }
                BattleManager_Single.Instance.ChangeDistance(num * (-1));
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    public void MoveBack(SolveTarget solveTarget, bool ifUseStamina, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (ifUseStamina)
                {
                    if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                    {
                        BattleManager_Single.Instance.ChangeSP(player, num * (-1));
                    }
                    if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                    {
                        BattleManager_Single.Instance.ChangeSP(player.opponent, num * (-1));
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
            Player player = getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            var attackCard = playerCard as AttackCard;
            int damage = attackCard.attackPower_current;
            int distance = attackCard.distance_current;
            if (!playerCard.ifNegated)
            {
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
            Player player = getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
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
            Player player = getUserByPhase();
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
                Debug.Log("����Ч");
            }
        }
    }

    public void Cure(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
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
                Debug.Log("����Ч");
            }
        }
    }

    public void RestoreSP(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
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
                Debug.Log("����Ч");
            }
        }
    }

    public void RestoreMP(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
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
                Debug.Log("����Ч");
            }
        }
    }

    public void DrawCard(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
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
                Debug.Log("����Ч");
            }
        }
    }

    public void Negate(SolveTarget solveTarget, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
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
                Debug.Log("����Ч");
            }
        }
    }

    public void CannotBeCountered(bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
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
            Player player = getUserByPhase();
            Card playerCard = player.fieldZoneTransform.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
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

}
