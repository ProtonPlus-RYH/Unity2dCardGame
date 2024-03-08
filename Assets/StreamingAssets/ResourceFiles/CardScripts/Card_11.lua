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
effectList:DeclareAttackByCard(false)
effectList:DoSelection(CS.SolveTarget.self,CS.SelectionType.selectTF,1,false)
end

function AfterSelection_Resolve()
effectList:ReturnCardToDeck(CS.SolveTarget.self,CS.EffectTarget.fieldCard,0,true,true,true)
effectList:DrawCard(CS.SolveTarget.self,1,true)
end