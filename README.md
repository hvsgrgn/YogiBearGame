# Yogi Bear Game 🐻🧺

A grid-based stealth game made with **C# / .NET 8** and **Avalonia UI**. Guide Yogi Bear through the forest, collect every picnic basket, and don't let the rangers spot you.

## How to play

- Yogi starts in the **top-left corner**. Move him with the **arrow keys**, one cell at a time, horizontally or vertically.
- Step on a **picnic basket** to collect it. Collect them all to **win**.
- **Trees** and **bushes** are obstacles that you can't walk through.
- **Rangers** patrol back and forth in a straight line. They take one step every second and turn around when they hit an obstacle or the edge of the forest.
- A ranger can see the **3×3 area around them**, diagonals included. If a ranger sees Yogi, you **lose**.

## Features

- Load any map from a text file. Three sizes are included: 8×8, 10×10 and 15×15.
- **Pause and resume.** While the game is paused, time stops and Yogi can't move.
- A status bar shows the **elapsed time** and the **number of baskets collected**.
- A pop-up tells you when you've won or lost, and a Rules dialog explains how to play.

## Architecture

The app uses the **MVVM** pattern:

| Layer | Project | Contents |
|---|---|---|
| **Model** | `YogiBearAvaloniaCore` | `GameModel` (game logic, timer, events), `YogiBear`, `Ranger` |
| **Persistence** | `YogiBearAvaloniaCore` | `IDataAccess` and the async `FileDataAccess` map loader, `GameBoard` |
| **ViewModel** | `YogiBearAvalonia` | `GameViewModel`, `CellViewModel`, `RelayCommand` |
| **View** | `YogiBearAvalonia` | `MainWindow.axaml` |
| **Desktop host** | `YogiBearAvalonia.Desktop` | the app's entry point |

The timer sits behind an `ITimer` interface, so the unit tests in `YogiBearAvaloniaTests` can use a `MockTimer`.

The full documentation, with use case and class diagrams, is in `Beadandó feladat dokumentáció.pdf`. It's written in Hungarian.

## Map file format

```
<size> <basketCount> <rangerCount>
<x> <y> <dirX> <dirY>        ← one line per ranger
<size lines of size numbers> ← the board: 0 = empty, 1 = tree, 2 = bush, 3 = basket
```

Example maps are in `tests/`: `map_small.txt`, `map_medium.txt` and `map_big.txt`. There's also `map_faulty.txt` for testing load errors.

## Running it

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```
dotnet run --project YogiBearAvalonia.Desktop
```

You can also open `YogiBearAvaloniaViewModel.sln` in Visual Studio or Rider. Choose **Game → New Game** and pick one of the map files from `tests/`.

### Tests

```
dotnet test
```

## About

Made as a university assignment at ELTE (2025).
