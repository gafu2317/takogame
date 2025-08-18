# CLAUDE.md
回答は日本語でお願いします
必要のないログは削除してください

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 6000.2.0f1 project for developing a 2D vertical scrolling wire-action endless game. The game features a circular player character who escapes from rising magma using wire-grappling mechanics while climbing as high as possible.

## Build and Development Commands

Since this is a Unity project, development is primarily done through the Unity Editor. Common operations:

- **Open Project**: Launch Unity Hub and open this project directory
- **Build Project**: File → Build Settings → Build (or Build and Run)
- **Play Mode**: Press Play button in Unity Editor or Ctrl/Cmd+P
- **Stop Play Mode**: Press Play button again or Ctrl/Cmd+P

## Project Structure

### Key Directories
- `Assets/` - All game assets and scripts
  - `Scenes/` - Game scenes (currently contains SampleScene.unity)
  - `Settings/` - Render pipeline and project settings
- `ProjectSettings/` - Unity project configuration files
- `Packages/` - Package dependencies (see manifest.json)
- `docs/` - Project documentation including requirements specification

### Important Files
- `docs/要件定義.md` - Complete game design document and requirements (in Japanese)
- `Packages/manifest.json` - Package dependencies including 2D tools, URP, Input System
- `ProjectSettings/ProjectVersion.txt` - Unity version information

## Architecture and Game Design

### Core Game Systems (from requirements)
1. **Player Controller**: Circular character with ground movement, jumping, and wire mechanics
2. **Wire System**: Single wire with auto-retraction, wall attachment, and physics-based movement
3. **Camera System**: Player-following camera with vertical scrolling
4. **Magma System**: Rising threat with acceleration over time
5. **Stage Generation**: Combination of pre-made patterns

### Input Controls
- Mouse cursor: Wire aiming
- Mouse click: Wire shooting
- A/D: Ground movement
- W/A/S/D: Wall climbing when attached
- Space: Jump
- Z: Release wall attachment

### Development Phases (Priority Order)
1. **Phase 1**: Basic movement (ground, jump, gravity), wire system, camera following
2. **Phase 2**: Magma scrolling system, death mechanics, basic stage generation
3. **Phase 3**: Stage patterns, UI/score system, effects
4. **Phase 4**: Balance tuning and polish

## Package Dependencies

Key packages for this 2D action game:
- Universal Render Pipeline (URP) 17.2.0 for 2D rendering
- 2D Animation/Sprite packages for character graphics
- Input System 1.14.1 for modern input handling
- Physics2D module for collision detection and wire physics

## Development Notes

- Project is set up for 2D development with URP
- Target platform: PC with 16:9 aspect ratio
- Uses Unity's new Input System
- Designed for endless/arcade-style gameplay
- Focus on responsive controls and physics-based wire mechanics

## Testing

Run the game in Unity Editor Play Mode to test mechanics. The main scene is `Assets/Scenes/SampleScene.unity`.