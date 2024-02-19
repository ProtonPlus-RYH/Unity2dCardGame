using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public enum GamePhase
{
    GameStart, StandbyPhase, MainPhase, EndPhase, OpponentStandbyPhase, OpponentMainPhase, OpponentEndPhase, GameOver
}

public class BattleManager_Single : MonoBehaviour
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

    private GamePhase gamePhase = GamePhase.GameStart;

    // Start is called before the first frame update
    void Start()
    {
        
        distanceInGame = 0;
        DistanceChange(maxDistance);
        DistanceTMP.text = distanceInGame.ToString();

        //投硬币决定先后手
        selfPlayerPrefab.GetComponent<Player>().setIfGoingFirst(true);
        int selfStartHand = firstPlayerStartHand;
        opponentPlayerPrefab.GetComponent<Player>().setIfGoingFirst(false);
        int opponentStartHand = secondPlayerStartHand;
        if (!coin())
        {
            selfPlayerPrefab.GetComponent<Player>().setIfGoingFirst(false);
            selfStartHand = secondPlayerStartHand;
            opponentPlayerPrefab.GetComponent<Player>().setIfGoingFirst(true);
            opponentStartHand = firstPlayerStartHand;
            PhaseChange(GamePhase.OpponentStandbyPhase);
        }
        else
        {
            PhaseChange(GamePhase.StandbyPhase);
        }

        selfPlayerPrefab.GetComponent<Player>().playerStartGame(library.readDeck("01"), maxLifePoint,maxStaminaPoint,maxManaPoint, selfStartHand);
        opponentPlayerPrefab.GetComponent<Player>().playerStartGame(library.readDeck("01"), maxLifePoint, maxStaminaPoint, maxManaPoint, opponentStartHand);


        
    }

    void Update()
    {
        
    }


    public void OnClickMoveForward()
    {
        DistanceChange(-1);
        selfPlayerPrefab.GetComponent<Player>().changeSP(-1);
    }

    public void OnClickMoveBack()
    {
        DistanceChange(1);
        selfPlayerPrefab.GetComponent<Player>().changeSP(-1);
    }

    public void OnClickTurnEnd()
    {
        gamePhase = GamePhase.EndPhase;
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

    /*public void infoDisappear()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            infoDisplayer.SetActive(false);
        }
    }*/

    public void DistanceChange(int num)
    {
        //若超过距离限制则走到边缘
        if(distanceInGame + num > maxDistance)
        {
            num = maxDistance - distanceInGame;
        }else if(distanceInGame + num < 0)
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

        if(num > 0)
        {
            for(int i = 0; i < num; i++)
            {
                GameObject newGroundBlock = Instantiate(groundBlock, DistanceBar);
            }
        }else if(num < 0)
        {
            for (int i = 0; i > num; i--)
            {
                Destroy(DistanceBar.GetChild(0).gameObject);
            }
        }
    }

    public void PhaseChange(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.StandbyPhase:
                PhaseTMP.text = "我方准备阶段";
                break;
            case GamePhase.MainPhase:
                PhaseTMP.text = "我方主要阶段";
                break;
            case GamePhase.EndPhase:
                PhaseTMP.text = "我方结束阶段";
                break;
            case GamePhase.OpponentStandbyPhase:
                PhaseTMP.text = "对方准备阶段";
                break;
            case GamePhase.OpponentMainPhase:
                PhaseTMP.text = "对方主要阶段";
                break;
            case GamePhase.OpponentEndPhase:
                PhaseTMP.text = "对方结束阶段";
                break;
        }
        gamePhase = phase;
    }


    #region supports

    public bool coin()
    {
        bool result = false;
        int coin = Random.Range(0, 2);
        if(coin == 1)
        {
            result = true;
        }
        return result;
    }

    public int dice(int faceCount)
    {
        int result = Random.Range(1 , faceCount + 1);
        return result;
    }

    #endregion

}
