using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum JudgeTarget
{
    LP, MaxLP, SP, MaxSP, MP, MaxMP, handCard, handZone, deckCard, deckZone, graveCard, graveZone, fieldCard, fieldZone
}

public enum JudgeType
{
    valueGreaterThan, valueLessThan, valueEqualTo, valueNotEqualTo, cardTypeIs, cardTypeIsNot
}

public enum SelectionType
{
    selectTF, selectCard, selectZone
}

public enum SolveTarget
{
    self, opponent, both
}

public class EffectList : MonoBehaviour
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

    public JudgeTarget judgingTarget;//�洢ִ�е��ж�Ŀ��
    public JudgeType judgingType;//�洢ִ�е��ж�����
    public void DoJudge(SolveTarget solveTarget, JudgeTarget judgeTarget, JudgeType judgeType, int judgingReference)
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            //EffectTransformer.Instance.ifPaused = true;
            //EffectTransformer.Instance.ifDoingJudgement = true;
            judgingTarget = judgeTarget;
            judgingType = judgeType;
            switch (judgeTarget)//������̫����Ҫ�õ��ټ�
            {
                case JudgeTarget.fieldCard:
                    bool ifOpponentUsedCard = false;
                    if (player.opponent.FieldZone.childCount != 0)
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
                                        if (player.opponent.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card.GetType() == typeof(AttackCard))
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

    #region common effects

    public void MoveForward(SolveTarget solveTarget, bool ifUseStamina, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (ifUseStamina)
                {
                    Player activer = getUserByPhase();
                    if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                    {
                        BattleManager_Single.Instance.changeSP(activer, num * (-1));
                    }
                    if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                    {
                        BattleManager_Single.Instance.changeSP(activer.opponent, num * (-1));
                    }
                }
                BattleManager_Single.Instance.changeDistance(num * (-1));
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
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (ifUseStamina)
                {
                    Player activer = getUserByPhase();
                    if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                    {
                        BattleManager_Single.Instance.changeSP(activer, num * (-1));
                    }
                    if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                    {
                        BattleManager_Single.Instance.changeSP(activer.opponent, num * (-1));
                    }
                }
                BattleManager_Single.Instance.changeDistance(num);
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
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
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
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
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
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.changeLP(player, num * (-1));
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.changeLP(player.opponent, num * (-1));
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
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.changeLP(player, num);
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.changeLP(player.opponent, num);
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
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.changeSP(player, num);
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.changeSP(player.opponent, num);
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
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.changeMP(player, num);
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.changeMP(player.opponent, num);
                }
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    public void drawCard(SolveTarget solveTarget, int num, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.drawCard(player, num);
                }
                if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                {
                    BattleManager_Single.Instance.drawCard(player.opponent, num);
                }
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    /*public void setAllCardIfQuick(SolveTarget solveTarget, bool result, int place, int type)
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            switch (place)
            {
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    break;
                case 3:

                    break;
            }
        }
    }*/

    public void Negate(SolveTarget solveTarget, bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
            bool ifOpponentCardExists = false;
            if (player.opponent.FieldZone.childCount != 0)
            {
                ifOpponentCardExists = true;
            }

            if (!playerCard.ifNegated)
            {
                if (solveTarget == SolveTarget.opponent && ifOpponentCardExists || solveTarget == SolveTarget.both && ifOpponentCardExists)
                {
                    int queueNum = player.opponent.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card.setIfNegated(true);
                }
                if (solveTarget == SolveTarget.self || solveTarget == SolveTarget.both)
                {
                    int queueNum = playerCard.setIfNegated(true);
                }
            }
            else
            {
                Debug.Log("����Ч");
            }
        }
    }

    public void cannotBeCountered(bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
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

    public void counterDelayed(bool ifJudged)
    {
        if (!ifJudged || EffectTransformer.Instance.judgeResult == true)
        {
            Player player = getUserByPhase();
            Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
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
