local script = {}

script.name = "doorsToStatic"
script.displayName = "Doors to Static Doors"

script.tooltip = "Converts vanilla doors to Frost Helper Static Doors"

script.parameters = {
    openSfx = "",
    closeSfx = "",
    lightOccludeAlpha = 1.0
}

script.fieldOrder = {
    "openSfx", "closeSfx", "lightOccludeAlpha"
}

function script.run(room, args)
    for _, entity in ipairs(room.entities) do
        if entity._name == "door" then
            entity._name = "FrostHelper/StaticDoor"
            entity.openSfx = args.openSfx
            entity.closeSfx = args.closeSfx
            entity.lightOccludeAlpha = args.lightOccludeAlpha
            --print(require("utils").serialize(entity))
        end
    end
end

return script