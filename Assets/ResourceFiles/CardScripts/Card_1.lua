local effectList = CS.EffectList()
ifCountered = true

function ActivationDeclare()
effectList:cannotBeCountered(false)
end

function CounterDeclare()
end

function WhileNotCountered()
ifCountered = false
end

function WhileCountered()
effectList:DoJudge(CS.SolveTarget.opponent, CS.JudgeTarget.fieldCard, CS.JudgeType.cardTypeIs, 1)
effectList:Negate(CS.SolveTarget.opponent,true)
end

function Resolve()
if(ifCountered) then
effectList:DoJudge(CS.SolveTarget.opponent, CS.JudgeTarget.fieldCard, CS.JudgeType.cardTypeIs, 1)
effectList:Negate(CS.SolveTarget.opponent,true)
end
end