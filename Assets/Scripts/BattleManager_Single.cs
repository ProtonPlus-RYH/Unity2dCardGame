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

    public GameObject selfPlayerPrefab; //存储我方玩家数据
    public GameObject opponentPlayerPrefab;  //存储对手玩家数据

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
            drawCard(self, selfStartHand);
            drawCard(opponent, opponentStartHand);
            //输硬币
            PhaseChange(GamePhase.OpponentStandbyPhase);
        }
        else
        {
            //赢硬币
            drawCard(self, selfStartHand);
            drawCard(opponent, opponentStartHand);
            PhaseChange(GamePhase.StandbyPhase);
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
        //判定胜负

        //结束游戏
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
                PhaseTMP.text = "我方准备阶段";
                if (self.deckList.Count == 0)
                {
                    //把墓地放回卡组洗牌
                }
                drawCard(self, 2);
                changeSP(self, 5);
                PhaseChange(GamePhase.MainPhase);
                break;
            case GamePhase.MainPhase:
                PhaseTMP.text = "我方主要阶段";
                break;
            case GamePhase.EndPhase:
                PhaseTMP.text = "我方结束阶段";

                PhaseChange(GamePhase.OpponentStandbyPhase);
                break;
            case GamePhase.OpponentStandbyPhase:
                PhaseTMP.text = "对方准备阶段";
                if (opponent.deckList.Count == 0)
                {
                    //把墓地放回卡组洗牌
                }
                drawCard(opponent, 2);
                changeSP(opponent, 5);
                PhaseChange(GamePhase.OpponentMainPhase);
                break;
            case GamePhase.OpponentMainPhase:
                PhaseTMP.text = "对方主要阶段";
                break;
            case GamePhase.OpponentEndPhase:
                PhaseTMP.text = "对方结束阶段";

                PhaseChange(GamePhase.StandbyPhase);
                break;
        }
    }

    #endregion


    public void useBuff(int turnLast, int actionLast, bool ifOneChain)//持续回合数，打完几张卡后就消失，是否连锁结束后就消失
    {

    }


    #region card treatments
    //预制件直接跟Player类关联就直接用Player里面的函数了

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
            Debug.Log("手牌数量int越界");
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
            Debug.Log("墓地数量int越界");
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
            Debug.Log("墓地数量int越界");
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
            //判定胜负
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
