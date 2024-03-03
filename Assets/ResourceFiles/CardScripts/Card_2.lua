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
effectList:Negate(CS.SolveTarget.opponent,true)
end

function Resolve()
effectList:PayCost()
if(ifCountered) then
effectList:DoJudge(CS.SolveTarget.opponent, CS.EffectTarget.fieldCard, CS.JudgeType.cardTypeIs, 1)
effectList:Negate(CS.SolveTarget.opponent,true)
end
effectList:DoSelection(CS.SolveTarget.self,CS.SelectionType.selectMovementWithCancel,1)
end

function AfterSelection_WhileCountered()
end

function AfterSelection_Resolve()
end