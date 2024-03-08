using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class Player : MonoBehaviour
{

    public int maxLP;
    public int maxSP;
    public int maxMP;
    public int LP;
    public int SP;
    public int MP;
    public List<Card> holdingCards;
    public List<Buff> buffList;
    public bool ifGoingFirst;

    public TextMeshProUGUI LifePointTMP;
    public TextMeshProUGUI StaminaPointTMP;
    public TextMeshProUGUI ManaPointTMP;
    public RectTransform LifeMask;
    public RectTransform StaminaMask;
    public RectTransform ManaMask;

    public GameObject DamageObj;
    public TextMeshProUGUI DamageNumTMP;

    public TextMeshProUGUI DeckCountTMP;
    public TextMeshProUGUI GraveCountTMP;
    public Transform handZoneTransform;
    public Transform deckZoneTransform;
    public Transform graveZoneTransform;
    public Transform fieldZoneTransform;
    public Zone handZone;
    public Zone deckZone;
    public Zone graveZone;
    public Zone fieldZone;

    public GameObject CardPrefab;

    public CardPool library;

    public Player opponent;

    public void Start()
    {
        library.getAllCards();
        holdingCards = new List<Card>();
        buffList = new List<Buff>();
        damageTarget = EffectTarget.LP;
        damageTargetRecord = new List<EffectTarget>{damageTarget};
        damageTargetOrderRecord = new List<Buff>();
    }


    public void initialStatusSet(int maxLp, int maxSp, int maxMp)//玩家初始数据加载
    {
        maxLP = maxLp;
        maxSP = maxSp;
        maxMP = maxMp;
        LP = maxLP;
        SP = maxSP;
        MP = maxMP;
        LifePointTMP.text = maxLP.ToString();
        StaminaPointTMP.text = maxSP.ToString();
        ManaPointTMP.text = maxMP.ToString();
    }

    public void handSpaceFix()//间距调整
    {
        int handCount = handZone.cardCount();
        float space = 25;
        if (handCount > 1)
        {
            space = (handZoneTransform.GetComponent<RectTransform>().rect.width - 200 * handCount) / (handCount - 1);
        }
        if (space < 25)
        {
            handZoneTransform.GetComponent<GridLayoutGroup>().spacing = new Vector2(space, 0);
        }
        else
        {
            handZoneTransform.GetComponent<GridLayoutGroup>().spacing = new Vector2(25, 0);
        }
    }

    #region Card Basic Treatments

    public void moveToDeckChange(GameObject card)//卡牌进入卡组时翻面
    {
        card.transform.Find("Informations").gameObject.SetActive(false);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 180f, 0f); //旋转使之面朝下
    }

    public void moveOutFromDeckChange(GameObject card)//卡牌退出卡组时翻面
    {
        card.transform.Find("Informations").gameObject.SetActive(true);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public GameObject cardGenerate(int id, Transform parent)//生成单卡
    {
        GameObject newCard = Instantiate(CardPrefab, parent);
        newCard.GetComponent<CardDisplay>().card = library.copyCard(library.cardPool[id]);
        newCard.GetComponent<CardDisplay>().card.HoldingPlayer = this;
        newCard.GetComponent<CardDisplay>().card.holdingPlayer = this;
        parent.GetComponent<Zone>().MoveInEffectCheck(newCard.GetComponent<CardDisplay>().card);
        holdingCards.Add(newCard.GetComponent<CardDisplay>().card);
        return newCard;
    }

    public void handGet(GameObject card)//手牌从单卡原来所在区域获得单卡
    {
        Transform fromWhere = card.transform.parent;
        card.transform.SetParent(null);
        if (fromWhere != null)
        {
            fromWhere.GetComponent<Zone>().MoveOutEffectCheck(card.GetComponent<CardDisplay>().card);
            card.transform.SetParent(handZoneTransform);
            handZone.MoveInEffectCheck(card.GetComponent<CardDisplay>().card);
            if(fromWhere == deckZoneTransform)
            {
                moveOutFromDeckChange(card);
                DeckCountTMP.text = deckZone.cardCount().ToString();
            }
            else if(fromWhere == graveZone)
            {
                GraveCountTMP.text = graveZone.cardCount().ToString();
            }
        }
        else
        {
            card.transform.SetParent(handZoneTransform);
            handZone.MoveInEffectCheck(card.GetComponent<CardDisplay>().card);
        }
        handSpaceFix();
    }

    public void handRemove(int orderInHand)//移除手牌单卡
    {
        if(orderInHand < handZone.cardCount())
        {
            Transform removingCard = handZoneTransform.GetChild(orderInHand).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
        }
        else
        {
            Debug.Log("手牌数量int越界");
        }
    }

    public void deckGet(GameObject card, bool ifBottom)//卡组从单卡原来所在区域获得单卡
    {
        int id = card.GetComponent<CardDisplay>().card.CardID;
        Transform fromWhere = card.transform.parent;
        card.transform.SetParent(null);
        if (fromWhere != null)
        {
            fromWhere.GetComponent<Zone>().MoveOutEffectCheck(card.GetComponent<CardDisplay>().card);
            card.transform.SetParent(deckZoneTransform);
            moveToDeckChange(card);
            deckZone.MoveInEffectCheck(card.GetComponent<CardDisplay>().card);
            if  (fromWhere == graveZoneTransform)
            {
                GraveCountTMP.text = graveZone.cardCount().ToString();
            }
            else if (fromWhere == handZoneTransform)
            {
                handSpaceFix();
            }
        }
        else
        {
            card.transform.SetParent(deckZoneTransform);
            moveToDeckChange(card);
            deckZone.MoveInEffectCheck(card.GetComponent<CardDisplay>().card);
        }
        if (ifBottom)//0是卡组底
        {
            card.transform.SetSiblingIndex(0);
        }
        DeckCountTMP.text = deckZone.cardCount().ToString();
    }

    public void deckRemove(int orderInDeck)//移除卡组单卡
    {
        if(orderInDeck < deckZone.cardCount())
        {
            Transform removingCard = deckZoneTransform.GetChild(orderInDeck).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
            DeckCountTMP.text = deckZone.cardCount().ToString();
        }
        else
        {
            Debug.Log("卡组数量int越界");
        }
    }

    public void graveGet(GameObject card)//墓地从单卡原来所在区域获得单卡
    {
        Transform fromWhere = card.transform.parent;
        card.transform.SetParent(null);
        if (fromWhere != null)
        {
            fromWhere.GetComponent<Zone>().MoveOutEffectCheck(card.GetComponent<CardDisplay>().card);
            card.transform.SetParent(graveZoneTransform);
            graveZone.MoveInEffectCheck(card.GetComponent<CardDisplay>().card);
            if (fromWhere == deckZoneTransform)
            {
                moveOutFromDeckChange(card);
                DeckCountTMP.text = deckZone.cardCount().ToString();
            }
            else if (fromWhere == handZoneTransform)
            {
                handSpaceFix();
            }
        }
        else
        {
            card.transform.SetParent(graveZoneTransform);
            graveZone.MoveInEffectCheck(card.GetComponent<CardDisplay>().card);
        }
        GraveCountTMP.text = graveZone.cardCount().ToString();
    }

    public void graveRemove(int orderInGrave)//移除墓地单卡
    {
        if (orderInGrave < graveZone.cardCount())
        {
            Transform removingCard = deckZoneTransform.GetChild(orderInGrave).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
            GraveCountTMP.text = graveZone.cardCount().ToString();
        }
        else
        {
            Debug.Log("墓地数量int越界");
        }
    }

    public void fieldGet(GameObject card)//墓地从单卡原来所在区域获得单卡
    {
        Transform fromWhere = card.transform.parent;
        if (fromWhere != null)
        {
            fromWhere.GetComponent<Zone>().MoveOutEffectCheck(card.GetComponent<CardDisplay>().card);
            card.transform.SetParent(fieldZoneTransform);
            fieldZone.MoveInEffectCheck(card.GetComponent<CardDisplay>().card);
            if (fromWhere == deckZoneTransform)
            {
                moveOutFromDeckChange(card);
                DeckCountTMP.text = deckZone.cardCount().ToString();
            }
            else if (fromWhere == graveZoneTransform)
            {
                GraveCountTMP.text = graveZone.cardCount().ToString();
            }
            else if (fromWhere == handZoneTransform)
            {
                handSpaceFix();
            }
        }
        else
        {
            card.transform.SetParent(fieldZoneTransform);
            fieldZone.MoveInEffectCheck(card.GetComponent<CardDisplay>().card);
        }
    }

    public void fieldRemove()//移除使用中单卡
    {
        if (fieldZone.cardCount() != 0)
        {
            Transform removingCard = fieldZoneTransform.GetChild(0).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
        }
        else
        {
            Debug.Log("墓地数量int越界");
        }
    }

    public void getDeck(string deckName)//初始化卡组
    {
        for (int i = 0; i < deckZone.cardCount(); i++)
        {
            Destroy(deckZoneTransform.GetChild(i).gameObject);
        }
        List<int> deck = library.ReadDeck(deckName);
        for(int i=0; i<deck.Count; i++)
        {
            Debug.Log("生成卡牌");
            GameObject newCard = cardGenerate(deck[i], deckZoneTransform);
            deckGet(newCard, true);
        }
    }

    public void shuffleDeck()//洗牌
    {
        for (int i = 0; i < deckZone.cardCount(); i++)
        {
            int randomCard = UnityEngine.Random.Range(0, deckZone.cardCount());
            GameObject randomCardObj = deckZoneTransform.GetChild(randomCard).gameObject;
            deckZoneTransform.GetChild(i).transform.SetSiblingIndex(randomCard);
            randomCardObj.transform.SetSiblingIndex(i);
        }
    }

    public float invokeCount_draw;
    public void drawMultipleCards(int num)
    {
        invokeCount_draw = num;
        InvokeRepeating(nameof(drawSingleCard), 0f, 0.3f);
    }
    public void drawSingleCard()
    {
        drawCardAnimation();
        Invoke(nameof(drawSingleCard_InvokedMethod), 0.25f);
    }
    public void drawSingleCard_InvokedMethod()
    {
        handGet(deckZoneTransform.GetChild(deckZoneTransform.childCount-1).gameObject);
        invokeCount_draw--;
        if (invokeCount_draw == 0)
        {
            CancelInvoke(nameof(drawSingleCard));
        }
    }

    #endregion

    #region animations

    public void drawCardAnimation()
    {
        LeanTween.moveLocalY(deckZoneTransform.GetChild(deckZoneTransform.childCount-1).gameObject, 500f, 0.2f);
    }

    public void DamageAppear(int num)
    {
        DamageNumTMP.text = num.ToString();
        DamageObj.SetActive(true);
        Invoke("DamageDisappear", 1.0f);
    }

    public void DamageDisappear()
    {
        DamageObj.SetActive(false);
    }

    #endregion

    #region about buffs

    public void Buff_BuffEnds(object sender, Buff.BuffEventArgs e)
    {
        RemoveBuff(e.buff);
    }

    public void AddBuff(Buff buff)
    {
        buffList.Add(buff);
        buff.BuffEnd += Buff_BuffEnds;
        BuffStartEffect(buff);
    }

    public void RemoveBuff(Buff buff)
    {
        BuffEndEffect(buff);
        buff.BuffEnd -= Buff_BuffEnds;
        buffList.Remove(buff);
    }

    public void BuffStartEffect(Buff buff)
    {
        switch (buff.effectTarget)
        {
            case EffectTarget.duelist:
                switch (buff.effectType)
                {
                    case EffectType.moveBeforeAttack:
                        BattleManager_Single.Instance.BeforeAttack += Move_Trigger;
                        moveBeforeAttackNumRecord = buff.effectReference;
                        break;
                    case EffectType.damageTargetChange:
                        switch (buff.effectReference)//0：LP;1：SP;2：MP
                        {
                            case 1:
                                damageTarget = EffectTarget.SP;
                                damageTargetRecord.Add(EffectTarget.SP);
                                damageTargetOrderRecord.Add(buff);
                                break;
                        }
                        break;
                }
                break;
            case EffectTarget.handZone:
                handZone.AddBuff(buff);
                break;
            case EffectTarget.holdingCard:
                handZone.AddBuff(buff);
                deckZone.AddBuff(buff);
                graveZone.AddBuff(buff);
                fieldZone.AddBuff(buff);
                break;
        }
    }

    public void BuffEndEffect(Buff buff)
    {
        switch (buff.effectTarget)
        {
            case EffectTarget.duelist:
                switch (buff.effectType)
                {
                    case EffectType.moveBeforeAttack:
                        BattleManager_Single.Instance.BeforeAttack -= Move_Trigger;
                        break;
                    case EffectType.damageTargetChange:
                        damageTarget = damageTargetRecord[damageTargetOrderRecord.IndexOf(buff)];
                        damageTargetRecord.RemoveAt(damageTargetOrderRecord.IndexOf(buff));
                        damageTargetOrderRecord.Remove(buff);
                        break;
                }
                break;
            case EffectTarget.handZone:
                handZone.RemoveBuff(buff);
                break;
            case EffectTarget.holdingCard:
                handZone.RemoveBuff(buff);
                deckZone.RemoveBuff(buff);
                graveZone.RemoveBuff(buff);
                fieldZone.RemoveBuff(buff);
                break;
        }
    }

    #endregion


    #region Trigger Effects

    public int moveBeforeAttackNumRecord;
    public EffectTarget damageTarget;
    public List<EffectTarget> damageTargetRecord;
    public List<Buff> damageTargetOrderRecord;
    public void Move_Trigger(object sender, BattleManager_Single.AttackEventArgs e)
    {
        if (e.attacker == this)
        {
            BattleManager_Single.Instance.ChangeDistance(moveBeforeAttackNumRecord);
        }
    }



    #endregion

}
