using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using XLua;
using TMPro;
using Unity.VisualScripting;
using System.Text;

public enum SolvingProcess
    {
        beforeActivation, activationDeclare, counterDeclare, ifNotCountered, ifCountered, counterResolve, activationResolve, delayResolve, endProcess
    }

public class EffectTransformer : MonoSingleton<EffectTransformer>
{
    public TextMeshProUGUI AskingDialog_ButtonText;
    
    public Card activingCard;
    public Card counterCard;
    public GameObject activingCardPrefab;
    public GameObject counterCardPrefab;
    public Card solvingCard;

    private LuaEnv luaenv_Active;
    private LuaEnv luaenv_Counter;
    
    public SolvingProcess processPhase = SolvingProcess.beforeActivation;

    public int negatedKey_Activer;
    public int negatedKey_Counter;
    public bool ifCounterDelayed;
    public bool ifCanBeCountered;

    public bool ifPaused;
    public bool ifDoingSelection;
    public int selectCount;
    public bool judgeResult;

    public bool ifReturnedToSomewhere_active;
    public bool ifReturnedToSomewhere_counter;

    public int distanceWhileDeclareActivation;

    void Start()
    {
        negatedKey_Activer = -1;
        negatedKey_Counter = -1;
        ifCounterDelayed = false;
        ifCanBeCountered = true;
        ifPaused = false;
        ifDoingSelection = false;
        judgeResult = false;
        ifReturnedToSomewhere_active = false;
        ifReturnedToSomewhere_counter = false;
        BattleManager_Single.Instance.CancelClick += askCounter_NoCounter;
    }

    void Update()
    {
        
    }

    #region card effect solving processes

    byte[] CardLuaLoader(ref string fileName)//����Lua�ļ���ȡ
    {
        StringBuilder pathSB = new StringBuilder(Application.streamingAssetsPath);
        pathSB.Append("/ResourceFiles/CardScripts/");
        pathSB.Append(fileName);
        pathSB.Append(".lua");
        byte[] result = File.ReadAllBytes(pathSB.ToString());
        if (!File.Exists(pathSB.ToString()))
        {
            result = null;
        }
        return result;
    }

    public void useCard(GameObject cardPrefab)//����Ч������ʱ
    {
        processPhase = SolvingProcess.activationDeclare;
        //���淢������
        Card card = cardPrefab.GetComponent<CardDisplay>().card;
        int id = card.CardID;
        activingCard = card;
        activingCardPrefab = cardPrefab;
        solvingCard = activingCard;
        BattleManager_Single.Instance.InfoDisplayer.GetComponent<InfoDisplay>().infoDisplay(card);
        //Debug.Log("���Է���");
        distanceWhileDeclareActivation = BattleManager_Single.Instance.distanceInGame;
        //����ʹ�ü�������
        card.useCount_turn++;
        card.useCount_duel++;
        //����LuaEnv,ִ�з���ʱЧ��
        luaenv_Active = new LuaEnv();
        luaenv_Active.AddLoader(CardLuaLoader);
        StringBuilder cardReadSB = new StringBuilder("require('Card_");
        cardReadSB.Append(id.ToString());
        cardReadSB.Append("')");
        luaenv_Active.DoString(cardReadSB.ToString());
        Action activationDeclare = luaenv_Active.Global.Get<Action>("ActivationDeclare");
        LuaFunction D_activationDeclare = luaenv_Active.Global.Get<LuaFunction>("ActivationDeclare");
        D_activationDeclare.Call();
        //�׶��ƽ�
        BattleManager_Single.Instance.ChangeController();
        if (BattleManager_Single.Instance.controller.fieldZone.cardCount() == 0)//�ж�����Ԥ��
        {
            if (!ifPaused)
            {
                EffectSolvingPhasePush();
            }
        }
        else
        {
            preSetCard = BattleManager_Single.Instance.controller.fieldZoneTransform.GetChild(0).gameObject;
            LeanTween.rotateLocal(preSetCard, new Vector3(0f, 0f, 0f), 0.2f);
            Invoke("PreSet_Invoke", 0.3f);
        }
    }

    public GameObject preSetCard;
    public void PreSet_Invoke()
    {
        counter(preSetCard);
    }

    public void askCounter()//ѯ�ʶ�Ӧ
    {
        BattleManager_Single.Instance.AskingDialog_Title.text = LanguageManager.Instance.GetLocalizedString("LocalizationText_Effect_AskCounter");
        BattleManager_Single.Instance.AskingDialog.SetActive(true);
    }

    public void askCounter_NoCounter(object sender, EventArgs e)//��ť��ȡ����Ӧ
    {
        if (processPhase == SolvingProcess.activationDeclare && !ifDoingSelection)
        {
            BattleManager_Single.Instance.AskingDialog.SetActive(false);
            ifNoCounter();
        }
    }

