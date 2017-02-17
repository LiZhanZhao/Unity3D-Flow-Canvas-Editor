
--BEGIN_VALUE_OUTPUT_CONFIG--
local valueoutputConfig = {
    target1 = "System.Object",
    target2 = "System.Object",
}
--END_VALUE_OUTPUT_CONFIG--


--inputDatas -> table
-- this is will getTarget Id
Main = function(inputDatas, outputDataKey)
	print(" *** get_targets Main *** ", "outputDataKey", "   ", outputDataKey)
	local arr1 = System.Array.CreateInstance(typeof(System.String), 2)
	arr1:SetValue("A11", 0)
	arr1:SetValue("A22", 1)

	local arr2 = System.Array.CreateInstance(typeof(System.String), 2)
	arr2:SetValue("T11", 0)
	arr2:SetValue("T22", 1)

	local data = {}
	data.target1 = arr1
	data.target2 = arr2

	return data[outputDataKey]
end