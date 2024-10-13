require 'middleclass'
require 'BaseWindow'
require 'ScriptWindow'
require 'bit'

function table.contains(table, element)
    for _, value in pairs(table) do
        if value == element then
            return true
        end
    end
    return false
end


-- Used for memory subscriptions and memory freezing
MemoryCondition = {
    Any = 1,
    Changed = 2,
    Above = 3,
    Below = 4,
    Equal = 5,
    NotEqual = 6
}
