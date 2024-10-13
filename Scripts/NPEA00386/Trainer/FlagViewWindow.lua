FlagViewWindow = class("FlagViewWindow", BaseWindow)

function FlagViewWindow:initialize(level, rows)
    self.game = Game()
    self.level = level
    self.rows = rows
    
    self.baseAddr = self.game.address.levelFlags + (self.level * 0x10)
    
    self.checkboxes = {}
    self.subIds = {}
    
    BaseWindow.initialize(self)
end

function FlagViewWindow:OnLoad()
    self:AddColumn(function(column)
        for i = 0, self.rows - 1 do
            column:AddRow(function(row)
                self.checkboxes[i+1] = {}

                row:AddLabel(string.format("0x%X", self.baseAddr + i))
                for j = 0, 7 do
                    local box = row:AddCheckbox("", function(value)
                        local data = Target:ReadMemory(self.baseAddr + i, 1)
                        local toData = data[0]
                        local flag = bit.blshift(1, j)
                        if value then
                            toData = bit.bor(toData, flag)
                        else
                            toData = bit.band(toData, bit.bnot(flag))
                        end
                        Target:WriteMemory(self.baseAddr + i, {toData})
                    end)

                    self.checkboxes[i+1][j+1] = box
                end
            end)
        end
    end)
    
    for i = 0, self.rows - 1 do
        self.subIds[#self.subIds+1] = Target:SubMemory(self.baseAddr + i, 1, function(data)
            for j = 0, 7 do
                self.checkboxes[i+1][j+1]:SetChecked(bit.band(data[0], bit.blshift(1, j)) ~= 0)
            end
        end)
    end
    
    self:AddColumn(function(column)
        column:AddButton("Enable all", function()
            Target:Memset(self.baseAddr, 0xFF, self.rows)
            for i = 0, self.rows - 1 do
                for j = 0, 7 do
                    self.checkboxes[i+1][j+1]:SetChecked(true)
                end
            end
        end)
        
        column:AddButton("Disable all", function()
            Target:Memset(self.baseAddr, 0x00, self.rows)
            for i = 0, self.rows - 1 do
                for j = 0, 7 do
                    self.checkboxes[i+1][j+1]:SetChecked(false)
                end
            end
        end)
    end)
end

function FlagViewWindow:OnClose()
    for _, subId in ipairs(self.subIds) do
        Target:ReleaseSubID(subId)
    end
end