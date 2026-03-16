# AI Bridge

English | [中文](./README_CN.md)

File-based communication framework between AI Code assistants and Unity Editor.

## Features

- **GameObject** - Create, destroy, find, rename, duplicate, toggle active
- **Transform** - Position, rotation, scale, parent hierarchy, look at
- **Component/Inspector** - Get/set properties, add/remove components
- **Scene** - Load, save, get hierarchy, create new
- **Prefab** - Instantiate, save, unpack, apply overrides
- **Asset** - Search, import, refresh, find by filter
- **Editor Control** - Compile, undo/redo, play mode, focus window
- **ET Framework** - HybridCLR hotfix DLL compilation (equivalent to F6 shortcut)
- **Screenshot & GIF** - Capture game view, record animated GIFs
- **Batch Commands** - Execute multiple commands efficiently
- **Runtime Extension** - Custom handlers for Play mode

## Why AI Bridge? (vs Unity MCP)

| Feature | AI Bridge | Unity MCP |
|---------|-----------|-----------|
| Communication | File-based | WebSocket |
| During Unity Compile | **Works normally** | Connection lost |
| Port Conflicts | None | May cause reconnection failure |
| Multi-Project Support | **Yes** | No |
| Stability | **High** | Affected by compile/restart |
| Context Usage | **Low** | Higher |
| Extensibility | Simple interface | Requires MCP protocol knowledge |

**The Problem with MCP**: Unity MCP uses persistent WebSocket connections. When Unity recompiles (which happens frequently during development), the connection breaks. Port conflicts can also prevent reconnection, leading to a frustrating experience.

**AI Bridge Solution**: By using file-based communication, AI Bridge completely avoids these issues. Commands are written as JSON files and results are read back - simple, stable, and reliable regardless of Unity's state.

## Overview

AI Bridge enables AI coding assistants (like Claude, GPT, etc.) to communicate with Unity Editor through a simple file-based protocol. This allows AI to:

- Create and manipulate GameObjects
- Modify Transforms and Components
- Load and save Scenes
- Capture screenshots and GIF recordings
- Execute menu items
- And much more...

## Installation

### Via Unity Package Manager

1. Open Unity Package Manager (Window > Package Manager)
2. Click "+" > "Add package from git URL"
3. Enter: `https://github.com/liyingsong99/AIBridge.git`

### Manual Installation

1. Download or clone this repository
2. Copy the entire folder to your Unity project's `Packages` folder

## Requirements

- Unity 2021.3 or later
- .NET 6.0 Runtime (for CLI tool)
- Newtonsoft.Json (com.unity.nuget.newtonsoft-json)

## Package Structure

```
cn.lys.aibridge/
├── package.json
├── README.md
├── Editor/
│   ├── cn.lys.aibridge.Editor.asmdef
│   ├── Core/
│   │   ├── AIBridge.cs              # Main entry point
│   │   ├── CommandWatcher.cs        # File watcher for commands
│   │   └── CommandQueue.cs          # Command processing queue
│   ├── Commands/
│   │   ├── ICommand.cs              # Command interface
│   │   ├── CommandRegistry.cs       # Command registration
│   │   └── ...                      # Various command implementations
│   ├── Models/
│   │   ├── CommandRequest.cs        # Request model
│   │   └── CommandResult.cs         # Result model
│   └── Utils/
│       ├── AIBridgeLogger.cs        # Logging utility
│       └── ComponentTypeResolver.cs  # Component type resolution
├── Runtime/
│   ├── cn.lys.aibridge.Runtime.asmdef
│   ├── AIBridgeRuntime.cs           # MonoBehaviour singleton for runtime
│   ├── AIBridgeRuntimeData.cs       # Runtime data classes
│   └── IAIBridgeHandler.cs          # Extension interface
└── Tools~/
    ├── CLI/
    │   └── AIBridgeCLI.exe          # Command line tool
    ├── AIBridgeCLI/                 # CLI source code
    └── Exchange/
        ├── commands/                # Command files written here
        ├── results/                 # Result files returned here
        └── screenshots/             # Screenshots saved here
```

## Usage

### Editor Mode

AI Bridge automatically starts when Unity Editor opens. Commands are processed from `AIBridgeCache/commands/`.

#### Menu Items
- `AIBridge/Process Commands Now` - Process pending commands immediately
- `AIBridge/Toggle Auto-Processing` - Enable/disable automatic command processing

### CLI Tool

The CLI tool (`AIBridgeCLI.exe`) provides a command-line interface for sending commands.

```bash
# Show help
AIBridgeCLI --help

# Send a log message
AIBridgeCLI editor log --message "Hello from AI!"

# Create a GameObject
AIBridgeCLI gameobject create --name "MyCube" --primitiveType Cube

# Set transform position
AIBridgeCLI transform set_position --path "MyCube" --x 1 --y 2 --z 3

# Get scene hierarchy
AIBridgeCLI scene get_hierarchy

# Get prefab hierarchy
AIBridgeCLI prefab get_hierarchy --prefabPath "Assets/Prefabs/Player.prefab"

# Capture screenshot
AIBridgeCLI screenshot game

# Record GIF
AIBridgeCLI screenshot gif --frameCount 60 --fps 20

# Record GIF with delayed start
AIBridgeCLI screenshot gif --frameCount 60 --fps 20 --startDelay 0.5
```

### Available Commands

