# Flappy Bird (Windows Forms C#)

A desktop recreation of the classic retro Flappy Bird game built from the ground up using modern .NET and C# Windows Forms. Features smooth physics dynamics, flicker-free rendering with double buffering, and comprehensive keyboard/mouse input support.

---

## 📋 Table of Contents
1. [Project Name and Brief Description](#1-project-name-and-brief-description)
2. [Technologies Used / NuGet Packages](#2-technologies-used--nuget-packages)
3. [Installation and Running Steps](#3-installation-and-running-steps)
4. [Key Features of the Application](#4-key-features-of-the-application)
5. [Screenshots](#5-screenshots)
6. [Known Limitations / Future Improvements](#6-known-limitations--future-improvements)
7. [Student Information](#7-student-information)

---

## 1. Project Name and Brief Description
- **Project Name:** Flappy Bird
- **Description:** A single-player arcade game developed using Windows Forms and the GDI+ vector graphics engine. The goal is to navigate the bird through randomly generated green pipes by jumping against gravity to achieve the highest possible score.

---

## 2. Technologies Used / NuGet Packages
- **Programming Language:** C# 11+
- **Runtime Environment:** .NET (Windows Forms / Windows Desktop SDK)
- **Graphics Engine:** System.Drawing (GDI+ Hardware-assisted Double-Buffered Vector Rendering)
- **NuGet Packages:** **None** (No third-party packages required). The application utilizes native, built-in .NET Desktop libraries for optimal portability and performance.

---

## 3. Installation and Running Steps

### Prerequisites
- Windows 10 / 11 Operating System
- [.NET SDK](https://dotnet.microsoft.com/download) (or Visual Studio 2022+)

### Step-by-Step Instructions
1. **Clone the Repository:**
   ```bash
   git clone https://github.com/mustafahaccar18-art/YMH219-FlappyBird.git
   cd YMH219-FlappyBird
   ```

2. **Build the Project:**
   ```bash
   dotnet build -c Release
   ```

3. **Run the Application:**
   ```bash
   dotnet run
   ```
   *(Alternatively, you can open `FlappyBird.csproj` in Visual Studio and press `F5` / `Ctrl+F5` to start).*

---

## 4. Key Features of the Application
- 🎮 **Universal Input Controls:** **Any key on the keyboard** or any **mouse click** triggers the bird's jump (`KeyPreview` & `MouseDown`).
- 🕊️ **Realistic Bird Physics & Animation:** Smooth gravity acceleration, dynamic nose-tilt angle (tilting downwards during free-fall and upwards during jumps), and a 3-frame wing-flapping animation.
- 🌵 **Procedural Pipe Generation:** Obstacle pipes generated with randomized gap heights, fitted with custom gradient shading and decorative rim caps.
- 🏆 **Live Score HUD & High Score:** Real-time on-screen score display and best score tracking preserved across attempts in the active session.
- 💥 **Accurate Collision Detection (Hitboxes):** Strict bounding-box collision detection against the ceiling, ground, and pipe bodies/caps.
- 🖥️ **Smooth 60 FPS Rendering:** High-performance, tear-free rendering powered by `DoubleBuffered` mode and a 60 FPS `System.Windows.Forms.Timer`.
- 🎖️ **Game Over & Medal Reward System:** Informative Game Over summary card awarding performance medals (🥇 Gold, 🥈 Silver, 🥉 Bronze) based on the final score.

---

## 5. Screenshots

| Screenshot 1: Starting Screen & Flight | Screenshot 2: Navigating Pipes & Live Score |
| :---: | :---: |
| <img src="screenshots/screenshot1.png" alt="Flappy Bird Flight" width="360"/> | <img src="screenshots/screenshot2.png" alt="Flappy Bird Gameplay" width="360"/> |

---

## 6. Known Limitations / Future Improvements
- **Sound Effects:** Audio effects (jump whoosh, point ding, and collision impact) are currently omitted; they can be integrated in future revisions using `System.Media.SoundPlayer`.
- **Persistent High Score:** Best score is stored in RAM during the session; persistent file/JSON or registry storage can be added for cross-session retention.
- **Difficulty Scaling:** Pipe speed and gap size remain steady; dynamic speed increments over time could provide an additional challenge curve.

---

## 7. Student Information
- **Student Full Name:** Mustafa Haccar
- **Student Number:** `[Please Insert Your Student ID Here]`
