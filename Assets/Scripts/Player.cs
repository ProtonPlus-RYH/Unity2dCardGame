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
    public List<int> deckList;
    public List<int> handList;
    public List<int> graveList;
    public bool ifGoingFirst;

    public TextMeshProUGUI LifePointTMP;
    public TextMeshProUGUI StaminaPointTMP;
    public TextMeshProUGUI ManaPointTMP;
    public TextMeshProUGUI DeckCountTMP;
    public TextMeshProUGUI GraveCountTMP;

    public Transform Hands;
    public Transform Deck;
    public Transform Grave;
    public RectTransform LifeMask;
    public RectTransform StaminaMask;
    public RectTransform ManaMask;

    public GameObject CardPrefab;

    public CardPool library;

    public Player opponent;

    public void Start()
    {
        library.getAllCards();
        deckList = new List<int>();
        handList = new List<int>();
        graveList = new List<int>();
    }

    //玩家初始数据加载
    public void initialStatusSet(int maxLp, int maxSp, int maxMp)
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


    public void moveToDeckChange(GameObject card)
    {
        card.transform.Find("Informations").gameObject.SetActive(false);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 180f, 0f); //旋转使之面朝下
    }//卡牌进入卡组时翻面，转向等

    public void moveOutFromDeckChange(GameObject card)
    {
        card.transform.Find("Informations").gameObject.SetActive(true);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, -90f);
    }//卡牌退出卡组时翻面，转向等

    public GameObject cardGenerate(int id, Transform parent)
    {
        GameObject newCard = Instantiate(CardPrefab, parent);
        newCard.GetComponent<CardDisplay>().card = library.copyCard(library.cardPool[id]);
        return newCard;
    }//生成单卡

    public void handGet(GameObject card)
    {
        handList.Add(card.GetComponent<CardDisplay>().card.CardID);
        int handCount = Hands.childCount + 1;
        //间距调整
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
        Transform fromWhere = card.transform.parent;
        if(fromWhere != null)
        {
            if(fromWhere == Deck)
            {
                moveOutFromDeckChange(card);
                deckList.RemoveAt(card.transform.GetSiblingIndex());
                DeckCountTMP.text = deckList.Count.ToString();
            }
            else if(fromWhere == Grave)
            {
                graveList.RemoveAt(card.transform.GetSiblingIndex());
                GraveCountTMP.text = graveList.Count.ToString();
            }
        }
        card.transform.SetParent(Hands);
    }//手牌从单卡原来所在区域获得单卡

    public void handRemove(int orderInHand)
    {
        if(orderInHand < handList.Count)
        {
            handList.RemoveAt(orderInHand);
            Transform removingCard = Hands.GetChild(orderInHand).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
        }
        else
        {
            Debug.Log("手牌数量int越界");
        }
    }//移除手牌单卡

    public void deckGet(GameObject card, bool ifBottom)
    {
        moveToDeckChange(card);
        int id = card.GetComponent<CardDisplay>().card.CardID;

        Transform fromWhere = card.transform.parent;
        if(fromWhere != null)
        {
            if (fromWhere == Hands)
            {
                handList.RemoveAt(card.transform.GetSiblingIndex());
            }
            else if (fromWhere == Grave)
            {
                graveList.RemoveAt(card.transform.GetSiblingIndex());
                GraveCountTMP.text = graveList.Count.ToString();
            }
        }
        
        card.transform.SetParent(Deck);
        if (ifBottom)
        {
            deckList.Add(id);
        }
        else
        {
            List<int> n = deckList;
            deckList.Clear();
            deckList.Add(id);
            deckList.AddRange(n);
            card.transform.SetSiblingIndex(0);
        }
        DeckCountTMP.text = deckList.Count.ToString();
    }//卡组从单卡原来所在区域获得单卡

    public void deckRemove(int orderInDeck)
    {
        if(orderInDeck < deckList.Count)
        {
            deckList.RemoveAt(orderInDeck);
            Transform removingCard = Deck.GetChild(orderInDeck).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
            DeckCountTMP.text = deckList.Count.ToString();
        }
        else
        {
            Debug.Log("卡组数量int越界");
        }
    }//移除卡组单卡

    public void graveGet(GameObject card)
    {
        graveList.Add(card.GetComponent<CardDisplay>().card.CardID);
        Transform fromWhere = card.transform.parent;
        if(fromWhere != null)
        {
            if (fromWhere == Hands)
            {
                handList.RemoveAt(card.transform.GetSiblingIndex());
            }
            else if (fromWhere == Deck)
            {
                moveOutFromDeckChange(card);
                deckList.RemoveAt(card.transform.GetSiblingIndex());
                DeckCountTMP.text = deckList.Count.ToString();
            }
        }
        card.transform.SetParent(Grave);
    }//墓地从单卡原来所在区域获得单卡

    public void graveRemove(int orderInGrave)//移除墓地单卡
    {
        if (orderInGrave < graveList.Count)
        {
            graveList.RemoveAt(orderInGrave);
            Transform removingCard = Deck.GetChild(orderInGrave).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
            GraveCountTMP.text = deckList.Count.ToString();
        }
        else
        {
            Debug.Log("墓地数量int越界");
        }
    }

    public void getDeck(string deckName)
    {
        for (int i = 0; i < Deck.childCount; i++)
        {
            Destroy(Deck.GetChild(i).gameObject);
        }
        deckList = new List<int>();
        List<int> deck = library.readDeck(deckName);
        for(int i=0; i<deck.Count; i++)
        {
            GameObject newCard = cardGenerate(deck[i], Deck);
            deckGet(newCard, true);
        }
    }//初始化卡组

    public void shuffleDeck()
    {
        for (int i = 0; i < deckList.Count; i++)
        {
            int randomCard = UnityEngine.Random.Range(0, deckList.Count);
            int n = deckList[i];
            deckList[i] = deckList[randomCard];
            deckList[randomCard] = n;
            GameObject randomCardObj = Deck.GetChild(randomCard).gameObject;
            Deck.GetChild(i).transform.SetSiblingIndex(randomCard);
            randomCardObj.transform.SetSiblingIndex(i);
        }
    }//洗牌

}
