local pwd
function GetPwd()
	if pwd then return pwd end
	local info = debug.getinfo(2, 'S')
	return info.source
end

----------------------------------------------------

__fs_require_loaded = {}
__fs_require_loading_time = {}

-- 以下重写class_cast和class函数是为了update
old_class_cast = class_cast
class_cast = function( Cls, Obj )
	Obj = old_class_cast(Cls, Obj)
	if Cls.__objs then
		Cls.__objs[Obj] = true
	end
	return Obj
end

old_class = class
class = function( ... )
    local NewClass = old_class(...)	
    NewClass.__objs = rawget(getfenv(2), "__class_objs")
    return NewClass
end

Import = function(name)
	local info = debug.getinfo(2, 'S')
    local source = nil
	if info.what == "main" or info.what == 'Lua' then
		source = info.source
		
		if info.what == 'Lua' then
			local p = string.gsub(GetPwd(), "[^\\]*.lua", "")
			source = string.gsub(source, p, "")
			source = string.gsub(source, "\\", "/")
		end
		local new_source = string.gsub(source, "[^/]*.lua", name)
		
		if FileExist(".", new_source) then
    		name = new_source
    	end
	end
	
	local ret = __fs_require_loaded[name]

    --print("Import:", name, " from ", source);
    if ( ret ) then
        -- TODO:记录Import了这个文件的source
        --print("Import:", name, " into __import_source:", source);
        ret.__import_source[source]  = true 
    end

	if ret then return ret end
	local mod = _Import(name)
    if ( mod ) then
        -- TODO:记录Import了这个文件的source
        --print("Import:", name, " into __import_source:", source);
        mod.__import_source[source]  = true 
    end
    return mod
end

_Import = function(name, reload)
	reload = reload and __fs_require_loaded[name] and true or false
	
	local get_class_name = function(mod, cls)
		for n, v in pairs(mod) do
            --print("get_class_name:", n, v);
			if type(v) == "table" and v == cls then return n end
		end
	end
	
	local restore_class_objs = function(mod)
        local idx = 1
		for obj, _ in pairs(mod.__class_objs) do
			obj.__class_name = get_class_name(mod, obj.__class)
            --print("restore_class_objs:", "[", idx,"]", obj, " ", obj.__class, " ", obj.__class_name);
            idx = idx + 1
		end
	end
	
	local resume_class_objs = function(mod)
		--print("resume_class_objs:", name, table.count(mod.__class_objs))
		for obj, _ in pairs(mod.__class_objs) do
            --print("resume_class_objs obj.__class_name:", obj, obj.__class_name)
            --print("resume_class_objs before cast:", obj.OnFingerDown)
			if obj.__class_name then
				old_class_cast(mod[obj.__class_name], obj)
                --print("resume_class_objs after cast:", obj.OnFingerDown)
                if obj.__update__ then
                    obj.__update__(mod, obj)
                end
			else
                -- print("to obj.Destroy")
				if obj.Destroy then obj:Destroy() end
			end
		end
	end

	
	local callinit = function(mod)
		if mod.__init__ then
			mod.__init__(mod)
		end
	end

	local calldestroy = function(mod)
		if mod.__destroy__ then
			mod.__destroy__(mod)
		end
		-- 可能重新加载模块后，这几个接口已经不存在，所以必须在老的环境先清除
		mod.__update__ = false
		mod.__init__   = false
		mod.__destroy__= false
		restore_class_objs(mod)
	end
	

	local callupdate = function(mod)
		if mod.__update__ then
			mod.__update__(mod)
        else
            callinit(mod)
		end
		resume_class_objs(mod)
	end
	
	--print("to load name:", name)
	local func = loadfile(name)
	if type(func) == "function" then
		local ok
		local env 
		
		if reload then
			env = __fs_require_loaded[name]
			calldestroy(env)
		else
			__fs_require_loaded[name] = {}
			env = __fs_require_loaded[name]
			env.__class_objs = {}
			env.__init__ = false
			env.__update__ = false
			env.__destroy__ = false
            env.__import_source = {}
			setmetatable(env.__class_objs, {__mode="kv"})
			setmetatable(env, {__index = _G})
		end
		
		local ok, ret = xpcall( 
			function()
				setfenv(func, env)()
				if reload then
					callupdate(env)
                else
                    callinit(env)
                end
				--__fs_require_loading_time[name] = iomanager:get_file_time("script", name)
				return env
			end,
			function(e)
				error(string.format("[fs_require failed]:%s", name), debug.traceback())
				error(e)
			end)

		return ret
	else
		error(string.format("[fs_require failed]:%s", name), debug.traceback())
		error(func)
	end
end

function update(name)
    collectgarbage("collect")
    collectgarbage("collect")

    if name then
        print("To update:", name);
        local mod = _Import(name, true)

        if mod and mod.__import_source then 
            for source, _ in pairs(mod.__import_source) do 
                print("Update source:", source);
                update(source)
            end
        else
            print("mod or mod.__import_source is nil:", mod, mod.__import_source);
        end
    else
        error("请输入更新文件名");
    end
end


function export(name, value)
	_G[name] = value
end

-- load a module globally, except the '__init__','__destroy__',... method
loadglobally = function(lf, t)
	t = t or _G
	for k,v in pairs(lf) do
		if type(k) == "string" and string.match(k, "__.*__") then
		elseif type(k) == "string" and k == "__class_objs" then
		else
			rawset(t, k, v)
		end
	end
	return lf
end

safe_dofile = function(path)
	local func = loadfile(path)
	
	if type(func) ~= "function" then
		log("error", "dofile failed:", path)
		return
	end

	return func()
end

dofile = safe_dofile

local loaded_file_cache = {}

static_dofile = function(path)
	if loaded_file_cache[path] then
		return loaded_file_cache[path]
	elseif loaded_file_cache[path] == false then
		return nil
	end

	local ret = safe_dofile(path)
	
	if not ret then
		loaded_file_cache[path] = false
	else
		loaded_file_cache[path] = ret
	end

	return ret
end

is_derive_class = function(obj, cls)
	return (cls == obj.__class) or (is_base_class(cls, obj.__class))
end
