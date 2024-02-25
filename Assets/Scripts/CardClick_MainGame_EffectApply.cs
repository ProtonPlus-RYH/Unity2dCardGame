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
                    gameObject.transform.SetParent(cardUser.FieldZone);
                    EffectTransformer.Instance.useCard(card);
                    break;
                case SolvingProcess.activationDeclare:
                    if(cardUser == EffectTransformer.Instance.activingCard.holdingPlayer.opponent && card.ifQuick_current)//��ʹ�����Լ�quick���ܶ�Ӧ
                    {
                        gameObject.transform.SetParent(cardUser.FieldZone);
                        EffectTransformer.Instance.counter(card);
                    }
                    break;
            }
            card.useCount_turn++;
            card.useCount_duel++;
        }
        else
        {
            Debug.Log("���ò���");
        }
    }
}
