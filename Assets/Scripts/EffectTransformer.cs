using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using XLua;
using TMPro;
using Unity.VisualScripting;

public enum SolvingProcess
    {
        beforeActivation, activationDeclare, counterDeclare, ifNotCountered, ifCountered, counterResolve, activationResolve, delayResolve, endProcess
    }

public class EffectTransformer : MonoSingleton<EffectTransformer>
{
    public GameObject AskingDialog;
    public TextMeshProUGUI AskingDialog_Title;
    public TextMeshProUGUI AskingDialog_ButtonText;
    
    public Card activingCard;
    public Card counterCard;
    public GameObject activingCardPrefab;
    public GameObject counterCardPrefab;

    private LuaEnv luaenv_Active;
    private LuaEnv luaenv_Counter;
    
    public SolvingProcess processPhase = SolvingProcess.beforeActivation;

    public bool ifNegated_Activer;
    public bool ifNegated_Counter;
    public bool ifCounterDelayed;
    public bool ifCanBeCountered;

    public bool ifPaused;
    public bool ifDoingSelection;
    public int selectCount;
    public bool judgeResult;

    void Start()
    {
        ifNegated_Activer = false;
        ifNegated_Counter = false;
        ifCounterDelayed = false;
        ifCanBeCountered = true;
        ifPaused = false;
        ifDoingSelection = false;
        judgeResult = false;
        BattleManager_Single.Instance.CancelClick += askCounter_NoCounter;
    }

    void Update()
    {
        
    }

    #region card effect solving processes

    byte[] CardLuaLoader(ref string fileName)//卡牌Lua文件读取
    {
        string path = "Assets\\ResourceFiles\\CardScripts\\" + fileName + ".lua";
        byte[] result = File.ReadAllBytes(path);
        if (!File.Exists(path))
        {
            result = null;
        }
        return result;
    }

    public void useCard(GameObject cardPrefab)//卡牌效果发动时
    {
        processPhase = SolvingProcess.activationDeclare;
        //储存发动卡牌
        Card card = cardPrefab.GetComponent<CardDisplay>().card;
        int id = card.CardID;
        activingCard = card;
        activingCardPrefab = cardPrefab;
        BattleManager_Single.Instance.InfoDisplayer.GetComponent<InfoDisplay>().infoDisplay(card);
        Debug.Log("宣言发动");
        //卡牌使用计数增加
        card.useCount_turn++;
        card.useCount_duel++;
        //创建LuaEnv,执行发动时效果
        luaenv_Active = new LuaEnv();
        luaenv_Active.AddLoader(CardLuaLoader);
        luaenv_Active.DoString("require('Card_" + id.ToString() + "')");
        Action activationDeclare = luaenv_Active.Global.Get<Action>("ActivationDeclare");
        LuaFunction D_activationDeclare = luaenv_Active.Global.Get<LuaFunction>("ActivationDeclare");
        D_activationDeclare.Call();
        //阶段推进
        if (BattleManager_Single.Instance.controller.opponent.fieldZone.cardCount() == 0)//判断有无预埋
        {
            if (!ifPaused)
            {
                phasePush();
            }
        }
        else
        {
            GameObject preSetCard = BattleManager_Single.Instance.controller.opponent.fieldZoneTransform.GetChild(0).gameObject;
            counter(preSetCard);
        }
    }

    public void askCounter()//询问对应
    {
        BattleManager_Single.Instance.ChangeController();
        AskingDialog_Title.text = "请选择对应卡片";
        AskingDialog.SetActive(true);
    }

    public void askCounter_NoCounter(object sender, EventArgs e)//按钮：取消对应
    {
        if (processPhase == SolvingProcess.activationDeclare && !ifDoingSelection)
        {
            BattleManager_Single.Instance.ChangeController();
            AskingDialog.SetActive(false);
            ifNoCounter();
        }
    }

    public void counter(GameObject cardPrefab)//对应发动时
    {
        AskingDialog.SetActive(false);
        processPhase = SolvingProcess.counterDeclare;
        //储存对应卡牌
        Card card = cardPrefab.GetComponent<CardDisplay>().card;
        int id = card.CardID;
        counterCard = card;
        if (counterCard == null)
        {
            Debug.Log("空的");
        }
        counterCardPrefab = cardPrefab;
        BattleManager_Single.Instance.InfoDisplayer.GetComponent<InfoDisplay>().infoDisplay(card);
        Debug.Log("宣言对应");
        //卡牌使用计数增加
        card.useCount_turn++;
        card.useCount_duel++;
        //创建LuaEnv,执行对应时效果
        luaenv_Counter = new LuaEnv();
        luaenv_Counter.AddLoader(CardLuaLoader);
        luaenv_Counter.DoString("require('Card_" + id.ToString() + "')");
        Action counterDeclare = luaenv_Counter.Global.Get<Action>("CounterDeclare");
        LuaFunction D_counterDeclare = luaenv_Counter.Global.Get<LuaFunction>("CounterDeclare");
        D_counterDeclare.Call();
        if (!ifPaused)
        {
            phasePush();
        }
    }

    public void ifNoCounter()//未被对应时
    {
        processPhase = SolvingProcess.ifNotCountered;
        Debug.Log("处理若未被对应"); 
        Action whileNotCountered = luaenv_Active.Global.Get<Action>("WhileNotCountered");
        LuaFunction D_whileNotCountered = luaenv_Active.Global.Get<LuaFunction>("WhileNotCountered");
        D_whileNotCountered.Call();
        if (!ifPaused)
        {
            phasePush();
        }
    }

