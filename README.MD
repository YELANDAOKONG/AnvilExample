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

**3. Show decoded bytecode and raw instruction bytes:**
```bash
dotnet run -- MyJavaClass.class --instructions
```

**4. Show the control-flow graph:**
```bash
dotnet run -- MyJavaClass.class --control-flow
```

**5. Show all advanced analysis:**
```bash
dotnet run -- MyJavaClass.class --all
```

**6. Default Behavior:**
If no arguments are provided, the program attempts to read a file named `Test.class` in the working directory.
```bash
dotnet run
```

### Options

| Option | Short | Description |
|---|---:|---|
| `--classic` | `-c` | Use plain-text output instead of Spectre.Console. |
| `--instructions` | `-i` | Show instruction offsets, raw bytes, opcodes, symbolic operands, and labels. |
| `--control-flow`, `--cfg` | `-g` | Show basic blocks and typed control-flow edges. |
| `--roundtrip` | `-r` | Rebuild each method and compare its bytecode. |
| `--all` | `-a` | Enable instructions, control flow, and round-trip analysis. |
| `--help` | `-h` | Show command-line help. |

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
*   **Detailed Bytecode**:
    *   Raw hexadecimal bytes for every instruction.
    *   Resolved field, method, type, constant, and invokedynamic operands.
    *   Complete `tableswitch` and `lookupswitch` case targets.
    *   String labels and exception-handler regions.
*   **Control-Flow Analysis**:
    *   Basic-block ranges and instruction counts.
    *   Entry, exit, reachable, and unreachable block states.
    *   Fall-through, conditional, unconditional, switch, and exception edges.
    *   Backward-edge detection for loop-oriented inspection.

## 🛠️ Code Overview

*   **`Program.cs`**: Entry point handling CLI arguments and error reporting.
*   **`ClassInspector.cs`**: The "Modern" implementation. It shows how to use `Spectre.Console` to render the `Anvil.Structures.ClassFile` object model into interactive grids and trees.
*   **`ClassicClassInspector.cs`**: A fallback implementation using standard `System.Console`. It demonstrates how to access the raw data properties of the library without external UI dependencies.
*   **`BytecodeFormatter.cs`**: Shared raw-byte and symbolic operand formatting.
*   **`ControlFlowGraphBuilder.cs`**: Builds basic blocks and typed edges from label-based `MethodBody` instructions.

## 📄 License

This example is licensed under the same terms as the parent **AnvilASM** project (Apache License 2.0).

---

## 💫 Example

Below is a sample output when running in **Classic Mode** (--classic). This mode generates a clean, structured text dump suitable for logging or piping into other tools.

