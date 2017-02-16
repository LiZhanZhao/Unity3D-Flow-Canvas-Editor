require "common/io"
require "common/class"
require "common/import"
require "common/table"

local StoryManager = Import("logic/story_manager").CStoryManager
gStoryManager = nil

--主入口函数。从这里开始lua逻辑
function Main()					
	print("**** Hello World ****")

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

-- actTargets, actArgsValue -> C# array
SMAddAction = function(actKey, actClsName, actTargetsArr, actArgsValueArr)
	-- A1,A2,A3...
 	local actTargetsTab = {}
 	local len = actTargetsArr.Length
 	for i = 0, len - 1 do
 		table.insert(actTargetsTab, actTargetsArr[i])
 	end

 	print("targets count: ", #actTargetsTab)

 	local actArgsValuesTab = {}
 	len = actArgsValueArr.Length
	for i = 0, len - 1 do
 		table.insert(actArgsValuesTab, actArgsValueArr[i])
 	end 	

 	-- ******* deal with the actTargetsArr ****************

	gStoryManager:AddAction(actKey, actClsName, actTargetsTab, actArgsValuesTab)
end

SMUpdateAction = function(actKey, deltaTime)
	print("*** SMUpdateAction ***")
	if gStoryManager:GetAction(actKey) then
		return gStoryManager:UpdateAction(actKey, deltaTime)
	else
		return true
	end
end

-- SMDelAction = function(actKey)
-- 	return gStoryManager:DelAction(actKey)
-- end