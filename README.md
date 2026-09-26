# Fireclams

Vintage Story 1.22 code mod for growing Nurru pearls in Kallu clams.

## Use

Place a clay Kallu clam in lava, then right click it with one vanilla `game:metalbit-*` of gold, silver, electrum, black bronze, nickel, meteoric iron, copper, or uranium. Place any other Kallu clam in water (fresh or salt), then right click it with a sand block of any type to grow a regular Nurru pearl. The clam closes and grows its pearl for 24 in-game hours. Removing the required liquid pauses the timer until the liquid returns. When it opens, hold right click with any knife for 1.25 seconds to collect the pearl. The clam has an equal chance to disappear or return to its empty open state.

The empty clams come in seven shell colors in the creative inventory. Hover over a processing clam to see the remaining in-game hours and minutes. The processing deadline survives save and reload and advances with the game calendar.

## Build

Set `VINTAGE_STORY` to the Vintage Story installation directory, then run:

```powershell
dotnet build Fireclams/Fireclams.csproj -c Release
```

The loadable mod directory is `Fireclams/bin/Release/Mods/mod`. You can add its parent folder with `--addModPath` for development or ZIP the directory contents for installation in the game's `Mods` folder.
