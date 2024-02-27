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
    GameStart, StandbyPhase, MainPhase, EndPhase, selfHandDiscarding, OpponentStandbyPhase, OpponentMainPhase, OpponentEndPhase, opponentHandDiscarding, GameOver
}

public class BattleManager_Single : MonoSingleton<BattleManager_Single>
{
    
    #region varieties

    public int maxDistance;
    public int maxLifePoint;
    public int maxStaminaPoint;
    public int maxManaPoint;
    public int distanceInGame;
    public int maxHand;

    public int firstPlayerStartHand;
    public int secondPlayerStartHand;

    public List<int> phaseChangeCardActivableKey;

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
    public GameObject textTMP;

    public GameObject selfPlayerPrefab; //�洢�ҷ��������
    public GameObject opponentPlayerPrefab;  //�洢�����������

    #endregion
    
    public CardPool library;
    public EffectTransformer effectManager;

    public Player self;
    public Player opponent;

    public Player controller;

    public GamePhase gamePhase = GamePhase.GameStart;

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
        changeLP(self, -5);
        changeLP(opponent, -5);

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
            controller = opponent;
            drawCard(self, selfStartHand);
            drawCard(opponent, opponentStartHand);
            //��Ӳ��
            opponentStandbyPhase();
        }
        else
        {
            controller = self;
            //ӮӲ��
            drawCard(self, selfStartHand);
            drawCard(opponent, opponentStartHand);
            standbyPhase();
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
        changeSP(controller, -1);
    }

    public void OnClickMoveBack()
    {
        changeDistance(1);
        changeSP(controller, -1);
    }

    public void OnClickTurnEnd()
    {
        if(gamePhase == GamePhase.MainPhase || gamePhase == GamePhase.OpponentMainPhase)
        {
            phasePush();
        }
    }

    public void OnClickSurrender()
    {
        //�ж�ʤ��

        //������Ϸ
        gameOver();
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


    #region Phases in Game

    public void standbyPhase()
    {
        gamePhase = GamePhase.StandbyPhase;
        PhaseTMP.text = "�ҷ�׼���׶�";
        if (self.Deck.childCount == 0)
        {
            //ǿ�ƿ����ٹ���
            deckRebuild(self);
        }
        drawCard(self, 2);
        changeSP(self, 5);
        phasePush();
    }

    public void mainPhase()
    {
        gamePhase = GamePhase.MainPhase;
        PhaseTMP.text = "�ҷ���Ҫ�׶�";
        phaseChangeCardActivableKey = setAllCardUsable(self);//�򿪿���ʹ�ÿ�����
    }

    public void endPhase()
    {
        gamePhase = GamePhase.EndPhase;
        PhaseTMP.text = "�ҷ������׶�";

        resetAllCardUsable(self, phaseChangeCardActivableKey);//�رտ���ʹ�ÿ�����
        resetCardCountInTurn(self);
        handCountCheck();
    }

    public void opponentStandbyPhase()
    {
        gamePhase = GamePhase.OpponentStandbyPhase;
        PhaseTMP.text = "�Է�׼���׶�";
        if (opponent.Deck.childCount == 0)
        {
            //ǿ�ƿ����ٹ���
            deckRebuild(opponent);
        }
        drawCard(opponent, 2);
        changeSP(opponent, 5);
        phasePush();
    }

    public void opponentMainPhase()
    {
        gamePhase = GamePhase.OpponentMainPhase;
        PhaseTMP.text = "�Է���Ҫ�׶�";
        phaseChangeCardActivableKey = setAllCardUsable(opponent);//�򿪿���ʹ�ÿ�����
    }

    public void opponentEndPhase()
    {
        gamePhase = GamePhase.OpponentEndPhase;
        PhaseTMP.text = "�Է������׶�";

        resetAllCardUsable(opponent, phaseChangeCardActivableKey);//�رտ���ʹ�ÿ�����
        resetCardCountInTurn(opponent);
        handCountCheck();
    }

    public void gameOver()
    {
        gamePhase = GamePhase.GameOver;
    }

    public void phasePush()
    {
        switch (gamePhase)
        {
            case GamePhase.StandbyPhase:
                mainPhase();
                break;
            case GamePhase.MainPhase:
                endPhase();
                break;
            case GamePhase.EndPhase:
                opponentStandbyPhase();
                break;
            case GamePhase.selfHandDiscarding:
                opponentStandbyPhase();
                break;
            case GamePhase.OpponentStandbyPhase:
                opponentMainPhase();
                break;
            case GamePhase.OpponentMainPhase:
                opponentEndPhase();
                break;
            case GamePhase.OpponentEndPhase:
                standbyPhase();
                break;
            case GamePhase.opponentHandDiscarding:
                standbyPhase();
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
        if (num > player.Deck.childCount)
        {
            num = player.Deck.childCount;
        }
        for (int i = 0; i < num; i++)
        {
            GameObject card = player.Deck.GetChild(0).gameObject;
            player.handGet(card);
        }
        //player.drawMultipleCard(num);
    }

    public void discardCard(Player player, int orderInHand)
    {
        if (orderInHand < player.Hands.childCount)
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
        if (orderInGrave < player.Grave.childCount)
        {
            GameObject card = player.Grave.GetChild(orderInGrave).gameObject;
            player.deckGet(card, ifBottom);
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
        if (orderInGrave < player.Grave.childCount)
        {
            GameObject card = player.Grave.GetChild(orderInGrave).gameObject;
            player.handGet(card);
        }
        else
        {
            Debug.Log("Ĺ������intԽ��");
        }
    }

    public void deckRebuild(Player player)
    {
        int graveCount = player.Grave.childCount;
        for(int i=0; i<graveCount; i++)
        {
            cardRecycleToDeck(player, 0, false, true);
        }
    }

    public int buffCardIfQuick(Card card, bool result)
    {
        int retInt = card.setIfQuick(result);
        return retInt;
    }

    public void handCountCheck()
    {
        if(controller.Hands.childCount > maxHand)
        {
            discardRequest();
        }
        else
        {
            if (gamePhase == GamePhase.EndPhase || gamePhase == GamePhase.selfHandDiscarding)
            {
                textTMP.SetActive(false);
                controller = opponent;
                phasePush();
            }
            else if(gamePhase == GamePhase.OpponentEndPhase || gamePhase == GamePhase.opponentHandDiscarding)
            {
                textTMP.SetActive(false);
                controller = self;
                phasePush();
            }
        }
    }

    public void discardRequest()
    {
        if(gamePhase == GamePhase.EndPhase)
        {
            textTMP.GetComponent<TextMeshProUGUI>().text = "������������" + maxHand.ToString() + "��";
            gamePhase = GamePhase.selfHandDiscarding;
        }
        else if(gamePhase == GamePhase.OpponentEndPhase)
        {
            textTMP.GetComponent<TextMeshProUGUI>().text = "��������������";
            gamePhase = GamePhase.opponentHandDiscarding;
        }
        textTMP.SetActive(true);
    }

    public void returnBuffIfQuick(Card card, int retKey)
    {
        card.extractIfQuick(retKey);
    }

    public List<int> setAllCardUsable(Player player)
    {
        List<int> usableKey = new List<int>();
        foreach(var card in player.holdingCards)
        {
            usableKey.Add(card.setIfActivable(true));
        }
        return usableKey;
    }

    public void resetAllCardUsable(Player player, List<int> usableKey)
    {
        if (phaseChangeCardActivableKey.Count != 0)
        {
            for (int i = 0; i < usableKey.Count; i++)
            {
                player.holdingCards[i].extractIfActivable(usableKey[i]);
            }
        }
        phaseChangeCardActivableKey = new List<int>();
    }

    public void resetCardCountInTurn(Player player)
    {
        foreach(var card in player.holdingCards)
        {
            card.useCount_turn = 0;
        }
    }
    
    #endregion

    
    public void changeController()
    {
        if (phaseChangeCardActivableKey.Count != 0)
        {
            resetAllCardUsable(controller, phaseChangeCardActivableKey);
            phaseChangeCardActivableKey = setAllCardUsable(controller.opponent);
            controller = controller.opponent;
        }
    }

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
