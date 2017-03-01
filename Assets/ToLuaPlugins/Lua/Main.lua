require "common/io"
require "common/class"
require "common/import"
require "common/table"

local StoryManager = Import("logic/story_manager").CStoryManager
gStoryManager = nil

--主入口函数。从这里开始lua逻辑
function Main()					
	gStoryManager = StoryManager:New()
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
end

--local RotateAction = Import("logic/rotate").AIAction


TestFuncArgsTable = function(keys, args)
	local len = keys.Length
	assert(len)

	for i = 0, len - 1 do
        print('keys: '..tostring(keys[i]))
    end

    len = args.Length
	for i = 0, len - 1 do
        print('Args: '..tostring(args[i]))
    end
end

-- actArgsValue -> C# array
-- actTargetsTypeStr ->"A1,A2,A3"
SMAddAction = function(actKey, actClsName, actTargetsTypeStr, actArgsValueArr)
	assert(actArgsValueArr)

 	local actTargetsTypeTab = {}

 	local list = string.split(actTargetsTypeStr, ',')
	for key, value in pairs(list) do
		table.insert(actTargetsTypeTab, value)
	end

 	print("targets count: ", #actTargetsTypeTab)

 	local actArgsValuesTab = {}
 	len = actArgsValueArr.Length
	for i = 0, len - 1 do
 		table.insert(actArgsValuesTab, actArgsValueArr[i])
 	end

	gStoryManager:AddAction(actKey, actClsName, actTargetsTypeTab, actArgsValuesTab)
end

SMUpdateAction = function(actKey, deltaTime)
	--print("*** SMUpdateAction ***")
	if gStoryManager:GetAction(actKey) then
		return gStoryManager:UpdateAction(actKey, deltaTime)
	else
		return true
	end
end

SMDelAction = function(actKey)
	return gStoryManager:DelAction(actKey)
end

-- SMGetOutputData = function(moduleName, inputDatasArr, outputDataKey)
-- 	assert(inputDatasArr)
-- 	local intputDatasTab = {}
--  	local len = inputDatasArr.Length
--  	for i = 0, len - 1 do
--  		table.insert(intputDatasTab, inputDatasArr[i])
--  	end

--  	return gStoryManager:GetOutputData(moduleName, intputDatasTab, outputDataKey)
-- end