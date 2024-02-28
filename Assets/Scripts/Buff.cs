using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//具体效果都写在预制件的脚本里面,计时存储在这个里面
public class Buff
{
    /*public bool ifTurn;
    public bool ifAction;
    public bool ifCard;
    public bool ifEternal;
    public List<int> effectKey;*/
    public int turnLast;
    public int actionLast;
    public int cardLast;

    public SolveTarget solveTarget;
    public EffectTarget effectTarget;
    public EffectType effectType;
    public int effectReference;
    public List<BuffLast> buffLast;
    public List<int> lastReference;

    public Buff(SolveTarget solveTarget1, EffectTarget effectTarget1, EffectType effectType1, int effectReference1, List<BuffLast> buffLast1, List<int> lastReference1)
    {
        solveTarget = solveTarget1;
        effectTarget = effectTarget1;
        effectType = effectType1;
        effectReference = effectReference1;
        buffLast = buffLast1;
        lastReference = lastReference1;

        for (int i=0; i<buffLast.Count; i++)
        {
            switch (buffLast[i])
            {
                case BuffLast.turnLast:
                    turnLast = lastReference[i];
                    break;
                case BuffLast.actionLast:
                    actionLast = lastReference[i];
                    break;
                case BuffLast.cardLast:
                    cardLast = lastReference[i];
                    break;
            }
        }
        /*ifTurn = false;
        ifAction = false;
        ifCard = false;
        ifEternal = false;

        for (int i = 0; i < buffLast.Count; i++)
        {
            if (buffLast[i] == BuffLast.turnLast)
            {
                ifTurn = true;
                turnLast = lastReference[i];
            }
            if (buffLast[i] == BuffLast.cardLast)
            {
                ifCard = true;
                cardLast = lastReference[i];
            }
            if (buffLast[i] == BuffLast.actionLast)
            {
                ifAction = true;
                actionLast = lastReference[i];
            }
            if (buffLast[i] == BuffLast.eternal)
            {
                ifEternal = true;
            }
        }
        EffectApply(player, solveTarget, effectTarget, effectType, effectReference);*/
    }
/*
    public void EffectApply(Player player, SolveTarget solveTarget, EffectTarget effectTarget, EffectType effectType, int effectReference)
    {
        switch (effectType)
        {
            case EffectType.ifQuickChange:
                switch (effectTarget)
                {
                    case EffectTarget.handZone:
                        if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                        {
                            for (int i = 0; i < player.opponent.handZoneTransform.childCount; i++)
                            {
                                effectKey.Add(player.opponent.handZoneTransform.GetChild(i).gameObject.GetComponent<CardDisplay>().card.SetIfQuickWithReturn(effectReference != 0));//0为false，1为true
                            }
                        }
                        break;
                }
                break;
        }
    }

    public void EffectEnd(Player player, SolveTarget solveTarget, EffectTarget effectTarget, EffectType effectType, int effectReference)
    {
        switch (effectType)
        {
            case EffectType.ifQuickChange:
                switch (effectTarget)
                {
                    case EffectTarget.handZone:
                        if (solveTarget == SolveTarget.opponent || solveTarget == SolveTarget.both)
                        {
                            for (int i = 0; i < player.opponent.handZoneTransform.childCount; i++)
                            {
                                
                            }
                        }
                        break;
                }
                break;
        }
    }*/

    /*public void countDownDecreaseWhen(BuffLastType buffType)
    {
        
        
    }*/

}
