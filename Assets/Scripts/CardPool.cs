using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using System.IO;
using System.Text;

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
        cardPool = new List<Card>();
        string[] cardCSVData = cardCSV.text.Split('\n');
        weaponAmount = 0;
        belongAmount = 0;
        weaponList = new List<string>();
        belongList = new List<string>();
        if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0])
        {
            weaponList.Add("通用");
            belongList.Add("魂");
        }
        else if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[1])
        {
            weaponList.Add("Common");
            belongList.Add("Dark Souls");
        }
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
                string weaponType_SC = cardData[1];
                string weaponType_EN = cardData[15];
                int weaponID = int.Parse(cardData[2]);
                string belongType_SC = cardData[3];
                string belongType_EN = cardData[16];
                int belongID = int.Parse(cardData[4]);
                string cardName_SC = cardData[6];
                string cardName_EN = cardData[18];
                int staminaCost = int.Parse(cardData[7]);
                int manaCost = int.Parse(cardData[8]);
                int atk = int.Parse(cardData[9]);
                int distance = int.Parse(cardData[10]);
                string discription_SC = cardData[11];
                string discription_EN = cardData[19];
                int turnLimit = int.Parse(cardData[13]);
                int duelLimit = int.Parse(cardData[14]);
                bool ifquick = false;
                if (cardData[12] == "是") 
                {
                    ifquick = true;
                }
                if (cardData[5] == "攻击" || cardData[5] == "attack")
                {
                    if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0])
                    {
                        cardPool.Add(new AttackCard(id, belongID, weaponID, cardName_SC, staminaCost, manaCost, discription_SC, ifquick, turnLimit, duelLimit, atk, distance));
                    }
                    else if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[1])
                    {
                        cardPool.Add(new AttackCard(id, belongID, weaponID, cardName_EN, staminaCost, manaCost, discription_EN, ifquick, turnLimit, duelLimit, atk, distance));
                    }
                }
                else if (cardData[5] == "行动" || cardData[5] == "action")
                {
                    if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0])
                    {
                        cardPool.Add(new ActionCard(id, belongID, weaponID, cardName_SC, staminaCost, manaCost, discription_SC, ifquick, turnLimit, duelLimit));
                    }
                    else if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[1])
                    {
                        cardPool.Add(new ActionCard(id, belongID, weaponID, cardName_EN, staminaCost, manaCost, discription_EN, ifquick, turnLimit, duelLimit));
                    }
                }
                
                if (weaponAmount < weaponID)
                {
                    weaponAmount = weaponID;
                    if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0])
                    {
                        weaponList.Add(weaponType_SC);
                    }
                    else if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[1])
                    {
                        weaponList.Add(weaponType_EN);
                    }
                }
                if (belongAmount < belongID)
                {
                    belongAmount = belongID;
                    if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[0])
                    {
                        belongList.Add(belongType_SC);
                    }
                    else if (LocalizationSettings.SelectedLocale == LocalizationSettings.AvailableLocales.Locales[1])
                    {
                        belongList.Add(belongType_EN);
                    }
                }
            }
        }
    }


    public List<int> ReadDeck(string deckName)
    {
        List<int> cardsInDeck = new List<int>();

        string[] path = Application.dataPath.Split("/");
        StringBuilder pathSB = new StringBuilder();
        for (int i = 0; i < path.Length - 1; i++)
        {
            pathSB.Append(path[i]);
            pathSB.Append("/");
        }
        pathSB.Append("Decks/");
        pathSB.Append(deckName);
        pathSB.Append(".csv");
        using (var reader = new StreamReader(pathSB.ToString()))
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
