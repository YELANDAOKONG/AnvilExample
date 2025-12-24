# AnvilASM (Example): Class File Inspector

This is a sample console application demonstrating the capabilities of the [AnvilASM](https://github.com/YELANDAOKONG/AnvilASM) library.

It acts as a CLI tool to inspect the internal structure of Java `.class` files, visualizing the **Constant Pool**, **Methods**, **Fields**, and **Attributes** (including StackMapTables and BootstrapMethods) in a readable format.

## 📦 Installation & Setup

**⚠️ Important: Manual Setup Required**

Since **AnvilASM** is currently under active development and **has not been published to NuGet**, you cannot simply `dotnet restore` from a standalone directory. You must ensure the project reference paths are valid.

### 1. Project Structure
The `AnvilExample` project expects the `AnvilASM` library to be located at `../AnvilASM/Anvil/Anvil.csproj`.

Your directory structure should look like this:

```text
/WorkSpace
│
├── /AnvilASM          <-- The Library Repository
│   └── /Anvil
│       └── Anvil.csproj
│
└── /AnvilExample      <-- This Example Project
    ├── AnvilExample.csproj
    ├── Program.cs
    └── ...
```

### 2. Steps to Run

1.  Clone the library repository:
    ```bash
    git clone https://github.com/YELANDAOKONG/AnvilASM.git
    ```
2.  Clone this example project (or place it) as a sibling directory to `AnvilASM`.
3.  Navigate to the example directory:
    ```bash
    cd AnvilExample
    ```
4.  Restore dependencies (this will fetch Spectre.Console and link the local Anvil library):
    ```bash
    dotnet restore
    ```

## 🚀 Usage

You can run the inspector using the `dotnet run` command.

### Basic Syntax
```bash
dotnet run -- [path-to-class-file] [options]
```

### Examples

**1. Inspect a specific class file (Modern UI):**
This uses [Spectre.Console](https://spectreconsole.net/) to render rich tables and trees.
```bash
dotnet run -- MyJavaClass.class
```

**2. Inspect using Classic Mode:**
If your terminal does not support ANSI colors or you want raw text output.
```bash
dotnet run -- MyJavaClass.class --classic
```

**3. Default Behavior:**
If no arguments are provided, the program attempts to read a file named `Test.class` in the working directory.
```bash
dotnet run
```

## ✨ Features

This example demonstrates how to traverse the AnvilASM object model:

*   **Header Parsing**: Magic numbers, Java versions, and Access Flags.
*   **Constant Pool Resolution**: Resolves complex indices (e.g., `MethodRef` -> `NameAndType` -> `Utf8`) into human-readable strings.
*   **Attribute Inspection**: Detailed breakdown of complex attributes like:
    *   `Code` (MaxStack, MaxLocals).
    *   `StackMapTable` (Frame types and verification types).
    *   `BootstrapMethods` (Invokedynamic resolution).
    *   `Module` (Exports, Requires, Opens).
    *   `Record` components.
    *   `Annotations` (RuntimeVisible/Invisible).

## 🛠️ Code Overview

*   **`Program.cs`**: Entry point handling CLI arguments and error reporting.
*   **`ClassInspector.cs`**: The "Modern" implementation. It shows how to use `Spectre.Console` to render the `Anvil.Structures.ClassFile` object model into interactive grids and trees.
*   **`ClassicClassInspector.cs`**: A fallback implementation using standard `System.Console`. It demonstrates how to access the raw data properties of the library without external UI dependencies.

## 📄 License

This example is licensed under the same terms as the parent **AnvilASM** project (Apache License 2.0).

---

