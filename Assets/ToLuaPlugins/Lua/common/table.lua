--判断一个table是否为空
table.is_empty = function(tbl)
	for _, _ in pairs(tbl) do
 		return false
 	end
 	
 	return true
end

table.keys = function(tbl)

    if ( not tbl) then return nil end

    local ks = {}
    local idx= 1
    for k, _ in pairs(tbl) do
        ks[idx] = k
        idx = idx + 1
    end

    return ks
end

--将table转化为字符串
table.get_table_str = function(tb, tb_deep, func_save_item)
	if type(tb) ~= "table" then return "" end
	
	tb_deep =  tb_deep or 20
	local cur_deep = 1
	local tb_cache = {}
	
	local function save_table(tb_data)
		--储存当前层table
		if type(tb_data) ~= "table"  then
			print("[Error]:".."存储类型必须为table")
			return
		end
		
		if tb_cache[tb_data] then
			print("[Error]".."无法继续存储，table中包含循环引用")
			return
		end
		
		tb_cache[tb_data] = true
		
		local k, v
		cur_deep = cur_deep + 1
		if cur_deep > tb_deep then
			print("[Error:]".."待存储table超过可允许的table深度")
			cur_deep = cur_deep -  1
			return
		end
		
		local tab = string.rep(" ", (cur_deep - 1)*4)
		local str = "{\n"
		
		-- 调整table存储顺序，按照key排序
		local keys_num = {}
		local keys_str = {}
		for k, v in pairs(tb_data) do
			if type(k) == "number" then
				table.insert(keys_num, k)
			elseif type(k) == "string" then
				table.insert(keys_str, k)
			end
		end
		table.sort(keys_str)
		table.sort(keys_num)
		
		local keys = {}
		for i, k in ipairs(keys_num) do
			table.insert(keys, k)
		end
		for i, k in ipairs(keys_str) do
			table.insert(keys, k)
		end
		for k, v in pairs(tb_data) do
			if type(k) ~= "number" and type(k) ~= "string" then
				table.insert(keys, k)
			end
		end
		
		-- 保存调整后的table
		local i
		for i, k in ipairs(keys) do
			v = tb_data[k]
			local arg, value
			if type(k) == "number" then
				arg = string.format("[%d]", k)   --认为key一定是整数
			end
			if type(k) == "string" then
				arg = string.format("[\"%s\"]", string.gsub(k,"\\","\\\\"))
			end
			if type(k) == "boolean" then
				value = tostring(k)
			end
			if type(v) == "number" then
				value = string.format("%f", v)
			end
			if type(v) == "string" then
				value = string.format("\"%s\"", string.gsub(v,"\\","\\\\"))
			end
			if type(v) == "table" then
				value = save_table(v)
			end
			if type(v) == "boolean" then
				value = tostring(v)
			end
			
			if arg and value then
				local item_str = func_save_item and func_save_item(tab, arg, value) or string.format("%s%s = %s,\n", tab, arg, value)
				str = str..item_str
			end
		end
		
		cur_deep = cur_deep -  1
		local tab = string.rep(" ", (cur_deep - 1) * 4)
		tb_cache[tb_data] = false
		return str..tab.."}"
	end
	
	local tb_str = save_table(tb)
	return tb_str
end

--此排序可以解决table对于相等元素的列表排序的问题
--外部必须重载sotf方法
--应用在比如战绩报告界面等
--算法:冒泡排序
table.fsort = function(list,sortf)
	 if sortf==nil then
		 table.sort(list)
	 end
	 local length = #list
	 for i=1,length do
		 for j=i+1,length do
			 if sortf(list[i],list[j]) then
				 local tmpObj = list[j]
				 list[j] = list[i]
				 list[i] =tmpObj
			 end
		 end
	 end
end

--将table保存为文件
table.save_fd = function(file, tb, tb_deep, func_save_item)
	if type(tb) ~= "table" or not file then
		return false
	end
	
	local tb_str = "return \n"..table.get_table_str(tb, tb_deep, func_save_item)
	file:write(tb_str)
	
	return true
end

--保存table的数据
table.save_data = function(file, tb, tb_deep, func_save_item)
	if type(tb) ~= "table" or not file then		
		return false
	end
	
	local tb_str = "data = "..table.get_table_str(tb, tb_deep, func_save_item)
	file:write(tb_str)
	return true
end

--保存table为文件
table.save = function(tb, path, tb_deep, is_compile, is_compress, func_save_item, is_data)
	local mode = is_compile and "wb" or "w"
	is_data = is_data or false
	
	if type(path)=="table" then
		 path = path[1].."/"..path[2]
	end
	
	local file = io.open(path, mode)
	if not file then
		print("table.save打开文件错误:"..path)
	end
	
	-- table.save_fd参数依次为：file, tb, tb_deep, func_save_item
	local rtn = false
	if is_data == true then
		rtn = table.save_data(file, tb, tb_deep, func_save_item)
	else
		rtn = table.save_fd(file, tb, tb_deep, func_save_item)
	end
	
	file:close()
	return rtn
end

--合并table
table.merge = function(dest, src)
	if type(dest) ~= "table" or type(src) ~= "table" then
		return
	end
	
	for k, v in pairs(src) do
		dest[k] = v
	end
	
	return dest
end

--克隆table
table.clone = function(src)
	if type(src) ~= "table" then
		return src
	end
	
	local table_already_clone = {}	-- 已经复制好的table，防止嵌套复制引起的死循环
	local copy_table
	local level = 0
	
	local clone_table = nil
	clone_table = function(t)
		level = level + 1
		if level > 20 then
			print("table clone failed, source table is too deep!")
		end
		
		local k, v
		local rel = {}
		
		table_already_clone[tostring(t)] = rel
		for k, v in pairs(t) do
			if type(v) == "table" then
				rel[k] = table_already_clone[tostring(v)] or clone_table(v)
			else
				rel[k] = v
			end
		end
		level = level - 1
		
		return rel
	end
	
	return clone_table(src)
