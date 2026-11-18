# 🐦 Flappy Bird — C# / .NET WinForms

A fully playable **Flappy Bird** clone built with **C# 11** and **Windows Forms on .NET 10**.

## Features

- 60 fps game loop with double-buffered rendering
- Animated bird with wing flap, tilt, eye, beak & blush
- Procedural pipes with random gap positions & heights
- Two-layer parallax scrolling background (clouds + ground)
- Increasing difficulty (speed ramps up over time)
- Score & Best Score across rounds
- Game states: Menu → Playing → Game Over → Retry
- Pause support (`P` key)

## Controls

| Key / Action | Effect |
|---|---|
| `Space` / Left Click | Flap |
| `P` | Pause / Resume |
| `Esc` | Quit |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (Windows only — uses WinForms)

## Run

```bash
git clone https://github.com/YOUR_USERNAME/FlappyBird.git
cd FlappyBird
dotnet run -c Release
```

## Project Structure

| File | Purpose |
|---|---|
| `Program.cs` | Entry point |
| `GameForm.cs` | Game loop, rendering, HUD, input |
| `Bird.cs` | Physics, drawing, collision |
| `Pipe.cs` | Pipe logic, rendering |
| `Background.cs` | Parallax sky, clouds, ground |

## License

MIT
