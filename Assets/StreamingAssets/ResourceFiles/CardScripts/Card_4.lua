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
effectList:DeclareAttackByCard(false)
effectList:DecreaseSP(CS.SolveTarget.opponent,3,false)
end