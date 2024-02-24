using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.IO;
using XLua;


public enum GamePhase
{
    GameStart, StandbyPhase, MainPhase, EndPhase, OpponentStandbyPhase, OpponentMainPhase, OpponentEndPhase, GameOver
}

public class BattleManager_Single : MonoSingleton<BattleManager_Single>
{
    
    #region varieties

    public int maxDistance;
    public int maxLifePoint;
    public int maxStaminaPoint;
    public int maxManaPoint;
    private int distanceInGame;

    public int firstPlayerStartHand;
    public int secondPlayerStartHand;


    #endregion

    #region UIs

    public TextMeshProUGUI DistanceTMP;
    
    public TextMeshProUGUI PhaseTMP;

    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI discriptionText;
    public TextMeshProUGUI staminaCostText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI attackPowerText;
    public TextMeshProUGUI attackPowerTitleText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI distanceTitleText;

    public Transform DistanceBar;
    public GameObject groundBlock;
    public GameObject infoDisplayer;

    public GameObject selfPlayerPrefab; //�洢�ҷ��������
    public GameObject opponentPlayerPrefab;  //�洢�����������

    #endregion
    
    public CardPool library;
    public EffectTransformer effectManager;
    public bool ifSelfPlayingCard;

    public Player self;
    public Player opponent;

    private GamePhase gamePhase = GamePhase.GameStart;

    void Start()
    {
        library.getAllCards();
        self = selfPlayerPrefab.GetComponent<Player>();
        opponent = opponentPlayerPrefab.GetComponent<Player>();
        self.getDeck("01");
        opponent.getDeck("01");
        self.shuffleDeck();
        opponent.shuffleDeck();


        distanceInGame = 0;
        changeDistance(maxDistance);
        DistanceTMP.text = distanceInGame.ToString();

        self.initialStatusSet(maxLifePoint, maxStaminaPoint, maxManaPoint);
        opponent.initialStatusSet(maxLifePoint, maxStaminaPoint, maxManaPoint);

        //ͶӲ�Ҿ����Ⱥ���
        self.ifGoingFirst = true;
        int selfStartHand = firstPlayerStartHand;
        opponent.ifGoingFirst = false;
        int opponentStartHand = secondPlayerStartHand;
        if (!coin())
        {
            self.ifGoingFirst = false;
            selfStartHand = secondPlayerStartHand;
            opponent.ifGoingFirst = true;
            opponentStartHand = firstPlayerStartHand;
            drawCard(self, selfStartHand);
            drawCard(opponent, opponentStartHand);
            //��Ӳ��
            PhaseChange(GamePhase.OpponentStandbyPhase);
        }
        else
        {
            //ӮӲ��
            drawCard(self, selfStartHand);
            drawCard(opponent, opponentStartHand);
            PhaseChange(GamePhase.StandbyPhase);
        }
        
    }

