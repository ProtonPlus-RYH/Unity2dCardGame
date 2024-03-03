local effectList = CS.EffectList()
ifCountered = false

function ActivationDeclare()
effectList:CannotBeCountered(false)
end

function CounterDeclare()
ifCountered = true
end

function WhileNotCountered()
end

function WhileCountered()
ifCountered = true
effectList:DoJudge(CS.SolveTarget.opponent, CS.EffectTarget.fieldCard, CS.JudgeType.cardTypeIs, 1)
local buffLastList={CS.BuffLast.actionLast}
local lastReferenceList={0}
effectList:GiveBuff(CS.SolveTarget.self,CS.EffectTarget.duelist,CS.EffectType.damageTargetChange,1,buffLastList,lastReferenceList,true)
end

function Resolve()
effectList:PayCost()
if(ifCountered) then
effectList:DoJudge(CS.SolveTarget.opponent, CS.EffectTarget.fieldCard, CS.JudgeType.cardTypeIs, 1)
local buffLastList={CS.BuffLast.actionLast}
local lastReferenceList={0}
effectList:GiveBuff(CS.SolveTarget.self,CS.EffectTarget.duelist,CS.EffectType.damageTargetChange,1,buffLastList,lastReferenceList,true)
end
end

function AfterSelection_WhileCountered()
end

function AfterSelection_Resolve()
end