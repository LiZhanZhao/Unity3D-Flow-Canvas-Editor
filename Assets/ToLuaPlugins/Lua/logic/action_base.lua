
--local ObjectCounter = Import("logic/common/object_counter")
--[[
-- 所有AI Action的基类
-- 每个AI Action的原始数据包括
-- 针对每个目标做什么动作
-- 实例化后会将每个目标记录在运行数据内
-- 同时记录每个目标在执行的action
-- 只有所有目标的动作都执行完毕，才能认为本action执行完毕
-- Action目前有剧情播放/delay/runto_xy/runto_id/kill_id 
-- 等有持续时间的ID
-- 其他大部分的action是即时的，如 设值,设置目标,冒泡说话,debug等
--]]

CAIAction = class()

CAIAction.Init  = function (self, ownerAI, targets, ...)
    --ObjectCounter.object_counter(self, "CAIAction")
    
    -- 构建Action运行时长,默认为0
    -- 持续时间为0的，会立即结束
	ownerAI.runningAction = self
	self.aiActors = ownerAI.actors
    self.duration = 0
    self.data = {}
    self:InitAction(...)
    self.running_targets = {}
    self.targets = targets
    self:InitRunningTargets()
	
	self.finish = false
end

CAIAction.Release = function(self)
	self.aiActors = nil
	self.duration = nil
	self.data = nil
	self.running_targets = nil
	self.targets = nil
end

CAIAction.GetId = function (self)
    return "ai_action_base"
end

CAIAction.InitAction = function (self, ...)
end

CAIAction.InitRunningTargets = function (self)
    -- 目标选择
    if not self.targets or #self.targets < 1 then 
		return
	end
 
    -- 初始化目标运行数据
    for _, target in ipairs(self.targets) do
        -- 目标在场
        self.running_targets[target] = {} 
        local tb_tmp = self.running_targets[target]
        tb_tmp.duration = 0
        self:__BeginAction(target)
    end
end

CAIAction.__BeginAction = function (self, target)
end

CAIAction.IsFinish = function(self)
	return self.finish  
end

CAIAction.Update = function (self, dt)
    if not self.targets or #self.targets < 1 then
        self.finish = true
		self:Release()
        return
    end
	
    -- 针对每个target，更新运行目录
    for target, _ in pairs(self.running_targets) do
        -- 针对每个目标处理
        self:DealAction(target, dt)    
    end
	
	-- 所有执行者完成该动作
    if table.count(self.running_targets) <= 0 then       
        self.finish = true
		self:Release()
        return
    end
end

-- 针对单个目标动作
CAIAction.DealAction = function (self, target, delta_time)
    local tb_tmp = self.running_targets[target]
    if not tb_tmp then return end
    tb_tmp.duration = tb_tmp.duration + delta_time

    local deal_result = self:__DealAction(target, delta_time)
    if not deal_result or tb_tmp.duration >= self.duration then
        self:__EndAction(target)
        -- 清空运行
        self.running_targets[target] = nil
    end
end

-- 针对单个目标需要做的事情
CAIAction.__DealAction = function (self, target, delta_time)
    -- 默认Action只与tb_tmp.duration有关,这里返回true
    -- 如果返回false表示，此目标执行本动作完毕
    return true
end


CAIAction.__EndAction = function (self, target)
end

CAIAction.OnComplete = function()
	
end

CAIAction.GetOwnerAI = function (self)
    return gGame:GetAiMgr():GetActionOwnerAI(self)
end

CAIAction.GetTargets = function (self)
    return self.targets
end

CAIAction.SetData = function (self, key, value)
    self.data[key] = value
end

CAIAction.GetData = function (self, key)
    return self.data[key]
end

CAIAction.AddCustomActor = function(self, id, actorType, actorObj)
    if __UNITY_EDITOR__ then
        SetActor(actorType, actorObj.name, actorObj)
    else
        local curScene = gGame:GetSceneMgr():GetCurrentScene()
        local actor = curScene:AddObject(id, actorObj)
        self:GetOwnerAI():AddActor(actorType, actor) 
    end
end

CAIAction.DelCustomActor = function(self, actorType)
    if __UNITY_EDITOR__ then
        DeleteActor(actorType)
    else
        local actor = self:GetOwnerAI():GetActor(actorType)
        if actor then
            local curScene = gGame:GetSceneMgr():GetCurrentScene()
			gGame:GetSceneMgr():ReuseCustomActorId(actor:GetId())
            --curScene:DelObject(actor:GetId())	
			gGame:GetObjMgr():RemoveScenePlayer(actor:GetId())		
            self:GetOwnerAI():AddActor(actorType, nil)
        end
    end
end

CAIAction.GetObjectById = function (self, target)
    local targetObj = nil

    if __UNITY_EDITOR__ then 
        targetObj = editor:GetActorByID( target )
    else
        local curScene = gGame:GetSceneMgr():GetCurrentScene()
		if curScene then 
			targetObj = curScene:GetObjectById(target)
		end

        if ( targetObj ) then return targetObj end
        local actors = self:GetOwnerAI().actors
        for type, actor in pairs(actors) do
            if type ~= "end_call_back" then
                if actor:GetId() == target then
                    targetObj = actor
                end
            end
        end
    end

    return targetObj
end

CAIAction.GetObjectGOById = function (self, target)
    local targetObj = nil

    if __UNITY_EDITOR__ then 
        targetObj = editor:GetActorByID( target )
    else      
        targetObj = self:GetObjectById(target)
    end

    if targetObj then
        local go = targetObj._go
        
        if not go then
            local gameObjectInfo = gGame:GetGameObjectMgr():GetGameObjectInfo(targetObj._gameobjectUID)
            if gameObjectInfo then
                go = gameObjectInfo._gameObject
            end
        end

        return go
    end

    return nil 
end

CAIAction.GetChildByName = function(self, targetGo, name)
    local tranComs = targetGo:GetComponentsInChildren(typeof(Transform), true)
    for i = 0, tranComs.Length - 1 do
        local tran = tranComs[i]
        if tran.name == name then
            return tran.gameObject
        end
    end
    return nil
end

CAIAction.GetGuaPoint = function(self, targetGo, name)
    local guaPointObj = self:GetChildByName(targetGo, name)
    if guaPointObj then
        return guaPointObj
    else
        return targetGo
    end
end

CAIAction.SetState = function(self, obj, stateName, stateValue, speed)
	local animComp = obj:GetComponent(typeof(UnityEngine.Animator))
	if animComp then 
		animComp:SetInteger(stateName, stateValue)
		animComp.speed = speed
	end
end
