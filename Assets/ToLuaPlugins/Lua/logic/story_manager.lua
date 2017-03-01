CStoryManager = class() 

local DEFAULT_AI_ACTION_DIR = "logic/ai_action/"
local DEFAULT_STORY_COMMAND_DIR = "logic/story_command/"

CStoryManager.Init = function(self)
	self._actions = {}
	self._targetType2Id = {}
end

-- actTargets = {"A1", "A2", "A3"}
CStoryManager.AddAction = function(self, actKey, actClsName, actTargetTypes, actArgsValue)

 	-- "A1,A2,A3" -> target Id
 	local actTargetsIds = {}
 	for i = 1, #actTargetTypes do
 		local at = actTargetTypes[i]
 		local actorId = nil
	 	if __UNITY_EDITOR__  then
 			local actor = editor:GetActor(at)
 			actorId =actor:GetId()
	 	else
	 		actorId = self:GetActorIdByType(at)
	 	end
	 	assert(actorId, at)
	 	table.insert(actTargetsIds, actorId)
	 end

	local actCls = Import(DEFAULT_AI_ACTION_DIR .. actClsName).AIAction
    local act = actCls:New({}, actTargetsIds, unpack(actArgsValue))
    assert(not self._actions[actKey], actKey)
    self._actions[actKey] = act
end

CStoryManager.DelAction = function(self, actKey)
	self._actions[actKey] = nil
end

CStoryManager.GetAction = function(self, actKey)
	return self._actions[actKey]
end

CStoryManager.DelAllAction = function(self)
	self._actions = {}
end

CStoryManager.UpdateAction = function(self, actKey, deltaTime)
	local act = self._actions[actKey]
	assert(act)
	act:Update(deltaTime)
	return act:IsFinish()
end


CStoryManager.AddActor = function(self, type, id)
	self._targetType2Id[type] = id
end

CStoryManager.DelActor = function(self, type)
	self._targetType2Id[type] = nil
end

CStoryManager.GetActorIdByType = function(self, type)
	return self._targetType2Id[type]
end



-- CStoryManager.GetOutputData = function(self, commandName, inputDatas, outputDataKey)
-- 	local modPath = DEFAULT_STORY_COMMAND_DIR .. commandName
-- 	local mod = Import(modPath)
-- 	assert(mod ~= nil, string.format("%s not exist", modPath))
-- 	return mod.Main(inputDatas, outputDataKey)
-- end



