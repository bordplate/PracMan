Item = class("Item")

function Item:initialize(name, unlock)
    self.name = name
    self.unlock = unlock
end

function Item:ToggleUnlock(enable)
    Target:WriteByte(self.unlock, enable and 1 or 0)
end

function Item:IsUnlocked()
    return Target:ReadByte(self.unlock) == 1
end