end

--把srct列表按顺序追加到desct列表的未尾
--要求srct和desct两个表都是顺序表
table.fcat = function(desct, srct)
	for k, v in ipairs( srct ) do
		table.insert( desct, v )
	end
end

--移除table中的某个元素
table.remove_element = function(t, e)
	for k,v in pairs(t) do
		if v == e then
			t[k] = nil
		end
	end
end

--获取table的元素个数
table.count = function(t)
	local c = 0
	for k, v in pairs(t) do
		c = c + 1
	end
	return c
end

--获取不为空值的元素个数
table.count_not_nil = function(t)
	local c = 0
	for k, v in pairs(t) do
		if v ~= nil then c = c + 1 end
	end
	return c
end

--table是否包含某个key值
table.contains_key = function(t, k)
	for _k,v in pairs(t) do
		if k == _k then
			return true
		end
	end
	return false
end

--table是否包含某个value值
table.contains_value = function(t, v)
	for _k, value in pairs(t) do
		if v == value then
			return true
		end
	end
	return false
end

--在一个列表里删除某项
table.delete = function(t, v)
	for _k,value in pairs(t) do
		if v == value then
			table.remove(t, _k)
		end
	end
end

--打包
table.pack = function(...)
	return {...}
end

--解包
table.unpack = function(t)
	if type(t) == "table" then
		return unpack(t)
	else
		return t
	end
end

local cmp = nil
cmp = function (a, b)
	if type(a) == "table" and type(b) == "table" then
		for k, v in pairs(a) do
			if not cmp(a[k], b[k]) then
				return false
			end
		end
		return true
	end
	return a == b
end

table.is_kv_same = function (t1, t2)
	return cmp(t1, t2) and cmp(t2, t1)
end

--交换table中的两个元素
table.swap = function(tbl, index1, index2)
	local item1 = tbl[index1]
	local item2 = tbl[index2]
	if item1 and item2 then
		tbl[index1] = item2
		tbl[index2] = item1
	end
end

--反应用，key值与value值互换
table.reverse_map = function(t)
	local rt = {}
  for k, v in pairs(t) do
  	rt[v] = k
  end
  
  return rt
end

-- 把Table转成字符串，支持可变参数
table.str = function( ... )
	local strTab = {}
    for i = 1, select('#', ...) do
        local arg = select(i, ...)
        if type(arg) == "table" then
        	arg = table.get_table_str(arg)
        elseif type(arg) ~= "string" then
        	arg = tostring(arg)
        end
        table.insert(strTab, arg)
    end
    return table.concat(strTab)
end

table.log = function( ... )
    log(table.str( ... ))
end

table.warn = function( ... )
    warn(table.str( ... ))
end

table.error = function( ... )
    error(table.str( ... ))
end

table.print = function( tb ) 
	if type(tb) ~= "table" then		
		return
	end
	
	local tb_deep =  20
	local cur_deep = 0
	local tb_cache = {}
	local function print_table(tb_data)
		-- 存储当前层table
		if type(tb_data) ~= "table"  then
			log("Error", "存储类型必须为table:", tb )
			return
		end
		if tb_cache[tb] then
			log("Error", "无法继续存储，table中包含循环引用，", tb )
			return
		end
		local k, v
		cur_deep = cur_deep + 1
		if cur_deep > tb_deep then
			cur_deep = cur_deep -  1
			return	"..."
		end
		local tab = string.rep(" ", (cur_deep-1)*4)
		local str = "{\n"
		
		-- 调整table存储顺序，按照key排序
		local keys_num = {}
		local keys_str = {}
		for k, v in pairs(tb_data) do
			if type(k) == "number" then
				table.insert(keys_num, k)
			elseif type(k) == "string" then
				table.insert(keys_str, k)
			end
		end
		table.sort(keys_str)
		table.sort(keys_num)
		
		local keys = {}
		for i, k in ipairs(keys_num) do
			table.insert(keys, k)
		end
		for i, k in ipairs(keys_str) do
			table.insert(keys, k)
		end
		for k, v in pairs(tb_data) do
			if type(k) ~= "number" and type(k) ~= "string" then
				table.insert(keys, k)
			end
		end
		
		-- 保存调整后的table
		local i
		for i, k in ipairs(keys) do
			v = tb_data[k]
			local arg, value
			if type(k) == "number" then
				arg = string.format("[%d]", k)   --认为key一定是整数
			elseif type(k) == "string" then
				arg = string.format("[\"%s\"]", string.gsub(k,"\\","\\\\"))
			else
				arg = string.format("[\"%s\"]", string.gsub(tostring(k),"\\","\\\\"))
			end

			if type(v) == "number" then
				value = string.format("%f", v)
			elseif type(v) == "string" then
				value = string.format("\"%s\"", string.gsub(v,"\\","\\\\"))		
			elseif type(v) == "table" then
				value = print_table(v)
			else 
				value = tostring(v)
			end
			
			
			if arg and value then
				str = str..string.format("%s%s = %s,\n", tab, arg, value)
			end
		end
		tb_cache[tb_data] = true
		cur_deep = cur_deep -  1
		return str..tab.."}"
	end
	
	local tb_str = print_table(tb)
	print( tb_str )
		
	return true
end

table.indexOf = function(t, object)
    if "table" == type( t ) then
        for i = 1, #t do
            if object == t[i] then
                return i
            end
        end
        return -1
    else
            error("table.indexOf expects table for first argument, " .. type(t) .. " given")
    end
end
