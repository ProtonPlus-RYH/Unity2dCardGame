using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public void MoveForward(bool ifActiver, bool ifUseStamina, int num)
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            if (ifUseStamina)
            {
                Player activer = getUserByPhase();
                if (ifActiver)
                {
                    BattleManager_Single.Instance.changeSP(activer, num * (-1));
                }
                else
                {
                    BattleManager_Single.Instance.changeSP(activer.opponent, num * (-1));
                }
            }
            BattleManager_Single.Instance.changeDistance(num * (-1));
        }
    }

    public void MoveBack(bool ifActiver, bool ifUseStamina, int num)
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            if (ifUseStamina)
            {
                Player activer = getUserByPhase();
                if (ifActiver)
                {
                    BattleManager_Single.Instance.changeSP(activer, num * (-1));
                }
                else
                {
                    BattleManager_Single.Instance.changeSP(activer.opponent, num * (-1));
                }
            }
            BattleManager_Single.Instance.changeDistance(num);
        }
    }

    public void DeclareAttack(bool ifActiverAttack, int distance, int damage)
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            //距离计算
            if (distance <= BattleManager_Single.Instance.distanceInGame)
            {
                //伤害计算
                DealDamage(!ifActiverAttack, damage);//攻击者来看的对手受伤
            }
            else
            {
                Debug.Log("逃出攻击范围，攻击不生效");
                negate(ifActiverAttack);
            }
        }
    }

    public void DealDamage(bool ifActiverHurt, int num)
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            if (ifActiverHurt)
            {
                BattleManager_Single.Instance.changeLP(player, num * (-1));
            }
            else
            {
                BattleManager_Single.Instance.changeLP(player.opponent, num * (-1));
            }
        }
    }

    public void Cure(bool ifActiver, int num)
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            if (ifActiver)
            {
                BattleManager_Single.Instance.changeLP(player, num);
            }
            else
            {
                BattleManager_Single.Instance.changeLP(player.opponent, num);
            }
        }
    }

    public void drawCard(bool ifActiver, int num)
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            if (ifActiver)
            {
                BattleManager_Single.Instance.drawCard(player, num);
            }
            else
            {
                BattleManager_Single.Instance.drawCard(player.opponent, num);
            }
        }
    }

    public void setAllCardIfQuick(bool ifActiver, bool result, int place, int type)
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
    }

    public void negate(bool ifActivingCard)//true就无效自己，false就无效对手的卡
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        Card opponentCard = player.opponent.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            if (ifActivingCard)
            {
                int queueNum = playerCard.setIfNegated(true);
            }
            else
            {
                int queueNum = opponentCard.setIfNegated(true);
            }
        }
    }

    public void canNotBeCountered()
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            EffectTransformer.Instance.ifCanBeCountered = false;
        }
    }

    public void counterDelayed()
    {
        Player player = getUserByPhase();
        Card playerCard = player.FieldZone.GetChild(0).gameObject.GetComponent<CardDisplay>().card;
        if (!playerCard.ifNegated)
        {
            EffectTransformer.Instance.ifCounterDelayed = true;
        }
    }

}
