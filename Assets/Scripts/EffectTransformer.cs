using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using XLua;

public enum SolvingProcess
    {
        beforeActivation, activationDeclare, counterDeclare, ifNotCountered, ifCountered, counterResolve, activationResolve, delayResolve, endProcess
    }

public class EffectTransformer : MonoSingleton<EffectTransformer>
{
    public GameObject CounterAskDialog;
    
    //public Player player_Active;
    //public Player player_Counter;

    public Card activingCard;
    public Card counterCard;


    private LuaEnv luaenv_Active;
    private LuaEnv luaenv_Counter;
    
    public SolvingProcess processPhase = SolvingProcess.beforeActivation;

    public bool ifNegated_Activer;
    public bool ifNegated_Counter;
    public bool ifCounterDelayed;
    public bool ifCanBeCountered;

    void Start()
    {
        ifNegated_Activer = false;
        ifNegated_Counter = false;
        ifCounterDelayed = false;
        ifCanBeCountered = true;
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

    public void useCard(Card card)//卡牌效果发动时
    {
        processPhase = SolvingProcess.activationDeclare;
        int id = card.CardID;
        activingCard = card;
        luaenv_Active = new LuaEnv();
        luaenv_Active.AddLoader(CardLuaLoader);
        luaenv_Active.DoString("require('Card_" + id.ToString() + "')");
        Action activationDeclare = luaenv_Active.Global.Get<Action>("ActivationDeclare");
        activationDeclare();
        LuaFunction D_activationDeclare = luaenv_Active.Global.Get<LuaFunction>("ActivationDeclare");
        D_activationDeclare.Call();
        if (BattleManager_Single.Instance.controller.opponent.FieldZone.childCount == 0)
        {
            if (ifCanBeCountered)
            {
                askCounter();
            }
            else
            {
                activationResolve();
            }
        }
        else
        {
            GameObject preSetCard = BattleManager_Single.Instance.controller.opponent.FieldZone.GetChild(0).gameObject;
            counter(preSetCard.gameObject.GetComponent<CardDisplay>().card);
        }
    }

    public void askCounter()//询问对应
    {
        CounterAskDialog.SetActive(true);
        BattleManager_Single.Instance.changeController();
    }

    public void OnButtonClickNoCounter()//按钮：取消对应
    {
        CounterAskDialog.SetActive(false);
        BattleManager_Single.Instance.changeController();
        ifNoCounter();
    }

    public void counter(Card card)//对应发动时
    {
        BattleManager_Single.Instance.changeController();
        processPhase = SolvingProcess.activationDeclare;
        int id = card.CardID;
        counterCard = card;
        CounterAskDialog.SetActive(false);
        luaenv_Counter = new LuaEnv();
        luaenv_Counter.AddLoader(CardLuaLoader);
        luaenv_Counter.DoString("require('Card_" + id.ToString() + "')");
        Action counterDeclare = luaenv_Counter.Global.Get<Action>("CounterDeclare");
        LuaFunction D_counterDeclare = luaenv_Active.Global.Get<LuaFunction>("CounterDeclare");
        D_counterDeclare.Call();
        ifCounter();
    }

    public void ifNoCounter()//未被对应时
    {
        processPhase = SolvingProcess.ifNotCountered;
        Action whileNotCountered = luaenv_Active.Global.Get<Action>("WhileNotCountered");
        LuaFunction D_whileNotCountered = luaenv_Active.Global.Get<LuaFunction>("WhileNotCountered");
        D_whileNotCountered.Call();
        activationResolve();
    }

    public void ifCounter()//被对应时
    {
        processPhase = SolvingProcess.ifCountered;
        Action whileCountered = luaenv_Active.Global.Get<Action>("WhileCountered");
        LuaFunction D_whileCountered = luaenv_Active.Global.Get<LuaFunction>("WhileCountered");
        D_whileCountered.Call();
        counterResolve();
    }

    public void counterResolve()//处理对应效果，若被延迟则进入发动卡牌的效果处理
    {
        if (!ifCounterDelayed)
        {
            processPhase = SolvingProcess.counterResolve;
            Action counterResolve = luaenv_Counter.Global.Get<Action>("Resolve");
            LuaFunction D_counterResolve = luaenv_Active.Global.Get<LuaFunction>("Resolve");
            D_counterResolve.Call();
            luaenv_Counter.Dispose();
        }
        activationResolve();
    }

    public void activationResolve()//处理发动卡牌效果
    {
        processPhase = SolvingProcess.activationResolve;
        Action activationResolve = luaenv_Active.Global.Get<Action>("Resolve");
        LuaFunction D_activationResolve = luaenv_Active.Global.Get<LuaFunction>("Resolve");
        D_activationResolve.Call();
        luaenv_Active.Dispose();
        if (ifCounterDelayed)
        {
            delayResolve();
        }
        else
        {
            processEnd();
        }
    }

    public void delayResolve()//延迟处理对应效果
    {
        processPhase = SolvingProcess.delayResolve;
        Action delayResolve = luaenv_Counter.Global.Get<Action>("Resolve");
        LuaFunction D_delayResolve = luaenv_Active.Global.Get<LuaFunction>("Resolve");
        D_delayResolve.Call();
        luaenv_Counter.Dispose();
        processEnd();
    }

    public void processEnd()
    {
        processPhase = SolvingProcess.endProcess;
        ifNegated_Activer = false;
        ifNegated_Counter = false;
        ifCounterDelayed = false;
        ifCanBeCountered = true;
        activingCard = null;
        counterCard = null;
        processPhase = SolvingProcess.beforeActivation;
    }

    #endregion 

}
