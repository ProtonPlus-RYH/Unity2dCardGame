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

    public GameObject selfPlayerPrefab; //存储我方玩家数据
    public GameObject opponentPlayerPrefab;  //存储对手玩家数据

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

        //投硬币决定先后手
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
            //输硬币
            opponentStandbyPhase();
        }
        else
        {
            controller = self;
            //赢硬币
            DrawCard(self, selfStartHand);
            DrawCard(opponent, opponentStartHand);
            standbyPhase();
        }
        
    }

    void Update()
    {
        //点击信息框外则卡片信息关闭
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
        //判定胜负

        //结束游戏
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
        PhaseTMP.text = "我方准备阶段";
        if (self.deckZone.cardCount() == 0)
        {
            //强制卡组再构筑
            DeckRebuild(self);
        }
        DrawCard(self, 2);
        ChangeSP(self, 5);
        phasePush();
    }

    public void mainPhase()
    {
        gamePhase = GamePhase.MainPhase;
        PhaseTMP.text = "我方主要阶段";
        OpenControlling();//打开卡牌使用可能性
    }

    public void endPhase()
    {
        gamePhase = GamePhase.EndPhase;
        PhaseTMP.text = "我方结束阶段";

        CloseControlling();//关闭卡牌使用可能性
        resetCardCountInTurn(self);
        HandCountCheck();
    }

    public void opponentStandbyPhase()
    {
        gamePhase = GamePhase.OpponentStandbyPhase;
        PhaseTMP.text = "对方准备阶段";
        if (opponent.deckZone.cardCount() == 0)
        {
            //强制卡组再构筑
            DeckRebuild(opponent);
        }
        DrawCard(opponent, 2);
        ChangeSP(opponent, 5);
        phasePush();
    }

    public void opponentMainPhase()
    {
        gamePhase = GamePhase.OpponentMainPhase;
        PhaseTMP.text = "对方主要阶段";
        OpenControlling();//打开卡牌使用可能性
    }

    public void opponentEndPhase()
    {
        gamePhase = GamePhase.OpponentEndPhase;
        PhaseTMP.text = "对方结束阶段";

        CloseControlling();//关闭卡牌使用可能性
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


    public void useBuff(int turnLast, int actionLast, bool ifOneChain)//持续回合数，打完几张卡后就消失，是否连锁结束后就消失
    {

    }


    #region card treatments
    //预制件直接跟Player类关联就直接用Player里面的函数了

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
            Debug.Log("手牌数量int越界");
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
            Debug.Log("墓地数量int越界");
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
            Debug.Log("墓地数量int越界");
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
            textTMP.GetComponent<TextMeshProUGUI>().text = "请弃置手牌至" + maxHand.ToString() + "张";
            gamePhase = GamePhase.selfHandDiscarding;
        }
        else if(gamePhase == GamePhase.OpponentEndPhase)
        {
            textTMP.GetComponent<TextMeshProUGUI>().text = "对手弃置手牌中";
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
            //判定胜负
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
        //若超过距离限制则走到边缘
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

        //改变间距
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
