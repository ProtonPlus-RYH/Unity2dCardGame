using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardClick_MainGame_EffectApply : MonoBehaviour, IPointerDownHandler
{

    void Start()
    {

    }

    void Update()
    {
        
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        Player cardUser = gameObject.transform.parent.parent.parent.gameObject.GetComponent<Player>();
        Card card = gameObject.GetComponent<CardDisplay>().card;
        if (card.ifActivable)
        {
            switch (EffectTransformer.Instance.processPhase)
            {
                case SolvingProcess.beforeActivation:
                    if (cardUser == BattleManager_Single.Instance.self)//我方使用
                    {
                        BattleManager_Single.Instance.ifSelfPlayingCard = true;
                        EffectTransformer.Instance.useCard(card.CardID);
                    }
                    else if (cardUser == BattleManager_Single.Instance.opponent)//对方使用
                    { 
                        BattleManager_Single.Instance.ifSelfPlayingCard = false;
                        EffectTransformer.Instance.useCard(card.CardID);
                    }
                    break;
                case SolvingProcess.activationDeclare:
                    if(cardUser != BattleManager_Single.Instance.getCardPlayer() && card.IfQuick)//非使用者以及quick才能对应
                    {
                        EffectTransformer.Instance.counter(card.CardID);
                    }
                    break;
            }
        }
        else
        {
            Debug.Log("还用不了");
        }
    }
}
