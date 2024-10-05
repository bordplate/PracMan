SelectorWindow = class("TrainerWindow", BaseWindow)

function SelectorWindow:initialize()
    BaseWindow.initialize(self, true)
end 

function SelectorWindow:OnLoad()
    self:SetTitle("Select game")
    
    self:AddColumn(function(column)
        column:AddButton("Ratchet & Clank 1", function()
            LoadModule("NPEA00385", "Trainer")
            SetTitleID("NPEA00385")
            Exit()
        end)

        column:AddButton("Ratchet & Clank 2", function()
            LoadModule("NPEA00386", "Trainer")
            SetTitleID("NPEA00386")
            Exit()
        end)
        
        column:AddButton("Ratchet & Clank 3", function()
            LoadModule("NPEA00387", "Trainer")
            SetTitleID("NPEA00387")
            Exit()
        end)
    end)
end 