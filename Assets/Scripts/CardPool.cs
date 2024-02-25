using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CardPool : MonoBehaviour
{
    public TextAsset cardCSV;
    public List<Card> cardPool = new List<Card>();
    private int weaponAmount;
    private int belongAmount;
    public List<string> weaponList;
    public List<string> belongList;

    public void getAllCards()
    {
        string[] cardCSVData = cardCSV.text.Split('\n');
        weaponAmount = 0;
        belongAmount = 0;
        weaponList = new List<string>(weaponAmount);
        weaponList.Add("通用");
        belongList = new List<string>(belongAmount);
        belongList.Add("通用");
        foreach (var row in cardCSVData)
        {
            string[] cardData = row.Split(',');
            if (int.TryParse(cardData[0], out int result) == false)
            {
                continue;
            }
            else
            {
                int id = int.Parse(cardData[0]);
                string weaponType = cardData[1];
                int weaponID = int.Parse(cardData[2]);
                string belongType = cardData[3];
                int belongID = int.Parse(cardData[4]);
                string cardName = cardData[6];
                int staminaCost = int.Parse(cardData[7]);
                int manaCost = int.Parse(cardData[8]);
                int atk = int.Parse(cardData[9]);
                int distance = int.Parse(cardData[10]);
                string discription = cardData[11];
                int turnLimit = int.Parse(cardData[13]);
                int duelLimit = int.Parse(cardData[14]);
                bool ifquick = false;
                if (cardData[12] == "是") 
                {
                    ifquick = true;
                }
                if (cardData[5] == "攻击" || cardData[5] == "attack")
                {
                    cardPool.Add(new AttackCard(id, belongID, weaponID, cardName, staminaCost, manaCost, discription, ifquick, turnLimit, duelLimit, atk, distance));
                }else if (cardData[5] == "行动" || cardData[5] == "action")
                {
                    cardPool.Add(new ActionCard(id, belongID, weaponID, cardName, staminaCost, manaCost, discription, ifquick, turnLimit, duelLimit));
                }
                
                if (weaponAmount < weaponID)
                {
                    weaponAmount = weaponID;
                    weaponList.Add(weaponType);
                }
                if (belongAmount < belongID)
                {
                    belongAmount = belongID;
                    belongList.Add(belongType);
                }
            }
        }
    }

    public List<int> readDeck(string deckName)
    {
        List<int> cardsInDeck = new List<int>();
        using (var reader = new StreamReader("Assets\\ResourceFiles\\Decks\\" + deckName + ".csv"))
        {
            int cardCount = 0;
            while (!reader.EndOfStream)
            {
                var card = reader.ReadLine();
                if (int.TryParse(card, out int result) == false)
                {
                    continue;
                }
                else
                {
                    cardsInDeck.Add(int.Parse(card));
                    cardCount++;
                }
            }
        }
        return cardsInDeck;
    }

    public Card copyCard(Card originalCard)
    {
        Card result = null;
        if (originalCard.GetType() == typeof(AttackCard))
        {
            var originalAttackCard = originalCard as AttackCard;
            AttackCard attackCard = new AttackCard(originalAttackCard.CardID, originalAttackCard.BelongID, originalAttackCard.WeaponID, originalAttackCard.CardName, originalAttackCard.StaminaCost, originalAttackCard.ManaCost, originalAttackCard.Discription, originalAttackCard.IfQuick, originalAttackCard.UseLimit_turn, originalAttackCard.UseLimit_duel, originalAttackCard.AttackPower, originalAttackCard.Distance); ;
            result = attackCard;
        }
        else if(originalCard.GetType() == typeof(ActionCard))
        {
            ActionCard actionCard = new ActionCard(originalCard.CardID, originalCard.BelongID, originalCard.WeaponID, originalCard.CardName, originalCard.StaminaCost, originalCard.ManaCost, originalCard.Discription, originalCard.IfQuick, originalCard.UseLimit_turn, originalCard.UseLimit_duel);
            result = actionCard;
        }
        return result;
    }
}
