using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public int CardID;
    public int BelongID;
    public int WeaponID;
    public string CardName;
    public int StaminaCost;
    public int ManaCost;
    public string Discription;
    public bool IfQuick;
    public int UseLimit_turn;
    public int useCount_turn;
    public int useLimit_duel;
    public int useCount_duel;
    public bool ifActivable;
    
    public Card(int id, string cardName, bool ifQuick, int staminaCost, int manaCost, string discription, int belongID, int weaponID)
    {
        CardID = id;
        CardName = cardName;
        IfQuick = ifQuick;
        StaminaCost = staminaCost;
        ManaCost = manaCost;
        Discription = discription;
        BelongID = belongID;
        WeaponID = weaponID;
    }
}

public class AttackCard : Card
{
    public int AttackPower;
    public int Distance;
    public AttackCard(int id, string cardName, bool ifQuick, int staminaCost, int manaCost, string discription, int belongID, int weaponID, int atk, int distance) : base(id, cardName, ifQuick, staminaCost, manaCost, discription, belongID, weaponID)
    {
        AttackPower = atk;
        Distance = distance;
    }
}

public class ActionCard : Card
{

    public ActionCard(int id, string cardName, bool ifQuick, int staminaCost, int manaCost, string discription, int belongID, int weaponID) : base(id, cardName, ifQuick, staminaCost, manaCost, discription, belongID, weaponID)
    {
        
    }
}
