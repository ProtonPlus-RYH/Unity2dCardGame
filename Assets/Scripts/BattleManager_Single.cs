using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Text;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;


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
    public Text PhaseChangeHintText;

    public TextMeshProUGUI quitTMP;

    public Transform DistanceBar;
    public GameObject groundBlock;
    public GameObject InfoDisplayer;
    public GameObject AskingDialog;
    public TextMeshProUGUI AskingDialog_Title;
    public GameObject AskingDialog_2;
    public TextMeshProUGUI AskingDialog_2_Title;
    public GameObject textTMP;


    public GameObject selfPlayerPrefab; //存储我方玩家数据
    public GameObject opponentPlayerPrefab;  //存储对手玩家数据

    #endregion

    private Locale currentLanguage;
    //private LocalizedStringTable languageTable;

    public CardPool library;
    public EffectTransformer effectManager;

    public List<Buff> buffListInGame;

    public Player self;
    public Player opponent;

    public Player controller;

    public GamePhase gamePhase = GamePhase.GameStart;

    void Start()
    {
        currentLanguage = LocalizationSettings.SelectedLocale;
        //languageTable = ;
        MoveForwardClick += Action_MoveForward;
        MoveBackClick += Action_MoveBack;
        QuitClick += QuitGame;
        ifGameOver = false;
        library.getAllCards();
        self = selfPlayerPrefab.GetComponent<Player>();
        opponent = opponentPlayerPrefab.GetComponent<Player>();
        buffListInGame = new List<Buff>();
        if (PlayerPrefs.HasKey("SelfDeck"))
        {
            self.getDeck(PlayerPrefs.GetString("SelfDeck"));
        }
        else
        {
            self.getDeck("new deck");
        }
        if (PlayerPrefs.HasKey("OpponentDeck"))
        {
            opponent.getDeck(PlayerPrefs.GetString("OpponentDeck"));
        }
        else
        {
            opponent.getDeck("new deck");
        }
        self.shuffleDeck();
        opponent.shuffleDeck();
        turnCount = 1;

        distanceInGame = 0;
        ChangeDistance(maxDistance);
        DistanceTMP.text = distanceInGame.ToString();

        self.initialStatusSet(maxLifePoint, maxStaminaPoint, maxManaPoint);
        opponent.initialStatusSet(maxLifePoint, maxStaminaPoint, maxManaPoint);
        PhaseTMP.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_Start");

        //投硬币决定先后手
        self.ifGoingFirst = true;
        controller = self;
        opponent.ifGoingFirst = false;
        if (!coin())
        {
            self.ifGoingFirst = false;
            opponent.ifGoingFirst = true;
            controller = opponent;
            gamePhase = GamePhase.selfHandDiscarding;
            firstPlayerCardDraw();
            Invoke("secondPlayerCardDraw", 0.3f * firstPlayerStartHand + 0.1f);
            //输硬币
            Invoke("phasePush", 0.3f * (firstPlayerStartHand + secondPlayerStartHand));
        }
        else
        {
            //赢硬币
            gamePhase = GamePhase.opponentHandDiscarding;
            firstPlayerCardDraw();
            Invoke("secondPlayerCardDraw", 0.3f * firstPlayerStartHand + 0.1f);
            Invoke("phasePush", 0.3f * (firstPlayerStartHand + secondPlayerStartHand));
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

    #region interactions

    public void hint(string str)
    {
        textTMP.GetComponent<TextMeshProUGUI>().text = str;
        textTMP.SetActive(true);
        Invoke(nameof(closeHint), 2.0f);
    }
    public void closeHint()
    {
        textTMP.SetActive(false);
    }
    #endregion


    #region buttons

    public void OnClickMoveForward()
    {
        MoveForwardClick?.Invoke(this, new EventArgs());
    }
    public event EventHandler<EventArgs> MoveForwardClick;
    public void Action_MoveForward(object sender, EventArgs e)
    {
        if (EffectTransformer.Instance.processPhase == SolvingProcess.beforeActivation && (gamePhase == GamePhase.MainPhase || gamePhase == GamePhase.OpponentMainPhase))
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
        if (EffectTransformer.Instance.processPhase == SolvingProcess.beforeActivation && (gamePhase == GamePhase.MainPhase || gamePhase == GamePhase.OpponentMainPhase))
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

    public event EventHandler<EventArgs> ConfirmClick;
    public void OnClickConfirm()
    {
        ConfirmClick?.Invoke(this, new EventArgs());
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
        if (self.deckZone.cardCount() == 0)
        {
            //强制卡组再构筑
            DeckRebuild(self);
        }
        int drawNum = 0;
        if (turnCount >= 3)
        {
            DrawCard(self, 2);
            drawNum = 2;
        }
        ChangeSP(self, 4);
        Invoke(nameof(phasePush), drawNum * 0.3f);
    }

    public void mainPhase()
    {
        OpenControlling();//打开卡牌使用可能性
    }

    public void endPhase()
    {
        //处理未发动的预埋卡
        if(turnCount != 1)
        {
            ChangeSP(self,1);
        }
        if (opponent.fieldZone.cardCount() != 0)
        {
            ChangeController();
            preSetCard = opponent.fieldZoneTransform.GetChild(0).gameObject;
            LeanTween.rotateLocal(preSetCard, new Vector3(0f, 0f, 0f), 0.2f);
            Invoke(nameof(PreSet_Use_Invoke), 0.3f);
        }
        else
        {
            CloseControlling();//关闭卡牌使用可能性
            resetCardCountInTurn(self);
            AskingPreSet();
        }
    }

    public void opponentStandbyPhase()
    {
        if (opponent.deckZone.cardCount() == 0)
        {
            //强制卡组再构筑
            DeckRebuild(opponent);
        }
        int drawNum = 0;
        if (turnCount >= 3)
        {
            DrawCard(opponent, 2);
            drawNum = 2;
        }
        ChangeSP(opponent, 4);
        Invoke(nameof(phasePush), drawNum * 0.3f);
    }

    public void opponentMainPhase()
    {
        OpenControlling();//打开卡牌使用可能性
    }

    public void opponentEndPhase()
    {
        //处理未发动的预埋卡
        if(turnCount != 1)
        {
            ChangeSP(opponent, 1);
        }
        if (self.fieldZone.cardCount() != 0)
        {
            ChangeController();
            preSetCard = self.fieldZoneTransform.GetChild(0).gameObject;
            LeanTween.rotateLocal(preSetCard, new Vector3(0f, 0f, 0f), 0.2f);
            Invoke(nameof(PreSet_Use_Invoke), 0.3f);
        }
        else
        {
            CloseControlling();//关闭卡牌使用可能性
            resetCardCountInTurn(opponent);
            AskingPreSet();
        }
    }
    
    public void AskingPreSet()
    {
        if (controller.handZone.cardCount() != 0)
        {
            AskingDialog_Title.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_AskPreset");
            CancelClick += CancelPreSet_event;
            AskingDialog.SetActive(true);
        }
        else
        {
            HandCountCheck();
        }
    }
    public void CancelPreSet_event(object sender, EventArgs e)
    {
        AskingDialog.SetActive(false);
        CancelClick -= CancelPreSet_event;
        HandCountCheck();
    }

    public void GameOver(Player winner)
    {
        quitTMP.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_Back");
        ifGameOver = true;
        gamePhase = GamePhase.GameOver;
        string playerName = LanguageManager.Instance.GetLocalizedString("LocalizationText_BelowPlayer");
        if (winner == opponent)
        {
            playerName = LanguageManager.Instance.GetLocalizedString("LocalizationText_UpperPlayer");
        }
        textTMP.GetComponent<TextMeshProUGUI>().text = playerName + LanguageManager.Instance.GetLocalizedString("LocalizationText_Win");
        textTMP.SetActive(true);
    }

    public void phasePush()
    {
        switch (gamePhase)
        {
            case GamePhase.StandbyPhase:
                gamePhase = GamePhase.MainPhase;
                PhaseTMP.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_MainPhase");
                Invoke("mainPhase", 0.5f);
                break;
            case GamePhase.MainPhase:
                gamePhase = GamePhase.EndPhase;
                PhaseTMP.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_EndPhase");
                Invoke("endPhase", 0.5f);
                break;
            case GamePhase.EndPhase:
                PhaseChangeHintText.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_OpponentTurn");
                phaseChangeHintAnimation();
                gamePhase = GamePhase.OpponentStandbyPhase;
                PhaseTMP.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_OpponentStandbyPhase");
                Invoke("opponentStandbyPhase", 0.5f);
                break;
            case GamePhase.selfHandDiscarding:
                PhaseChangeHintText.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_OpponentTurn");
                phaseChangeHintAnimation();
                gamePhase = GamePhase.OpponentStandbyPhase;
                PhaseTMP.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_OpponentStandbyPhase");
                Invoke("opponentStandbyPhase", 0.5f);
                break;
            case GamePhase.OpponentStandbyPhase:
                gamePhase = GamePhase.OpponentMainPhase;
                PhaseTMP.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_OpponentMainPhase");
                Invoke("opponentMainPhase", 0.5f);
                break;
            case GamePhase.OpponentMainPhase:
                gamePhase = GamePhase.OpponentEndPhase;
                PhaseTMP.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_OpponentEndPhase");
                Invoke("opponentEndPhase", 0.5f);
                break;
            case GamePhase.OpponentEndPhase:
                PhaseChangeHintText.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_MyTurn");
                phaseChangeHintAnimation();
                gamePhase = GamePhase.StandbyPhase;
                PhaseTMP.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_StandbyPhase");
                Invoke("standbyPhase", 0.5f);
                break;
            case GamePhase.opponentHandDiscarding:
                PhaseChangeHintText.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_MyTurn");
                phaseChangeHintAnimation();
                gamePhase = GamePhase.StandbyPhase;
                PhaseTMP.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_StandbyPhase");
                Invoke("standbyPhase", 0.5f);
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
            StringBuilder discardHintSB = new StringBuilder(LanguageManager.Instance.GetLocalizedString("LocalizationText_DiscardRequest"));
            discardHintSB.Append(maxHand.ToString());
            textTMP.GetComponent<TextMeshProUGUI>().text = discardHintSB.ToString();
            gamePhase = GamePhase.selfHandDiscarding;
        }
        else if (gamePhase == GamePhase.OpponentEndPhase)
        {
            textTMP.GetComponent<TextMeshProUGUI>().text = LanguageManager.Instance.GetLocalizedString("LocalizationText_OpponentHandDiscarding");
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

    public void holdingCardCountdownIncrease(Card card)
    {
        foreach (var holdCard in card.holdingPlayer.holdingCards)
        {
            if (holdCard.CardID == card.CardID)
            {
                holdCard.useCount_turn++;
                holdCard.useCount_duel++;
            }
        }
    }

    public void phaseChangeHintAnimation()
    {
        Color thisColor;
        if (controller == self)
        {
            thisColor = Color.blue;
        }
        else
        {
            thisColor = Color.red;
        }
        LeanTween.textColor(PhaseChangeHintText.GetComponent<RectTransform>(), thisColor, 1.5f);
        Invoke("phaseChangeDisappearAnimation", 3.5f);
    }
    public void phaseChangeDisappearAnimation()
    {
        Color thisColor = PhaseChangeHintText.color;
        thisColor.a = 0;
        LeanTween.textColor(PhaseChangeHintText.GetComponent<RectTransform>(), thisColor, 1.5f);
        Invoke("colorChange", 2.0f);
    }
    public void colorChange()
    {
        if (controller == self)
        {
            PhaseChangeHintText.color = new Color(1.0f, 0f, 0f, 0f);
        }
        else
        {
            PhaseChangeHintText.color = new Color(0f, 0f, 1.0f, 0f);
        }
    }

    #endregion


    #region card treatments
    //预制件直接跟Player类关联就直接用Player里面的函数了

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
            //Debug.Log("手牌数量int越界");
        }
    }

    public void GraveCardRecycleToDeck(Player player, int orderInGrave, bool ifBottom, bool ifShuffle)
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
            //Debug.Log("墓地数量int越界");
        }
    }

    public void GraveCardReturnToHand(Player player, int orderInGrave)
    {
        if (orderInGrave < player.graveZone.cardCount())
        {
            GameObject card = player.graveZoneTransform.GetChild(orderInGrave).gameObject;
            player.handGet(card);
        }
        else
        {
            //Debug.Log("墓地数量int越界");
        }
    }

    public void FieldCardReturnToHand(Player player)
    {
        player.handGet(player.fieldZoneTransform.GetChild(0).gameObject);
    }

    public void FieldCardReturnToDeck(Player player, bool ifBottom, bool ifShuffle)
    {
        player.deckGet(player.fieldZoneTransform.GetChild(0).gameObject, ifBottom);
        if (ifShuffle)
        {
            player.shuffleDeck();
        }
    }

    public void DeckRebuild(Player player)
    {
        int graveCount = player.graveZone.cardCount();
        for(int i=0; i<graveCount; i++)
        {
            GraveCardRecycleToDeck(player, 0, false, true);
        }
    }

    public void CardSet(GameObject card)
    {
        AskingDialog.SetActive(false);
        CancelClick -= CancelPreSet_event;
        Card thisCard = card.GetComponent<CardDisplay>().card;
        thisCard.holdingPlayer.fieldZone.MoveOutEffectCheck(thisCard);
        card.transform.SetParent(null);
        card.transform.SetParent(thisCard.holdingPlayer.fieldZoneTransform);
        card.GetComponent<RectTransform>().localPosition = new Vector3(0f, 0f, 0f);
        card.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 180f, 0f);
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


    #region Actions

    public void MoveForward_Action()
    {
        ChangeDistance(-1);
        ChangeSP(controller, -1);
        if (EffectTransformer.Instance.processPhase == SolvingProcess.beforeActivation)//效果产生的行动不减倒数
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
        if (EffectTransformer.Instance.processPhase == SolvingProcess.beforeActivation)//效果产生的行动不减倒数
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


    public event EventHandler<AttackEventArgs> BeforeAttack;
    public void AttackDeclare(Player player0)
    {
        BeforeAttack?.Invoke(this, new AttackEventArgs(player0));
    }
    public class AttackEventArgs : EventArgs
    {
        public Player attacker;
        public AttackEventArgs(Player attacker0)
        {
            attacker = attacker0;
        }
    }


    public GameObject preSetCard;
    public void PreSet_Use_Invoke()
    {
        EffectTransformer.Instance.useCard(preSetCard);
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
