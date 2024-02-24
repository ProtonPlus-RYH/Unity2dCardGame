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
                    if (cardUser == BattleManager_Single.Instance.self)//�ҷ�ʹ��
                    {
                        BattleManager_Single.Instance.ifSelfPlayingCard = true;
                        EffectTransformer.Instance.useCard(card.CardID);
                    }
                    else if (cardUser == BattleManager_Single.Instance.opponent)//�Է�ʹ��
                    { 
                        BattleManager_Single.Instance.ifSelfPlayingCard = false;
                        EffectTransformer.Instance.useCard(card.CardID);
                    }
                    break;
                case SolvingProcess.activationDeclare:
                    if(cardUser != BattleManager_Single.Instance.getCardPlayer() && card.IfQuick)//��ʹ�����Լ�quick���ܶ�Ӧ
                    {
                        EffectTransformer.Instance.counter(card.CardID);
                    }
                    break;
            }
        }
        else
        {
            Debug.Log("���ò���");
        }
    }
}
