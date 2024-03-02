using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//具体效果都写在预制件的脚本里面,计时存储在这个里面
public class Buff
{
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
                case BuffLast.turnLast_opponent:
                    bool a = (BattleManager_Single.Instance.turnCount % 2 == 1);//奇数回合还是偶数回合
                    bool b = EffectTransformer.Instance.getUserByPhase().ifGoingFirst;//被施加buff的是不是先手
                    if(a==b)
                    {
                        turnLast = lastReference[i] * 2 - 1;
                    }
                    else
                    {
                        turnLast = lastReference[i] * 2 - 2;
                    }
                    break;
                case BuffLast.actionLast:
                    actionLast = lastReference[i];
                    break;
                case BuffLast.cardLast:
                    cardLast = lastReference[i];
                    break;
            }
        }
    }

    public void CountdownDecrease(BuffLast lastType)
    {
        switch (lastType)
        {
            case BuffLast.turnLast:
                turnLast--;
                if(turnLast < 0)
                {
                    BuffEnd?.Invoke(this,new BuffEventArgs(this));
                }
                break;
            case BuffLast.actionLast:
                actionLast--;
                if(actionLast < 0)
                {
                    BuffEnd?.Invoke(this, new BuffEventArgs(this));
                }
                break;
            case BuffLast.cardLast:
                cardLast--;
                if(cardLast < 0)
                {
                    BuffEnd?.Invoke(this, new BuffEventArgs(this));
                }
                break;
        }
    }

    public event EventHandler<BuffEventArgs> BuffEnd;
    public class BuffEventArgs : EventArgs
    {
        public Buff buff;
        public BuffEventArgs(Buff buffInput)
        {
            buff = buffInput;
        }
    }
}
