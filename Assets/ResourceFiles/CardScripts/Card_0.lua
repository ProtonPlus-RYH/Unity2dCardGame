local effectList = CS.EffectList()

function ActivationDeclare()
--�ѶԷ��Ŀ������ó�ifQuick = true
local buffLastList={CS.BuffLast.actionLast}
local lastReferenceList={0}
effectList:GiveBuff(CS.SolveTarget.opponent,CS.EffectTarget.handZone,CS.EffectType.ifQuickChange,1,buffLastList,lastReferenceList,false)
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
