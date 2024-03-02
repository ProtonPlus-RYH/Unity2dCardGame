using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Threading;
using UnityEngine.SceneManagement;


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

    public int turnCount;
    public bool ifGameOver;

    public int firstPlayerStartHand;
    public int secondPlayerStartHand;


    //public List<int> phaseChangeCardActivableKey;

    #endregion

    #region UIs

    public TextMeshProUGUI DistanceTMP;
    public TextMeshProUGUI PhaseTMP;

    public TextMeshProUGUI quitTMP;

    public Transform DistanceBar;
    public GameObject groundBlock;
    public GameObject infoDisplayer;
    public GameObject textTMP;

    public GameObject InfoDisplayer;

    public GameObject selfPlayerPrefab; //�洢�ҷ��������
    public GameObject opponentPlayerPrefab;  //�洢�����������

    #endregion
    
    public CardPool library;
    public EffectTransformer effectManager;

    public List<Buff> buffListInGame;

    public Player self;
    public Player opponent;

    public Player controller;

    public GamePhase gamePhase = GamePhase.GameStart;

    void Start()
    {
        MoveForwardClick += Action_MoveForward;
        MoveBackClick += Action_MoveBack;
        QuitClick += QuitGame;
        ifGameOver = false;
        library.getAllCards();
        self = selfPlayerPrefab.GetComponent<Player>();
        opponent = opponentPlayerPrefab.GetComponent<Player>();
        buffListInGame = new List<Buff>();
        self.getDeck(PlayerPrefs.GetString("SelfDeck"));
        opponent.getDeck(PlayerPrefs.GetString("OpponentDeck"));
        self.shuffleDeck();
        opponent.shuffleDeck();
        turnCount = 1;

        distanceInGame = 0;
        ChangeDistance(maxDistance);
        DistanceTMP.text = distanceInGame.ToString();

        self.initialStatusSet(maxLifePoint, maxStaminaPoint, maxManaPoint);
        opponent.initialStatusSet(maxLifePoint, maxStaminaPoint, maxManaPoint);
        PhaseTMP.text = "��Ϸ��ʼ";

        //ͶӲ�Ҿ����Ⱥ���
        self.ifGoingFirst = true;
        controller = self;
        opponent.ifGoingFirst = false;
        if (!coin())
        {
            self.ifGoingFirst = false;
            opponent.ifGoingFirst = true;
            controller = opponent;
            firstPlayerCardDraw();
            Invoke("secondPlayerCardDraw", 0.3f * firstPlayerStartHand + 0.1f);
            //��Ӳ��
            Invoke("opponentStandbyPhase", 0.3f * (firstPlayerStartHand + secondPlayerStartHand));
        }
        else
        {
            //ӮӲ��
            firstPlayerCardDraw();
            Invoke("secondPlayerCardDraw", 0.3f * firstPlayerStartHand + 0.1f);
            Invoke("standbyPhase", 0.3f * (firstPlayerStartHand + secondPlayerStartHand));
        }
        
    }

    public void firstPlayerCardDraw()
    {
        DrawCard(controller, firstPlayerStartHand);
        ChangeController();
    }
    public void secondPlayerCardDraw()
    {
        DrawCard(controller, secondPlayerStartHand);
        ChangeController();
    }

    void Update()
    {
        
    }

    #region buttons

    public void OnClickMoveForward()
    {
        MoveForwardClick?.Invoke(this, new EventArgs());
    }
    public event EventHandler<EventArgs> MoveForwardClick;
    public void Action_MoveForward(object sender, EventArgs e)
    {
        if (EffectTransformer.Instance.processPhase == SolvingProcess.beforeActivation)
        {
            MoveForward_Action();
        }
    }

    public void OnClickMoveBack()
    {
        MoveBackClick?.Invoke(this, new EventArgs());
    }
    public event EventHandler<EventArgs> MoveBackClick;
    public void Action_MoveBack(object sender, EventArgs e)
    {
        if (EffectTransformer.Instance.processPhase == SolvingProcess.beforeActivation)
        {
            MoveBack_Action();
        }
    }

    public void OnClickTurnEnd()
    {
        if(gamePhase == GamePhase.MainPhase || gamePhase == GamePhase.OpponentMainPhase)
        {
            phasePush();
        }
    }


    public event EventHandler<EventArgs> QuitClick;
    public void OnClickSurrender()
    {
        QuitClick?.Invoke(this, new EventArgs());
    }
    public void QuitGame(object sender, EventArgs e)
    {
        if (ifGameOver)
        {
            SceneManager.LoadScene("MenuPage");
        }
        else
        {
            GameOver(controller.opponent);
        }
    }

    public event EventHandler<EventArgs> CancelClick;
    public void OnClickCancel()
    {
        CancelClick?.Invoke(this, new EventArgs());
    }

    #endregion


    #region Controller Management

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
    }

    #endregion


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
        int drawNum = 0;
        if (turnCount >= 3)
        {
            DrawCard(self, 2);
            drawNum = 2;
        }
        ChangeSP(self, 5);
        Invoke("phasePush",drawNum * 0.3f);
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
        int drawNum = 0;
        if (turnCount >= 3)
        {
            DrawCard(opponent, 2);
            drawNum = 2;
        }
        ChangeSP(opponent, 5);
        Invoke("phasePush", drawNum * 0.3f);
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

    public void GameOver(Player winner)
    {
        quitTMP.text = "����";
        ifGameOver = true;
        gamePhase = GamePhase.GameOver;
        string playerName = "�·����";
        if (winner == opponent)
        {
            playerName = "�Ϸ����";
        }
        textTMP.GetComponent<TextMeshProUGUI>().text = playerName + "��ʤ";
        textTMP.SetActive(true);
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

    public void HandCountCheck()
    {
        if (controller.handZone.cardCount() > maxHand)
        {
            DiscardRequest();
        }
        else
        {
            textTMP.SetActive(false);
            foreach (var buff in BattleManager_Single.Instance.buffListInGame)
            {
                if (buff.buffLast.Contains(BuffLast.turnLast) || buff.buffLast.Contains(BuffLast.turnLast_opponent))
                {
                    buff.CountdownDecrease(BuffLast.turnLast);
                }
            }
            controller = controller.opponent;
            turnCount++;
            phasePush();
        }
    }

    public void DiscardRequest()
    {
        if (gamePhase == GamePhase.EndPhase)
        {
            textTMP.GetComponent<TextMeshProUGUI>().text = "������������" + maxHand.ToString() + "��";
            gamePhase = GamePhase.selfHandDiscarding;
        }
        else if (gamePhase == GamePhase.OpponentEndPhase)
        {
            textTMP.GetComponent<TextMeshProUGUI>().text = "��������������";
            gamePhase = GamePhase.opponentHandDiscarding;
        }
        textTMP.SetActive(true);
    }

    public void resetCardCountInTurn(Player player)
    {
        foreach (var card in player.holdingCards)
        {
            card.useCount_turn = 0;
        }
    }

    #endregion


    #region card treatments
    //Ԥ�Ƽ�ֱ�Ӹ�Player�������ֱ����Player����ĺ�����

    public void DrawCard(Player player, int num)
    {
        player.drawMultipleCards(num);
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

    
    #endregion


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
            GameOver(player.opponent);
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
            num = -1 * sp;
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
            num = -1 * mp;
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

    public void MoveForward_Action()
    {
        ChangeDistance(-1);
        ChangeSP(controller, -1);
        if (EffectTransformer.Instance.processPhase == SolvingProcess.beforeActivation)//Ч���������ж���������
        {
            foreach (var buff in buffListInGame)
            {
                if (buff.buffLast.Contains(BuffLast.actionLast))
                {
                    buff.CountdownDecrease(BuffLast.actionLast);
                }
            }
        }
    }

    public void MoveBack_Action()
    {
        ChangeDistance(1);
        ChangeSP(controller, -1);
        if (EffectTransformer.Instance.processPhase == SolvingProcess.beforeActivation)//Ч���������ж���������
        {
            foreach (var buff in buffListInGame)
            {
                if (buff.buffLast.Contains(BuffLast.actionLast))
                {
                    buff.CountdownDecrease(BuffLast.actionLast);
                }
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
