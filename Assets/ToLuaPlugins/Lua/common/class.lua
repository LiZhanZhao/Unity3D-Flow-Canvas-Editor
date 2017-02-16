obj_cls_map = {}
setmetatable(obj_cls_map, { __mode = "kv" })

local search_base 
search_base = function(t, k)

	local base_list = rawget(t, "__base_list")
	
	if not base_list then
		return
	end

	local v
	for i = 1, #base_list do
		local base = base_list[i]		

		v = rawget(base, k)
		if v then
			t[k] = v -- 缓存搜索结果
			return v
		end
		
		v = search_base(base, k)
		if v then
			t[k] = v -- 缓存搜索结果
			return v
		end
	end
end


is_class_type = function(obj)
	return type(obj) == "table" and rawget(obj, "__class") ~= nil
end

is_class_of = function(obj, class)
	return is_class_type(obj) and obj.__class == class
end

is_base_class = function(base, derive)
	local base_list = rawget(derive, "__base_list")

	if not base_list then return false end

	for _, b_class in ipairs(base_list) do
		if (base == b_class) or is_base_class(base, b_class) then
			return true
		end
	end

	return false
end

class_cast = function(Cls, Obj)
	-- 不允许子类往基类转，因为这样可能导致转完后的对象找不到子类上的函数
	if Obj.__class and is_base_class(Cls, Obj.__class) then
		return Obj
	end

	Obj.__raw_tostring = tostring(Obj)
	
	setmetatable(Obj, Cls.__objmt)
	Obj.__class = Cls
	return Obj
end

class = function(...)
	local arg = {...}
	local NewClass = {}

	NewClass.__base_list = {}
	for i = 1, #arg do
		if arg[i] == nil then
			error("定义class的函数传入的基类为nil！")
		end
		if type(arg[i]) ~= "table" or not arg[i].__objmt then
			error("定义class的函数传入的基类不是自定义类型！")
		end
		
		table.insert( NewClass.__base_list, arg[i] )

		-- 将基类记录到类的table里面用于查询继承关系
		NewClass[ arg[i] ] = true
	end
	-- 将基类记录到类的table里面用于查询继承关系
	NewClass[ NewClass ] = true
	
	NewClass.__prop_list = {}
	
	NewClass.__objmt = {
		__index = function(self, key)
			if NewClass.__prop_list[key] then
				return NewClass.__prop_list[key](self)
			end
			return NewClass[key]
		end,
		
		__newindex = function(self, key, value)
			if NewClass.__prop_list[key] then
				NewClass.__prop_list[key](self, value)
			else
				rawset(self, key, value)
			end
		end,
		
		__tostring = function(obj)
		 	return "<object of "..tostring(obj.__class).." at("..obj.__raw_tostring..")>"
		end
	}
	
	NewClass.New = function(self, ...)
		local LuaObj = {}
		class_cast(NewClass, LuaObj)
		
		--在调用Ctor之前先把_CreateingInstance属性设置好，不然在Ctor里又引用自己的话就会导致额外创建。wellbye
		self._CreateingInstance = LuaObj
		
		--记录New类对象的地方，调试用
		-- if GlobalCfg.ClsObjTraBacDebug then
		-- 	local traceback = debug.traceback("", 2)--debug.getinfo(2, 'Sl')
		-- 	rawset(LuaObj, "__traceback", traceback)
		-- end

		--c++对象的构造
		if self.ctor then
			self.ctor(LuaObj)
		elseif self.__cppclass then
			error("C++类"..self.__name.."不允许在脚本层构造！")
		end
		
		-- lua类的初始化
		if self.Init then
			self.Init(LuaObj, ...)
		end
		
		self._CreateingInstance = nil
		
		obj_cls_map[LuaObj] = NewClass
		return LuaObj
	end
	
	NewClass.copy = function(self, ...)
		local LuaObj = {}
		class_cast(NewClass, LuaObj)
		
		--在调用Ctor之前先把_CreateingInstance属性设置好，不然在Ctor里又引用自己的话就会导致额外创建。wellbye
		self._CreateingInstance = LuaObj
		
		local arg = {...}
		local src_obj = arg[1]
		if not is_class_type(src_obj) or (self ~= src_obj.__class and
			not is_base_class(self, src_obj.__class)) then
			error("参数类型错误，必须是"..tostring(self).."或其派生类的对象")
		end

		--c++对象的复制构造
		if self.copy_ctor then
			self.copy_ctor(LuaObj, src_obj)
		elseif self.__cppclass then
			error("C++类"..self.__name.."不允许在脚本层复制构造！")
		end
	
		-- lua类的初始化
		if self.copy_init then
			self.copy_init(LuaObj, ...)
		end
		
		self._CreateingInstance = nil
		return LuaObj
	end
	
	local prefix = "<class "
	local postfix = "("..tostring(NewClass)..")"..">"
	
	setmetatable(NewClass, {
		__index = search_base,
		__call = NewClass.New,
		__tostring = function(cls)
			if rawget(NewClass, "__cppclass") then
				prefix = "<class(C++) "
			end
			if NewClass.__name then
				return prefix..NewClass.__name.." "..postfix
			else
				return prefix..postfix
			end
		end
	})

	return NewClass
end

