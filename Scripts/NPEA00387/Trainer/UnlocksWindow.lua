UnlocksWindow = class('UnlocksWindow', BaseWindow)


function UnlocksWindow:initialize(game)
    self.game = game

    BaseWindow.initialize(self)

    self:Show()
end

function UnlocksWindow:OnLoad()
    self:SetTitle("Unlocks")

    checkboxes = {}
    weaponLevels = {}
    
    self:AddColumn(function(column)
        column:AddLabel("Unlocks")
        
        column:AddButton("Unlock all", function()
            for _, checkbox in ipairs(checkboxes) do
                checkbox:SetChecked(true, true)
            end
        end)
        
        column:AddButton("Remove all", function()
            for _, checkbox in ipairs(checkboxes) do
                checkbox:SetChecked(false, true)
            end
        end)
        
        column:AddRow(function(row) 
            itemColumn1 = row:AddColumn()
            itemColumn2 = row:AddColumn()
        end)

        for index, item in ipairs(self.game.items) do
            local row = index % 2 == 1 and itemColumn1 or itemColumn2
            
            checkboxes[#checkboxes+1] = row:AddCheckbox(item.name, function(value)
                item:ToggleUnlock(value)
            end)
            checkboxes[#checkboxes].Checked = item:IsUnlocked()
        end
        
        column:AddButton("Setup NG+ weapons", function()
            self.game:SetupNGWeapons()
        end)
        
        column:AddButton("Equip Bomb Glove", function()
            self.game:EquipBombGlove()
        end)
    end)
    
    self:AddColumn(function(column)
        column:AddLabel("Levelling")
        
        column:AddButton("Max all", function()
            for _, weaponItem in ipairs(weaponLevels) do
                local item = weaponItem[1]
                local dropdown = weaponItem[2]
                
                dropdown:SetSelectedIndex(item.levels, true)
            end
        end)
        
        column:AddButton("v1 all", function()
            Item:SetAllItemExp(0, 726, 0)
            
            for _, weaponItem in ipairs(weaponLevels) do
                local item = weaponItem[1]
                local dropdown = weaponItem[2]
                
                item:SetExp(0)
                
                dropdown:SetSelectedIndex(1, true)
            end
        end)
        
        for _, item in ipairs(self.game.items) do
            if item.levels <= 1 then
                goto continue_levelling_items
            end
            
            local levels = {}
            for i = 1, item.levels do
                levels[i] = i
            end
            
            column:AddRow(function(row)
                local dropdown = row:AddDropdown(levels, function(index, value)
                    item:SetVersion(index)
                end)
                dropdown:SetSelectedIndex(item:GetVersionHeuristic())
                
                row:AddLabel(item.name)

                weaponLevels[#weaponLevels+1] = {item, dropdown}
            end)
            
            ::continue_levelling_items::
        end
    end)
end 