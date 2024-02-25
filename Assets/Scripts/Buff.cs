using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffLastType
{
    turnLast, actionLast, cardLast, duringThisAction, eternal
}

public enum BuffAffectTarget
{
    selfLP, selfMaxLP, selfSP, selfMaxSP, selfMP, selfMaxMP, opponentLP, opponentMaxLP, opponentSP, opponentMaxSP, opponentMP, opponentMaxMP, handCard, handZone, deckCard, deckZone, graveCard, graveZone
}

public class Buff
{
    public bool ifTurn = false;
    public int turnLast;
    public bool ifAction = false;
    public int actionLast;
    public bool ifCard = false;
    public int cardLast;
    public bool ifDuringThisAction = false;
    public bool ifEternal = false;

    public Buff(List<BuffLastType> typeOfLast, List<BuffAffectTarget> typeOfTarget) 
    {
        
    }


    public void countDownDecreaseWhen(BuffLastType buffType)
    {
        switch (buffType)
        {
            case BuffLastType.turnLast:
                if (ifTurn)
                {
                    turnLast--;
                    if(turnLast == 0)
                    {
                        buffEnd();
                    }
                }
                break;
            case BuffLastType.actionLast:
                if (ifAction)
                {
                    actionLast--;
                    if (actionLast == 0)
                    {
                        buffEnd();
                    }
                }
                break;
            case BuffLastType.cardLast:
                if (ifCard)
                {
                    cardLast--;
                    if (cardLast == 0)
                    {
                        buffEnd();
                    }
                }
                break;
        }
        
    }

    public void buffBegin_recordStart()
    {

    }

    public void buffBegin_recordEnd()
    {

    }

    public void buffEnd()
    {

    }
}
