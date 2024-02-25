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


    public void initialStatusSet(int maxLp, int maxSp, int maxMp)//��ҳ�ʼ���ݼ���
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


    public void moveToDeckChange(GameObject card)//���ƽ��뿨��ʱ���棬ת���
    {
        card.transform.Find("Informations").gameObject.SetActive(false);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 180f, 0f); //��תʹ֮�泯��
    }

    public void moveOutFromDeckChange(GameObject card)//�����˳�����ʱ���棬ת���
    {
        card.transform.Find("Informations").gameObject.SetActive(true);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, -90f);
    }

    public GameObject cardGenerate(int id, Transform parent)//���ɵ���
    {
        GameObject newCard = Instantiate(CardPrefab, parent);
        newCard.GetComponent<CardDisplay>().card = library.copyCard(library.cardPool[id]);
        newCard.GetComponent<CardDisplay>().card.HoldingPlayer = this;
        newCard.GetComponent<CardDisplay>().card.holdingPlayer = this;
        holdingCards.Add(newCard.GetComponent<CardDisplay>().card);
        return newCard;
    }

    public void handGet(GameObject card)//���ƴӵ���ԭ�����������õ���
    {
        int handCount = Hands.childCount + 1;
        //������
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
                DeckCountTMP.text = Deck.childCount.ToString();
            }
            else if(fromWhere == Grave)
            {
                GraveCountTMP.text = Grave.childCount.ToString();
            }
        }
        card.transform.SetParent(Hands);
    }

    public void handRemove(int orderInHand)//�Ƴ����Ƶ���
    {
        if(orderInHand < Hands.childCount)
        {
            Transform removingCard = Hands.GetChild(orderInHand).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
        }
        else
        {
            Debug.Log("��������intԽ��");
        }
    }

    public void deckGet(GameObject card, bool ifBottom)//����ӵ���ԭ�����������õ���
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
        }
        
        card.transform.SetParent(Deck);
        if (!ifBottom)
        {
            card.transform.SetSiblingIndex(0);
        }
        DeckCountTMP.text = Deck.childCount.ToString();
    }

    public void deckRemove(int orderInDeck)//�Ƴ����鵥��
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
            Debug.Log("��������intԽ��");
        }
    }

    public void graveGet(GameObject card)//Ĺ�شӵ���ԭ�����������õ���
    {
        Transform fromWhere = card.transform.parent;
        if(fromWhere != null)
        {
            if (fromWhere == Deck)
            {
                moveOutFromDeckChange(card);
                DeckCountTMP.text = Deck.childCount.ToString();
            }
        }
        card.transform.SetParent(Grave);
        GraveCountTMP.text = Grave.childCount.ToString();
    }

    public void graveRemove(int orderInGrave)//�Ƴ�Ĺ�ص���
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
            Debug.Log("Ĺ������intԽ��");
        }
    }

    public void getDeck(string deckName)//��ʼ������
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

    public void shuffleDeck()//ϴ��
    {
        for (int i = 0; i < Deck.childCount; i++)
        {
            int randomCard = UnityEngine.Random.Range(0, Deck.childCount);
            GameObject randomCardObj = Deck.GetChild(randomCard).gameObject;
            Deck.GetChild(i).transform.SetSiblingIndex(randomCard);
            randomCardObj.transform.SetSiblingIndex(i);
        }
    }

}
