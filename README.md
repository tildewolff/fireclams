# Fireclams

Vintage Story 1.22 code mod for growing Nurru pearls in Kallu clams.

## Use

Place an empty Kallu clam. Right click it with one vanilla `game:metalbit-*` of gold, silver, electrum, black bronze, nickel, meteoric iron, copper, or uranium. The clam closes and grows a pearl for 24 in game hours. When it opens, hold right click with any knife for 1.25 seconds to collect the matching Nurru pearl. The clam has an equal chance to disappear or return to its empty open state.

The empty clams come in seven shell colors in the creative inventory. Hover over a processing clam to see the remaining in-game hours and minutes. The processing deadline survives save and reload and advances with the game calendar.

## Build

Set `VINTAGE_STORY` to the Vintage Story installation directory, then run:

```powershell
dotnet build Fireclams/Fireclams.csproj -c Release
```

The loadable mod directory is `Fireclams/bin/Release/Mods/mod`. You can add its parent folder with `--addModPath` for development or ZIP the directory contents for installation in the game's `Mods` folder.
