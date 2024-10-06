ScriptWindow = class("ScriptWindow", BaseWindow)

function ScriptWindow:initialize(mainWindow)
    BaseWindow.initialize(self, mainWindow)
end

function ScriptWindow:OnLoad()
    self:SetTitle("Script")
    
    self:AddColumn(function(column)
        textField = column:AddTextArea(20)
        textField:SetMonospaced(true)
        textField:SetText("-- Write your Lua script here\nratchet.z = 100")
        
        column:AddRow(function(row)
            runButton = row:AddButton("Run", function()
                Execute(textField:GetText())
            end)
            row:AddButton("Load", function()
                textField:SetText(LoadFileFromDialog())
            end)
        end)
    end)
end

_scriptWindow = ScriptWindow(false)