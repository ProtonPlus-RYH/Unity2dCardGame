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

    //public List<int> phaseChangeCardActivableKey;

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
        ChangeDistance(maxDistance);
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
            controller = opponent;
            DrawCard(self, selfStartHand);
            DrawCard(opponent, opponentStartHand);
            //��Ӳ��
            opponentStandbyPhase();
        }
        else
        {
            controller = self;
            //ӮӲ��
            DrawCard(self, selfStartHand);
            DrawCard(opponent, opponentStartHand);
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
        ChangeDistance(-1);
        ChangeSP(controller, -1);
    }

    public void OnClickMoveBack()
    {
        ChangeDistance(1);
        ChangeSP(controller, -1);
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
        if (self.deckZone.cardCount() == 0)
        {
            //ǿ�ƿ����ٹ���
            DeckRebuild(self);
        }
        DrawCard(self, 2);
        ChangeSP(self, 5);
        phasePush();
    }

    public void mainPhase()
    {
        gamePhase = GamePhase.MainPhase;
        PhaseTMP.text = "�ҷ���Ҫ�׶�";
        OpenControlling();//�򿪿���ʹ�ÿ�����
    }

    public void endPhase()
    {
        gamePhase = GamePhase.EndPhase;
        PhaseTMP.text = "�ҷ������׶�";

        CloseControlling();//�رտ���ʹ�ÿ�����
        resetCardCountInTurn(self);
        HandCountCheck();
    }

    public void opponentStandbyPhase()
    {
        gamePhase = GamePhase.OpponentStandbyPhase;
        PhaseTMP.text = "�Է�׼���׶�";
        if (opponent.deckZone.cardCount() == 0)
        {
            //ǿ�ƿ����ٹ���
            DeckRebuild(opponent);
        }
        DrawCard(opponent, 2);
        ChangeSP(opponent, 5);
        phasePush();
    }

    public void opponentMainPhase()
    {
        gamePhase = GamePhase.OpponentMainPhase;
        PhaseTMP.text = "�Է���Ҫ�׶�";
        OpenControlling();//�򿪿���ʹ�ÿ�����
    }

    public void opponentEndPhase()
    {
        gamePhase = GamePhase.OpponentEndPhase;
        PhaseTMP.text = "�Է������׶�";

        CloseControlling();//�رտ���ʹ�ÿ�����
        resetCardCountInTurn(opponent);
        HandCountCheck();
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

    public void DrawCard(Player player, int num)
    {
        if (num > player.deckZone.cardCount())
        {
            num = player.deckZone.cardCount();
        }
        for (int i = 0; i < num; i++)
        {
            GameObject card = player.deckZoneTransform.GetChild(0).gameObject;
            player.handGet(card);
        }
        //player.drawMultipleCard(num);
    }

    public void DiscardCard(Player player, int orderInHand)
    {
        if (orderInHand < player.handZone.cardCount())
        {
            GameObject card = player.handZoneTransform.GetChild(orderInHand).gameObject;
            player.graveGet(card);
        }
        else
        {
            Debug.Log("��������intԽ��");
        }
        
    }

    public void CardRecycleToDeck(Player player, int orderInGrave, bool ifBottom, bool ifShuffle)
    {
        if (orderInGrave < player.graveZone.cardCount())
        {
            GameObject card = player.graveZoneTransform.GetChild(orderInGrave).gameObject;
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

    public void CardReturnToHand(Player player, int orderInGrave)
    {
        if (orderInGrave < player.graveZone.cardCount())
        {
            GameObject card = player.graveZoneTransform.GetChild(orderInGrave).gameObject;
            player.handGet(card);
        }
        else
        {
            Debug.Log("Ĺ������intԽ��");
        }
    }

    public void DeckRebuild(Player player)
    {
        int graveCount = player.graveZone.cardCount();
        for(int i=0; i<graveCount; i++)
        {
            CardRecycleToDeck(player, 0, false, true);
        }
    }

    /*public int BuffCardIfQuick(Card card, bool result)
    {
        int retInt = card.SetIfQuickWithReturn(result);
        return retInt;
    }*/

    public void HandCountCheck()
    {
        if(controller.handZone.cardCount() > maxHand)
        {
            DiscardRequest();
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

    public void DiscardRequest()
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
/*
    public void ReturnBuffIfQuick(Card card, int retKey)
    {
        card.ExtractIfQuick(retKey);
    }

    public List<int> SetAllCardUsable(Player player)
    {
        List<int> usableKey = new List<int>();
        foreach(var card in player.holdingCards)
        {
            usableKey.Add(card.SetIfActivable(true));
        }
        return usableKey;
    }

    public void ResetAllCardUsable(Player player, List<int> usableKey)
    {
        if (phaseChangeCardActivableKey.Count != 0)
        {
            for (int i = 0; i < usableKey.Count; i++)
            {
                player.holdingCards[i].ExtractIfActivable(usableKey[i]);
            }
        }
        phaseChangeCardActivableKey = new List<int>();
    }*/

    public void resetCardCountInTurn(Player player)
    {
        foreach(var card in player.holdingCards)
        {
            card.useCount_turn = 0;
        }
    }
    
    #endregion

    public void OpenControlling()
    {
        foreach (var card in controller.holdingCards)
        {
            card.ifControlling = true;
        }
    }

    public void CloseControlling()
    {
        foreach (var card in controller.holdingCards)
        {
            card.ifControlling = false;
        }
    }
    
    public void ChangeController()
    {
        CloseControlling();
        controller = controller.opponent;
        OpenControlling();
        /*if (phaseChangeCardActivableKey.Count != 0)
        {
            ResetAllCardUsable(controller, phaseChangeCardActivableKey);
            phaseChangeCardActivableKey = SetAllCardUsable(controller.opponent);
            controller = controller.opponent;
        }
        else
        {
            phaseChangeCardActivableKey = SetAllCardUsable(controller.opponent);
            controller = controller.opponent;
        }*/
    }

    #region numerical changes

    public void ChangeLP(Player player, int num)
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

    public void ChangeSP(Player player, int num)
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

    public void ChangeMP(Player player, int num)
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

    public void ChangeDistance(int num)
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
