Game = class('Game')


function Game:initialize()
    self.address = {
        bolts = 0x969CA0,
        position = {
            x = 0x969D60,
            y = 0x969D64,
            z = 0x969D68
        },
        currentPlanet = 0x969C70,
        shouldLoadPlanet = 0xA10700,
        planetTarget = 0xA10704,
        ghostTimer = 0x969EAC,
        goodiesMenu = 0x969CD3,
        playerHealth = 0x96BF88,
        playerState = 0x96BD64,
        planetFrameCount = 0xA10710,
        gameState = 0x00A10708,
        loadingScreenID = 0x9645C8,
        drekSkip = 0xFACC7B,
        goldItems = 0x969CA8,
        goldBolts = 0xA0CA34,
        debugUpdateOptions = 0x95c5c8,
        debugModeControl = 0x95c5d4,
        infobotFlags = 0x96CA0C,
        moviesFlags = 0x96BFF0,
        unlockArray = 0x96C140,
        inputOffset = 0x964AF0,
        analogOffset = 0x964A40,
        fastLoad = 0x9645CF,
        levelFlags = 0xA0CA84,
        miscLevelFlags = 0xA0CD1C,
    }
    
    self.planets = {
        { 0, "Veldin" },
        { 1, "Novalis" },
        { 2, "Aridia" },
        { 3, "Kerwan" },
        { 4, "Eudora" },
        { 6, "Blarg" },
        { 5, "Rilgar" },
        { 7, "Umbris" },
        { 8, "Batalia" },
        { 9, "Gaspar" },
        { 10, "Orxon" },
        { 11, "Pokitaru" },
        { 12, "Hoven" },
        { 13, "Gemlik" },
        { 14, "Oltanis" },
        { 15, "Quartu" },
        { 16, "Kalebo III" },
        { 17, "Drek's Fleet" },
        { 18, "Veldin 2" }
    }
    
    self.unlocks = {
        HeliPack = { "Heli-Pack", 2 },
        ThrusterPack = { "Thruster-Pack", 3 },
        HydroPack = { "Hydro-Pack", 4 },
        SonicSummoner = { "Sonic Summoner", 5 },
        O2Mask = { "O2 Mask", 6 },
        PilotsHelmet = { "Pilots Helmet", 7 },
        Wrench = { "Wrench", 8 },
        SuckCannon = { "Suck Cannon", 9 },
        BombGlove = { "Bomb Glove", 10 },
        Devastator = { "Devastator", 11 },
        Swingshot = { "Swingshot", 12 },
        Visibomb = { "Visibomb", 13 },
        Taunter = { "Taunter", 14 },
        Blaster = { "Blaster", 15 },
        Pyrocitor = { "Pyrocitor", 16 },
        MineGlove = { "Mine Glove", 17 },
        Walloper = { "Walloper", 18 },
        TeslaClaw = { "Tesla Claw", 19 },
        GloveOfDoom = { "Glove Of Doom", 20 },
        MorphORay = { "Morph-O-Ray", 21 },
        Hydrodisplacer = { "Hydrodisplacer", 22 },
        RYNO = { "RYNO", 23 },
        DroneDevice = { "Drone Device", 24 },
        DecoyGlove = { "Decoy Glove", 25 },
        Trespasser = { "Trespasser", 26 },
        MetalDetector = { "Metal Detector", 27 },
        Magneboots = { "Magneboots", 28 },
        Grindboots = { "Grindboots", 29 },
        Hoverboard = { "Hoverboard", 30 },
        Hologuise = { "Hologuise", 31 },
        PDA = { "PDA", 32 },
        MapOMatic = { "Map-O-Matic", 33 },
        BoltGrabber = { "Bolt Grabber", 34 },
        Persuader = { "Persuader", 35 }
    }
    
    self.ghostRatchetSubID = -1
end

function Game:Planets()
    return self.planets
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

function Game:GetCurrentPlanet()
    return Target:ReadInt(self.address.currentPlanet)
end

function Game:LoadPlanet(index, resetLevelFlags, resetGoldBolts)
    if resetLevelFlags then
        self:ResetLevelFlags()
    end
    
    if resetGoldBolts then
        self:ResetGoldBolts(index)
    end
    
    Target:WriteMemory(self.address.planetTarget, index)
    Target:WriteMemory(self.address.shouldLoadPlanet, 1)
end

function Game:SetBoltCount(count)
    Target:WriteMemory(self.address.bolts, count)
end

function Game:Kill()
    Target:WriteMemory(self.address.position.z, 0xC2480000)
end

function Game:SetGhostRatchet(toggle)
    if toggle then
        self.ghostRatchetSubID = Target:FreezeMemory(self.address.ghostTimer, 10)
    else
        Target:ReleaseSubID(self.ghostRatchetSubID)
    end
