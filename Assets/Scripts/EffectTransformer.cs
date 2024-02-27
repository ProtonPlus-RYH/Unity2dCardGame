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
    //public bool ifDoingJudgement;
    public bool judgeResult;

    void Start()
    {
        ifNegated_Activer = false;
        ifNegated_Counter = false;
        ifCounterDelayed = false;
        ifCanBeCountered = true;
        ifPaused = false;
        ifDoingSelection = false;
        //ifDoingJudgement = false;
        judgeResult = false;
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

    byte[] CardLuaLoader1(ref string fileName1)//卡牌Lua文件读取
    {
        string path = "Assets\\ResourceFiles\\CardScripts\\" + fileName1 + ".lua";
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
        Debug.Log("宣言发动");
        //卡牌使用计数增加
        card.useCount_turn++;
        card.useCount_duel++;
        //创建LuaEnv,执行发动时效果
        luaenv_Active = new LuaEnv();
        luaenv_Active.AddLoader(CardLuaLoader);
        luaenv_Active.DoString("require('Card_" + id.ToString() + "')");
        luaenv_Active.Global.Set("processPhase", typeof(SolvingProcess));
        Action activationDeclare = luaenv_Active.Global.Get<Action>("ActivationDeclare");
        LuaFunction D_activationDeclare = luaenv_Active.Global.Get<LuaFunction>("ActivationDeclare");
        D_activationDeclare.Call();
        //阶段推进
        if (BattleManager_Single.Instance.controller.opponent.FieldZone.childCount == 0)//判断有无预埋
        {
            if (!ifPaused)
            {
                phasePush();
            }
            else
            {
                if (ifDoingSelection)
                {
                    ifPaused = false;
                    ifDoingSelection = false;
                }
                /*if (ifDoingJudgement)
                {
                    ifPaused = false;
                    ifDoingJudgement = false;
                }*/
            }
        }
        else
        {
            GameObject preSetCard = BattleManager_Single.Instance.controller.opponent.FieldZone.GetChild(0).gameObject;
            counter(preSetCard);
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

    public void counter(GameObject cardPrefab)//对应发动时
    {
        CounterAskDialog.SetActive(false);
        BattleManager_Single.Instance.changeController();
        processPhase = SolvingProcess.counterDeclare;
        //储存对应卡牌
        Card card = cardPrefab.GetComponent<CardDisplay>().card;
        int id = card.CardID;
        counterCard = card;
        counterCardPrefab = cardPrefab;
        Debug.Log("宣言对应");
        //卡牌使用计数增加
        card.useCount_turn++;
        card.useCount_duel++;
        //创建LuaEnv,执行对应时效果
        luaenv_Counter = new LuaEnv();
        luaenv_Counter.AddLoader(CardLuaLoader1);
        luaenv_Counter.DoString("require('Card_" + id.ToString() + "')");
        Action counterDeclare = luaenv_Counter.Global.Get<Action>("CounterDeclare");
        LuaFunction D_counterDeclare = luaenv_Counter.Global.Get<LuaFunction>("CounterDeclare");
        D_counterDeclare.Call();
        if (!ifPaused)
        {
            phasePush();
        }
        else
        {
            if (ifDoingSelection)
            {
                ifPaused = false;
                ifDoingSelection = false;
            }
            /*if (ifDoingJudgement)
            {
                ifPaused = false;
                ifDoingJudgement = false;
            }*/
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
        else
        {
            if (ifDoingSelection)
            {
                ifPaused = false;
                ifDoingSelection = false;
            }
            /*if (ifDoingJudgement)
            {
                ifPaused = false;
                ifDoingJudgement = false;
            }*/
        }
    }

    public void ifCounter()//被对应时
    {
        processPhase = SolvingProcess.ifCountered;
        Debug.Log("处理若被对应");
        Action whileCountered = luaenv_Active.Global.Get<Action>("WhileCountered");
        LuaFunction D_whileCountered = luaenv_Active.Global.Get<LuaFunction>("WhileCountered");
        D_whileCountered.Call();
        if (!ifPaused)
        {
            phasePush();
        }
        else
        {
            if (ifDoingSelection)
            {
                ifPaused = false;
                ifDoingSelection = false;
            }
            /*if (ifDoingJudgement)
            {
                ifPaused = false;
                ifDoingJudgement = false;
            }*/
        }
    }

    public void counterResolve()//处理对应效果，若被延迟则进入发动卡牌的效果处理
    {
        processPhase = SolvingProcess.counterResolve;
        if (!ifCounterDelayed)
        {
            Debug.Log("处理对应");
            Action counterResolve = luaenv_Counter.Global.Get<Action>("Resolve");
            LuaFunction D_counterResolve = luaenv_Counter.Global.Get<LuaFunction>("Resolve");
            D_counterResolve.Call();
            luaenv_Counter.Dispose(); 
        }
        if (!ifPaused)
        {
            phasePush();
        }
        else
        {
            if (ifDoingSelection)
            {
                ifPaused = false;
                ifDoingSelection = false;
            }
            /*if (ifDoingJudgement)
            {
                ifPaused = false;
                ifDoingJudgement = false;
            }*/
        }
    }

    public void activationResolve()//处理发动卡牌效果
    {
        processPhase = SolvingProcess.activationResolve;
        Debug.Log("处理效果");
        Action activationResolve = luaenv_Active.Global.Get<Action>("Resolve");
        LuaFunction D_activationResolve = luaenv_Active.Global.Get<LuaFunction>("Resolve");
        D_activationResolve.Call();
        luaenv_Active.Dispose();
        if (!ifPaused)
        {
            phasePush();
        }
        else
        {
            if (ifDoingSelection)
            {
                ifPaused = false;
                ifDoingSelection = false;
            }
            /*if (ifDoingJudgement)
            {
                ifPaused = false;
                ifDoingJudgement = false;
            }*/
        }
    }

    public void delayResolve()//延迟处理对应效果
    {
        processPhase = SolvingProcess.delayResolve;
        Action delayResolve = luaenv_Counter.Global.Get<Action>("Resolve");
        LuaFunction D_delayResolve = luaenv_Counter.Global.Get<LuaFunction>("Resolve");
        D_delayResolve.Call();
        luaenv_Counter.Dispose();
        if (!ifPaused)
        {
            phasePush();
        }
        else
        {
            if (ifDoingSelection)
            {
                ifPaused = false;
                ifDoingSelection = false;
            }
            /*if (ifDoingJudgement)
            {
                ifPaused = false;
                ifDoingJudgement = false;
            }*/
        }
    }

    public void processEnd()
    {
        processPhase = SolvingProcess.endProcess;
        if (BattleManager_Single.Instance.controller.FieldZone.childCount != 0)
        {
            BattleManager_Single.Instance.controller.graveGet(BattleManager_Single.Instance.controller.FieldZone.transform.GetChild(0).gameObject);//发动者卡牌进墓
        }
        if (BattleManager_Single.Instance.controller.opponent.FieldZone.childCount != 0)
        {
            BattleManager_Single.Instance.controller.opponent.graveGet(BattleManager_Single.Instance.controller.opponent.FieldZone.transform.GetChild(0).gameObject);//对应者卡牌进墓
        }
        ifNegated_Activer = false;
        ifNegated_Counter = false;
        ifCounterDelayed = false;
        ifCanBeCountered = true;
        ifPaused = false;
        //ifDoingJudgement = false;
        ifDoingSelection = false;
        activingCard = null;
        counterCard = null;
        processPhase = SolvingProcess.beforeActivation;
    }

    public void selectSolve()
    {
        Action activationDeclare = luaenv_Active.Global.Get<Action>("AfterSelection");
        LuaFunction D_activationDeclare = luaenv_Active.Global.Get<LuaFunction>("AfterSelection");
        D_activationDeclare.Call();
        if (!ifPaused)
        {
            phasePush();
        }
        else
        {
            if (ifDoingSelection)
            {
                ifPaused = false;
                ifDoingSelection = false;
            }
            /*if (ifDoingJudgement)
            {
                ifPaused = false;
                ifDoingJudgement = false;
            }*/
        }
    }

    /*public void judgeSolve()
    {
        Action afterJudge = luaenv_Active.Global.Get<Action>("AfterJudging");
        LuaFunction D_afterJudge = luaenv_Active.Global.Get<LuaFunction>("AfterJudging(" + processPhase + ")");
        D_afterJudge.Call();
        if (!ifPaused)
        {
            phasePush();
        }
        else
        {
            if (ifDoingSelection)
            {
                ifPaused = false;
                ifDoingSelection = false;
            }
            if (ifDoingJudgement)
            {
                ifPaused = false;
                ifDoingJudgement = false;
            }
        }
    }*/

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

    #endregion 

}
