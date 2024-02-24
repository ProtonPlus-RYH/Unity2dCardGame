
--local battleManager = gameObject:GetComponent(typeof(BattleManager_Single))
local effectTransformer = CS.EffectTransformer()


function ActivationDeclare()

end

function CounterDeclare()

end

function WhileNotCountered()

end

function WhileCountered()

end

function Resolve()
effectTransformer:Cure(true,5)
end