    public void counter(GameObject cardPrefab)//��Ӧ����ʱ
    {
        BattleManager_Single.Instance.AskingDialog.SetActive(false);
        processPhase = SolvingProcess.counterDeclare;
        //�����Ӧ����
        Card card = cardPrefab.GetComponent<CardDisplay>().card;
        int id = card.CardID;
        counterCard = card;
        counterCardPrefab = cardPrefab;
        solvingCard = counterCard;
        BattleManager_Single.Instance.InfoDisplayer.GetComponent<InfoDisplay>().infoDisplay(card);
        //Debug.Log("���Զ�Ӧ");
        //����ʹ�ü�������
        card.useCount_turn++;
        card.useCount_duel++;
        //����LuaEnv,ִ�ж�ӦʱЧ��
        luaenv_Counter = new LuaEnv();
        luaenv_Counter.AddLoader(CardLuaLoader);
        StringBuilder cardReadSB = new StringBuilder("require('Card_");
        cardReadSB.Append(id.ToString());
        cardReadSB.Append("')");
        luaenv_Counter.DoString(cardReadSB.ToString());
        Action counterDeclare = luaenv_Counter.Global.Get<Action>("CounterDeclare");
        LuaFunction D_counterDeclare = luaenv_Counter.Global.Get<LuaFunction>("CounterDeclare");
        D_counterDeclare.Call();
        if (!ifPaused)
        {
            EffectSolvingPhasePush();
        }
    }

    public void ifNoCounter()//δ����Ӧʱ
    {
        processPhase = SolvingProcess.ifNotCountered;
        solvingCard = activingCard;
        //Debug.Log("������δ����Ӧ");
        LuaFunction D_whileNotCountered = luaenv_Active.Global.Get<LuaFunction>("WhileNotCountered");
        D_whileNotCountered.Call();
        if (!ifPaused)
        {
            EffectSolvingPhasePush();
        }
    }

    public void ifCounter()//����Ӧʱ
    {
        processPhase = SolvingProcess.ifCountered;
        solvingCard = activingCard;
        //Debug.Log("����������Ӧ");
        LuaFunction D_whileCountered = luaenv_Active.Global.Get<LuaFunction>("WhileCountered");
        D_whileCountered.Call();
        if (!ifPaused)
        {
            EffectSolvingPhasePush();
        }
    }

    public void counterResolve()//�����ӦЧ���������ӳ�����뷢�����Ƶ�Ч������
    {
        processPhase = SolvingProcess.counterResolve;
        solvingCard = counterCard;
        if (!ifCounterDelayed)
        {
            //Debug.Log("�����Ӧ");
            LuaFunction D_counterResolve = luaenv_Counter.Global.Get<LuaFunction>("Resolve");
            D_counterResolve.Call();
        }
        if (!ifPaused)
        {
            EffectSolvingPhasePush();
        }
    }

    public void activationResolve()//����������Ч��
    {
        
        processPhase = SolvingProcess.activationResolve;
        solvingCard = activingCard;
        //Debug.Log("����Ч��");
        LuaFunction D_activationResolve = luaenv_Active.Global.Get<LuaFunction>("Resolve");
        D_activationResolve.Call();
        if (!ifPaused)
        {
            EffectSolvingPhasePush();
        }
    }

    public void delayResolve()//�ӳٴ����ӦЧ��
    {
        processPhase = SolvingProcess.delayResolve;
        solvingCard = counterCard;
        //Debug.Log("�����ӳٶ�Ӧ");
        LuaFunction D_delayResolve = luaenv_Counter.Global.Get<LuaFunction>("Resolve");
        D_delayResolve.Call();
        if (!ifPaused)
        {
            EffectSolvingPhasePush();
        }
    }

    public void processEnd()
    {
        processPhase = SolvingProcess.endProcess;
        luaenv_Active.Dispose();
        if (counterCard != null)
        {
            luaenv_Counter.Dispose();
        }
        if (activingCard != null && activingCard.holdingPlayer.fieldZone.cardCount() != 0)
        {
            activingCard.holdingPlayer.graveGet(activingCard.holdingPlayer.fieldZoneTransform.transform.GetChild(0).gameObject);//�����߿��ƽ�Ĺ
        }
        if (counterCard != null && counterCard.holdingPlayer.fieldZone.cardCount() != 0)
        {
            counterCard.holdingPlayer.graveGet(counterCard.holdingPlayer.fieldZoneTransform.transform.GetChild(0).gameObject);//��Ӧ�߿��ƽ�Ĺ
        }
        foreach (var buff in BattleManager_Single.Instance.buffListInGame)//�ж����������Ƽ���buff��������
        {
            if (buff.buffLast.Contains(BuffLast.actionLast))
            {
                buff.CountdownDecrease(BuffLast.actionLast);
            }
            if (buff.buffLast.Contains(BuffLast.cardLast))
            {
                buff.CountdownDecrease(BuffLast.cardLast);
            }
        }
        if (negatedKey_Activer != -1)
        {
            activingCard.ExtractIfNegated(negatedKey_Activer);
            negatedKey_Activer = -1;
        }
        if (negatedKey_Counter != -1)
        {
            counterCard.ExtractIfNegated(negatedKey_Counter);
            negatedKey_Counter = -1;
        }
        BattleManager_Single.Instance.textTMP.SetActive(false);
        ifCounterDelayed = false;
        ifCanBeCountered = true;
        ifPaused = false;
        ifDoingSelection = false;
        ifReturnedToSomewhere_active = false;
        ifReturnedToSomewhere_counter = false;
        activingCard = null;
        counterCard = null;
        solvingCard = null;
        processPhase = SolvingProcess.beforeActivation;
        if (BattleManager_Single.Instance.gamePhase == GamePhase.EndPhase)
        {
            BattleManager_Single.Instance.endPhase();
        }
        else if (BattleManager_Single.Instance.gamePhase == GamePhase.OpponentEndPhase)
        {
            BattleManager_Single.Instance.opponentEndPhase();
        }
    }

