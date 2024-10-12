require 'Item'

Game = class("Game")

function Game:initialize()
    self.address = {
        position = {
            x = 0xDA2870,
            y = 0xDA2874,
            z = 0xDA2878
        },
        currentPlanet = 0xC1E438,
        destinationPlanet = 0xEE9314,
        shouldLoadPlanet = 0xEE9310,
        playerState = 0xDA4DB4,
        gameState = 0xEE9334,
        loadingScreenID = 0xD99114,
        ghostTimer = 0xDA29de,
        deathCount = 0xED7F14,
        planetFrameCount = 0x1A70B30,
        marcadiaMission = 0xD3AABC,
        loadedChunk = 0xF08100,
        boltCount = 0xc1e4dc,
        healthXP = 0xc1e510,
        playerHealth = 0xda5040,
        currentArmor = 0xC1E51C,
        challengeMode = 0xC1E50e,
        HeldItemFoo = 0xDA27CB,
        HeldItemBar = 0xDA3A1B,
        HeldItemBaz = 0xDA4E07,
        shipColor = 0xDA55E8,
        titaniumBoltsArray = 0xECE53D,
        skillPointsArray = 0xDA521D,
        itemArray = 0xc1e43c,
        unlockArray = 0xDA56EC,
        expArray = 0xDA5834,
        ammoArray = 0xDA5240,
        vidComics = 0xda650b,
        quickSelectPause = 0xC1E652,
        ccHelpDesk = 0x148A100,
        vidComicMenu = 0xC4F918,
        klunkTuning1 = 0xC9165C,
        klunkTuning2 = 0xC36BCC,
        neffyTuning = 0xEF6098,
        fastLoad1 = 0x134EBD4,
        fastLoad2 = 0x134EE70,
        levelFlags = 0xECE675
    }

    self.planets = {
        { 1, "Veldin" },
        { 2, "Florana" },
        { 3, "Starship Phoenix" },
        { 4, "Marcadia" },
        { 5, "Daxx" },
        { 6, "Phoenix Rescue" },
        { 7, "Annihilation Nation" },
        { 8, "Aquatos" },
        { 9, "Tyhrranosis" },
        { 10, "Zeldrin Starport" },
        { 11, "Obani Gemini" },
        { 12, "Blackwater City" },
        { 13, "Holostar" },
        { 14, "Koros" },
        { 16, "Metropolis" },
        { 17, "Crash Site" },
        { 18, "Aridia" },
        { 19, "Qwark's Hideout" },
        { 20, "Launch Site" },
        { 21, "Obani Draco" },
        { 22, "Command Center" },
        { 23, "Holostar Clank" },
        { 24, "Insomniac Museum" },
        { 26, "Metropolis Rangers" },
        { 27, "Aquatos Clank" },
        { 28, "Aquatos Sewers" },
        { 29, "Tyhrranosis Rangers" },
        { 31, "Vid-Comic 1" },
        { 33, "Vid-Comic 2" },
        { 34, "Vid-Comic 3" },
        { 32, "Vid-Comic 4" },
        { 35, "Vid-Comic 5" },
        { 30, "Vid-Comic 6" },
        { 36, "Vid-Comic 1 Special Edition" }
    }
    
    self.items = {
        Item("Heli Pack", 0x02, 0x4AA, 0x5F8, 1),
        Item("Thruster Pack", 0x03, 0x4AB, 0x5FC, 1),
        Item("Bolt Grabber v2", 0x07, 0x4AF, 0x60C, 1),
        Item("Bomb Glove", 0x0A, 0x4B2, 0x618, 1),
        Item("Hypershot", 0x0B, 0x4B3, 0x61C, 1),
        Item("Gravity Boots", 0x0D, 0x4B5, 0x624, 1),
        Item("Grindboots", 0x0E, 0x4B6, 0x628, 1),
        Item("Plasma Coil", 0x10, 0x4B8, 0x630, 8),
        Item("Lava Gun", 0x11, 0x4B9, 0x634, 8),
        Item("Refractor", 0x12, 0x4BA, 0x638, 1),
        Item("Bouncer", 0x13, 0x4BB, 0x63C, 8),
        Item("The Hacker", 0x14, 0x4BC, 0x640, 1),
        Item("Miniturret", 0x15, 0x4BD, 0x644, 8),
        Item("Shield Charger", 0x16, 0x4BE, 0x648, 8),
        Item("The Hacker", 0x1A, 0x4C2, 0x658, 1),
        Item("Grindboots", 0x1C, 0x4C4, 0x660, 1),
        Item("Charge Boots", 0x1D, 0x4C5, 0x664, 1),
        Item("Tyhrra Guise", 0x1E, 0x4C6, 0x668, 1),
        Item("Warp Pad", 0x1F, 0x4C7, 0x66C, 1),
        Item("Nano Pak", 0x20, 0x4C8, 0x670, 1),
        Item("Master Plan", 0x22, 0x4CA, 0x678, 1),
        Item("PDA", 0x23, 0x4CB, 0x67C, 1),
        Item("Shock Blaster", 0x27, 0x4CF, 0x68C, 8),
        Item("N60 Storm", 0x2F, 0x4D7, 0x6AC, 8),
        Item("Infector", 0x37, 0x4DF, 0x6CC, 8),
        Item("Annihilator", 0x3F, 0x4E7, 0x6EC, 8),
        Item("Spitting Hydra", 0x47, 0x4EF, 0x70C, 8),
        Item("Disc Blade Gun", 0x4F, 0x4F7, 0x72C, 8),
        Item("Agents of Doom", 0x57, 0x4FF, 0x74C, 8),
        Item("Rift Inducer", 0x5F, 0x507, 0x76C, 8),
        Item("Holoshield", 0x67, 0x50F, 0x78C, 8),
        Item("Flux Rifle", 0x6F, 0x517, 0x7AC, 8),
        Item("Nitro Launcher", 0x77, 0x51F, 0x7CC, 8),
        Item("Plasma Whip", 0x7F, 0x527, 0x7EC, 8),
        Item("Suck Cannon", 0x87, 0x52F, 0x80C, 8),
        Item("Quack-O-Ray", 0x8F, 0x537, 0x82C, 8),
        Item("R3YNO", 0x97, 0x53F, 0x84C, 5)
    }
