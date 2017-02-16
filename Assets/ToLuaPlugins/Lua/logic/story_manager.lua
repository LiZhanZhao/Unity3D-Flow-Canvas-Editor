CStoryManager = class() 

local DEFAULT_AI_ACTION_DIR = "logic/ai_action/"
local DEFAULT_STORY_COMMAND_DIR = "logic/story_command/"

CStoryManager.Init = function(self)
	print("*** StoryManager *** ")
	self._actions = {}
end

CStoryManager.AddAction = function(self, actKey, actClsName, actTargets, actArgsValue)
	local actCls = Import(DEFAULT_AI_ACTION_DIR .. actClsName).AIAction
    local act = actCls:New({}, actTargets, unpack(actArgsValue))
    assert(not self._actions[actKey])
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

CStoryManager.GetOutputData = function(self, commandName, inputDatas, outputDataKey)
	local modPath = DEFAULT_STORY_COMMAND_DIR .. commandName
	local mod = Import(modPath)
	assert(mod ~= nil, string.format("%s not exist", modPath))
	return mod.Main(inputDatas, outputDataKey)
end



