using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum nullableBool
{
    b_true, b_false, b_null
}

public class Card 
{
    //����ĸ��дΪԭʼ���ԣ�СдΪ��������
    public int CardID;
    public int BelongID;
    public int WeaponID;
    public string CardName;
    public int StaminaCost;
    public int ManaCost;
    public string Discription;
    public bool IfQuick;
    public int UseLimit_turn;
    public int UseLimit_duel;

    public Player HoldingPlayer;
    public Player holdingPlayer;//�Ǽǳ�����

    public int staminaCost_current;
    public int manaCost_current;
    public bool ifQuick_current;
    public List<nullableBool> ifQuickRecord;//���ڵ���bool���͵��޸�
    public int useCount_turn;
    public int useCount_duel;
    private bool ifActivable;
    public List<nullableBool> ifActivableRecord;//���ڵ���bool���͵��޸�
    public bool ifNegated;
    public List<nullableBool> ifNegatedRecord;//���ڵ���bool���͵��޸�

    public Card(int idGet, int belongIDGet, int weaponIDGet, string cardNameGet,  int staminaCostGet, int manaCostGet, string discriptionGet, bool ifQuickGet,int turnLimitGet, int duelLimitGet)
    {
        CardID = idGet;
        BelongID = belongIDGet;
        WeaponID = weaponIDGet;
        CardName = cardNameGet;
        StaminaCost = staminaCostGet;
        ManaCost = manaCostGet;
        Discription = discriptionGet;
        IfQuick = ifQuickGet;
        UseLimit_turn = turnLimitGet;
        UseLimit_duel = duelLimitGet;
        staminaCost_current = StaminaCost;
        manaCost_current = ManaCost;
        useCount_turn = 0;
        useCount_duel = 0;
        ifQuick_current = IfQuick;
        ifQuickRecord = new List<nullableBool>();
        ifQuickRecord.Add(boolToNullable(IfQuick));
        ifActivable = false;
        ifActivableRecord = new List<nullableBool>();
        ifActivableRecord.Add(boolToNullable(ifActivable));
        ifNegated = false;
        ifNegatedRecord = new List<nullableBool>();
        ifNegatedRecord.Add(boolToNullable(ifNegated));
    }

    public int setIfQuick(bool result)//���ظ��Ƕ������
    {
        ifQuickRecord.Add(boolToNullable(result));
        ifQuick_current = result;
        return ifQuickRecord.Count;
    }
    
    public void extractIfQuick(int queueNum)//�Ȱ�ȡ������գ��������ǰ���������յľ�����
    {
        ifQuickRecord[queueNum] = nullableBool.b_null;
        bool ifGetRecord = false;
        int recordCount = ifQuickRecord.Count;
        while(!ifGetRecord)
        {
            if (ifQuickRecord[recordCount] != nullableBool.b_null)
            {
                ifQuick_current = nullableToBool(ifQuickRecord[recordCount]);
                ifGetRecord = true;
            }
            else
            {
                ifQuickRecord.RemoveAt(recordCount);
                recordCount--;
            }
        }
    }

    public int setIfActivable(bool result)//���ظ��Ƕ������
    {
        ifActivableRecord.Add(boolToNullable(result));
        ifActivable = result;
        return ifActivableRecord.Count - 1;
    }

    public void extractIfActivable(int queueNum)//�Ȱ�ȡ������գ��������ǰ���������յľ�����
    {
        ifActivableRecord[queueNum] = nullableBool.b_null;
        bool ifGetRecord = false;
        int recordCount = ifActivableRecord.Count - 1;
        while (!ifGetRecord)
        {
            if (ifActivableRecord[recordCount] != nullableBool.b_null)
            {
                ifActivable = nullableToBool(ifActivableRecord[recordCount]);
                ifGetRecord = true;
            }
            else
            {
                ifActivableRecord.RemoveAt(recordCount);
                recordCount--;
            }
        }
    }

    public bool getIfActivable()
    {
        bool result = ifActivable;
        if (useCount_turn>UseLimit_turn || useCount_duel>UseLimit_duel)
        {
            result = false;
        }
        return result;
    }

    public int setIfNegated(bool result)//���ظ��Ƕ������
    {
        ifNegatedRecord.Add(boolToNullable(result));
        ifNegated = result;
        return ifNegatedRecord.Count;
    }

    public void extractIfNegated(int queueNum)//�Ȱ�ȡ������գ��������ǰ���������յľ�����
    {
        ifNegatedRecord[queueNum] = nullableBool.b_null;
        bool ifGetRecord = false;
        int recordCount = ifNegatedRecord.Count;
        while (!ifGetRecord)
        {
            if (ifNegatedRecord[recordCount] != nullableBool.b_null)
            {
                ifNegated = nullableToBool(ifNegatedRecord[recordCount]);
                ifGetRecord = true;
            }
            else
            {
                ifNegatedRecord.RemoveAt(recordCount);
                recordCount--;
            }
        }
    }

    public nullableBool boolToNullable(bool result)
    {
        nullableBool retValue = new nullableBool();
        if (result)
        {
            retValue = nullableBool.b_true;
        }
        else
        {
            retValue = nullableBool.b_false;
        }
        return retValue;
    }

    public bool nullableToBool(nullableBool result)
    {
        bool retValue = true;
        if (result == nullableBool.b_false)
        {
            retValue = false;
        }
        else if(result == nullableBool.b_null)
        {
            Debug.Log("bool���ˣ����ظ�true��");
        }
        return retValue;
    }
}

public class AttackCard : Card
{
    public int AttackPower;
    public int Distance;
    public int attackPower_current;
    public int distance_current;
    public AttackCard(int idGet, int belongIDGet, int weaponIDGet, string cardNameGet, int staminaCostGet, int manaCostGet, string discriptionGet, bool ifQuickGet, int turnLimitGet, int duelLimitGet, int atkGet, int distanceGet) : base(idGet, belongIDGet, weaponIDGet, cardNameGet, staminaCostGet, manaCostGet, discriptionGet, ifQuickGet, turnLimitGet, duelLimitGet)
    {
        AttackPower = atkGet;
        Distance = distanceGet;
        attackPower_current = AttackPower;
        distance_current = Distance;
    }
}

public class ActionCard : Card
{

    public ActionCard(int idGet, int belongIDGet, int weaponIDGet, string cardNameGet, int staminaCostGet, int manaCostGet, string discriptionGet, bool ifQuickGet, int turnLimitGet, int duelLimitGet) : base(idGet, belongIDGet, weaponIDGet, cardNameGet, staminaCostGet, manaCostGet, discriptionGet, ifQuickGet, turnLimitGet, duelLimitGet)
    {
        
    }
}