end

function Game:SavePosition(savedPositionIndex)
    local position = {
        x = Target:ReadFloat(self.address.position.x),
        y = Target:ReadFloat(self.address.position.y),
        z = Target:ReadFloat(self.address.position.z)
    }

    Settings:SetFloat("SavedPositions." .. self:GetCurrentPlanet() .. "." .. savedPositionIndex .. ".x", position.x)
    Settings:SetFloat("SavedPositions." .. self:GetCurrentPlanet() .. "." .. savedPositionIndex .. ".y", position.y)
    Settings:SetFloat("SavedPositions." .. self:GetCurrentPlanet() .. "." .. savedPositionIndex .. ".z", position.z)
end

function Game:LoadPosition(savedPositionIndex)
    local position = {
        x = Settings:GetFloat("SavedPositions." .. self:GetCurrentPlanet() .. "." .. savedPositionIndex .. ".x", 0),
        y = Settings:GetFloat("SavedPositions." .. self:GetCurrentPlanet() .. "." .. savedPositionIndex .. ".y", 0),
        z = Settings:GetFloat("SavedPositions." .. self:GetCurrentPlanet() .. "." .. savedPositionIndex .. ".z", 0)
    }

    Target:WriteFloat(self.address.position.x, position.x)
    Target:WriteFloat(self.address.position.y, position.y)
    Target:WriteFloat(self.address.position.z, position.z)
end

function Game:Kill()
    Target:WriteMemory(self.address.position.z, 0xC2480000)
end

function Game:GetBolts()
    return Target:ReadInt(self.address.boltCount)
end

function Game:SetBolts(count)
    Target:WriteMemory(self.address.boltCount, count)
end

function Game:GetCurrentPlanet()
    return Target:ReadInt(self.address.currentPlanet)
end

function Game:LoadPlanet(index)
    Target:WriteMemory(self.address.destinationPlanet, index)
    Target:WriteMemory(self.address.shouldLoadPlanet, 1)
end

function Game:LoadAsideFile()
    Target:WriteByte(0xD9FF01, 1)
end

function Game:SetAsideFile()
    Target:WriteByte(0xD9FF02, 1)
end

function Game:ToggleInfiniteAmmo(toggle)
    if (toggle) then
        Target:WriteUInt(0x182A88, 0x60000000)  -- Write NOPs
    else
        Target:WriteUInt(0x182A88, 0x7c85312e)  -- Restore original instruction
    end
end

function Game:ToggleInfiniteHealth(toggle)
    if (toggle) then
        infHealthMemSubID = Target:FreezeMemory(self.address.playerHealth, 200)
    else
        Target:ReleaseSubID(infHealthMemSubID)
    end
end

function Game:ToggleQuickSelect()
    local quickSelectOn = Target:ReadByte(self.address.quickSelectPause) == 1
    
    if (quickSelectOn) then
        Target:WriteByte(self.address.quickSelectPause, 0)
    else
        Target:WriteByte(self.address.quickSelectPause, 1)
    end
end

function SetQuickSelectPause(toggle)
    if (toggle) then
        Target:WriteByte(self.address.quickSelectPause, 0)
    else
        Target:WriteByte(self.address.quickSelectPause, 1)
    end
end

