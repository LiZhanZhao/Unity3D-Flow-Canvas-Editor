
if not __UNITY_EDITOR__ then return end

----------------------------------------------------------
---- 编辑器显示单元类
------------------------------------------------------------
--local ModAI = Import("logic/modules/ai/ai_base")
--local AI_OPPORTUNITY = ModAI.AI_OPPORTUNITY
--local FIGHT_CONST = Import("logic/fight/fight_const").FIGHT_CONST

CEditorUnit = class()

CEditorUnit.Init = function (self, id, actorType, actorName, actorGO)
    self._id = id
    self._type = actorType
    self._name = actorName
    self._go = actorGO

end

CEditorUnit.GetId = function (self)
    return self._id
end

-- CEditorUnit.HeartBeat = function( self, dt )
--     -- AI更新
--     self:UpdateAI(dt)
-- end

-- CEditorUnit.GetType = function( self )
--     return FIGHT_CONST.FIGHTER_TYPE_EDITOR_UNIT
-- end

-----------------------------------------------------------------------
-- AI有关函数
-----------------------------------------------------------------------

-- AI更新
-- CEditorUnit.UpdateAI = function (self, dt)
	
-- 	gGame:GetAiMgr():UpdateAI(self, dt)
	
--     -- 策略
--     --self:TryOpportunity(AI_OPPORTUNITY.TACTIC, {})
-- end

-- -- AI释放
-- CEditorUnit.AIRelease = function(self)
--     --TODO:

-- end
