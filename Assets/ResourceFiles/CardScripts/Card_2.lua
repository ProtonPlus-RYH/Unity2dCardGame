local effectList = CS.EffectList()
ifCountered = true

function ActivationDeclare()
effectList:CannotBeCountered(false)
end

function CounterDeclare()
end

function WhileNotCountered()
ifCountered = false
end

function WhileCountered()
effectList:DoJudge(CS.SolveTarget.opponent, CS.EffectTarget.fieldCard, CS.JudgeType.cardTypeIs, 1)
effectList:Negate(CS.SolveTarget.opponent,true)
effectList:DoSelection(CS.SolveTarget.self,CS.SelectionType.selectMovementWithCancel,1)
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