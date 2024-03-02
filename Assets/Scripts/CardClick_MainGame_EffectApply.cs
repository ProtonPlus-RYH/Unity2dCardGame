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
        Player cardUser = transform.parent.parent.parent.GetComponent<Player>();
        Card card = gameObject.GetComponent<CardDisplay>().card;
        if (card.GetIfActivable() && card.ifControlling && cardUser.SP != 0 && cardUser.MP >= card.manaCost_current)
        {
            switch (EffectTransformer.Instance.processPhase)
            {
                case SolvingProcess.beforeActivation:
                    cardUser.fieldGet(gameObject);
                    gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(100f, -150f, 0f);
                    EffectTransformer.Instance.useCard(gameObject);
                    break;
                case SolvingProcess.activationDeclare:
                    if(cardUser == EffectTransformer.Instance.activingCard.holdingPlayer.opponent && card.ifQuick_current)//��ʹ�����Լ�quick���ܶ�Ӧ
                    {
                        cardUser.fieldGet(gameObject);
                        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(100f, -150f, 0f);
                        EffectTransformer.Instance.counter(gameObject);
                    }
                    else if (!card.ifQuick_current)
                    {
                        Debug.Log("���ò���");
                    }
                    break;
            }
            card.useCount_turn++;
            card.useCount_duel++;
        }
        else if (cardUser == BattleManager_Single.Instance.self && BattleManager_Single.Instance.gamePhase == GamePhase.selfHandDiscarding || cardUser == BattleManager_Single.Instance.opponent && BattleManager_Single.Instance.gamePhase == GamePhase.opponentHandDiscarding)
        //���ƽ׶�
        {
            BattleManager_Single.Instance.DiscardCard(cardUser, gameObject.transform.GetSiblingIndex());
            BattleManager_Single.Instance.HandCountCheck();
        }
        else
        {
            if (!card.GetIfActivable()) {
                Debug.Log("������");
            }
            if (!card.ifControlling)
            {
                Debug.Log("����Ȩ�ڶ���");
            }
            if (cardUser.SP == 0)
            {
                Debug.Log("��������");
            }
            if (cardUser.MP < card.manaCost_current)
            {
                Debug.Log("����������");
            }
        }
    }
}
