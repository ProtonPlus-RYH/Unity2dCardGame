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
end