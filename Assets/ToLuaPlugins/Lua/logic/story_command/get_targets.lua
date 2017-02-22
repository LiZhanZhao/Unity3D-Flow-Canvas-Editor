
--BEGIN_VALUE_INPUT_CONFIG--
local valueInputConfig = {
    actorTypes = "System.Object",
    actorGoNames = "System.Object"
}
--END_VALUE_INPUT_CONFIG--


--BEGIN_VALUE_OUTPUT_CONFIG--
local valueOutputConfig = {
    target1 = "System.Object"
}
--END_VALUE_OUTPUT_CONFIG--



--inputDatas -> table
-- this is will getTarget Id
Main = function(inputDatas, outputDataKey)
	

	-- print(" *** get_targets Main *** ", "outputDataKey", "   ", outputDataKey)
	-- local arr1 = System.Array.CreateInstance(typeof(System.String), 2)
	-- arr1:SetValue("A11", 0)
	-- arr1:SetValue("A22", 1)

	-- local arr2 = System.Array.CreateInstance(typeof(System.String), 2)
	-- arr2:SetValue("T11", 0)
	-- arr2:SetValue("T22", 1)

	-- local data = {}
	-- data.target1 = arr1
	-- data.target2 = arr2
	--return data[outputDataKey]

	local actorTypes = inputDatas[1]
	local actorGoNames = inputDatas[2]
	assert(actorTypes.Length == actorGoNames.Length)

	---------------------editor------------------------

	if __UNITY_EDITOR__  then
		local len = actorTypes.Length
		local arr = System.Array.CreateInstance(typeof(System.String), len)
		for i = 0, len - 1 do
	        local at = actorTypes[i]
	        local agn = actorGoNames[i]
	        local ag = UnityEngine.GameObject.Find(agn)
	        editor:SetActor(at, agn, ag)
	        local actor = editor:GetActor(at)
	        arr:SetValue(actor:GetId(), i)
	    end

	    local data = {}
		data.target1 = arr
		return data[outputDataKey]
	---------------------editor------------------------
	else
		-- 在这里直接用游戏的逻辑代码来或者target Id
	end

	
	
	
    

end