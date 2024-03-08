local effectList = CS.EffectList()


function ActivationDeclare()
end

function CounterDeclare()
end

function WhileNotCountered()
end

function WhileCountered()
end

function Resolve()
effectList:PayCost()
effectList:DoJudge(CS.SolveTarget.both,CS.EffectTarget.CurrentDistance,CS.JudgeType.ifDistanceChanged,0)
effectList:Negate(CS.SolveTarget.self,true)
effectList:DeclareAttackByCard(false)
local buffLastList={CS.BuffLast.turnLast}
local lastReferenceList={3}
effectList:GiveBuff(CS.SolveTarget.opponent,CS.EffectTarget.holdingCard,CS.EffectType.banCard,0,buffLastList,lastReferenceList,false)
effectList:GiveBuff(CS.SolveTarget.opponent,CS.EffectTarget.holdingCard,CS.EffectType.banCard,1,buffLastList,lastReferenceList,false)
end