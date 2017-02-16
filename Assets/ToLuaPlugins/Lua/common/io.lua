GetFileContent = function(dir, file)
	local f = io.open(string.format("%s/%s", dir or "", file or ""), "rb")
	
	if f then
		local str = f:read("*a")
		f:close()
		return str
	end
	
	return ""
end

GetFileForStr = function(path)
	local f = io.open(path, "r")
	if f then
		local str = f:read("*a")
		f:close()
		return str
	end
	
	return ""
end

FileExist = function(dir, file)
	local path = string.format("%s/%s", dir or "", file or "")
	return IsFileExist(path)
end

IsFileExist = function(path)
	local f = io.open(path, "rb")
	
	if f then
		f:close()
		return true
	end
	
	return false
end

WriteAllTextToFile = function(path, content)
	local f = io.open(path, "w")
	if f then
		f:write(content)
		f:close()
		return true
	end
	return false
end


WriteAllBytesToFile = function(path, content)
	local f = io.open(path, "wb")
	if f then
		f:write(content)
		f:close()
		return true
	end
	return false
end

