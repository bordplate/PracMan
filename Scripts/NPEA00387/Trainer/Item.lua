Item = class("Item")

GC_ITEMS = {"Plasma Coil", "Lava Gun", "Bouncer", "Miniturret", "Shield Charger"}

function Item:initialize(name, index, unlockOffset, expOffset, levels)
    self.name = name
    self.index = index
    self.unlockOffset = unlockOffset - 0x4A8
    self.expOffset = expOffset - 0x5F0
    self.levels = levels
end

function Item:VersionNTableOffset(version)
    if version == 1 then
        return self.index
    end

    if self.name == "Bouncer" then
        if version == 2 then
            return 0xA6
        else
            return 0xB1 + version
        end
    elseif self.name == "Plasma Coil" then
        if version == 2 then
            return 0xA0
        else
            return 0xB7 + version
        end
    elseif self.name == "Shield Charger" then
        if version == 2 then
            return 0xA7
        else
            return 0xBD + version
        end
    elseif self.name == "Lava Gun" then
        if version == 2 then
            return 0xA1
        else
            return 0xAB + version
        end
    elseif self.name == "Miniturret" then
        if version == 2 then
            return 0xA2
        else
            return 0xA5 + version
        end
    end

    return self.index + version - 1
end 

function Item:IsUnlocked()
    return Target:ReadByte(0xDA56EC + self.unlockOffset) == 1
end

function Item:ToggleUnlock(toggle)
    Target:WriteByte(0xDA56EC + self.unlockOffset, toggle and 1 or 0)
end

function Item:SetExp(exp)
    Target:WriteUInt(0xDA5834 + self.expOffset, exp)
end

function Item:SetVersion(version)
    if self.levels <= 1 then
        return
    end

    Target:WriteByte(0xc1e43c + self.index, self:VersionNTableOffset(version))
end

function Item:GetVersionHeuristic()
    if table.contains(GC_ITEMS, self.name) then
        return 1
    end

    return Target:ReadByte(0xc1e43c + self.index) - self.index + 1
end

function Item.static:SetAllItemExp(start, range, exp)
    local expBytes = { bit.band(exp, 0xFF), bit.band(bit.brshift(exp, 8), 0xFF), bit.band(bit.brshift(exp, 16), 0xFF), bit.band(bit.brshift(exp, 24), 0xFF) }
    local newBytes = {}
    for i = 1, range do
        newBytes[i] = expBytes[((i - 1) % 4) + 1]
    end

    Target:WriteMemory(0xDA5834 + start, TableToByteArray(newBytes))
end