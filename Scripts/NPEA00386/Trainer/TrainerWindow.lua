require 'Game'
require 'UnlocksWindow'
require 'FlagViewWindow'

TrainerWindow = class("TrainerWindow", BaseWindow)

function TrainerWindow:initialize()
    BaseWindow.initialize(self, true)
    
    self.game = Game()
    
    self.unlocksWindow = nil
end

function TrainerWindow:OnLoad()
    self:SetTitle("Ratchet & Clank 2 (PAL)")

    AddMenu("Trainer", function(menu)
        menu:AddItem("Level flag viewer...", function()
            if self.flagViewWindow == nil then
                self.flagViewWindow = FlagViewWindow(self.game:GetCurrentPlanet(), 0x10)
            elseif self.flagViewWindow.level ~= self.game:GetCurrentPlanet() then
                self.flagViewWindow:Close()
                self.flagViewWindow = FlagViewWindow(self.game, self.game:GetCurrentPlanet(), 0x10)
            end

            self.flagViewWindow:Show()
        end)
    end)
    
    AddMenu("Debug", function(menu)
        menu:AddCheckItem("Enable debug mode", function(value)
            self.game:ToggleDebugFeatures(value)
        end)
        
        menu:AddItem("Activate QE...", function()
            QEInputWindow():Show()
        end)
    end)
    
    self:AddColumn(function(column)
        column:AddRow(function(row)
            row:AddLabel("Position:")
            
            row:AddDropdown({"1", "2", "3", "4", "5", "6", "7", "8", "9", "10"}, function(index, value)
                savedPositionIndex = index
            end)
            savedPositionIndex = 1
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
            self.game:Die()
        end)
        
        column:AddSpacer()
        
        column:AddRow(function(row)
            row:AddButton("Set Aside File", function()
                LoadModule("NPEA00386", "rc2-save")
                self.game:SetAsideFile()
            end)
            
            row:AddButton("Load Aside File", function()
                LoadModule("NPEA00386", "rc2-save")
                self.game:LoadAsideFile()
            end)
        end)
        
        column:AddSpacer()
        
        column:AddLabel("Load Planet:")
        column:AddRow(function(row)
            row:AddDropdown(self.game.planets, function(index, value)
                selectedPlanet = index
            end):SetSelectedIndex(self.game:GetCurrentPlanet(), true)
            
            row:AddButton("Load", function()
                self.game:LoadPlanet(selectedPlanet, resetFlagsCheckbox.Checked)
            end)
        end)
        column:AddRow(function(row)
            row:AddCheckbox("Fast Loads", function(value)
                self.game:ToggleFastLoads(value)
            end)
            resetFlagsCheckbox = row:AddCheckbox("Reset Flags", function(value) end)
        end)
        
        column:AddSpacer()
        
        column:AddLabel("File Setup")
        column:AddRow(function(row)
            row:AddButton("Unlocks", function()
                if self.unlocksWindow == nil then
                    self.unlocksWindow = UnlocksWindow()
                end
                
                self.unlocksWindow:Show()
            end)
            row:AddButton("Store Swingshot", function()
                self.game:StoreSwingshot()
            end)
        end)
        column:AddCheckbox("Enable weapon insta-upgrades", function(value)
            if value then
                expEconomySubID = Target:FreezeByte(self.game.address.expEconomy, MemoryCondition.Changed, 100);
            else
                Target:ReleaseSubID(expEconomySubID)
                Target:WriteByte(self.game.address.expEconomy, 0)
            end
        end)
        column:AddSpacer()
        column:AddCheckbox("Show Coordinates", function(value)
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
        
        column:AddSpacer()
        
        xCoordsLabel = column:AddLabel("")
        yCoordsLabel = column:AddLabel("")
        zCoordsLabel = column:AddLabel("")
    end)
    
    self:AddColumn(function(column)
        column:AddRow(function(row)
            row:AddColumn(function(column1)
                column1:AddLabel("Challenge Mode:")
                column1:AddStepper(-INT_MAX, INT_MAX, 1, function(value)
                    self.game:SetChallengeMode(value)
                end)
                
                column1:AddLabel("Raritanium:")
                column1:AddStepper(-INT_MAX, INT_MAX, 1, function(value)
                    self.game:SetRaritanium(value)
                end)
            end)
            
            row:AddColumn(function(column1)
                column1:AddLabel("Bolts Count:")
                column1:AddStepper(-INT_MAX, INT_MAX, 100, function(value)
                    self.game:SetBoltCount(value)
                end)
                
                column1:AddLabel("Health XP:")
                column1:AddStepper(0, UINT_MAX, 100, function(value)
                    self.game:SetHealthXP(value)
                end)
            end)
        end)

        column:AddButton("Unlock all Platinum Bolts", function()
            self.game:UnlockAllPlatinumBolts()
        end)
        
        column:AddButton("Reset Platinum Bolts", function()
            self.game:ResetAllPlatinumBolts()
        end)
        
        column:AddSpacer()
        
        freezeAmmoCheckbox = column:AddCheckbox("Infinite Ammo", function(value)
            self.game:ToggleInfiniteAmmo(value)
        end)
        
        column:AddCheckbox("Freeze Health", function(value)
            if value then
                healthFreezeSubID = Target:FreezeMemory(self.game.address.playerHealth, 42069)
            else
                Target:ReleaseSubID(healthFreezeSubID)
            end
        end)
        
        column:AddCheckbox("Ghost Ratchet", function(value)
            if value then
                ghostRatchetSubID = Target:FreezeMemory(self.game.address.ghostTimer, 10)
            else
                Target:ReleaseSubID(ghostRatchetSubID)
            end
        end)
        
        column:AddSpacer()
        
        column:AddLabel("Category Setup")
        column:AddRow(function(row)
            row:AddColumn(function(col)
                col:AddButton("NG+ No IMG", function()
                    self.game:SetupNGNoIMG()
                end)
                col:AddButton("NG+ All Missions", function()
                    self.game:SetupAllMissions()
                end)
            end)
            
            row:AddColumn(function(col)
                col:AddButton("Any%", function()
                    self.game:SetupAnyPercent()
                end)
                col:AddButton("NG+", function()
                    self.game:SetupNGPlus()
                end)
            end)
        end)
        
        column:AddSpacer()
        
        column:AddButton("Reset menu storage", function()
            self.game:ResetMenuStorage()
            Target:Notify("Reset menu storage!")
        end)
        
        column:AddCheckbox("Auto-reset menus", function(value)
            if value then
                autoResetMenusSubID = Target:SubMemory(self.game.address.loadingScreenCount, 1, function(value)
                    if self.game:GetCurrentPlanet() == 0 then
                        self.game:ResetMenuStorage()
                        freezeAmmoCheckbox:SetChecked(false, true)
                    end
                end)
            else
                Target:ReleaseSubID(autoResetMenusSubID)
            end
        end)
    end)
end

QEInputWindow = class("QEInputWindow", BaseWindow)

function QEInputWindow:initialize()
    BaseWindow.initialize(self, false)
    
    self.game = Game()
end

function QEInputWindow:OnLoad()
    self:AddColumn(function(column)
        self.inputField = column:AddStepper(-INT_MAX, INT_MAX, 1, function(value)
            self.okButton:Activate()
        end)
        
        column:AddRow(function(row)
            self.okButton = row:AddButton("Ok", function()
                self.game:SetSaveWriteOffset(self.inputField:GetValue())
                
                self:Close()
            end)
            
            row:AddButton("Cancel", function() 
                self:Close()
            end)
        end)
    end)
end 