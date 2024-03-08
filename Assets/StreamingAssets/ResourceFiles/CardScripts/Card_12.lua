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
end

function AfterSelection_Resolve()
end