end

function Game:SetFastLoads(toggle)
    if toggle then
        Target:WriteMemory(0x0DF254, 0x60000000)
        Target:WriteMemory(0x165450, 0x2C03FFFF)
    else
        Target:WriteMemory(0x0DF254, 0x40820188)
        Target:WriteMemory(0x165450, 0x2c030000)
    end
end

function Game:SetInfiniteHealth(toggle)
    if toggle then
        Target:WriteMemory(0x96BF88, 0x30640000)
    else
        Target:WriteMemory(0x96BF88, 0x30649CE0)
    end
end

function Game:SetInfiniteAmmo(toggle)
    if toggle then
        Target:WriteMemory(0xAA2DC, 0x60000000)
    else
        Target:WriteMemory(0xAA2DC, 0x7D05392E)
    end
end

function Game:SetDrekSkip(toggle)
    Target:WriteMemory(self.address.drekSkip, toggle and 1 or 0)
end

function Game:SetUnlock(item, unlocked, gold)
    Target:WriteByte((gold and self.address.goldItems or self.address.unlockArray) + item[2], unlocked and 1 or 0)
end

function Game:HasUnlock(item, gold)
    return Target:ReadByte((gold and self.address.goldItems or self.address.unlockArray) + item[2]) == 1
    
    --return gold and self.ownedGoldItems[item[2]] or self.ownedUnlocks[item[2]]
end

function Game:GetUnlocks()
    local unlocks = {}
    
    for _, item in pairs(self.unlocks) do
        table.insert(unlocks, item)
    end
    
    return unlocks
end

function Game:ResetLevelFlags()
    local planetToLoad = self:GetCurrentPlanet()
    
    Target:Memset(self.address.levelFlags + (planetToLoad * 0x10), 0x10, 0)
    Target:Memset(self.address.miscLevelFlags + (planetToLoad * 0x100), 0x100, 0)
    Target:Memset(self.address.infobotFlags + planetToLoad, 1, 0)
    Target:Memset(self.address.moviesFlags, 0xC0, 0)
    
    if planetToLoad == 3 then
        Target:Memset(0x96C378, 0xF0, 0)
        self:SetUnlock(self.unlocks.HeliPack, false)
        self:SetUnlock(self.unlocks.Swingshot, false)
    end
    
    if planetToLoad == 4 then
        Target:Memset(0x96C468, 0x40, 0)
        self:SetUnlock(self.unlocks.SuckCannon, false)
    end
    
    if planetToLoad == 5 then
        Target:Memset(0x96C498, 0xA0, 0)
    end
    
    if planetToLoad == 6 then
        self:SetUnlock(self.unlocks.Grindboots, false)
    end
    
    if planetToLoad == 8 then
        Target:Memset(0x96C5A8, 0x40, 0)
    end
    
    if planetToLoad == 9 then
        Target:Memset(0x96C5E8, 0x20, 0)
        self:SetUnlock(self.unlocks.PilotsHelmet, false)
    end
    
    if planetToLoad == 10 then
        self:SetUnlock(self.unlocks.Magneboots, false)
        
        if self:HasUnlock(self.unlocks.O2Mask) then
            Target:WriteMemory(self.address.infobotFlags + 11, 1)
        end
    end
    
    if planetToLoad == 11 then
        self:SetUnlock(self.unlocks.ThrusterPack, false)
        self:SetUnlock(self.unlocks.O2Mask, false)
    end
end

function Game:ResetGoldBolts(planetIndex)
    Target:WriteMemory(self.address.goldBolts + (planetIndex * 4), 0)
end

function Game:ResetAllGoldBolts()
    Target:Memset(self.address.goldBolts, 80, 0)
end

function Game:UnlockAllGoldBolts()
    Target:Memset(self.address.goldBolts, 80, 1)
end

function Game:GoodiesMenuEnabled()
    return Target:ReadBool(self.address.goodiesMenu)
end

function Game:SetGoodies(enabled)
    Target:WriteByte(self.address.goodiesMenu, enabled and 1 or 0)
end

function Game:SetShootSkillPoints(reset)
    if reset then
        Target:Memset(0x96C9DC, 32, 0)
        Target:Memset(0xA15F3C, 8, 0)
    else
        Target:WriteMemory(0x96C9DC, 32, "000000200000002000000020000000200000002000000200000002000000020")
        Target:WriteMemory(0xA15F3C, 8, "0000000100000001")
    end
end

function Game:SetupAllMissions()
    Target:WriteMemory(0xA0CD04, 0)
    
    Target:WriteMemory(0xE5EFD0, 0)
    Target:WriteMemory(0xE5EFD4, 0)
    Target:WriteMemory(0xE5EFD8, 0)
    
    Alert("Blarg bridge and rilgar race reset for any% all missions. Good luck!")
end 