
--BEGIN_VALUE_INPUT_CONFIG--
local config = {
    speed = "System.Single",
    time = "System.Single"
}
--END_VALUE_INPUT_CONFIG--



local AIActionBase = Import("logic/action_base").CAIAction

AIAction = class(AIActionBase) 

AIAction.GetId = function (self)
    return "rotate"
end

AIAction.InitAction = function(self, speed, time)
    self.speed = speed
    self.duration = time or 99999
    print(" *** rotate InitAction *** ")
    print("self.speed : ", self.speed)
    print("self.duration : ", self.duration)
end


AIAction.__DealAction = function (self, target, delta_time)
    local targetObj = self:GetObjectGOById(target)

    if ( not targetObj ) then
        --Logger.debug("has no OBJ");
        print("has no OBJ", target)
        return false
    end

	if self.duration == 0 then 
		targetObj.transform:Rotate(Vector3.up, self.speed)
	else
		targetObj.transform:Rotate(Vector3.up, self.speed * delta_time)
	end
    
    return true

    --print(" rotate __DealAction ", target, "  ", self.running_targets[target].duration)
    --return true
end


AIAction.__EndAction = function (self, target)
    print(" rotate __EndAction ", target)
end