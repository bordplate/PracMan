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
        shipColour = 0xDA55E8,
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
    return Target:ReadByte(self.address.currentPlanet)
end

function Game:LoadPlanet(index)
    Target:WriteMemory(self.address.destinationPlanet, index)
    Target:WriteMemory(self.address.shouldLoadPlanet, 1)
end