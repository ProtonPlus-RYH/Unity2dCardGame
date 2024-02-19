using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    private int maxLP;
    private int maxSP;
    private int maxMP;
    private int LP;
    private int SP;
    private int MP;
    private List<int> deckList;
    private List<int> handList;
    private bool ifGoingFirst;

    public TextMeshProUGUI LifePointTMP;
    public TextMeshProUGUI StaminaPointTMP;
    public TextMeshProUGUI ManaPointTMP;

    public Transform Hands;
    public RectTransform LifeMask;
    public RectTransform StaminaMask;
    public RectTransform ManaMask;

    public GameObject handCardPrefab;

    public CardPool library;

    public void Start()
    {
        library.getAllCards();
    }

    //玩家初始数据加载
    public void playerStartGame(List<int> decklist, int maxLp, int maxSp, int maxMp, int starterHand)
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
        deckList = decklist;
        handList = new List<int>();
        shuffleDeck();
        drawFromDeck(starterHand);
    }

    public void getLifePointTMP(TextMeshProUGUI target)
    {
        LifePointTMP = target;
    }

    public void setIfGoingFirst(bool result)
    {
        ifGoingFirst = result;
    }

    public void changeLP(int num)
    {
        float newScale = ((float)LP + (float)num) / (float)maxLP;
        LifeMask.localScale = new Vector3(newScale, 1.0f, 1.0f);
        LP += num;
        LifePointTMP.text = LP.ToString();
        if(LP <= 0)
        {

        }
    }

    public void changeSP(int num)
    {
        float newScale = ((float)SP + (float)num) / (float)maxSP;
        StaminaMask.localScale = new Vector3(newScale, 1.0f, 1.0f);
        SP += num;
        StaminaPointTMP.text = SP.ToString();
    }

    public void changeMP(int num)
    {
        float newScale = ((float)MP + (float)num) / (float)maxMP;
        ManaMask.localScale = new Vector3(newScale, 1.0f, 1.0f);
        MP += num;
        ManaPointTMP.text = MP.ToString();
    }



    public void handGet(int id)
    {
        handList.Add(id);
        int handCount = Hands.childCount + 1;
        float space = 25;
        if(handCount > 1)
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
        GameObject newCard = Instantiate(handCardPrefab, Hands);
        newCard.GetComponent<CardDisplay>().card = library.cardPool[id];
    }

    public void getDeck(string deckName)
    {
        deckList = library.readDeck(deckName);
    }

    public void shuffleDeck()
    {
        for (int i = 0; i < deckList.Count; i++)
        {
            int randomCard = Random.Range(0, deckList.Count);
            int n = deckList[i];
            deckList[i] = deckList[randomCard];
            deckList[randomCard] = n;
        }
    }

    public void drawFromDeck(int drawQuantity)
    {
        for(int i=0; i<drawQuantity; i++)
        {
            handGet(deckList[0]);
            deckList.RemoveAt(0);
        }
    }

}
