UnlocksWindow = class("UnlocksWindow", BaseWindow)

function UnlocksWindow:initialize()
    BaseWindow.initialize(self, false)
    
    self.game = Game()
end

function UnlocksWindow:OnLoad()
    self:SetTitle("Unlocks")
    
    weaponCheckboxes = {}
    gadgetCheckboxes = {}
    itemCheckboxes = {}
    
    self:AddColumn(function(column)
        column:AddLabel("Weapons")
        
        column:AddRow(function(row)
            col1 = row:AddColumn()
            col2 = row:AddColumn()
        end)

        for index, weapon in ipairs(self.game.weapons) do
            local col = index % 2 == 0 and col2 or col1
            
            local checkbox = col:AddCheckbox(weapon.name, function(enable)
                weapon:ToggleUnlock(enable)
            end)
            
            checkbox:SetChecked(weapon:IsUnlocked())
            
            weaponCheckboxes[#weaponCheckboxes + 1] = checkbox
        end
        
        column:AddButton("Unlock All", function()
            for _, checkbox in ipairs(weaponCheckboxes) do
                checkbox:SetChecked(true, true)
            end
        end)
    end)
    
    self:AddColumn(function(column)
        column:AddLabel("Gadgets")
        
        column:AddSpacer()
        
        for _, gadget in ipairs(self.game.gadgets) do
            local checkbox = column:AddCheckbox(gadget.name, function(enable)
                gadget:ToggleUnlock(enable)
            end)
            
            checkbox:SetChecked(gadget:IsUnlocked())
            
            gadgetCheckboxes[#gadgetCheckboxes + 1] = checkbox
        end
        
        column:AddSpacer()
        
        column:AddButton("Unlock All", function()
            for _, checkbox in ipairs(gadgetCheckboxes) do
                checkbox:SetChecked(true, true)
            end
        end)
    end)

    self:AddColumn(function(column)
        column:AddLabel("Items")
        
        column:AddSpacer()
        
        for _, item in ipairs(self.game.items) do
            local checkbox = column:AddCheckbox(item.name, function(enable)
                item:ToggleUnlock(enable)
            end)
            
            checkbox:SetChecked(item:IsUnlocked())
            
            itemCheckboxes[#itemCheckboxes + 1] = checkbox
        end
        
        column:AddSpacer()
        
        column:AddButton("Unlock All", function()
            for _, checkbox in ipairs(itemCheckboxes) do
                checkbox:SetChecked(true, true)
            end
        end)
    end)
    
    self:AddColumn(function(column)
        column:AddButton("Unlock Everything", function()
            for _, checkbox in ipairs(weaponCheckboxes) do
                checkbox:SetChecked(true, true)
            end
            
            for _, checkbox in ipairs(gadgetCheckboxes) do
                checkbox:SetChecked(true, true)
            end
            
            for _, checkbox in ipairs(itemCheckboxes) do
                checkbox:SetChecked(true, true)
            end
        end)
        
        column:AddButton("Remove Everything", function()
            for _, checkbox in ipairs(weaponCheckboxes) do
                checkbox:SetChecked(false, true)
            end

            for _, checkbox in ipairs(gadgetCheckboxes) do
                checkbox:SetChecked(false, true)
            end

            for _, checkbox in ipairs(itemCheckboxes) do
                checkbox:SetChecked(false, true)
            end
        end)
    end)
end