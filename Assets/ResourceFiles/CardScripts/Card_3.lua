local effectList = CS.EffectList()


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
effectList:RestoreMP(CS.SolveTarget.self,5,false)
end