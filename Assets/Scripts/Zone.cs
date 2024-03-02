using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{

    public List<Buff> buffList;
    //public List<EffectType> effectList;

    void Start()
    {
        buffList = new List<Buff>();
        //effectList = new List<EffectType>();
    }

    public int cardCount()
    {
        return transform.childCount;
    }

    /*public void Buff_BuffEnds(object sender, Buff.BuffEventArgs e)
    {
        RemoveBuff(e.buff);
    }*/

    public void AddBuff(Buff buff)
    {
        //AddEffect(buff.effectType, buff.effectReference);
        //return buffList.Count - 1;
        buffList.Add(buff);
        //buff.BuffEnd += Buff_BuffEnds;
        for (int i = 0; i < cardCount(); i++)
        {
            MoveInEffectCheck(transform.GetChild(i).GetComponent<CardDisplay>().card);
        }
    }

    public void RemoveBuff(Buff buff)
    {
        for (int i = 0; i < cardCount(); i++)
        {
            MoveOutEffectCheck(transform.GetChild(i).GetComponent<CardDisplay>().card);
        }
        //buff.BuffEnd -= Buff_BuffEnds;
        buffList.Remove(buff);
    }

    public void MoveInEffectCheck(Card card)
    {
        foreach (var buff in buffList)
        {
            if (!card.buffList.Contains(buff))
            {
                card.AddBuff(buff);
            }
        }
    }

    public void MoveOutEffectCheck(Card card)
    {
        foreach (var buff in buffList)
        {
            if (card.buffList.Contains(buff))
            {
                card.RemoveBuff(buff);
            }
        }
    }

    /*public void AddEffect(EffectType effectType, int effectReference)
    {
        for (int i = 0; i < gameObject.transform.childCount; i++) 
        {
            ApplySingleEffect(gameObject.transform.GetChild(i).GetComponent<CardDisplay>().card, effectType, effectReference);
        }
    }
    
    public void ApplySingleEffect(Card card, EffectType effectType, int effectReference)
    {
        switch (effectType)
        {
            case EffectType.ifQuickChange:
                card.SetIfQuickWithReturn(effectReference != 0);//0Îªfalse£¬1Îªtrue
                break;
        }
    }
    
    public void RemoveEffect(int effectKey)
    {

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            
        }
    }

    

    public void RemoveSingleEffect(Card card, EffectType effectType, int effectReference)
    {
        switch (effectType)
        {
            case EffectType.ifQuickChange:

                break;
        }
    }*/
}
