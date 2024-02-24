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
    
    public Player player_Active;
    public Player player_Counter;

    public CardPool library;

    private LuaEnv luaenv_Active;
    private LuaEnv luaenv_Counter;
    
    public SolvingProcess processPhase = SolvingProcess.beforeActivation;

    private bool ifNegated_Activer;
    private bool ifNegated_Counter;
    private bool ifCounterDelayed;
    private bool ifCanBeCountered;

    void Start()
    {
        library.getAllCards();
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

    public void useCard(int id)//卡牌效果发动时
    {
        processPhase = SolvingProcess.activationDeclare;
        luaenv_Active = new LuaEnv();
        luaenv_Active.AddLoader(CardLuaLoader);
        luaenv_Active.DoString("require('Card_" + id.ToString() + "')");
        Action activationDeclare = luaenv_Active.Global.Get<Action>("ActivationDeclare");
        activationDeclare();
        LuaFunction D_activationDeclare = luaenv_Active.Global.Get<LuaFunction>("ActivationDeclare");
        D_activationDeclare.Call();
        if (ifCanBeCountered)
        {
            askCounter();
        }
        else
        {
            activationResolve();
        }
    }

    public void askCounter()//询问对应
    {
        CounterAskDialog.SetActive(true);
    }

    public void OnButtonClickNoCounter()//按钮：取消对应
    {
        CounterAskDialog.SetActive(false);
        ifNoCounter();
    }

    public void counter(int id)//对应发动时
    {
        processPhase = SolvingProcess.activationDeclare;
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
        processPhase = SolvingProcess.counterResolve;
        if (!ifCounterDelayed && !ifNegated_Counter)
        {
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
        if (!ifNegated_Activer)
        {
            Action activationResolve = luaenv_Active.Global.Get<Action>("Resolve");
            LuaFunction D_activationResolve = luaenv_Active.Global.Get<LuaFunction>("Resolve");
            D_activationResolve.Call();
        }
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
        if (!ifNegated_Counter)
        {
            Action delayResolve = luaenv_Counter.Global.Get<Action>("Resolve");
            LuaFunction D_delayResolve = luaenv_Active.Global.Get<LuaFunction>("Resolve");
            D_delayResolve.Call();
            luaenv_Counter.Dispose();
        }
        processEnd();
    }

    public void processEnd()
    {
        processPhase = SolvingProcess.endProcess;
        ifNegated_Activer = false;
        ifNegated_Counter = false;
        ifCounterDelayed = false;
        ifCanBeCountered = true;
        processPhase = SolvingProcess.beforeActivation;
    }

    #endregion 

    public Player getUserInPhase()
    {
        Player result = BattleManager_Single.Instance.getCardPlayer();
        switch (processPhase)
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

    #region effects
    //为了规范修改都放到BattleManager里面去了

    public void MoveForward(bool ifActiver, bool ifUseStamina, int num)
    {
        if (ifUseStamina)
        {
            Player activer = getUserInPhase();
            if (ifActiver)
            {
                BattleManager_Single.Instance.changeSP(activer, num * (-1));
            }
            else
            {
                BattleManager_Single.Instance.changeSP(activer.opponent, num * (-1));
            }
        }
        BattleManager_Single.Instance.changeDistance(num * (-1));
        
    }

    public void MoveBack(bool ifActiver, bool ifUseStamina, int num)
    {
        if (ifUseStamina)
        {
            Player activer = getUserInPhase();
            if (ifActiver)
            {
                BattleManager_Single.Instance.changeSP(activer, num * (-1));
            }
            else
            {
                BattleManager_Single.Instance.changeSP(activer.opponent, num * (-1));
            }
        }
        BattleManager_Single.Instance.changeDistance(num);
    }
    
    public void DealDamage(bool ifActiverHurt, int num)
    {
        Player activer = getUserInPhase();
        if (ifActiverHurt)
        {
            BattleManager_Single.Instance.changeLP(activer, num * (-1));
        }
        else
        {
            BattleManager_Single.Instance.changeLP(activer.opponent, num * (-1));
        }
    }

    public void Cure(bool ifActiver, int num)
    {
        Player activer = getUserInPhase();
        if (ifActiver)
        {
            BattleManager_Single.Instance.changeLP(activer, num);
        }
        else
        {
            BattleManager_Single.Instance.changeLP(activer.opponent, num);
        }
    }

    public void drawCard(bool ifActiver, int num)
    {
        Player activer = getUserInPhase();
        if (ifActiver)
        {
            BattleManager_Single.Instance.drawCard(activer, num);
        }
        else
        {
            BattleManager_Single.Instance.drawCard(activer.opponent, num);
        }
    }

    public void setAllCardIfQuick(bool ifActiver, bool result, int place, int type)
    {
        Player activer = getUserInPhase();
        switch (place)
        {
            case 0:

                break;
            case 1:

                break;
            case 2:

                break;
            case 3:

                break;
        }
    }

    public void negate(bool ifActivingCard)
    {
        if (ifActivingCard)
        {
            ifNegated_Activer = true;
        }
        else
        {
            ifNegated_Counter = true;
        }
    }

    public void canNotBeCountered()
    {
        ifCanBeCountered = false;
    }

    public void counterDelayed()
    {
        ifCounterDelayed = true;
    }

    #endregion
}
