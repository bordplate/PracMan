require 'Game'
require 'UnlocksWindow'
require 'FlagViewWindow'

TrainerWindow = class("TrainerWindow", BaseWindow)

function TrainerWindow:initialize()
    self.game = Game()

    BaseWindow.initialize(self, true)

    self:Show()
    
    self.unlocksWindow = nil
    self.flagViewWindow = nil
end

function TrainerWindow:OnLoad()
    self:SetTitle("Ratchet & Clank 3 (PAL)")
    
    AddMenu("Trainer", function(menu)
        menu:AddItem("Level flag viewer...", function()
            if self.flagViewWindow == nil then
                self.flagViewWindow = FlagViewWindow(self.game, self.game:GetCurrentPlanet(), 0x10)
            elseif self.flagViewWindow.level ~= self.game:GetCurrentPlanet() then
                self.flagViewWindow:Close()
                self.flagViewWindow = FlagViewWindow(self.game, self.game:GetCurrentPlanet(), 0x10)
            end
            
            self.flagViewWindow:Show()
        end)
    end)
    
    self:AddColumn(function(column)
        column:AddRow(function(row)
            row:AddLabel("Position:")

            row:AddDropdown({"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}, function(index, value)
                savedPositionIndex = index
            end)
        end)
        
        column:AddRow(function(row)
            row:AddButton("Save Position", function()
                self.game:SavePosition(savedPositionIndex)
            end)

            row:AddButton("Load Position", function()
                self.game:LoadPosition(savedPositionIndex)
            end)
        end)
        
        column:AddButton("Die", function()
            self.game:Kill()
        end)
        
        column:AddSpacer()
        
        column:AddRow(function(row)
            row:AddButton("Set aside file", function()
                LoadModule("NPEA00387", "rc3-save")
                self.game:SetAsideFile()
            end)
            
            row:AddButton("Load file", function()
                LoadModule("NPEA00387", "rc3-save")
                self.game:LoadAsideFile()
            end)
        end)
        
        column:AddSpacer()
        
        column:AddLabel("Load Planet:")
        column:AddRow(function(row)
            local dropdown = row:AddDropdown(self.game.planets, function(index, value)
                selectedPlanet = index
            end)
            
            dropdown:SetSelectedIndex(self.game:GetCurrentPlanet(), true)
            
            row:AddButton("Load", function()
                self.game:LoadPlanet(selectedPlanet)
            end)
        end)
        
        column:AddRow(function(row)
            row:AddColumn(function(col)
                col:AddLabel("Toggles:")
                
                col:AddCheckbox("Ghost Ratchet", function(value)
                    self.game:ToggleGhostRatchet(value)
                end)
                
                col:AddCheckbox("One-Hit KO", function(value)
                    if (value) then
                        ohkoMemSubID = Target:FreezeMemory(self.game.address.playerHealth, 3, 1)
                    else
                        Target:ReleaseSubID(ohkoMemSubID)
                    end
                end)
                
                col:AddCheckbox("Freeze Ammo", function(value)
                    self.game:ToggleInfiniteAmmo(value)
                end)
                
                col:AddCheckbox("Freeze Health", function(value)
                    self.game:ToggleInfiniteHealth(value)
                end)
                
                col:AddCheckbox("Show coordinates", function(value)
                    if (value) then
                        coordsSubID = Target:SubMemory(self.game.address.position.x, 8, function(value)
                            local x = BytesToFloat(value, 0)
                            local y = BytesToFloat(value, 4)
                            
                            xCoordsLabel:SetText(string.format("X: %.4f", x))
                            yCoordsLabel:SetText(string.format("Y: %.4f", y))
                        end)
                        
                        coordsZSubID = Target:SubMemory(self.game.address.position.z, 4, function(value)
                            local z = BytesToFloat(value, 0)
                            zCoordsLabel:SetText(string.format("Z: %.4f", z))
                        end)
                    else
                        Target:ReleaseSubID(coordsSubID)
                        Target:ReleaseSubID(coordsZSubID)
                        
                        xCoordsLabel:SetText("")
                        yCoordsLabel:SetText("")
                        zCoordsLabel:SetText("")
                    end
                end)
                
                col:AddCheckbox("Untune Klunk", function(value)
                    self.game:ToggleKlunkTuneFreeze(value)
                end)
            end)
            
            row:AddColumn(function(col)
                col:AddLabel("Vid Comics:")
                
                col:AddCheckbox("Vid Comic 1", function(value)
                    self.game:SetVidComic(1, value)
                end).Checked = self.game:GetVidComic(1)
                
                col:AddCheckbox("Vid Comic 2", function(value)
                    self.game:SetVidComic(2, value)
                end).Checked = self.game:GetVidComic(2)
                
                col:AddCheckbox("Vid Comic 3", function(value)
                    self.game:SetVidComic(3, value)
                end).Checked = self.game:GetVidComic(3)
                
                col:AddCheckbox("Vid Comic 4", function(value)
                    self.game:SetVidComic(4, value)
                end).Checked = self.game:GetVidComic(4)
                
                col:AddCheckbox("Vid Comic 5", function(value)
                    self.game:SetVidComic(5, value)
                end).Checked = self.game:GetVidComic(5)
            end)
        end)
        
        column:AddRow(function(row)
            row:AddButton("Unlocks", function()
                if self.unlocksWindow == nil then
                    self.unlocksWindow = UnlocksWindow(self.game)
                end
                
                self.unlocksWindow:Show()
            end)
            row:AddButton("Setup NG+ or No QE File", function()
                self.game:SetupNoQEFile()
            end)
        end)
        
        column:AddLabel("File Time:")
        column:AddTextField(function(value)
            LoadModule("NPEA00387", "rc3-save")
            
            local time = tonumber(value)
            
            if time == nil then
                Alert("File time must be a number")
                return
            end
            
            Target:WriteUInt(0xDA64E0, time)
        end)
    end)
    
    self:AddColumn(function(column)
        column:AddLabel("Bolt count:")
        
        local boltStepper = column:AddStepper(-INT_MAX, INT_MAX, 100, function(value)
            local count = tonumber(value)

            if count == nil then
                alert("Bolt count must be a number")
                return
            end

            self.game:SetBolts(count)
        end)
        boltStepper:SetValue(self.game:GetBolts())
        
        column:AddLabel("Challenge Mode:")
        local challengeModeStepper = column:AddStepper(-INT_MAX, INT_MAX, 1, function(value)
            self.game:SetChallengeMode(value)
        end)
        challengeModeStepper:SetValue(self.game:GetChallengeMode())
        
        column:AddLabel("Armor:")
        column:AddDropdown({
            "Alpha Combat Suit", "Magnaplate Armor", "Adamantine Armor", "Aegis Mark V Armor", "Infernox Armor", 
            "OG Ratchet Skin", "Snowman Skin", "Tux Skin",
        }, function(index, value)
            self.game:SetArmor(index-1)
        end)
        
        column:AddButton("Unlock All Titanium Bolts", function()
            self.game:GiveAllTitaniumBolts()
        end)
        column:AddButton("Reset All Titanium Bolts", function()
            self.game:ResetAllTitaniumBolts()
        end)
        
        column:AddSpacer()
        
        column:AddButton("Unlock All Skill Points", function()
            self.game:GiveAllSkillpoints()
        end)
        column:AddButton("Reset All Skill Points", function()
            self.game:ResetAllSkillpoints()
        end)
        
        column:AddSpacer()
        
        column:AddButton("Setup No QE", function()
            self.game:SetupNoQEFile()
        end)
        
        column:AddButton("Toggle QS Pause", function()
            self.game:ToggleQuickSelect()
        end)
        
        column:AddLabel("Ship Color:")
        column:AddDropdown({
            "Blargian Red", "Orxon Green", "Bogon Blue", "Insomniac Special", "Dark Nebula", "Drek's Black Heart",
            "Space Storm", "Lunar Eclipse", "Plaidtastic", "Supernova", "Solar Wind", "Clowner", "Silent Strike",
            "Lombax Orange", "Neutron Star", "Star Traveller", "Hooked on Onyx", "Tyhrranoid Void", "Zeldrin Sunset",
            "Ghost Pirate Purple", "Qwark Green", "Agent Orange", "Helga Hues", "Amoeboid Green", "Obani Orange",
            "Pulsing Purple", "Low Rider", "Black Hole", "Sun Storm", "Sasha Scarlet", "Florana Breeze", "Ozzy Kamikaze",
        }, function(index, value)
            self.game:SetShipColor(index-1)
            Target:Notify("Reload the planet to see the changes to the ship color")
        end)
        
        column:AddSpacer()
        
        xCoordsLabel = column:AddLabel("")
        yCoordsLabel = column:AddLabel("")
        zCoordsLabel = column:AddLabel("")
    end)
end 