    public void ifCounter()//被对应时
    {
        BattleManager_Single.Instance.ChangeController();
        processPhase = SolvingProcess.ifCountered;
        Debug.Log("处理若被对应");
        Action whileCountered = luaenv_Active.Global.Get<Action>("WhileCountered");
        LuaFunction D_whileCountered = luaenv_Active.Global.Get<LuaFunction>("WhileCountered");
        D_whileCountered.Call();
        if (!ifPaused)
        {
            phasePush();
        }
    }

    public void counterResolve()//处理对应效果，若被延迟则进入发动卡牌的效果处理
    {
        BattleManager_Single.Instance.ChangeController();
        processPhase = SolvingProcess.counterResolve;
        if (!ifCounterDelayed)
        {
            Debug.Log("处理对应");
            Action counterResolve = luaenv_Counter.Global.Get<Action>("Resolve");
            LuaFunction D_counterResolve = luaenv_Counter.Global.Get<LuaFunction>("Resolve");
            D_counterResolve.Call();
        }
        if (!ifPaused)
        {
            phasePush();
        }
    }

    public void activationResolve()//处理发动卡牌效果
    {
        if (counterCard != null)
        {
            Debug.Log("拉回控制权");
            BattleManager_Single.Instance.ChangeController();
        }
        processPhase = SolvingProcess.activationResolve;
        Debug.Log("处理效果");
        Action activationResolve = luaenv_Active.Global.Get<Action>("Resolve");
        LuaFunction D_activationResolve = luaenv_Active.Global.Get<LuaFunction>("Resolve");
        D_activationResolve.Call();
        if (!ifPaused)
        {
            phasePush();
        }
    }

    public void delayResolve()//延迟处理对应效果
    {
        BattleManager_Single.Instance.ChangeController();
        processPhase = SolvingProcess.delayResolve;
        Action delayResolve = luaenv_Counter.Global.Get<Action>("Resolve");
        LuaFunction D_delayResolve = luaenv_Counter.Global.Get<LuaFunction>("Resolve");
        D_delayResolve.Call();
        if (!ifPaused)
        {
            BattleManager_Single.Instance.ChangeController();
            phasePush();
        }
    }

    public void processEnd()
    {
        luaenv_Active.Dispose();
        if (counterCard != null)
        {
            luaenv_Counter.Dispose();
        }
        processPhase = SolvingProcess.endProcess;
        if (BattleManager_Single.Instance.controller.fieldZone.cardCount() != 0)
        {
            activingCard.holdingPlayer.graveGet(BattleManager_Single.Instance.controller.fieldZoneTransform.transform.GetChild(0).gameObject);//发动者卡牌进墓
        }
        if (BattleManager_Single.Instance.controller.opponent.fieldZone.cardCount() != 0)
        {
            counterCard.holdingPlayer.graveGet(BattleManager_Single.Instance.controller.opponent.fieldZoneTransform.transform.GetChild(0).gameObject);//对应者卡牌进墓
        }
        foreach (var buff in BattleManager_Single.Instance.buffListInGame)
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
        ifNegated_Activer = false;
        ifNegated_Counter = false;
        ifCounterDelayed = false;
        ifCanBeCountered = true;
        ifPaused = false;
        ifDoingSelection = false;
        activingCard = null;
        counterCard = null;
        processPhase = SolvingProcess.beforeActivation;
    }

    public void selectSolve()
    {
        string phase = null;
        switch (processPhase)
        {
            case SolvingProcess.beforeActivation:
                Debug.Log("在处理阶段外");
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
                Debug.Log("处理结束");
                break;
            default:
                phase = "Resolve";
                break;
        }
        EffectTransformer.Instance.ifPaused = false;
        if (processPhase == SolvingProcess.counterDeclare || processPhase == SolvingProcess.counterResolve || processPhase == SolvingProcess.delayResolve)
        {
            Action afterSelection = luaenv_Counter.Global.Get<Action>("AfterSelection_" + phase);
            LuaFunction D_afterSelection = luaenv_Counter.Global.Get<LuaFunction>("AfterSelection_" + phase);
            D_afterSelection.Call();
        }
        else
        {
            Action afterSelection = luaenv_Active.Global.Get<Action>("AfterSelection_" + phase);
            LuaFunction D_afterSelection = luaenv_Active.Global.Get<LuaFunction>("AfterSelection_" + phase);
            D_afterSelection.Call();
        }
        if (!ifPaused)
        {
            EffectTransformer.Instance.ifDoingSelection = false;
            phasePush();
        }
    }


    public void phasePush()
    {
        switch (processPhase)
        {
            case SolvingProcess.activationDeclare:
                if (ifCanBeCountered)
                {
                    askCounter();
                }
                else
                {
                    activationResolve();
                }
                break;
            case SolvingProcess.counterDeclare:
                ifCounter();
                break;
            case SolvingProcess.ifNotCountered:
                activationResolve();
                break;
            case SolvingProcess.ifCountered:
                counterResolve();
                break;
            case SolvingProcess.counterResolve:
                activationResolve();
                break;
            case SolvingProcess.activationResolve:
                if (ifCounterDelayed)
                {
                    delayResolve();
                }
                else
                {
                    processEnd();
                }
                break;
            case SolvingProcess.delayResolve:
                processEnd();
                break;
        }
    }

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

    #endregion 

}
