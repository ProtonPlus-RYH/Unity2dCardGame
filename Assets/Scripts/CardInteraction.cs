using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardInteraction : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{

    public LeanTweenType easeType;
    //public Vector3 mousePosition;//要把鼠标位置设在卡牌中央就不用这个
    public Vector3 originalPosition;
    public void OnBeginDrag(PointerEventData eventData)
    {
        //mousePosition = new Vector3(transform.position.x - eventData.position.x, transform.position.y - eventData.position.y, transform.position.z);
        originalPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        float zoomSize = GetComponent<ZoomUI_WithInfo>().zoomSize;
        transform.localScale = new Vector3(zoomSize, zoomSize, 1.0f);
        //transform.position += mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Player cardUser = transform.parent.parent.parent.GetComponent<Player>();
        if (transform.position.y > 200 && cardUser == BattleManager_Single.Instance.self)
        {
            DragCardUse();
        }
        else if (transform.position.y < 300 && cardUser == BattleManager_Single.Instance.opponent)
        {
            DragCardUse();
        }
        else
        {
            LeanTween.move(gameObject, originalPosition, 0.05f).setEase(easeType);
        }
    }


    public void DragCardUse()
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
                    if (cardUser == BattleManager_Single.Instance.controller && card.ifQuick_current)//切换之后的使用者以及quick才能对应
                    {
                        cardUser.fieldGet(gameObject);
                        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(100f, -150f, 0f);
                        EffectTransformer.Instance.counter(gameObject);
                    }
                    else if (!card.ifQuick_current)
                    {
                        transform.position = originalPosition;
                        Debug.Log("还用不了");
                    }
                    break;
            }
            BattleManager_Single.Instance.holdingCardCountdownIncrease(card);
        }
        else
        {
            transform.position = originalPosition;
            if (!card.GetIfActivable())
            {
                Debug.Log("被禁用");
            }
            if (!card.ifControlling)
            {
                Debug.Log("控制权在对手");
            }
            if (cardUser.SP == 0)
            {
                Debug.Log("精力不足");
            }
            if (cardUser.MP < card.manaCost_current)
            {
                Debug.Log("集中力不足");
            }
        }
    }

    public void CardPreSet()
    {
        Player cardUser = transform.parent.parent.parent.GetComponent<Player>();
        Card card = gameObject.GetComponent<CardDisplay>().card;
        if (cardUser.SP != 0 && cardUser.MP >= card.manaCost_current && (cardUser == BattleManager_Single.Instance.self && BattleManager_Single.Instance.gamePhase == GamePhase.EndPhase && EffectTransformer.Instance.processPhase!= SolvingProcess.activationDeclare || cardUser == BattleManager_Single.Instance.opponent && BattleManager_Single.Instance.gamePhase == GamePhase.OpponentEndPhase && EffectTransformer.Instance.processPhase != SolvingProcess.activationDeclare))
        //预埋阶段，要求cost满足以及阶段正确
        {
            BattleManager_Single.Instance.CardSet(gameObject);
            BattleManager_Single.Instance.HandCountCheck();
        }else if (cardUser.SP == 0)
        {
            Debug.Log("精力不足");
        }
        if (cardUser.MP < card.manaCost_current)
        {
            Debug.Log("集中力不足");
        }
    }
    public void CardDiscard()
    {
        Player cardUser = transform.parent.parent.parent.GetComponent<Player>();
        if (cardUser == BattleManager_Single.Instance.self && BattleManager_Single.Instance.gamePhase == GamePhase.selfHandDiscarding || cardUser == BattleManager_Single.Instance.opponent && BattleManager_Single.Instance.gamePhase == GamePhase.opponentHandDiscarding)
        //弃牌阶段
        {
            BattleManager_Single.Instance.DiscardCard(cardUser, gameObject.transform.GetSiblingIndex());
            BattleManager_Single.Instance.HandCountCheck();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        CardPreSet();
        CardDiscard();
    }
}
