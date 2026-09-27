# Fireclams

Vintage Story 1.22 code mod for growing Nurru pearls in Kallu clams.

## Use

Place a clay Kallu clam directly beside lava, with one block between it and the lava, or diagonally one block up or down and one block to the side of lava. Then right click it with one vanilla `game:metalbit-*` of gold, silver, electrum, black bronze, nickel, meteoric iron, copper, or uranium. Place any other Kallu clam in salt water, then right click it with a sand block of any type to grow a regular Nurru pearl. The clam closes and grows its pearl for 24 in-game hours. Removing the required liquid pauses the timer until it returns. When it opens, hold right click with any knife for 1.25 seconds to collect the pearl. The clam has an equal chance to disappear or return to its empty open state.

The empty clams come in seven shell colors in the creative inventory. Clams face the direction you are looking when placed and keep that orientation as they grow and are harvested. Holding right click with a knife to harvest shows the game's progress bar and plays the vanilla knife cutting animation. Hover over an empty clam to see a cycling list of accepted ingredients, or over a processing clam to see the remaining in-game hours and minutes. The processing deadline survives save and reload and advances with the game calendar.

Metal Nurru pearls can be ground in a quern into their matching Nurru powder at a 1:1 ratio. Regular Nurru pearls cannot be ground.

## Build

Set `VINTAGE_STORY` to the Vintage Story installation directory, then run:

```powershell
dotnet build Fireclams/Fireclams.csproj -c Release
```

The loadable mod directory is `Fireclams/bin/Release/Mods/mod`. You can add its parent folder with `--addModPath` for development or ZIP the directory contents for installation in the game's `Mods` folder.
