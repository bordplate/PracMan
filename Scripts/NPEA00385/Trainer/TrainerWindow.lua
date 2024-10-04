require 'Game'
require 'UnlocksWindow'

TrainerWindow = class("TrainerWindow", BaseWindow)

function TrainerWindow:initialize()
    BaseWindow.initialize(self, true)
    self.game = Game()
    
    self.unlocksWindow = nil
end

function TrainerWindow:OnLoad()
    self:SetTitle("Ratchet & Clank 1 (PAL)")
    
    AddMenu("Debug", function(menu)
        menu:AddItem("Shoit", function()
            Alert("Shoit")
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
        
        column:AddLabel("Load Planet:")
        column:AddRow(function(row)
            local dropdown = row:AddDropdown(self.game:Planets(), function(index, value)
                selectedPlanet = index
            end)
            
            dropdown:SetSelectedIndex(self.game:GetCurrentPlanet())
            
            row:AddButton("Load", function()
                self.game:LoadPlanet(selectedPlanet, resetLevelFlagsCheck:IsChecked(), resetGoldBoltsCheck:IsChecked())
            end)
        end)
        
        resetLevelFlagsCheck = column:AddCheckbox("Reset Level Flags", function(value)
        end)
        resetGoldBoltsCheck = column:AddCheckbox("Reset Gold Bolts", function(value)
        end)
        
        column:AddButton("Unlocks", function()
            if self.unlocksWindow == nil then
                self.unlocksWindow = UnlocksWindow(self.game)
            end
            
            self.unlocksWindow:Show()
        end)
    end)
    
    self:AddColumn(function(column)
        column:AddLabel("Bolt count:")
        
        column:AddTextField(function(value)
            local count = tonumber(value)

            if count == nil then
                alert("Bolt count must be a number")
                return
            end
            
            self.game:SetBoltCount(count)
        end)
        
        column:AddButton("Turn on Drek Skip", function()
            self.game:SetDrekSkip(true)
        end)
        
        column:AddSpacer()
        
        column:AddButton("Reset all Gold Bolts", function()
            self.game:ResetAllGoldBolts()
        end)
        
        column:AddButton("Unlock all Gold", function()
            self.game:UnlockAllGoldBolts()
        end)
        
        column:AddSpacer()

        column:AddButton("Reset Shoot SPs", function()
            self.game:SetShootSkillPoints(true)
        end)

        column:AddButton("Setup Shoot SPs", function()
            self.game:SetShootSkillPoints(false)
        end)
        
        column:AddSpacer()

        column:AddButton("Setup All Missions", function()
            self.game:SetupAllMissions()
        end)
        
        column:AddCheckbox("Infinite Health", function(value)
        end)

        column:AddCheckbox("Ghost Ratchet", function(value)
            self.game:SetGhostRatchet(value)
        end)
        
        local goodies = column:AddCheckbox("Goodies Menu", function(value)
            self.game:SetGoodies(value)
        end)
        goodies:SetChecked(self.game:GoodiesMenuEnabled())
        
        column:AddCheckbox("Fast Loads", function(value)
            self.game:SetFastLoads(value)
        end)
        
        column:AddCheckbox("Freeze Ammo", function(value)
        end)
    end)
end 