    public void selectSolve()
    {
        string phase = null;
        switch (processPhase)
        {
            case SolvingProcess.beforeActivation:
                //Debug.Log("�ڴ���׶���");
                break;
            case SolvingProcess.activationDeclare:
                phase = "ActivationDeclare";
                break;
            case SolvingProcess.counterDeclare:
                phase = "CounterDeclare";
                break;
            case SolvingProcess.ifNotCountered:
                phase = "WhileNotCountered";
                break;
            case SolvingProcess.ifCountered:
                phase = "WhileCountered";
                break;
            case SolvingProcess.endProcess:
                phase = "AfterResolve";
                //Debug.Log("�������");
                break;
            default:
                phase = "Resolve";
                break;
        }
        ifPaused = false;
        StringBuilder phaseFunctionSB = new StringBuilder("AfterSelection_");
        phaseFunctionSB.Append(phase);
        if (processPhase == SolvingProcess.counterDeclare || processPhase == SolvingProcess.counterResolve || processPhase == SolvingProcess.delayResolve)
        {
            LuaFunction D_afterSelection = luaenv_Counter.Global.Get<LuaFunction>(phaseFunctionSB.ToString());
            D_afterSelection.Call();
        }
        else
        {
            LuaFunction D_afterSelection = luaenv_Active.Global.Get<LuaFunction>(phaseFunctionSB.ToString());
            D_afterSelection.Call();
        }
        if (!ifPaused)
        {
            ifDoingSelection = false;
            EffectSolvingPhasePush();
        }
    }


    public void EffectSolvingPhasePush()
    {
        switch (processPhase)
        {
            case SolvingProcess.activationDeclare:
                if (ifCanBeCountered)
                {
                    Invoke(nameof(askCounter), 0.5f);
                }
                else
                {
                    Invoke(nameof(ifNoCounter), 0.5f);
                }
                break;
            case SolvingProcess.counterDeclare:
                BattleManager_Single.Instance.ChangeController();
                Invoke(nameof(ifCounter), 0.5f);
                break;
            case SolvingProcess.ifNotCountered:
                BattleManager_Single.Instance.ChangeController();
                Invoke(nameof(activationResolve), 0.5f);
                break;
            case SolvingProcess.ifCountered:
                BattleManager_Single.Instance.ChangeController();
                Invoke(nameof(counterResolve), 0.5f);
                break;
            case SolvingProcess.counterResolve:
                BattleManager_Single.Instance.ChangeController();
                BattleManager_Single.Instance.textTMP.GetComponent<TextMeshProUGUI>().text = LanguageManager.Instance.GetLocalizedString("LocalizationText_Effect_SolvingCounter");
                BattleManager_Single.Instance.textTMP.SetActive(true);
                Invoke(nameof(activationResolve), 0.5f);
                break;
            case SolvingProcess.activationResolve:
                BattleManager_Single.Instance.textTMP.GetComponent<TextMeshProUGUI>().text = LanguageManager.Instance.GetLocalizedString("LocalizationText_Effect_SolvingActivation");
                BattleManager_Single.Instance.textTMP.SetActive(true);
                if (ifCounterDelayed)
                {
                    BattleManager_Single.Instance.ChangeController();
                    Invoke(nameof(delayResolve), 0.5f);
                }
                else
                {
                    Invoke(nameof(processEnd), 0.5f);
                }
                break;
            case SolvingProcess.delayResolve:
                BattleManager_Single.Instance.ChangeController();
                BattleManager_Single.Instance.textTMP.GetComponent<TextMeshProUGUI>().text = LanguageManager.Instance.GetLocalizedString("LocalizationText_Effect_DelayResolved");
                BattleManager_Single.Instance.textTMP.SetActive(true);
                Invoke(nameof(processEnd), 0.5f);
                break;
        }
    }

    #endregion 

    public Player getUserByPhase()
    {
        Player result = EffectTransformer.Instance.activingCard.holdingPlayer;
        switch (EffectTransformer.Instance.processPhase)
        {
            case SolvingProcess.counterDeclare:
                result = result.opponent;
                break;
            case SolvingProcess.counterResolve:
                result = result.opponent;
                break;
            case SolvingProcess.delayResolve:
                result = result.opponent;
                break;
        }
        return result;
    }

}
