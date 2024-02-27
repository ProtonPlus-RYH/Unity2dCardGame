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
        if (card.getIfActivable())
        {
            switch (EffectTransformer.Instance.processPhase)
            {
                case SolvingProcess.beforeActivation:
                    cardUser.fieldGet(gameObject);
                    gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(100f, -150f, 0f);
                    EffectTransformer.Instance.useCard(gameObject);
                    break;
                case SolvingProcess.activationDeclare:
                    if(cardUser == EffectTransformer.Instance.activingCard.holdingPlayer.opponent && card.ifQuick_current)//非使用者以及quick才能对应
                    {
                        cardUser.fieldGet(gameObject);
                        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(100f, -150f, 0f);
                        EffectTransformer.Instance.counter(gameObject);
                    }
                    else if (!card.ifQuick_current)
                    {
                        Debug.Log("还用不了");
                    }
                    break;
            }
            card.useCount_turn++;
            card.useCount_duel++;
        }
        else if (cardUser == BattleManager_Single.Instance.self && BattleManager_Single.Instance.gamePhase == GamePhase.selfHandDiscarding || cardUser == BattleManager_Single.Instance.opponent && BattleManager_Single.Instance.gamePhase == GamePhase.opponentHandDiscarding)
        {
            BattleManager_Single.Instance.discardCard(cardUser, gameObject.transform.GetSiblingIndex());
            BattleManager_Single.Instance.handCountCheck();
        }
        else
        {
            Debug.Log("还用不了");
        }
    }
}
