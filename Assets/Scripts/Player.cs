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
    public RectTransform LifeMask;
    public RectTransform StaminaMask;
    public RectTransform ManaMask;

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


    public void moveToDeckChange(GameObject card)//���ƽ��뿨��ʱ����
    {
        card.transform.Find("Informations").gameObject.SetActive(false);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 180f, 0f); //��תʹ֮�泯��
    }

    public void moveOutFromDeckChange(GameObject card)//�����˳�����ʱ����
    {
        card.transform.Find("Informations").gameObject.SetActive(true);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, 0f);
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

    public void handSpaceFix()//������
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

    public void handGet(GameObject card)//���ƴӵ���ԭ�����������õ���
    {
        Transform fromWhere = card.transform.parent;
        card.transform.SetParent(handZoneTransform);
        if(fromWhere != null)
        {
            if(fromWhere == deckZoneTransform)
            {
                moveOutFromDeckChange(card);
                DeckCountTMP.text = deckZone.cardCount().ToString();
            }
            else if(fromWhere == graveZoneTransform)
            {
                GraveCountTMP.text = graveZone.cardCount().ToString();
            }
        }
        handSpaceFix();
    }

    public void handRemove(int orderInHand)//�Ƴ����Ƶ���
    {
        if(orderInHand < handZone.cardCount())
        {
            Transform removingCard = handZoneTransform.GetChild(orderInHand).transform;
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
        card.transform.SetParent(deckZoneTransform);
        if(fromWhere != null)
        {
            if  (fromWhere == graveZoneTransform)
            {
                GraveCountTMP.text = graveZone.cardCount().ToString();
            }
            else if (fromWhere == handZoneTransform)
            {
                handSpaceFix();
            }
        }
        if (!ifBottom)
        {
            card.transform.SetSiblingIndex(0);
        }
        DeckCountTMP.text = deckZone.cardCount().ToString();
    }

    public void deckRemove(int orderInDeck)//�Ƴ����鵥��
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
            Debug.Log("��������intԽ��");
        }
    }

    public void graveGet(GameObject card)//Ĺ�شӵ���ԭ�����������õ���
    {
        Transform fromWhere = card.transform.parent;
        card.transform.SetParent(graveZoneTransform);
        if(fromWhere != null)
        {
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
        GraveCountTMP.text = graveZone.cardCount().ToString();
    }

    public void graveRemove(int orderInGrave)//�Ƴ�Ĺ�ص���
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
            Debug.Log("Ĺ������intԽ��");
        }
    }

    public void fieldGet(GameObject card)//Ĺ�شӵ���ԭ�����������õ���
    {
        Transform fromWhere = card.transform.parent;
        card.transform.SetParent(fieldZoneTransform);
        if (fromWhere != null)
        {
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
    }

    public void fieldRemove()//�Ƴ�ʹ���е���
    {
        if (fieldZone.cardCount() != 0)
        {
            Transform removingCard = fieldZoneTransform.GetChild(0).transform;
            removingCard.SetParent(null);
            Destroy(removingCard.gameObject);
        }
        else
        {
            Debug.Log("Ĺ������intԽ��");
        }
    }

    public void getDeck(string deckName)//��ʼ������
    {
        for (int i = 0; i < deckZone.cardCount(); i++)
        {
            Destroy(deckZoneTransform.GetChild(i).gameObject);
        }
        List<int> deck = library.readDeck(deckName);
        for(int i=0; i<deck.Count; i++)
        {
            GameObject newCard = cardGenerate(deck[i], deckZoneTransform);
            deckGet(newCard, true);
        }
    }

    public void shuffleDeck()//ϴ��
    {
        for (int i = 0; i < deckZone.cardCount(); i++)
        {
            int randomCard = UnityEngine.Random.Range(0, deckZone.cardCount());
            GameObject randomCardObj = deckZoneTransform.GetChild(randomCard).gameObject;
            deckZoneTransform.GetChild(i).transform.SetSiblingIndex(randomCard);
            randomCardObj.transform.SetSiblingIndex(i);
        }
    }

    /*public int invokeCount;

    public void drawMultipleCard(int num)
    {
        if (num > deckZoneTransform.childCount)
        {
            num = deckZoneTransform.childCount;
        }
        invokeCount = num;
        InvokeRepeating("DrawCard", 0f, 0.8f);
    }

    public void DrawCard()
    {
        GameObject card = deckZoneTransform.GetChild(0).gameObject;
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
            CancelInvoke("DrawCard");
        }
        GameObject card = deckZoneTransform.GetChild(0).gameObject;
        handGet(card);
    }*/

}
