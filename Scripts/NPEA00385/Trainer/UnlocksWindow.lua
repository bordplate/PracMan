UnlocksWindow = class('UnlocksWindow', BaseWindow)


function UnlocksWindow:initialize(game)
    self.game = game
    
    BaseWindow.initialize(self)
    
    self:Show()
end

function UnlocksWindow:OnLoad()
    self:SetTitle("Unlocks")
    
    checkboxes = {}

    self:AddColumn(function(column)
        column:AddLabel("Item Unlocks")
        
        for i, unlock in pairs(self.game.unlocks) do
            checkboxes[#checkboxes+1] = column:AddCheckbox(unlock[1], function(value)
                self.game:SetUnlock(unlock, value, false)
            end)
            
            checkboxes[#checkboxes]:SetChecked(self.game:HasUnlock(unlock, false))
        end

        column:AddLabel("")
        column:AddCheckbox("Unlock all", function(value)
            for _, unlock in pairs(self.game.unlocks) do
                self.game:SetUnlock(unlock, value, false)

                for _, checkbox in pairs(checkboxes) do
                    checkbox:SetChecked(value)
                end
            end
        end)
    end)
    
    goldCheckboxes = {}
    
    self:AddColumn(function(column)
        column:AddLabel("Gold Weapons")
        
        for i, unlock in pairs(self.game.unlocks) do
            goldCheckboxes[#goldCheckboxes+1] = column:AddCheckbox(unlock[1], function(value)
                self.game:SetUnlock(unlock, value, true)
            end)

            goldCheckboxes[#goldCheckboxes]:SetChecked(self.game:HasUnlock(unlock, true))
        end
        
        column:AddLabel("")
        column:AddCheckbox("Unlock all", function(value)
            for _, unlock in pairs(self.game.unlocks) do
                self.game:SetUnlock(unlock, value, true)

                for _, checkbox in pairs(goldCheckboxes) do
                    checkbox:SetChecked(value)
                end
            end
        end)
    end)
end