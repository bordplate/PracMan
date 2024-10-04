BaseWindow = class("BaseWindow")

function BaseWindow:initialize(isMainWindow)
    Module:CreateWindow(self, isMainWindow)
end

function BaseWindow:SetTitle(title)
    self.native_window:SetTitle(title)
end

function BaseWindow:Show()
    self.native_window:Show()
end

function BaseWindow:Close()
    self.native_window:Close()
end

function BaseWindow:AddColumn(callback)
    column = self.native_window:AddColumn()
    callback(column)
    
    return column
end