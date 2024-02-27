using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using XLua;
using System.IO;
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
    public bool ifGoingFirst;

    public TextMeshProUGUI LifePointTMP;
    public TextMeshProUGUI StaminaPointTMP;
    public TextMeshProUGUI ManaPointTMP;
    public TextMeshProUGUI DeckCountTMP;
    public TextMeshProUGUI GraveCountTMP;

    public Transform Hands;
    public Transform Deck;
    public Transform Grave;
    public Transform FieldZone;
    public RectTransform LifeMask;
    public RectTransform StaminaMask;
    public RectTransform ManaMask;

    public GameObject CardPrefab;

    public CardPool library;

    public Player opponent;

    public void Start()
    {
        library.getAllCards();
        holdingCards = new List<Card>();
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


    public void moveToDeckChange(GameObject card)//卡牌进入卡组时翻面，转向等
    {
        card.transform.Find("Informations").gameObject.SetActive(false);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 180f, 0f); //旋转使之面朝下
    }

    public void moveOutFromDeckChange(GameObject card)//卡牌退出卡组时翻面，转向等
    {
        card.transform.Find("Informations").gameObject.SetActive(true);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, -90f);
    }

    public GameObject cardGenerate(int id, Transform parent)//生成单卡
    {
        GameObject newCard = Instantiate(CardPrefab, parent);
        newCard.GetComponent<CardDisplay>().card = library.copyCard(library.cardPool[id]);
        newCard.GetComponent<CardDisplay>().card.HoldingPlayer = this;
        newCard.GetComponent<CardDisplay>().card.holdingPlayer = this;
        holdingCards.Add(newCard.GetComponent<CardDisplay>().card);
        return newCard;
    }

    public void handSpaceFix()//间距调整
    {
        int handCount = Hands.childCount;
        float space = 25;
        if (handCount > 1)
        {
            space = (Hands.GetComponent<RectTransform>().rect.width - 200 * handCount) / (handCount - 1);
        }
        if (space < 25)
        {
            Hands.GetComponent<GridLayoutGroup>().spacing = new Vector2(space, 0);
        }
        else
        {
            Hands.GetComponent<GridLayoutGroup>().spacing = new Vector2(25, 0);
        }
    }

    public void handGet(GameObject card)//手牌从单卡原来所在区域获得单卡
    {
        Transform fromWhere = card.transform.parent;
        if(fromWhere != null)
        {
            if(fromWhere == Deck)
            {
                moveOutFromDeckChange(card);
                DeckCountTMP.text = Deck.childCount.ToString();
            }
            else if(fromWhere == Grave)
            {
                GraveCountTMP.text = Grave.childCount.ToString();
            }
        }
        card.transform.SetParent(Hands);
        handSpaceFix();
    }

    public void handRemove(int orderInHand)//移除手牌单卡
    {
        if(orderInHand < Hands.childCount)
        {
            Transform removingCard = Hands.GetChild(orderInHand).transform;
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
        moveToDeckChange(card);
        int id = card.GetComponent<CardDisplay>().card.CardID;

        Transform fromWhere = card.transform.parent;
        if(fromWhere != null)
        {
            if  (fromWhere == Grave)
            {
                GraveCountTMP.text = Grave.childCount.ToString();
            }
            else if (fromWhere == Hands)
            {
                handSpaceFix();
            }
        }
        
        card.transform.SetParent(Deck);
        if (!ifBottom)
        {
            card.transform.SetSiblingIndex(0);
        }
        DeckCountTMP.text = Deck.childCount.ToString();
    }

    public void deckRemove(int orderInDeck)//移除卡组单卡
    {
        if(orderInDeck < Deck.childCount)
        {
            Transform removingCard = Deck.GetChild(orderInDeck).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
            DeckCountTMP.text = Deck.childCount.ToString();
        }
        else
        {
            Debug.Log("卡组数量int越界");
        }
    }

    public void graveGet(GameObject card)//墓地从单卡原来所在区域获得单卡
    {
        Transform fromWhere = card.transform.parent;
        if(fromWhere != null)
        {
            if (fromWhere == Deck)
            {
                moveOutFromDeckChange(card);
                DeckCountTMP.text = Deck.childCount.ToString();
            }
            else if (fromWhere == Hands)
            {
                handSpaceFix();
            }
        }
        card.transform.SetParent(Grave);
        GraveCountTMP.text = Grave.childCount.ToString();
    }

    public void graveRemove(int orderInGrave)//移除墓地单卡
    {
        if (orderInGrave < Grave.childCount)
        {
            Transform removingCard = Deck.GetChild(orderInGrave).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
            GraveCountTMP.text = Grave.childCount.ToString();
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
            if (fromWhere == Deck)
            {
                moveOutFromDeckChange(card);
                DeckCountTMP.text = Deck.childCount.ToString();
            }
            else if (fromWhere == Grave)
            {
                GraveCountTMP.text = Grave.childCount.ToString();
            }
            else if (fromWhere == Hands)
            {
                handSpaceFix();
            }
        }
        card.transform.SetParent(FieldZone);
    }

    public void fieldRemove()//移除使用中单卡
    {
        if (FieldZone.childCount != 0)
        {
            Transform removingCard = FieldZone.GetChild(0).transform;
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
        for (int i = 0; i < Deck.childCount; i++)
        {
            Destroy(Deck.GetChild(i).gameObject);
        }
        List<int> deck = library.readDeck(deckName);
        for(int i=0; i<deck.Count; i++)
        {
            GameObject newCard = cardGenerate(deck[i], Deck);
            deckGet(newCard, true);
        }
    }

    public void shuffleDeck()//洗牌
    {
        for (int i = 0; i < Deck.childCount; i++)
        {
            int randomCard = UnityEngine.Random.Range(0, Deck.childCount);
            GameObject randomCardObj = Deck.GetChild(randomCard).gameObject;
            Deck.GetChild(i).transform.SetSiblingIndex(randomCard);
            randomCardObj.transform.SetSiblingIndex(i);
        }
    }

    /*public int invokeCount;

    public void drawMultipleCard(int num)
    {
        if (num > Deck.childCount)
        {
            num = Deck.childCount;
        }
        invokeCount = num;
        InvokeRepeating("drawCard", 0f, 0.8f);
    }

    public void drawCard()
    {
        GameObject card = Deck.GetChild(0).gameObject;
        Vector3 drawShowingPosition = new Vector3(0f, 600f, 0f);
        Vector3 drawShowingRotation = new Vector3(180f, 0f, -90f);
        LeanTween.moveLocal(card, drawShowingPosition, 0.3f);
        LeanTween.rotateLocal(card, drawShowingRotation, 0.3f);
        Invoke("dr", 0.4f);
    }

    public void dr()
    {
        invokeCount--;
        if(invokeCount == 0)
        {
            CancelInvoke("drawCard");
        }
        GameObject card = Deck.GetChild(0).gameObject;
        handGet(card);
    }*/

}
