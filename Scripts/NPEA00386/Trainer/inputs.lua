function OnLoad()
    buttonsOffset = 0x147A430
    analogROffset = 0x147A60C
    analogLOffset = 0x147A60C + 8

    buttons = 0
    analogLX = 0
    analogLY = 0
    analogRX = 0
    analogRY = 0

    buttonsBitmask = {
        l2 = 0x1,
        r2 = 0x2,
        l1 = 0x4,
        r1 = 0x8,
        triangle = 0x10,
        circle = 0x20,
        cross = 0x40,
        square = 0x80,
        select = 0x100,
        l3 = 0x200,
        r3 = 0x400,
        start = 0x800,
        up = 0x1000,
        right = 0x2000,
        down = 0x4000,
        left = 0x8000,
    }

    buttonMaskSubID = Target:SubMemory(buttonsOffset, 4, function(value)
        buttons = BytesToUInt(value)
    end)

    analogRSubID = Target:SubMemory(analogROffset, 8, function(value)
        analogRX = BytesToFloat(value, 4)
        analogRY = BytesToFloat(value)
    end)

    analogLSubID = Target:SubMemory(analogLOffset, 8, function(value)
        analogLX = BytesToFloat(value, 4)
        analogLY = BytesToFloat(value)
    end)
end

function GetInputs()
    return NewInputState(
            (buttons & buttonsBitmask.cross) > 0,
            (buttons & buttonsBitmask.circle) > 0,
            (buttons & buttonsBitmask.triangle) > 0,
            (buttons & buttonsBitmask.square) > 0,
            (buttons & buttonsBitmask.up) > 0,
            (buttons & buttonsBitmask.down) > 0,
            (buttons & buttonsBitmask.left) > 0,
            (buttons & buttonsBitmask.right) > 0,
            (buttons & buttonsBitmask.l1) > 0,
            (buttons & buttonsBitmask.l2) > 0,
            (buttons & buttonsBitmask.l3) > 0,
            (buttons & buttonsBitmask.r1) > 0,
            (buttons & buttonsBitmask.r2) > 0,
            (buttons & buttonsBitmask.r3) > 0,
            (buttons & buttonsBitmask.start) > 0,
            (buttons & buttonsBitmask.select) > 0,
            analogLX,
            analogLY,
            analogRX,
            analogRY
    )
end

function OnUnload()

end