    void Update()
    {
        //�����Ϣ������Ƭ��Ϣ�ر�
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            infoDisplayer.SetActive(false);
        }
    }


    public void OnClickMoveForward()
    {
        changeDistance(-1);
        changeSP(self, -1);
    }

    public void OnClickMoveBack()
    {
        changeDistance(1);
        changeSP(self, -1);
    }

    public void OnClickTurnEnd()
    {
        if(gamePhase == GamePhase.MainPhase)
        {
            PhaseChange(GamePhase.EndPhase);
        }else if (gamePhase == GamePhase.OpponentMainPhase)
        {
            PhaseChange(GamePhase.OpponentEndPhase);
        }
    }

    public void OnClickSurrender()
    {
        //�ж�ʤ��

        //������Ϸ
        PhaseChange(GamePhase.GameOver);
    }


    public void infoDisplay(Card card)
    {
        infoDisplayer.SetActive(true);
        cardNameText.text = card.CardName;
        discriptionText.text = card.Discription;
        staminaCostText.text = card.StaminaCost.ToString();
        manaCostText.text = card.ManaCost.ToString();
        if (card.GetType() == typeof(AttackCard))
        {
            var attackCard = card as AttackCard;
            attackPowerTitleText.gameObject.SetActive(true);
            attackPowerText.gameObject.SetActive(true);
            distanceTitleText.gameObject.SetActive(true);
            distanceText.gameObject.SetActive(true);
            attackPowerText.text = attackCard.AttackPower.ToString();
            distanceText.text = attackCard.Distance.ToString();
        }
        else if(card.GetType() == typeof(ActionCard))
        {
            attackPowerTitleText.gameObject.SetActive(false);
            attackPowerText.gameObject.SetActive(false);
            distanceTitleText.gameObject.SetActive(false);
            distanceText.gameObject.SetActive(false);
        }
    }

    
    

    #region Processes

    public void PhaseChange(GamePhase phase)
    {
        gamePhase = phase;
        
        switch (phase)
        {
            case GamePhase.StandbyPhase:
                PhaseTMP.text = "�ҷ�׼���׶�";
                if (self.deckList.Count == 0)
                {
                    //��Ĺ�طŻؿ���ϴ��
                }
                drawCard(self, 2);
                changeSP(self, 5);
                PhaseChange(GamePhase.MainPhase);
                break;
            case GamePhase.MainPhase:
                PhaseTMP.text = "�ҷ���Ҫ�׶�";
                break;
            case GamePhase.EndPhase:
                PhaseTMP.text = "�ҷ������׶�";

                PhaseChange(GamePhase.OpponentStandbyPhase);
                break;
            case GamePhase.OpponentStandbyPhase:
                PhaseTMP.text = "�Է�׼���׶�";
                if (opponent.deckList.Count == 0)
                {
                    //��Ĺ�طŻؿ���ϴ��
                }
                drawCard(opponent, 2);
                changeSP(opponent, 5);
                PhaseChange(GamePhase.OpponentMainPhase);
                break;
            case GamePhase.OpponentMainPhase:
                PhaseTMP.text = "�Է���Ҫ�׶�";
                break;
            case GamePhase.OpponentEndPhase:
                PhaseTMP.text = "�Է������׶�";

                PhaseChange(GamePhase.StandbyPhase);
                break;
        }
    }

    #endregion


    public void useBuff(int turnLast, int actionLast, bool ifOneChain)//�����غ��������꼸�ſ������ʧ���Ƿ��������������ʧ
    {

    }


    #region card treatments
    //Ԥ�Ƽ�ֱ�Ӹ�Player�������ֱ����Player����ĺ�����

    public void drawCard(Player player, int num)
    {
        if (num > player.deckList.Count)
        {
            num = player.deckList.Count;
        }
        for (int i = 0; i < num; i++)
        {
            GameObject card = player.Deck.GetChild(0).gameObject;
            player.handGet(card);
        }
    }

    public void discardCard(Player player, int orderInHand)
    {
        if (orderInHand < player.handList.Count)
        {
            GameObject card = player.Hands.GetChild(orderInHand).gameObject;
            player.graveGet(card);
        }
        else
        {
            Debug.Log("��������intԽ��");
        }
        
    }

    public void cardRecycleToDeck(Player player, int orderInGrave, bool ifBottom, bool ifShuffle)
    {
        if (orderInGrave < player.graveList.Count)
        {
            Transform card = player.Grave.GetChild(orderInGrave);
            card.SetParent(player.Deck);
            player.moveToDeckChange(card.gameObject);
            if (!ifBottom)
            {
                card.transform.SetSiblingIndex(0);
            }
            if (ifShuffle)
            {
                player.shuffleDeck();
            }
        }
        else
        {
            Debug.Log("Ĺ������intԽ��");
        }
    }

    public void cardReturnToHand(Player player, int orderInGrave)
    {
        if (orderInGrave < player.graveList.Count)
        {
            GameObject card = player.Grave.GetChild(orderInGrave).gameObject;
            player.handGet(card);
        }
        else
        {
            Debug.Log("Ĺ������intԽ��");
        }
    }


    public void setIfQuick(Card card, bool result)
    {
        card.IfQuick = result;
    }

    #endregion

    #region numerical changes

    public void changeLP(Player player, int num)
    {
        int lp = player.LP;
        if (lp + num > maxLifePoint)
        {
            num = maxLifePoint - lp;
        }
        float newScale = ((float)lp + (float)num) / (float)maxLifePoint;
        player.LifeMask.localScale = new Vector3(newScale, 1.0f, 1.0f);
        player.LP = lp + num;
        player.LifePointTMP.text = player.LP.ToString();
        if (player.LP <= 0)
        {
            //�ж�ʤ��
        }
    }

    public void changeSP(Player player, int num)
    {
        int sp = player.SP;
        if(sp + num > maxStaminaPoint)
        {
            num = maxStaminaPoint - sp;
        }
        if (sp + num <0)
        {
            num = sp;
        }
        float newScale = ((float)sp + (float)num) / (float)maxStaminaPoint;
        player.StaminaMask.localScale = new Vector3(newScale, 1.0f, 1.0f);
        player.SP = sp + num;
        player.StaminaPointTMP.text = player.SP.ToString();

    }

    public void changeMP(Player player, int num)
    {
        int mp = player.MP;
        if (mp + num > maxManaPoint)
        {
            num = maxManaPoint - mp;
        }
        if (mp + num < 0)
        {
            num = mp;
        }
        float newScale = ((float)mp + (float)num) / (float)maxManaPoint;
        player.ManaMask.localScale = new Vector3(newScale, 1.0f, 1.0f);
        player.MP = mp + num;
        player.ManaPointTMP.text = player.MP.ToString();
    }

    public void changeDistance(int num)
    {
        //�����������������ߵ���Ե
        if (distanceInGame + num > maxDistance)
        {
            num = maxDistance - distanceInGame;
        }
        else if (distanceInGame + num < 0)
        {
            num = distanceInGame;
        }

        distanceInGame += num;
        DistanceTMP.text = distanceInGame.ToString();

        //�ı���
        float space = 40;
        if (distanceInGame > 1)
        {
            space = (DistanceBar.GetComponent<RectTransform>().rect.width - 60 * distanceInGame) / (distanceInGame - 1);
        }
        if (space < 40)
        {
            DistanceBar.GetComponent<GridLayoutGroup>().spacing = new Vector2(space, 0);
        }
        else
        {
            DistanceBar.GetComponent<GridLayoutGroup>().spacing = new Vector2(40, 0);
        }

        if (num > 0)
        {
            for (int i = 0; i < num; i++)
            {
                GameObject newGroundBlock = Instantiate(groundBlock, DistanceBar);
            }
        }
        else if (num < 0)
        {
            for (int i = 0; i > num; i--)
            {
                Destroy(DistanceBar.GetChild(0).gameObject);
            }
        }
    }

    #endregion

    public Player getCardPlayer()
    {
        if (ifSelfPlayingCard)
        {
            return self;
        }
        else
        {
            return opponent;
        }
    }


    #region supports

    public bool coin()
    {
        bool result = false;
        int coin = UnityEngine.Random.Range(0, 2);
        if(coin == 1)
        {
            result = true;
        }
        return result;
    }

    public int dice(int faceCount)
    {
        int result = UnityEngine.Random.Range(1 , faceCount + 1);
        return result;
    }

    #endregion

}
