local effectList = CS.EffectList()
local 

function ActivationDeclare()
--�ѶԷ��Ŀ������ó�ifQuick = true

end

function CounterDeclare()
end

function WhileNotCountered()
end

function WhileCountered()
end

function Resolve()
effectList:Cure(CS.SolveTarget.self,5,false)
end
