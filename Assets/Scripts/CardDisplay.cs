using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI discriptionText;
    public TextMeshProUGUI staminaCostText;
    public TextMeshProUGUI staminaCostTitleText;
    public TextMeshProUGUI manaCostText;
    public TextMeshProUGUI manaCostTitleText;
    public TextMeshProUGUI attackPowerText;
    public TextMeshProUGUI attackPowerTitleText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI distanceTitleText;

    public GameObject quickMark;

    public Card card;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ShowCard();
        if (card.ifQuick_current)
        {
            quickMark.SetActive(true);
        }
        else
        {
            quickMark.SetActive(false);
        }
    }

    public void ShowCard()
    {
        cardNameText.text = card.CardName;
        discriptionText.text = card.Discription;
        staminaCostText.text = card.staminaCost_current.ToString();
        manaCostText.text = card.manaCost_current.ToString();
        if (card.GetType() == typeof(AttackCard))
        {
            var attackCard = card as AttackCard;
            attackPowerText.text = attackCard.attackPower_current.ToString();
            distanceText.text = attackCard.distance_current.ToString();
        }
        else
        {
            //var actionCard = card as ActionCard;
            attackPowerText.gameObject.SetActive(false);
            attackPowerTitleText.gameObject.SetActive(false);
            distanceText.gameObject.SetActive(false);
            distanceTitleText.gameObject.SetActive(false);
        }
    }
}
