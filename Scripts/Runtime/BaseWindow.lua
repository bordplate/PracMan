BaseWindow = class("BaseWindow")

function BaseWindow:initialize(isMainWindow)
    self.native_window = Module:CreateWindow(self.class.name, isMainWindow)
    self.native_window:SetLuaContext(self)
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