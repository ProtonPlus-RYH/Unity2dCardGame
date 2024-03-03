using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{

    public List<Buff> buffList;

    void Start()
    {
        buffList = new List<Buff>();
    }

    public int cardCount()
    {
        return transform.childCount;
    }


    public void AddBuff(Buff buff)
    {
        buffList.Add(buff);
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
}
