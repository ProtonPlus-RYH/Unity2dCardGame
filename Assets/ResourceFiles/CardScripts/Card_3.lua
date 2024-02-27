local effectList = CS.EffectList()


function ActivationDeclare()
--把对方的卡都设置成ifQuick = true

end

function CounterDeclare()
end

function WhileNotCountered()
end

function WhileCountered()
end

function Resolve()
effectList:RestoreMP(CS.SolveTarget.self,5,false)
end