function Game:ToggleKlunkTuneFreeze(toggle)
    if (toggle) then
        klunkTuning1MemSubID = Target:FreezeMemory(self.address.klunkTuning1, 0)
        klunkTuning2MemSubID = Target:FreezeMemory(self.address.klunkTuning2, 0)
    else
        Target:ReleaseSubID(klunkTuning1MemSubID)
        Target:ReleaseSubID(klunkTuning2MemSubID)
    end
end

function Game:ToggleGhostRatchet(toggle)
    if (toggle) then
        ghostRatchetSubID = Target:FreezeMemory(self.address.ghostTimer, 10)
    else
        Target:ReleaseSubID(ghostRatchetSubID)
    end
end 

function Game:SetVidComic(number, enabled)
    Target:WriteByte(self.address.vidComics + number, enabled and 1 or 0)
end

function Game:GetVidComic(number)
    return Target:ReadByte(self.address.vidComics + number) == 1
end

function Game:ResetAllTitaniumBolts()
    Target:Memset(self.address.titaniumBoltsArray, 128, 0)
end

function Game:GiveAllTitaniumBolts()
    Target:Memset(self.address.titaniumBoltsArray, 128, 1)
end

function Game:ResetAllSkillpoints()
    Target:Memset(self.address.skillPointsArray, 30, 0)
end

function Game:GiveAllSkillpoints()
    Target:Memset(self.address.skillPointsArray, 30, 1)
end

function Game:GetChallengeMode()
    return Target:ReadInt(self.address.challengeMode)
end

function Game:SetChallengeMode(mode)
    Target:WriteMemory(self.address.challengeMode, mode)
end

function Game:SetArmor(armor)
    Target:WriteShort(self.address.currentArmor, armor)
end

function Game:SetupNoQEFile()
    Target:WriteMemory(self.address.klunkTuning1, 0x7)
    Target:WriteMemory(self.address.klunkTuning2, 0x3)
    Target:WriteMemory(self.address.neffyTuning, 0xE)
    Target:WriteMemory(self.address.vidComicMenu, 2)
    Target:WriteMemory(self.address.ccHelpDesk, 1)
    
    Target:Notify("Klunk, Neffy, Vid Comic Menu and CC Helpdesk is now setup for runs")
end

function Game:SetShipColor(number)
    Target:WriteShort(self.address.shipColor, number)
end

function SetHealthXP(exp)
    Target:WriteMemory(self.address.healthXP, exp)
end

function SetPlayerHealth(health)
    Target:WriteMemory(self.address.playerHealth, health)
end

function SetupNGOrNoQEFile()
    self:SetupNGWeapons()
    self:SetBolts(1561120)
    
    self:SetQuickSelectPause(false)
    
    self:SetHealthXP(50000000)
    self:SetPlayerHealth(200)
    
    -- In-game time
    Target:WriteUInt(0xDA64E0, 2228300)
    
    self:SetArmor(4)
    
    self:SetChallengeMode(13)
    Target:Notify("Set up weapons, armor and health for NG+ categories! Setup bolts, IGT and challenge mode for QE!")
end

function Game:SetAllItemExp(start, range, exp)
    Item:SetAllItemExp(start, range, exp)
end

function Game:GetItemByName(name)
    for _, item in ipairs(self.items) do
        if (item.name == name) then
            return item
        end
    end
end

function Game:SetupNGWeapons()
    local ryno = self:GetItemByName("R3YNO")
    ryno:ToggleUnlock(true)
    ryno:SetVersion(4)
    ryno:SetExp(2560000)

    self:SetAllItemExp(364, 363, 2880000)
    
    local neededItems = {"Miniturret", "Shield Charger", "Shock Blaster", "Rift Inducer", "Flux Rifle", "Plasma Coil",
                    "Nitro Launcher", "Plasma Whip", "Suck Cannon", "PDA", "Charge Boots", "Nano Pak",
                    "Heli Pack", "Thruster Pack"}
    
    for _, itemName in ipairs(neededItems) do
        local item = self:GetItemByName(itemName)
        item:ToggleUnlock(true)
        item:SetVersion(item.levels)
    end
    
    local agents = self:GetItemByName("Agents of Doom")
    agents:ToggleUnlock(true)
    agents:SetVersion(1)
    self:SetAllItemExp(0, 363, 0)
    agents:SetExp(0)
end

function Game:EquipBombGlove()
    local bomb = self:GetItemByName("Bomb Glove")
    local heldWeaponAddrs = { self.address.HeldItemFoo, self.address.HeldItemBar, self.address.HeldItemBaz }
    for _, heldWeaponAddr in ipairs(heldWeaponAddrs) do
        Target:WriteByte(heldWeaponAddr, bomb.index)
    end

    Target:WriteByte(0xDA526B, 40)
end