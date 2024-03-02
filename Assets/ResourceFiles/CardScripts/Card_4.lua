local effectList = CS.EffectList()


function ActivationDeclare()
effectList:CannotBeCountered(false)
end

function CounterDeclare()
end

function WhileNotCountered()
end

function WhileCountered()
end

function Resolve()
effectList:PayCost()
local buffLastList={CS.BuffLast.cardLast,CS.BuffLast.turnLast_opponent}
local lastReferenceList={1,1}
effectList:GiveBuff(CS.SolveTarget.self,CS.EffectTarget.holdingCard,CS.EffectType.atkChange,1,buffLastList,lastReferenceList,false)
end