```text
--------------------------------------------------
=== Class File Header ===
Magic:        0xCAFEBABE (Valid)
Version:      69.0 (Java 25)
Access Flags: Public, Super
This Class:   #8 (Test)
Super Class:  #2 (java/lang/Object)

=== Constant Pool (289 entries) ===
#1    = Methodref          #2.#3
#2    = Class              #4 // java/lang/Object
#3    = NameAndType        #5:#6 // <init>:()V
#4    = Utf8               "java/lang/Object"
#5    = Utf8               "<init>"
#6    = Utf8               "()V"
#7    = Fieldref           #8.#9
#8    = Class              #10 // Test
#9    = NameAndType        #11:#12 // isRunning:Z
#10   = Utf8               "Test"
#11   = Utf8               "isRunning"
#12   = Utf8               "Z"
#13   = Class              #14 // [[[D
#14   = Utf8               "[[[D"
#15   = Fieldref           #8.#16
#16   = NameAndType        #17:#14 // complexArray:[[[D
#17   = Utf8               "complexArray"
#18   = String             #19 // "Zero"
#19   = Utf8               "Zero"
#20   = String             #21 // "One"
#21   = Utf8               "One"
#22   = String             #23 // "Two"
#23   = Utf8               "Two"
#24   = String             #25 // "Hundred"
#25   = Utf8               "Hundred"
#26   = String             #27 // "Five Thousand"
#27   = Utf8               "Five Thousand"
#28   = String             #29 // "Unknown"
#29   = Utf8               "Unknown"
#30   = Class              #31 // java/io/IOException
#31   = Utf8               "java/io/IOException"
#32   = String             #33 // "System not running"
#33   = Utf8               "System not running"
#34   = Methodref          #30.#35
#35   = NameAndType        #5:#36 // <init>:(Ljava/lang/String;)V
#36   = Utf8               "(Ljava/lang/String;)V"
#37   = Fieldref           #38.#39
#38   = Class              #40 // java/lang/System
#39   = NameAndType        #41:#42 // out:Ljava/io/PrintStream;
#40   = Utf8               "java/lang/System"
#41   = Utf8               "out"
#42   = Utf8               "Ljava/io/PrintStream;"
#43   = String             #44 // "Cleanup executed"
#44   = Utf8               "Cleanup executed"
#45   = Methodref          #46.#47
#46   = Class              #48 // java/io/PrintStream
#47   = NameAndType        #49:#36 // println:(Ljava/lang/String;)V
#48   = Utf8               "java/io/PrintStream"
#49   = Utf8               "println"
#50   = Class              #51 // java/lang/NullPointerException
#51   = Utf8               "java/lang/NullPointerException"
#52   = Fieldref           #38.#53
#53   = NameAndType        #54:#42 // err:Ljava/io/PrintStream;
#54   = Utf8               "err"
#55   = Methodref          #56.#57
#56   = Class              #58 // java/lang/Exception
#57   = NameAndType        #59:#60 // getMessage:()Ljava/lang/String;
#58   = Utf8               "java/lang/Exception"
#59   = Utf8               "getMessage"
#60   = Utf8               "()Ljava/lang/String;"
#61   = InvokeDynamic      Bootstrap: #0, NameAndType: #62
#62   = NameAndType        #63:#64 // makeConcatWithConstants:(Ljava/lang/String;)Ljava/lang/String;
#63   = Utf8               "makeConcatWithConstants"
#64   = Utf8               "(Ljava/lang/String;)Ljava/lang/String;"
#65   = InvokeDynamic      Bootstrap: #1, NameAndType: #62
#66   = Class              #67 // java/lang/String
#67   = Utf8               "java/lang/String"
#68   = String             #69 // "Alice"
#69   = Utf8               "Alice"
#70   = String             #71 // "Bob"
#71   = Utf8               "Bob"
#72   = String             #73 // "Charlie"
#73   = Utf8               "Charlie"
#74   = Methodref          #75.#76
#75   = Class              #77 // java/util/Arrays
#76   = NameAndType        #78:#79 // asList:([Ljava/lang/Object;)Ljava/util/List;
#77   = Utf8               "java/util/Arrays"
#78   = Utf8               "asList"
#79   = Utf8               "([Ljava/lang/Object;)Ljava/util/List;"
#80   = InvokeDynamic      Bootstrap: #2, NameAndType: #81
#81   = NameAndType        #82:#83 // accept:()Ljava/util/function/Consumer;
#82   = Utf8               "accept"
#83   = Utf8               "()Ljava/util/function/Consumer;"
#84   = Methodref          #85.#86
#85   = Class              #87 // java/util/Objects
#86   = NameAndType        #88:#89 // requireNonNull:(Ljava/lang/Object;)Ljava/lang/Object;
#87   = Utf8               "java/util/Objects"
#88   = Utf8               "requireNonNull"
#89   = Utf8               "(Ljava/lang/Object;)Ljava/lang/Object;"
#90   = InvokeDynamic      Bootstrap: #3, NameAndType: #91
#91   = NameAndType        #82:#92 // accept:(Ljava/io/PrintStream;)Ljava/util/function/Consumer;
#92   = Utf8               "(Ljava/io/PrintStream;)Ljava/util/function/Consumer;"
#93   = InterfaceMethodref #94.#95
#94   = Class              #96 // java/util/List
#95   = NameAndType        #97:#98 // forEach:(Ljava/util/function/Consumer;)V
#96   = Utf8               "java/util/List"
#97   = Utf8               "forEach"
#98   = Utf8               "(Ljava/util/function/Consumer;)V"
#99   = InterfaceMethodref #94.#100
#100  = NameAndType        #101:#102 // stream:()Ljava/util/stream/Stream;
#101  = Utf8               "stream"
#102  = Utf8               "()Ljava/util/stream/Stream;"
#103  = InvokeDynamic      Bootstrap: #4, NameAndType: #104
#104  = NameAndType        #105:#106 // test:()Ljava/util/function/Predicate;
#105  = Utf8               "test"
#106  = Utf8               "()Ljava/util/function/Predicate;"
#107  = InterfaceMethodref #108.#109
#108  = Class              #110 // java/util/stream/Stream
#109  = NameAndType        #111:#112 // filter:(Ljava/util/function/Predicate;)Ljava/util/stream/Stream;
#110  = Utf8               "java/util/stream/Stream"
#111  = Utf8               "filter"
#112  = Utf8               "(Ljava/util/function/Predicate;)Ljava/util/stream/Stream;"
#113  = InterfaceMethodref #108.#114
#114  = NameAndType        #115:#116 // count:()J
#115  = Utf8               "count"
#116  = Utf8               "()J"
#117  = InvokeDynamic      Bootstrap: #5, NameAndType: #118
#118  = NameAndType        #63:#119 // makeConcatWithConstants:(J)Ljava/lang/String;
#119  = Utf8               "(J)Ljava/lang/String;"
#120  = InterfaceMethodref #94.#121
#121  = NameAndType        #122:#123 // iterator:()Ljava/util/Iterator;
#122  = Utf8               "iterator"
#123  = Utf8               "()Ljava/util/Iterator;"
#124  = InterfaceMethodref #125.#126
#125  = Class              #127 // java/util/Iterator
#126  = NameAndType        #128:#129 // hasNext:()Z
#127  = Utf8               "java/util/Iterator"
#128  = Utf8               "hasNext"
#129  = Utf8               "()Z"
#130  = InterfaceMethodref #125.#131
#131  = NameAndType        #132:#133 // next:()Ljava/lang/Object;
#132  = Utf8               "next"
#133  = Utf8               "()Ljava/lang/Object;"
#134  = Class              #135 // java/lang/Number
#135  = Utf8               "java/lang/Number"
#136  = Methodref          #134.#137
#137  = NameAndType        #138:#139 // doubleValue:()D
#138  = Utf8               "doubleValue"
#139  = Utf8               "()D"
#140  = InvokeDynamic      Bootstrap: #6, NameAndType: #141
#141  = NameAndType        #63:#142 // makeConcatWithConstants:(D)Ljava/lang/String;
#142  = Utf8               "(D)Ljava/lang/String;"
#143  = Class              #144 // java/lang/Math
#144  = Utf8               "java/lang/Math"
#145  = Double             6.283185307179586d
#147  = InvokeDynamic      Bootstrap: #7, NameAndType: #141
#148  = InvokeDynamic      Bootstrap: #8, NameAndType: #149
#149  = NameAndType        #63:#150 // makeConcatWithConstants:(I)Ljava/lang/String;
#150  = Utf8               "(I)Ljava/lang/String;"
#151  = Class              #152 // Test$1LocalClass
#152  = Utf8               "Test$1LocalClass"
#153  = Methodref          #151.#154
#154  = NameAndType        #5:#155 // <init>:(LTest;)V
#155  = Utf8               "(LTest;)V"
#156  = Methodref          #151.#157
#157  = NameAndType        #158:#6 // print:()V
#158  = Utf8               "print"
#159  = Class              #160 // Test$1
#160  = Utf8               "Test$1"
#161  = Methodref          #159.#154
#162  = InterfaceMethodref #163.#164
#163  = Class              #165 // java/lang/Runnable
#164  = NameAndType        #166:#6 // run:()V
#165  = Utf8               "java/lang/Runnable"
#166  = Utf8               "run"
#167  = String             #168 // "=== Starting Bytecode Analysis Test ==="
#168  = Utf8               "=== Starting Bytecode Analysis Test ==="
#169  = Methodref          #8.#3
#170  = Methodref          #8.#171
#171  = NameAndType        #172:#150 // testControlFlow:(I)Ljava/lang/String;
#172  = Utf8               "testControlFlow"
#173  = InvokeDynamic      Bootstrap: #9, NameAndType: #62
#174  = Methodref          #8.#175
#175  = NameAndType        #176:#6 // testLambdaAndStreams:()V
#176  = Utf8               "testLambdaAndStreams"
#177  = Methodref          #8.#178
#178  = NameAndType        #179:#6 // testInnerClasses:()V
#179  = Utf8               "testInnerClasses"
#180  = Methodref          #8.#181
#181  = NameAndType        #182:#6 // testExceptions:()V
#182  = Utf8               "testExceptions"
#183  = String             #184 // "=== Test Completed ==="
#184  = Utf8               "=== Test Completed ==="
#185  = String             #186 // "A"
#186  = Utf8               "A"
#187  = Methodref          #66.#188
#188  = NameAndType        #189:#190 // startsWith:(Ljava/lang/String;)Z
#189  = Utf8               "startsWith"
#190  = Utf8               "(Ljava/lang/String;)Z"
#191  = InvokeDynamic      Bootstrap: #10, NameAndType: #62
#192  = Class              #193 // java/lang/Cloneable
#193  = Utf8               "java/lang/Cloneable"
#194  = Utf8               "MAGIC_CONST"
#195  = Utf8               "I"
#196  = Utf8               "ConstantValue"
#197  = Integer            42
#198  = Utf8               "GREETING"
#199  = Utf8               "Ljava/lang/String;"
#200  = String             #201 // "Hello Bytecode"
#201  = Utf8               "Hello Bytecode"
#202  = Utf8               "tempCache"
#203  = Utf8               "Ljava/lang/Object;"
#204  = Utf8               "Code"
#205  = Utf8               "LineNumberTable"
#206  = Utf8               "StackMapTable"
#207  = Class              #208 // java/lang/Throwable
#208  = Utf8               "java/lang/Throwable"
#209  = Utf8               "Exceptions"
#210  = Class              #211 // java/lang/IllegalArgumentException
#211  = Utf8               "java/lang/IllegalArgumentException"
#212  = Utf8               "testGenerics"
#213  = Utf8               "(Ljava/util/List;Ljava/util/Map;)V"
#214  = Utf8               "Signature"
#215  = Utf8               "<E:Ljava/lang/Number;>(Ljava/util/List<TE;>;Ljava/util/Map<Ljava/lang/String;TE;>;)V"
#216  = Utf8               "testSyncAndMath"
#217  = Utf8               "testVarargs"
#218  = Utf8               "([Ljava/lang/String;)V"
#219  = Utf8               "nativeMethodExample"
#220  = Utf8               "main"
#221  = Class              #222 // [Ljava/lang/String;
#222  = Utf8               "[Ljava/lang/String;"
#223  = Utf8               "lambda$testLambdaAndStreams$1"
#224  = Utf8               "lambda$testLambdaAndStreams$0"
#225  = Utf8               "<T:Ljava/lang/Object;>Ljava/lang/Object;Ljava/lang/Cloneable;"
#226  = Utf8               "SourceFile"
#227  = Utf8               "Test.java"
#228  = Utf8               "RuntimeVisibleAnnotations"
#229  = Utf8               "LTestMetadata;"
#230  = Utf8               "version"
#231  = Integer            1
#232  = Utf8               "NestMembers"
#233  = Class              #234 // Test$InnerMember
#234  = Utf8               "Test$InnerMember"
#235  = Class              #236 // Test$StaticNested
#236  = Utf8               "Test$StaticNested"
#237  = Utf8               "BootstrapMethods"
#238  = String             #239 // "IO or Null Error: "
#239  = Utf8               "IO or Null Error: "
#240  = String             #241 // "General Error: "
#241  = Utf8               "General Error: "
#242  = MethodType         Descriptor: #243 // (Ljava/lang/Object;)V
#243  = Utf8               "(Ljava/lang/Object;)V"
#244  = MethodHandle       Kind: 06, Ref: #245
#245  = Methodref          #8.#246
#246  = NameAndType        #224:#36 // lambda$testLambdaAndStreams$0:(Ljava/lang/String;)V
#247  = MethodType         Descriptor: #36 // (Ljava/lang/String;)V
#248  = MethodHandle       Kind: 05, Ref: #45
#249  = MethodType         Descriptor: #250 // (Ljava/lang/Object;)Z
#250  = Utf8               "(Ljava/lang/Object;)Z"
#251  = MethodHandle       Kind: 06, Ref: #252
#252  = Methodref          #8.#253
#253  = NameAndType        #223:#190 // lambda$testLambdaAndStreams$1:(Ljava/lang/String;)Z
#254  = MethodType         Descriptor: #190 // (Ljava/lang/String;)Z
#255  = String             #256 // "Count: "
#256  = Utf8               "Count: "
#257  = String             #258 // "Number: "
#258  = Utf8               "Number: "
#259  = String             #260 // "Strict Result: "
#260  = Utf8               "Strict Result: "
#261  = String             #262 // "Args count: "
#262  = Utf8               "Args count: "
#263  = String             #264 // "Switch: "
#264  = Utf8               "Switch: "
#265  = String             #266 // "Name: "
#266  = Utf8               "Name: "
#267  = MethodHandle       Kind: 06, Ref: #268
#268  = Methodref          #269.#270
#269  = Class              #271 // java/lang/invoke/StringConcatFactory
#270  = NameAndType        #63:#272 // makeConcatWithConstants:(Ljava/lang/invoke/MethodHandles$Lookup;Ljava/lang/String;Ljava/lang/invoke/MethodType;Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/invoke/CallSite;
#271  = Utf8               "java/lang/invoke/StringConcatFactory"
#272  = Utf8               "(Ljava/lang/invoke/MethodHandles$Lookup;Ljava/lang/String;Ljava/lang/invoke/MethodType;Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/invoke/CallSite;"
#273  = MethodHandle       Kind: 06, Ref: #274
#274  = Methodref          #275.#276
#275  = Class              #277 // java/lang/invoke/LambdaMetafactory
#276  = NameAndType        #278:#279 // metafactory:(Ljava/lang/invoke/MethodHandles$Lookup;Ljava/lang/String;Ljava/lang/invoke/MethodType;Ljava/lang/invoke/MethodType;Ljava/lang/invoke/MethodHandle;Ljava/lang/invoke/MethodType;)Ljava/lang/invoke/CallSite;
#277  = Utf8               "java/lang/invoke/LambdaMetafactory"
#278  = Utf8               "metafactory"
#279  = Utf8               "(Ljava/lang/invoke/MethodHandles$Lookup;Ljava/lang/String;Ljava/lang/invoke/MethodType;Ljava/lang/invoke/MethodType;Ljava/lang/invoke/MethodHandle;Ljava/lang/invoke/MethodType;)Ljava/lang/invoke/CallSite;"
#280  = Utf8               "InnerClasses"
#281  = Utf8               "LocalClass"
#282  = Utf8               "InnerMember"
#283  = Utf8               "StaticNested"
#284  = Class              #285 // java/lang/invoke/MethodHandles$Lookup
#285  = Utf8               "java/lang/invoke/MethodHandles$Lookup"
#286  = Class              #287 // java/lang/invoke/MethodHandles
#287  = Utf8               "java/lang/invoke/MethodHandles"
#288  = Utf8               "Lookup"

=== Interfaces (1) ===
Interface: #192 (java/lang/Cloneable)

=== Fields (5) ===
Field: Public, Static, Final MAGIC_CONST I
  - ConstantValue: 
      Value Index: #197 (42)
Field: Public, Static, Final GREETING Ljava/lang/String;
  - ConstantValue: 
      Value Index: #200 ("Hello Bytecode")
Field: Private, Volatile isRunning Z
Field: Private, Transient tempCache Ljava/lang/Object;
Field: Private complexArray [[[D

=== Methods (12) ===
Method: Public <init> ()V
  - Code: 
      MaxStack: 4, MaxLocals: 1, Code Size: 21 bytes
        Instructions (12):
          [0000] 2A                       ALOAD_0              ; labels: [L0000]
          [0001] B7 00 01                 INVOKESPECIAL      java/lang/Object.<init> ()V  ; labels: [L0001]
          [0004] 2A                       ALOAD_0              ; labels: [L0004]
          [0005] 04                       ICONST_1             ; labels: [L0005]
          [0006] B5 00 07                 PUTFIELD           Test.isRunning Z  ; labels: [L0006]
          [0009] 2A                       ALOAD_0              ; labels: [L0009]
          [000A] 05                       ICONST_2             ; labels: [L000A]
          [000B] 05                       ICONST_2             ; labels: [L000B]
          [000C] 05                       ICONST_2             ; labels: [L000C]
          [000D] C5 00 0D 03              MULTIANEWARRAY     [[[D dims=3  ; labels: [L000D]
          [0011] B5 00 0F                 PUTFIELD           Test.complexArray [[[D  ; labels: [L0011]
          [0014] B1                       RETURN               ; labels: [L0014]
        Control Flow Graph:
          Blocks: 1, Edges: 0, Reachable: 1, Exits: 1, Backward: 0, Exception: 0
          Basic Blocks:
            B0   [0000..0015) entry, exit; instructions=12; successors=<exit>
          Edges:
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=12, TryCatch=0, CP entries=19
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (4):
              PC 0 -> Line 63
              PC 4 -> Line 64
              PC 9 -> Line 65
              PC 20 -> Line 66
Method: Public testControlFlow (I)Ljava/lang/String;
  - Code: 
      MaxStack: 1, MaxLocals: 2, Code Size: 73 bytes
        Instructions (16):
          [0000] 1B                       ILOAD_1              ; labels: [L0000]
          [0001] AA 00 00 00 00 00 24 00 00 00 00 00 00 00 02 00 00 00 1B 00 00 00 1E 00 00 00 21 TABLESWITCH        default->L0025; 0->L001C, 1->L001F, 2->L0022  ; labels: [L0001]
          [001C] 12 12                    LDC                "Zero"  ; labels: [L001C]
          [001E] B0                       ARETURN              ; labels: [L001E]
          [001F] 12 14                    LDC                "One"  ; labels: [L001F]
          [0021] B0                       ARETURN              ; labels: [L0021]
          [0022] 12 16                    LDC                "Two"  ; labels: [L0022]
          [0024] B0                       ARETURN              ; labels: [L0024]
          [0025] 1B                       ILOAD_1              ; labels: [L0025]
          [0026] AB 00 00 00 00 20 00 00 00 02 00 00 00 64 00 00 00 1A 00 00 13 88 00 00 00 1D LOOKUPSWITCH       default->L0046; 100->L0040, 5000->L0043  ; labels: [L0026]
          [0040] 12 18                    LDC                "Hundred"  ; labels: [L0040]
          [0042] B0                       ARETURN              ; labels: [L0042]
          [0043] 12 1A                    LDC                "Five Thousand"  ; labels: [L0043]
          [0045] B0                       ARETURN              ; labels: [L0045]
          [0046] 12 1C                    LDC                "Unknown"  ; labels: [L0046]
          [0048] B0                       ARETURN              ; labels: [L0048]
        Control Flow Graph:
          Blocks: 8, Edges: 7, Reachable: 8, Exits: 6, Backward: 0, Exception: 0
          Basic Blocks:
            B0   [0000..001C) entry; instructions=2; successors=SwitchDefault:default->B4, SwitchCase:case 0->B1, SwitchCase:case 1->B2, SwitchCase:case 2->B3
            B1   [001C..001F) exit; instructions=2; successors=<exit>
            B2   [001F..0022) exit; instructions=2; successors=<exit>
            B3   [0022..0025) exit; instructions=2; successors=<exit>
            B4   [0025..0040) reachable; instructions=2; successors=SwitchDefault:default->B7, SwitchCase:case 100->B5, SwitchCase:case 5000->B6
            B5   [0040..0043) exit; instructions=2; successors=<exit>
            B6   [0043..0046) exit; instructions=2; successors=<exit>
            B7   [0046..0049) exit; instructions=2; successors=<exit>
          Edges:
            B0 -> B4: SwitchDefault (default)
            B0 -> B1: SwitchCase (case 0)
            B0 -> B2: SwitchCase (case 1)
            B0 -> B3: SwitchCase (case 2)
            B4 -> B7: SwitchDefault (default)
            B4 -> B5: SwitchCase (case 100)
            B4 -> B6: SwitchCase (case 5000)
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=16, TryCatch=0, CP entries=14
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (8):
              PC 0 -> Line 78
              PC 28 -> Line 79
              PC 31 -> Line 80
              PC 34 -> Line 81
              PC 37 -> Line 85
              PC 64 -> Line 86
              PC 67 -> Line 87
              PC 70 -> Line 88
        - StackMapTable: 
            Entries: 7
              [0] Type: 28  (Same) Delta: 28
              [1] Type: 2   (Same) Delta: 2
              [2] Type: 2   (Same) Delta: 2
              [3] Type: 2   (Same) Delta: 2
              [4] Type: 26  (Same) Delta: 26
              [5] Type: 2   (Same) Delta: 2
              [6] Type: 2   (Same) Delta: 2
Method: Public testExceptions ()V
  - Code: 
      MaxStack: 3, MaxLocals: 3, Code Size: 94 bytes
        Instructions (39):
          [0000] 2A                       ALOAD_0              ; labels: [L0000]
          [0001] B4 00 07                 GETFIELD           Test.isRunning Z  ; labels: [L0001]
          [0004] 9A 00 0D                 IFNE               -> L0011  ; labels: [L0004]
          [0007] BB 00 1E                 NEW                java/io/IOException  ; labels: [L0007]
          [000A] 59                       DUP                  ; labels: [L000A]
          [000B] 12 20                    LDC                "System not running"  ; labels: [L000B]
          [000D] B7 00 22                 INVOKESPECIAL      java/io/IOException.<init> (Ljava/lang/String;)V  ; labels: [L000D]
          [0010] BF                       ATHROW               ; labels: [L0010]
          [0011] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L0011]
          [0014] 12 2B                    LDC                "Cleanup executed"  ; labels: [L0014]
          [0016] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L0016]
          [0019] A7 00 44                 GOTO               -> L005D  ; labels: [L0019]
          [001C] 4C                       ASTORE_1             ; labels: [L001C]
          [001D] B2 00 34                 GETSTATIC          java/lang/System.err Ljava/io/PrintStream;  ; labels: [L001D]
          [0020] 2B                       ALOAD_1              ; labels: [L0020]
          [0021] B6 00 37                 INVOKEVIRTUAL      java/lang/Exception.getMessage ()Ljava/lang/String;  ; labels: [L0021]
          [0024] BA 00 3D 00 00           INVOKEDYNAMIC      bootstrap=0 makeConcatWithConstants (Ljava/lang/String;)Ljava/lang/String;  ; labels: [L0024]
          [0029] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L0029]
          [002C] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L002C]
          [002F] 12 2B                    LDC                "Cleanup executed"  ; labels: [L002F]
          [0031] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L0031]
          [0034] A7 00 29                 GOTO               -> L005D  ; labels: [L0034]
          [0037] 4C                       ASTORE_1             ; labels: [L0037]
          [0038] B2 00 34                 GETSTATIC          java/lang/System.err Ljava/io/PrintStream;  ; labels: [L0038]
          [003B] 2B                       ALOAD_1              ; labels: [L003B]
          [003C] B6 00 37                 INVOKEVIRTUAL      java/lang/Exception.getMessage ()Ljava/lang/String;  ; labels: [L003C]
          [003F] BA 00 41 00 00           INVOKEDYNAMIC      bootstrap=1 makeConcatWithConstants (Ljava/lang/String;)Ljava/lang/String;  ; labels: [L003F]
          [0044] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L0044]
          [0047] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L0047]
          [004A] 12 2B                    LDC                "Cleanup executed"  ; labels: [L004A]
          [004C] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L004C]
          [004F] A7 00 0E                 GOTO               -> L005D  ; labels: [L004F]
          [0052] 4D                       ASTORE_2             ; labels: [L0052]
          [0053] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L0053]
          [0056] 12 2B                    LDC                "Cleanup executed"  ; labels: [L0056]
          [0058] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L0058]
          [005B] 2C                       ALOAD_2              ; labels: [L005B]
          [005C] BF                       ATHROW               ; labels: [L005C]
          [005D] B1                       RETURN               ; labels: [L005D]
        TryCatchBlocks:
          try [L0000..L0011) catch(java/io/IOException) -> L001C
          try [L0000..L0011) catch(java/lang/NullPointerException) -> L001C
          try [L0000..L0011) catch(java/lang/Exception) -> L0037
          try [L0000..L0011) catch(finally) -> L0052
          try [L001C..L002C) catch(finally) -> L0052
          try [L0037..L0047) catch(finally) -> L0052
        Control Flow Graph:
          Blocks: 9, Edges: 17, Reachable: 9, Exits: 3, Backward: 0, Exception: 10
          Basic Blocks:
            B0   [0000..0007) entry; instructions=3; successors=ConditionalBranch:condition true->B2, FallThrough:condition false->B1, Exception:java/io/IOException->B3, Exception:java/lang/NullPointerException->B3, Exception:java/lang/Exception->B5, Exception:finally->B7
            B1   [0007..0011) exit; instructions=5; successors=Exception:java/io/IOException->B3, Exception:java/lang/NullPointerException->B3, Exception:java/lang/Exception->B5, Exception:finally->B7
            B2   [0011..001C) reachable; instructions=4; successors=UnconditionalBranch->B8
            B3   [001C..002C) reachable; instructions=6; successors=FallThrough->B4, Exception:finally->B7
            B4   [002C..0037) reachable; instructions=4; successors=UnconditionalBranch->B8
            B5   [0037..0047) reachable; instructions=6; successors=FallThrough->B6, Exception:finally->B7
            B6   [0047..0052) reachable; instructions=4; successors=UnconditionalBranch->B8
            B7   [0052..005D) exit; instructions=6; successors=<exit>
            B8   [005D..005E) exit; instructions=1; successors=<exit>
          Edges:
            B0 -> B2: ConditionalBranch (condition true)
            B0 -> B1: FallThrough (condition false)
            B2 -> B8: UnconditionalBranch
            B3 -> B4: FallThrough
            B4 -> B8: UnconditionalBranch
            B5 -> B6: FallThrough
            B6 -> B8: UnconditionalBranch
            B0 -> B3: Exception (java/io/IOException)
            B1 -> B3: Exception (java/io/IOException)
            B0 -> B3: Exception (java/lang/NullPointerException)
            B1 -> B3: Exception (java/lang/NullPointerException)
            B0 -> B5: Exception (java/lang/Exception)
            B1 -> B5: Exception (java/lang/Exception)
            B0 -> B7: Exception (finally)
            B1 -> B7: Exception (finally)
            B3 -> B7: Exception (finally)
            B5 -> B7: Exception (finally)
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=39, TryCatch=6, CP entries=45
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (15):
              PC 0 -> Line 98
              PC 7 -> Line 99
              PC 17 -> Line 107
              PC 25 -> Line 108
              PC 28 -> Line 102
              PC 29 -> Line 103
              PC 44 -> Line 107
              PC 52 -> Line 108
              PC 55 -> Line 104
              PC 56 -> Line 105
              PC 71 -> Line 107
              PC 79 -> Line 108
              PC 82 -> Line 107
              PC 91 -> Line 108
              PC 93 -> Line 109
        - StackMapTable: 
            Entries: 5
              [0] Type: 17  (Same) Delta: 17
              [1] Type: 74  (SameLocals1StackItem) Delta: 10 Stack: Obj(#56 java/lang/Exception)
              [2] Type: 90  (SameLocals1StackItem) Delta: 26 Stack: Obj(#56 java/lang/Exception)
              [3] Type: 90  (SameLocals1StackItem) Delta: 26 Stack: Obj(#207 java/lang/Throwable)
              [4] Type: 10  (Same) Delta: 10
  - Exceptions: 
      Throws: java/io/IOException, java/lang/IllegalArgumentException
Method: Public testLambdaAndStreams ()V
  - Code: 
      MaxStack: 4, MaxLocals: 5, Code Size: 83 bytes
        Instructions (36):
          [0000] 06                       ICONST_3             ; labels: [L0000]
          [0001] BD 00 42                 ANEWARRAY          java/lang/String  ; labels: [L0001]
          [0004] 59                       DUP                  ; labels: [L0004]
          [0005] 03                       ICONST_0             ; labels: [L0005]
          [0006] 12 44                    LDC                "Alice"  ; labels: [L0006]
          [0008] 53                       AASTORE              ; labels: [L0008]
          [0009] 59                       DUP                  ; labels: [L0009]
          [000A] 04                       ICONST_1             ; labels: [L000A]
          [000B] 12 46                    LDC                "Bob"  ; labels: [L000B]
          [000D] 53                       AASTORE              ; labels: [L000D]
          [000E] 59                       DUP                  ; labels: [L000E]
          [000F] 05                       ICONST_2             ; labels: [L000F]
          [0010] 12 48                    LDC                "Charlie"  ; labels: [L0010]
          [0012] 53                       AASTORE              ; labels: [L0012]
          [0013] B8 00 4A                 INVOKESTATIC       java/util/Arrays.asList ([Ljava/lang/Object;)Ljava/util/List;  ; labels: [L0013]
          [0016] 4C                       ASTORE_1             ; labels: [L0016]
          [0017] BA 00 50 00 00           INVOKEDYNAMIC      bootstrap=2 accept ()Ljava/util/function/Consumer;  ; labels: [L0017]
          [001C] 4D                       ASTORE_2             ; labels: [L001C]
          [001D] 2B                       ALOAD_1              ; labels: [L001D]
          [001E] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L001E]
          [0021] 59                       DUP                  ; labels: [L0021]
          [0022] B8 00 54                 INVOKESTATIC       java/util/Objects.requireNonNull (Ljava/lang/Object;)Ljava/lang/Object;  ; labels: [L0022]
          [0025] 57                       POP                  ; labels: [L0025]
          [0026] BA 00 5A 00 00           INVOKEDYNAMIC      bootstrap=3 accept (Ljava/io/PrintStream;)Ljava/util/function/Consumer;  ; labels: [L0026]
          [002B] B9 00 5D 02 00           INVOKEINTERFACE    java/util/List.forEach (Ljava/util/function/Consumer;)V  ; labels: [L002B]
          [0030] 2B                       ALOAD_1              ; labels: [L0030]
          [0031] B9 00 63 01 00           INVOKEINTERFACE    java/util/List.stream ()Ljava/util/stream/Stream;  ; labels: [L0031]
          [0036] BA 00 67 00 00           INVOKEDYNAMIC      bootstrap=4 test ()Ljava/util/function/Predicate;  ; labels: [L0036]
          [003B] B9 00 6B 02 00           INVOKEINTERFACE    java/util/stream/Stream.filter (Ljava/util/function/Predicate;)Ljava/util/stream/Stream;  ; labels: [L003B]
          [0040] B9 00 71 01 00           INVOKEINTERFACE    java/util/stream/Stream.count ()J  ; labels: [L0040]
          [0045] 42                       LSTORE_3             ; labels: [L0045]
          [0046] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L0046]
          [0049] 21                       LLOAD_3              ; labels: [L0049]
          [004A] BA 00 75 00 00           INVOKEDYNAMIC      bootstrap=5 makeConcatWithConstants (J)Ljava/lang/String;  ; labels: [L004A]
          [004F] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L004F]
          [0052] B1                       RETURN               ; labels: [L0052]
        Control Flow Graph:
          Blocks: 1, Edges: 0, Reachable: 1, Exits: 1, Backward: 0, Exception: 0
          Basic Blocks:
            B0   [0000..0053) entry, exit; instructions=36; successors=<exit>
          Edges:
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=36, TryCatch=0, CP entries=69
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (8):
              PC 0 -> Line 116
              PC 23 -> Line 119
              PC 29 -> Line 122
              PC 48 -> Line 125
              PC 59 -> Line 126
              PC 64 -> Line 127
              PC 70 -> Line 129
              PC 82 -> Line 130
Method: Public testGenerics (Ljava/util/List;Ljava/util/Map;)V
  - Code: 
      MaxStack: 3, MaxLocals: 5, Code Size: 47 bytes
        Instructions (17):
          [0000] 2B                       ALOAD_1              ; labels: [L0000]
          [0001] B9 00 78 01 00           INVOKEINTERFACE    java/util/List.iterator ()Ljava/util/Iterator;  ; labels: [L0001]
          [0006] 4E                       ASTORE_3             ; labels: [L0006]
          [0007] 2D                       ALOAD_3              ; labels: [L0007]
          [0008] B9 00 7C 01 00           INVOKEINTERFACE    java/util/Iterator.hasNext ()Z  ; labels: [L0008]
          [000D] 99 00 21                 IFEQ               -> L002E  ; labels: [L000D]
          [0010] 2D                       ALOAD_3              ; labels: [L0010]
          [0011] B9 00 82 01 00           INVOKEINTERFACE    java/util/Iterator.next ()Ljava/lang/Object;  ; labels: [L0011]
          [0016] C0 00 86                 CHECKCAST          java/lang/Number  ; labels: [L0016]
          [0019] 3A 04                    ASTORE             4  ; labels: [L0019]
          [001B] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L001B]
          [001E] 19 04                    ALOAD              4  ; labels: [L001E]
          [0020] B6 00 88                 INVOKEVIRTUAL      java/lang/Number.doubleValue ()D  ; labels: [L0020]
          [0023] BA 00 8C 00 00           INVOKEDYNAMIC      bootstrap=6 makeConcatWithConstants (D)Ljava/lang/String;  ; labels: [L0023]
          [0028] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L0028]
          [002B] A7 FF DC                 GOTO               -> L0007  ; labels: [L002B]
          [002E] B1                       RETURN               ; labels: [L002E]
        Control Flow Graph:
          Blocks: 4, Edges: 4, Reachable: 4, Exits: 1, Backward: 1, Exception: 0
          Basic Blocks:
            B0   [0000..0007) entry; instructions=3; successors=FallThrough->B1
            B1   [0007..0010) reachable; instructions=3; successors=ConditionalBranch:condition true->B3, FallThrough:condition false->B2
            B2   [0010..002E) reachable; instructions=10; successors=UnconditionalBranch->B1
            B3   [002E..002F) exit; instructions=1; successors=<exit>
          Edges:
            B0 -> B1: FallThrough
            B1 -> B3: ConditionalBranch (condition true)
            B1 -> B2: FallThrough (condition false)
            B2 -> B1: UnconditionalBranch [backward]
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=17, TryCatch=0, CP entries=40
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (4):
              PC 0 -> Line 137
              PC 27 -> Line 138
              PC 43 -> Line 139
              PC 46 -> Line 140
        - StackMapTable: 
            Entries: 2
              [0] Type: 252 (Append 1) Delta: 7 Locals: [Obj(#125 java/util/Iterator)]
              [1] Type: 250 (Chop 1) Delta: 38
  - Signature: 
      Signature: <E:Ljava/lang/Number;>(Ljava/util/List<TE;>;Ljava/util/Map<Ljava/lang/String;TE;>;)V
Method: Public, Synchronized testSyncAndMath ()V
  - Code: 
      MaxStack: 3, MaxLocals: 5, Code Size: 35 bytes
        Instructions (21):
          [0000] 0E                       DCONST_0             ; labels: [L0000]
          [0001] 48                       DSTORE_1             ; labels: [L0001]
          [0002] 2A                       ALOAD_0              ; labels: [L0002]
          [0003] 59                       DUP                  ; labels: [L0003]
          [0004] 4E                       ASTORE_3             ; labels: [L0004]
          [0005] C2                       MONITORENTER         ; labels: [L0005]
          [0006] 14 00 91                 LDC2_W             6.283185307179586d  ; labels: [L0006]
          [0009] 48                       DSTORE_1             ; labels: [L0009]
          [000A] 2D                       ALOAD_3              ; labels: [L000A]
          [000B] C3                       MONITOREXIT          ; labels: [L000B]
          [000C] A7 00 0A                 GOTO               -> L0016  ; labels: [L000C]
          [000F] 3A 04                    ASTORE             4  ; labels: [L000F]
          [0011] 2D                       ALOAD_3              ; labels: [L0011]
          [0012] C3                       MONITOREXIT          ; labels: [L0012]
          [0013] 19 04                    ALOAD              4  ; labels: [L0013]
          [0015] BF                       ATHROW               ; labels: [L0015]
          [0016] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L0016]
          [0019] 27                       DLOAD_1              ; labels: [L0019]
          [001A] BA 00 93 00 00           INVOKEDYNAMIC      bootstrap=7 makeConcatWithConstants (D)Ljava/lang/String;  ; labels: [L001A]
          [001F] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L001F]
          [0022] B1                       RETURN               ; labels: [L0022]
        TryCatchBlocks:
          try [L0006..L000C) catch(finally) -> L000F
          try [L000F..L0013) catch(finally) -> L000F
        Control Flow Graph:
          Blocks: 6, Edges: 6, Reachable: 6, Exits: 2, Backward: 0, Exception: 2
          Basic Blocks:
            B0   [0000..0006) entry; instructions=6; successors=FallThrough->B1
            B1   [0006..000C) reachable; instructions=4; successors=FallThrough->B2, Exception:finally->B3
            B2   [000C..000F) reachable; instructions=1; successors=UnconditionalBranch->B5
            B3   [000F..0013) reachable; instructions=3; successors=FallThrough->B4, Exception:finally->B3
            B4   [0013..0016) exit; instructions=2; successors=<exit>
            B5   [0016..0023) exit; instructions=5; successors=<exit>
          Edges:
            B0 -> B1: FallThrough
            B1 -> B2: FallThrough
            B2 -> B5: UnconditionalBranch
            B3 -> B4: FallThrough
            B1 -> B3: Exception (finally)
            B3 -> B3: Exception (finally)
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=21, TryCatch=2, CP entries=20
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (6):
              PC 0 -> Line 147
              PC 2 -> Line 148
              PC 6 -> Line 149
              PC 10 -> Line 150
              PC 22 -> Line 151
              PC 34 -> Line 152
        - StackMapTable: 
            Entries: 2
              [0] Type: 255 (Full) Delta: 15
                  Locals: [Obj(#8 Test), Double, Obj(#2 java/lang/Object)]
                  Stack:  [Obj(#207 java/lang/Throwable)]
              [1] Type: 250 (Chop 1) Delta: 6
Method: Public, Varargs testVarargs ([Ljava/lang/String;)V
  - Code: 
      MaxStack: 2, MaxLocals: 2, Code Size: 14 bytes
        Instructions (6):
          [0000] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L0000]
          [0003] 2B                       ALOAD_1              ; labels: [L0003]
          [0004] BE                       ARRAYLENGTH          ; labels: [L0004]
          [0005] BA 00 94 00 00           INVOKEDYNAMIC      bootstrap=8 makeConcatWithConstants (I)Ljava/lang/String;  ; labels: [L0005]
          [000A] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L000A]
          [000D] B1                       RETURN               ; labels: [L000D]
        Control Flow Graph:
          Blocks: 1, Edges: 0, Reachable: 1, Exits: 1, Backward: 0, Exception: 0
          Basic Blocks:
            B0   [0000..000E) entry, exit; instructions=6; successors=<exit>
          Edges:
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=6, TryCatch=0, CP entries=18
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (2):
              PC 0 -> Line 159
              PC 13 -> Line 160
Method: Public, Native nativeMethodExample ()V
Method: Public testInnerClasses ()V
  - Code: 
      MaxStack: 3, MaxLocals: 2, Code Size: 27 bytes
        Instructions (13):
          [0000] BB 00 97                 NEW                Test$1LocalClass  ; labels: [L0000]
          [0003] 59                       DUP                  ; labels: [L0003]
          [0004] 2A                       ALOAD_0              ; labels: [L0004]
          [0005] B7 00 99                 INVOKESPECIAL      Test$1LocalClass.<init> (LTest;)V  ; labels: [L0005]
          [0008] B6 00 9C                 INVOKEVIRTUAL      Test$1LocalClass.print ()V  ; labels: [L0008]
          [000B] BB 00 9F                 NEW                Test$1  ; labels: [L000B]
          [000E] 59                       DUP                  ; labels: [L000E]
          [000F] 2A                       ALOAD_0              ; labels: [L000F]
          [0010] B7 00 A1                 INVOKESPECIAL      Test$1.<init> (LTest;)V  ; labels: [L0010]
          [0013] 4C                       ASTORE_1             ; labels: [L0013]
          [0014] 2B                       ALOAD_1              ; labels: [L0014]
          [0015] B9 00 A2 01 00           INVOKEINTERFACE    java/lang/Runnable.run ()V  ; labels: [L0015]
          [001A] B1                       RETURN               ; labels: [L001A]
        Control Flow Graph:
          Blocks: 1, Edges: 0, Reachable: 1, Exits: 1, Backward: 0, Exception: 0
          Basic Blocks:
            B0   [0000..001B) entry, exit; instructions=13; successors=<exit>
          Edges:
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=13, TryCatch=0, CP entries=20
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (4):
              PC 0 -> Line 187
              PC 11 -> Line 190
              PC 20 -> Line 196
              PC 26 -> Line 197
Method: Public, Static main ([Ljava/lang/String;)V
  - Code: 
      MaxStack: 3, MaxLocals: 3, Code Size: 57 bytes
        Instructions (25):
          [0000] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L0000]
          [0003] 12 A7                    LDC                "=== Starting Bytecode Analysis Test ==="  ; labels: [L0003]
          [0005] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L0005]
          [0008] BB 00 08                 NEW                Test  ; labels: [L0008]
          [000B] 59                       DUP                  ; labels: [L000B]
          [000C] B7 00 A9                 INVOKESPECIAL      Test.<init> ()V  ; labels: [L000C]
          [000F] 4C                       ASTORE_1             ; labels: [L000F]
          [0010] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L0010]
          [0013] 2B                       ALOAD_1              ; labels: [L0013]
          [0014] 04                       ICONST_1             ; labels: [L0014]
          [0015] B6 00 AA                 INVOKEVIRTUAL      Test.testControlFlow (I)Ljava/lang/String;  ; labels: [L0015]
          [0018] BA 00 AD 00 00           INVOKEDYNAMIC      bootstrap=9 makeConcatWithConstants (Ljava/lang/String;)Ljava/lang/String;  ; labels: [L0018]
          [001D] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L001D]
          [0020] 2B                       ALOAD_1              ; labels: [L0020]
          [0021] B6 00 AE                 INVOKEVIRTUAL      Test.testLambdaAndStreams ()V  ; labels: [L0021]
          [0024] 2B                       ALOAD_1              ; labels: [L0024]
          [0025] B6 00 B1                 INVOKEVIRTUAL      Test.testInnerClasses ()V  ; labels: [L0025]
          [0028] 2B                       ALOAD_1              ; labels: [L0028]
          [0029] B6 00 B4                 INVOKEVIRTUAL      Test.testExceptions ()V  ; labels: [L0029]
          [002C] A7 00 04                 GOTO               -> L0030  ; labels: [L002C]
          [002F] 4D                       ASTORE_2             ; labels: [L002F]
          [0030] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L0030]
          [0033] 12 B7                    LDC                "=== Test Completed ==="  ; labels: [L0033]
          [0035] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L0035]
          [0038] B1                       RETURN               ; labels: [L0038]
        TryCatchBlocks:
          try [L0028..L002C) catch(java/lang/Exception) -> L002F
        Control Flow Graph:
          Blocks: 5, Edges: 5, Reachable: 5, Exits: 1, Backward: 0, Exception: 1
          Basic Blocks:
            B0   [0000..0028) entry; instructions=17; successors=FallThrough->B1
            B1   [0028..002C) reachable; instructions=2; successors=FallThrough->B2, Exception:java/lang/Exception->B3
            B2   [002C..002F) reachable; instructions=1; successors=UnconditionalBranch->B4
            B3   [002F..0030) reachable; instructions=1; successors=FallThrough->B4
            B4   [0030..0039) exit; instructions=4; successors=<exit>
          Edges:
            B0 -> B1: FallThrough
            B1 -> B2: FallThrough
            B2 -> B4: UnconditionalBranch
            B3 -> B4: FallThrough
            B1 -> B3: Exception (java/lang/Exception)
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=25, TryCatch=1, CP entries=43
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (10):
              PC 0 -> Line 203
              PC 8 -> Line 205
              PC 16 -> Line 208
              PC 32 -> Line 211
              PC 36 -> Line 214
              PC 40 -> Line 218
              PC 44 -> Line 221
              PC 47 -> Line 219
              PC 48 -> Line 223
              PC 56 -> Line 224
        - StackMapTable: 
            Entries: 2
              [0] Type: 255 (Full) Delta: 47
                  Locals: [Obj(#221 [Ljava/lang/String;), Obj(#8 Test)]
                  Stack:  [Obj(#56 java/lang/Exception)]
              [1] Type: 0   (Same) Delta: 0
Method: Private, Static, Synthetic lambda$testLambdaAndStreams$1 (Ljava/lang/String;)Z
  - Code: 
      MaxStack: 2, MaxLocals: 1, Code Size: 7 bytes
        Instructions (4):
          [0000] 2A                       ALOAD_0              ; labels: [L0000]
          [0001] 12 B9                    LDC                "A"  ; labels: [L0001]
          [0003] B6 00 BB                 INVOKEVIRTUAL      java/lang/String.startsWith (Ljava/lang/String;)Z  ; labels: [L0003]
          [0006] AC                       IRETURN              ; labels: [L0006]
        Control Flow Graph:
          Blocks: 1, Edges: 0, Reachable: 1, Exits: 1, Backward: 0, Exception: 0
          Basic Blocks:
            B0   [0000..0007) entry, exit; instructions=4; successors=<exit>
          Edges:
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=4, TryCatch=0, CP entries=10
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (1):
              PC 0 -> Line 126
Method: Private, Static, Synthetic lambda$testLambdaAndStreams$0 (Ljava/lang/String;)V
  - Code: 
      MaxStack: 2, MaxLocals: 1, Code Size: 13 bytes
        Instructions (5):
          [0000] B2 00 25                 GETSTATIC          java/lang/System.out Ljava/io/PrintStream;  ; labels: [L0000]
          [0003] 2A                       ALOAD_0              ; labels: [L0003]
          [0004] BA 00 BF 00 00           INVOKEDYNAMIC      bootstrap=10 makeConcatWithConstants (Ljava/lang/String;)Ljava/lang/String;  ; labels: [L0004]
          [0009] B6 00 2D                 INVOKEVIRTUAL      java/io/PrintStream.println (Ljava/lang/String;)V  ; labels: [L0009]
          [000C] B1                       RETURN               ; labels: [L000C]
        Control Flow Graph:
          Blocks: 1, Edges: 0, Reachable: 1, Exits: 1, Backward: 0, Exception: 0
          Basic Blocks:
            B0   [0000..000D) entry, exit; instructions=5; successors=<exit>
          Edges:
        Round-Trip:
          DIFF: Same length but bytes differ (constant-pool indices may vary).
          Instructions=5, TryCatch=0, CP entries=18
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (1):
              PC 0 -> Line 119

=== Class Attributes (6) ===
- Signature: 
    Signature: <T:Ljava/lang/Object;>Ljava/lang/Object;Ljava/lang/Cloneable;
- SourceFile: 
    Source File: Test.java
- RuntimeVisibleAnnotations: 
    @LTestMetadata;(version=1)
- NestMembers: 
    Members: Test$InnerMember, Test$StaticNested, Test$1, Test$1LocalClass
- BootstrapMethods: 
    Bootstrap Methods (11):
      [0] Ref: #267, Args: [#238]
      [1] Ref: #267, Args: [#240]
      [2] Ref: #273, Args: [#242, #244, #247]
      [3] Ref: #273, Args: [#242, #248, #247]
      [4] Ref: #273, Args: [#249, #251, #254]
      [5] Ref: #267, Args: [#255]
      [6] Ref: #267, Args: [#257]
      [7] Ref: #267, Args: [#259]
      [8] Ref: #267, Args: [#261]
      [9] Ref: #267, Args: [#263]
      [10] Ref: #267, Args: [#265]
- InnerClasses: 
    Inner Classes (5):
      Test$1LocalClass (Outer: None) - 0
      Test$1 (Outer: None) - 0
      Test$InnerMember (Outer: Test) - Private
      Test$StaticNested (Outer: Test) - Public, Static
      java/lang/invoke/MethodHandles$Lookup (Outer: java/lang/invoke/MethodHandles) - Public, Static, Final

--------------------------------------------------
Parse Completed Successfully.
```