| Command | Description |
|---------|-------------|
| `editor` | Editor operations (log, undo, redo, play mode, etc.) |
| `compile` | Compilation operations (unity, dotnet, **et_hotfix**) |
| `gameobject` | GameObject operations (create, destroy, find, etc.) |
| `transform` | Transform operations (position, rotation, scale, parent) |
| `inspector` | Component/Inspector operations |
| `selection` | Selection operations |
| `scene` | Scene operations (load, save, hierarchy) |
| `prefab` | Prefab operations (instantiate, inspect, save, unpack) |
| `asset` | AssetDatabase operations |
| `menu_item` | Invoke Unity menu items |
| `get_logs` | Get Unity console logs |
| `batch` | Execute multiple commands |
| `screenshot` | Capture screenshots and GIF recordings |
| `focus` | Bring Unity Editor to foreground (CLI-only) |

### Runtime Extension

For runtime (Play mode) support, add `AIBridgeRuntime` component to your scene:

```csharp
// Option 1: Add via code
if (AIBridgeRuntime.Instance == null)
{
    var go = new GameObject("AIBridgeRuntime");
    go.AddComponent<AIBridgeRuntime>();
}

// Option 2: Add via Inspector
// Create empty GameObject and add AIBridgeRuntime component
```

#### Implementing Custom Handlers

```csharp
using AIBridge.Runtime;

public class MyCustomHandler : IAIBridgeHandler
{
    public string[] SupportedActions => new[] { "my_action", "another_action" };

    public AIBridgeRuntimeCommandResult HandleCommand(AIBridgeRuntimeCommand command)
    {
        switch (command.Action)
        {
            case "my_action":
                // Handle the command
                return AIBridgeRuntimeCommandResult.FromSuccess(command.Id, new { result = "success" });

            case "another_action":
                // Handle another command
                return AIBridgeRuntimeCommandResult.FromSuccess(command.Id);

            default:
                return null; // Not handled
        }
    }
}

// Register the handler
AIBridgeRuntime.Instance.RegisterHandler(new MyCustomHandler());
```

## Command Protocol

Commands are JSON files placed in `AIBridgeCache/commands/`:

```json
{
    "id": "cmd_123456789",
    "type": "gameobject",
    "params": {
        "action": "create",
        "name": "MyCube",
        "primitiveType": "Cube"
    }
}
```

Results are returned in `AIBridgeCache/results/`:

```json
{
    "id": "cmd_123456789",
    "success": true,
    "data": {
        "name": "MyCube",
        "instanceId": 12345,
        "path": "MyCube"
    },
    "executionTime": 15
}
```

## ET Framework Integration

AI Bridge supports [ET Framework](https://github.com/egametang/ET) projects with a dedicated hotfix DLL compilation command — equivalent to the **F6** shortcut in Unity Editor.

Unlike the standard `compile unity` command (which triggers Unity's incremental script compilation), `compile et_hotfix` runs the full ET hotfix DLL build pipeline:

1. Refreshes assets
2. Sets up asmdef files based on `CodeMode` (Client / Server / ClientServer)
3. Compiles scripts to `Temp/Bin/Debug/` via `PlayerBuildInterface.CompilePlayerScripts`
4. XOR-encodes the output DLLs
5. Copies them to `Assets/Bundles/Code/`

### Setup

**Step 1**: Copy `MCPBridgeCommand.cs` into your Unity project:

```
Assets/Scripts/Editor/Assembly/MCPBridgeCommand.cs
```

Source: [`Templates~/ET/MCPBridgeCommand.cs`](./Templates~/ET/MCPBridgeCommand.cs)

> This file is NOT auto-installed. Copy it manually once into your ET project.

**Step 2**: Install AI Bridge as a package (see [Installation](#installation)).

**Step 3**: Open Unity — the CLI will be auto-copied to `AIBridgeCache/CLI/`.

### Usage

```bash
# Compile ET hotfix DLLs (same as F6 in Unity Editor)
# --timeout 300000 = 5 minutes, adjust based on project size
AIBridgeCache/CLI/AIBridgeCLI.exe compile et_hotfix --timeout 300000 --raw
```

**Success response:**
```json
{
  "success": true,
  "data": {
    "success": true,
    "duration": 45.20,
    "errorCount": 0,
    "warningCount": 3
  }
}
```

**Failure response:**
```json
{
  "success": false,
  "error": "ET hotfix compile failed with 2 error(s). Check Unity console for details.",
  "data": {
    "success": false,
    "duration": 12.50,
    "errorCount": 2,
    "warningCount": 1,
    "errors": [
      "Assets/Scripts/Hotfix/...: error CS0103: The name 'xxx' does not exist"
    ]
  }
}
```

### How It Works

`CompileCommand` (AIBridge) uses reflection to locate and call `ET.MCPBridgeCommand.CompileHotfix()` in the `Unity.Editor` assembly. This avoids any direct assembly dependency between AIBridge and your ET project.

```
AIBridgeCLI.exe compile et_hotfix
    → CompileCommand.RunETHotfixCompile()          [AIBridge]
        → Reflection: ET.MCPBridgeCommand.CompileHotfix()  [Unity.Editor]
            → AssemblyTool.DoCompile()             [ET project]
                → PlayerBuildInterface.CompilePlayerScripts()
                → XOR encode + copy to Assets/Bundles/Code/
            → Returns JSON result string
        → Parse JSON → CommandResult
```

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
