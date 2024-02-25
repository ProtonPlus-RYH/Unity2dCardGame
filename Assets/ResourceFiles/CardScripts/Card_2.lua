
--local battleManager = gameObject:GetComponent(typeof(BattleManager_Single))
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
effectList:Cure(true,5)
end