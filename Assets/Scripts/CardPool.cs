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
                string cardName = cardData[2];
                bool ifquick = false;
                if (cardData[3] == "是") 
                {
                    ifquick = true;
                }
                int staminaCost = int.Parse(cardData[4]);
                int manaCost = int.Parse(cardData[5]);
                int atk = int.Parse(cardData[6]);
                int distance = int.Parse(cardData[7]);
                string discription = cardData[8];
                string belongType = cardData[9];
                int belongID = int.Parse(cardData[10]);
                string weaponType = cardData[11];
                int weaponID = int.Parse(cardData[12]);
                if (cardData[1] == "攻击" || cardData[1] == "attack")
                {
                    cardPool.Add(new AttackCard(id, cardName, ifquick, staminaCost, manaCost, discription, belongID, weaponID, atk, distance));
                }else if (cardData[1] == "行动" || cardData[1] == "action")
                {
                    cardPool.Add(new ActionCard(id, cardName, ifquick, staminaCost, manaCost, discription, belongID, weaponID));
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
}
