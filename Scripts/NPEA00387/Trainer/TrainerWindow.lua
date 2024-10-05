require 'Game'
--require 'UnlocksWindow'

TrainerWindow = class("TrainerWindow", BaseWindow)

function TrainerWindow:initialize()
    self.game = Game()

    BaseWindow.initialize(self, true)

    self:Show()
end

function TrainerWindow:OnLoad()
    self:SetTitle("Ratchet & Clank 3 (PAL)")
    
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
                
            end)
            
            row:AddButton("Load file", function()
                
            end)
        end)
        
        column:AddSpacer()
        
        column:AddLabel("Load Planet:")
        column:AddRow(function(row)
            local dropdown = row:AddDropdown(self.game.planets, function(index, value)
                selectedPlanet = index
            end)
            
            dropdown:SetSelectedIndex(self.game:GetCurrentPlanet())
            
            row:AddButton("Load", function()
                self.game:LoadPlanet(selectedPlanet)
            end)
        end)
        
        column:AddRow(function(row)
            row:AddColumn(function(col)
                col:AddLabel("Toggles:")
                
                col:AddCheckbox("Ghost Ratchet", function(value)
                    self.game:SetGhostRatchet(value)
                end)
                
                col:AddCheckbox("One-Hit KO", function(value)
                    
                end)
                
                col:AddCheckbox("Freeze Ammo", function(value)
                    
                end)
                
                col:AddCheckbox("Freeze Health", function(value)
                    
                end)
                
                col:AddCheckbox("Show coordinates", function(value)
                    
                end)
                
                col:AddCheckbox("Untune Klunk", function(value)
                    
                end)
            end)
            
            row:AddColumn(function(col)
                col:AddLabel("Vid Comics:")
                
                col:AddCheckbox("Vid Comic 1", function(value)
                    
                end)
                col:AddCheckbox("Vid Comic 2", function(value)
                    
                end)
                col:AddCheckbox("Vid Comic 3", function(value)
                    
                end)
                col:AddCheckbox("Vid Comic 4", function(value)
                    
                end)
                col:AddCheckbox("Vid Comic 5", function(value)
                    
                end)
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
                
            end)
        end)
        
        column:AddLabel("File Time:")
        column:AddTextField(function(value)
            LoadModule("NPEA00387", "rc3-save")
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
        column:AddStepper(-INT_MAX, INT_MAX, 1, function(value)
            
        end)
        
        column:AddLabel("Armor:")
        column:AddDropdown({"None", "Carbonox", "Eclipse", "Hyperflux", "Quantum", "Reactive", "Terraflux"}, function(index, value)
            
        end)
        
        column:AddButton("Unlock All Titanium Bolts", function()
            
        end)
        column:AddButton("Reset All Titanium Bolts", function()
            
        end)
        
        column:AddSpacer()
        
        column:AddButton("Unlock All Skill Points", function()
            
        end)
        column:AddButton("Reset All Skill Points", function()
            
        end)
        
        column:AddSpacer()
        
        column:AddButton("Setup No QE", function()
            
        end)
        
        column:AddButton("Toggle QS Pause", function()
            
        end)
        
        column:AddLabel("Ship Color:")
        column:AddDropdown({"Default", "Black", "Blue", "Green", "Orange", "Pink", "Purple", "Red", "White", "Yellow"}, function(index, value)
            
        end)
    end)
end 