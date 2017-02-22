
print("Begin lua adapter");
-- 如果在编辑其中执行
if not __UNITY_EDITOR__ then return end

-- import Main.lua
--require "Main"

--print("require Global");


--local CGame = Import("logic/game").CGame
--gGame = CGame:New()

--gGame = class() 
--local CAiManager = Import("logic/modules/ai/ai_manager").CAiManager
--local aiMgr = CAiManager:New()
--gGame.GetAiMgr = function(self)
-- 	return aiMgr
-- end

-- Debugger.Log( "Application.platform == {0}", Application.platform );

-- local ModAI = Import("logic/modules/ai/ai_base")
-- local AI_OPPORTUNITY = ModAI.AI_OPPORTUNITY

-- local FIGHT_CONST = Import("logic/fight/fight_const").FIGHT_CONST


local CurUnitId = 1
----------------------------------------------------------
-- 编辑器类
----------------------------------------------------------
CEditor = class()

CEditor.Init = function (self)
    -- 所有的演员
    self.__all_actors = {}
	self.__all_actors_init_pos = {}

    -- 注册心跳
    --UpdateBeat:Add( self.Update, self )

    self._timeScale = 1

    self.__all_scenePos = {}
end 

CEditorUnit = Import( "editor/editor_unit").CEditorUnit
CEditor.SetActor = function(self, actorType, actorName, actorGO)
    --Debugger.Log( "SetActor:{0},{1},{2}", actorType, actorName, actorGO );
    -- 演员
    self.__all_actors[actorType] = CEditorUnit:New( CurUnitId, actorType, actorName, actorGO );
	self.__all_actors_init_pos[actorType] = actorGO.transform.position
	CurUnitId = CurUnitId + 1
end

CEditor.GetCurUnitId = function(self)
	return CurUnitId
end

CEditor.GetTeamActorID = function(self, teamType)
	local actorTypeList = {}
	local actorTypeStart = teamType == 1 and "friend" or "enemy"
	for k, v in pairs(self.__all_actors) do
		if string.find(k, actorTypeStart) then 
			table.insert(actorTypeList, v:GetId())
		end
	end
	return actorTypeList
end

-- CEditor.GetType = function(self)
--     return FIGHT_CONST.FIGHTER_TYPE_EDITOR 
-- end

-- CEditor.GetId = function(self)
--     return -2
-- end

CEditor.DeleteActor = function( self, actorType )
    self.__all_actors[actorType] = nil
end

CEditor.GetActor = function( self, actorType )
    return self.__all_actors[actorType]
end

CEditor.GetActorByID = function( self, id )
    for at, actorObj in pairs(self.__all_actors) do
        if ( actorObj:GetId() == id ) then return actorObj end
    end

    return nil
end

-- CEditor.GetActorInitPos = function(self, actorType)
-- 	return self.__all_actors_init_pos[actorType]
-- end

-- CEditor.GetActorInitPosByID = function(self, id)
-- 	local pos = nil
-- 	for at, actorObj in pairs(self.__all_actors) do
-- 		if (actorObj:GetId() == id ) then
-- 			pos = self.__all_actors_init_pos[actorObj._type]
-- 		end
-- 	end
-- 	return pos
-- end	

-- CEditor.Update = function(self)
--     local dt = Time.deltaTime
--     dt = dt * self._timeScale

--     self:UpdateAI(dt);
--     -- Debugger.Log("EditorUpdate: {0}", dt);

--     for at, actorObj in pairs(self.__all_actors) do
--         actorObj:HeartBeat(dt);
--     end
-- end

-- CEditor.SetScenePos = function(self, posName, go)
--     self.__all_scenePos[posName] = go
-- end

-- CEditor.DeleteScenePos = function( self, posName )
--     self.__all_scenePos[posName] = nil
-- end

-- CEditor.GetScenePos = function(self, posName)
--     return self.__all_scenePos[posName]
-- end
-----------------------------------------------------------------------
-- AI有关函数
-----------------------------------------------------------------------

-- AI更新
-- CEditor.UpdateAI = function (self, dt)
-- 	gGame:GetAiMgr():UpdateAI(self, dt)
-- end

-- -- AI释放
-- CEditor.AIRelease = function(self)
--     --TODO:

-- end

----------------------------------------------------------
-- 结构性代码
--
editor = CEditor:New()

-- SetActor = function(actorType, actorName, actorGO)
--     editor:SetActor(actorType, actorName, actorGO)
-- end

-- DeleteActor = function(actorType)
--     editor:DeleteActor( actorType )
-- end

-- EditorRunAI = function( ai_id )
-- 	local aiObj = gGame:GetAiMgr():CreateAI(ai_id, editor)
-- 	gGame:GetAiMgr():TryRunAI(aiObj)
-- end

-- UnitRunAI = function( actorType, ai_id )
--     local actorObj = editor:GetActor(actorType)

--     assert( actorObj, "目标演员不存在" );

--     Debugger.Log("ai_id:{0}", ai_id)

--     --local aiObj = actorObj:AddAI(ai_id, {})
--     --aiObj:TryRun(AI_OPPORTUNITY.RUN, {})
--     local aiobject = gGame:GetAiMgr():CreateAI(ai_id, actorObj)
-- 	gGame:GetAiMgr():TryRunAI(aiobject)
-- end

-- SetScenePos = function(posName, posGo)
--     editor:SetScenePos(posName, posGo)
-- end


