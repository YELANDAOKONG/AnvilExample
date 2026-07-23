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

=== Constant Pool (320 entries) ===
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
#206  = Utf8               "LocalVariableTable"
#207  = Utf8               "this"
#208  = Utf8               "LTest;"
#209  = Utf8               "LocalVariableTypeTable"
#210  = Utf8               "LTest<TT;>;"
#211  = Utf8               "value"
#212  = Utf8               "StackMapTable"
#213  = Utf8               "MethodParameters"
#214  = Utf8               "e"
#215  = Utf8               "Ljava/lang/Exception;"
#216  = Class              #217 // java/lang/Throwable
#217  = Utf8               "java/lang/Throwable"
#218  = Utf8               "Exceptions"
#219  = Class              #220 // java/lang/IllegalArgumentException
#220  = Utf8               "java/lang/IllegalArgumentException"
#221  = Utf8               "names"
#222  = Utf8               "Ljava/util/List;"
#223  = Utf8               "printer"
#224  = Utf8               "Ljava/util/function/Consumer;"
#225  = Utf8               "J"
#226  = Utf8               "Ljava/util/List<Ljava/lang/String;>;"
#227  = Utf8               "Ljava/util/function/Consumer<Ljava/lang/String;>;"
#228  = Utf8               "testGenerics"
#229  = Utf8               "(Ljava/util/List;Ljava/util/Map;)V"
#230  = Utf8               "num"
#231  = Utf8               "Ljava/lang/Number;"
#232  = Utf8               "numbers"
#233  = Utf8               "map"
#234  = Utf8               "Ljava/util/Map;"
#235  = Utf8               "TE;"
#236  = Utf8               "Ljava/util/List<TE;>;"
#237  = Utf8               "Ljava/util/Map<Ljava/lang/String;TE;>;"
#238  = Utf8               "Signature"
#239  = Utf8               "<E:Ljava/lang/Number;>(Ljava/util/List<TE;>;Ljava/util/Map<Ljava/lang/String;TE;>;)V"
#240  = Utf8               "testSyncAndMath"
#241  = Utf8               "result"
#242  = Utf8               "D"
#243  = Utf8               "testVarargs"
#244  = Utf8               "([Ljava/lang/String;)V"
#245  = Utf8               "args"
#246  = Utf8               "[Ljava/lang/String;"
#247  = Utf8               "nativeMethodExample"
#248  = Utf8               "r"
#249  = Utf8               "Ljava/lang/Runnable;"
#250  = Utf8               "main"
#251  = Utf8               "LTest<Ljava/lang/Integer;>;"
#252  = Class              #246 // [Ljava/lang/String;
#253  = Utf8               "lambda$testLambdaAndStreams$1"
#254  = Utf8               "s"
#255  = Utf8               "lambda$testLambdaAndStreams$0"
#256  = Utf8               "<T:Ljava/lang/Object;>Ljava/lang/Object;Ljava/lang/Cloneable;"
#257  = Utf8               "SourceFile"
#258  = Utf8               "Test.java"
#259  = Utf8               "RuntimeVisibleAnnotations"
#260  = Utf8               "LTestMetadata;"
#261  = Utf8               "version"
#262  = Integer            1
#263  = Utf8               "NestMembers"
#264  = Class              #265 // Test$InnerMember
#265  = Utf8               "Test$InnerMember"
#266  = Class              #267 // Test$StaticNested
#267  = Utf8               "Test$StaticNested"
#268  = Utf8               "BootstrapMethods"
#269  = String             #270 // "IO or Null Error: "
#270  = Utf8               "IO or Null Error: "
#271  = String             #272 // "General Error: "
#272  = Utf8               "General Error: "
#273  = MethodType         Descriptor: #274 // (Ljava/lang/Object;)V
#274  = Utf8               "(Ljava/lang/Object;)V"
#275  = MethodHandle       Kind: 06, Ref: #276
#276  = Methodref          #8.#277
#277  = NameAndType        #255:#36 // lambda$testLambdaAndStreams$0:(Ljava/lang/String;)V
#278  = MethodType         Descriptor: #36 // (Ljava/lang/String;)V
#279  = MethodHandle       Kind: 05, Ref: #45
#280  = MethodType         Descriptor: #281 // (Ljava/lang/Object;)Z
#281  = Utf8               "(Ljava/lang/Object;)Z"
#282  = MethodHandle       Kind: 06, Ref: #283
#283  = Methodref          #8.#284
#284  = NameAndType        #253:#190 // lambda$testLambdaAndStreams$1:(Ljava/lang/String;)Z
#285  = MethodType         Descriptor: #190 // (Ljava/lang/String;)Z
#286  = String             #287 // "Count: "
#287  = Utf8               "Count: "
#288  = String             #289 // "Number: "
#289  = Utf8               "Number: "
#290  = String             #291 // "Strict Result: "
#291  = Utf8               "Strict Result: "
#292  = String             #293 // "Args count: "
#293  = Utf8               "Args count: "
#294  = String             #295 // "Switch: "
#295  = Utf8               "Switch: "
#296  = String             #297 // "Name: "
#297  = Utf8               "Name: "
#298  = MethodHandle       Kind: 06, Ref: #299
#299  = Methodref          #300.#301
#300  = Class              #302 // java/lang/invoke/StringConcatFactory
#301  = NameAndType        #63:#303 // makeConcatWithConstants:(Ljava/lang/invoke/MethodHandles$Lookup;Ljava/lang/String;Ljava/lang/invoke/MethodType;Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/invoke/CallSite;
#302  = Utf8               "java/lang/invoke/StringConcatFactory"
#303  = Utf8               "(Ljava/lang/invoke/MethodHandles$Lookup;Ljava/lang/String;Ljava/lang/invoke/MethodType;Ljava/lang/String;[Ljava/lang/Object;)Ljava/lang/invoke/CallSite;"
#304  = MethodHandle       Kind: 06, Ref: #305
#305  = Methodref          #306.#307
#306  = Class              #308 // java/lang/invoke/LambdaMetafactory
#307  = NameAndType        #309:#310 // metafactory:(Ljava/lang/invoke/MethodHandles$Lookup;Ljava/lang/String;Ljava/lang/invoke/MethodType;Ljava/lang/invoke/MethodType;Ljava/lang/invoke/MethodHandle;Ljava/lang/invoke/MethodType;)Ljava/lang/invoke/CallSite;
#308  = Utf8               "java/lang/invoke/LambdaMetafactory"
#309  = Utf8               "metafactory"
#310  = Utf8               "(Ljava/lang/invoke/MethodHandles$Lookup;Ljava/lang/String;Ljava/lang/invoke/MethodType;Ljava/lang/invoke/MethodType;Ljava/lang/invoke/MethodHandle;Ljava/lang/invoke/MethodType;)Ljava/lang/invoke/CallSite;"
#311  = Utf8               "InnerClasses"
#312  = Utf8               "LocalClass"
#313  = Utf8               "InnerMember"
#314  = Utf8               "StaticNested"
#315  = Class              #316 // java/lang/invoke/MethodHandles$Lookup
#316  = Utf8               "java/lang/invoke/MethodHandles$Lookup"
#317  = Class              #318 // java/lang/invoke/MethodHandles
#318  = Utf8               "java/lang/invoke/MethodHandles"
#319  = Utf8               "Lookup"

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
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (4):
              PC 0 -> Line 63
              PC 4 -> Line 64
              PC 9 -> Line 65
              PC 20 -> Line 66
        - LocalVariableTable: 
            Local Variables (1):
              Slot 0: this LTest; (PC 0-21)
        - LocalVariableTypeTable: 
            Local Variable Types (1):
              Slot 0: this LTest<TT;>;
Method: Public testControlFlow (I)Ljava/lang/String;
  - Code: 
      MaxStack: 1, MaxLocals: 2, Code Size: 73 bytes
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
        - LocalVariableTable: 
            Local Variables (2):
              Slot 0: this LTest; (PC 0-73)
              Slot 1: value I (PC 0-73)
        - LocalVariableTypeTable: 
            Local Variable Types (1):
              Slot 0: this LTest<TT;>;
        - StackMapTable: 
            Entries: 7
              [0] Type: 28  (Same) Delta: 28
              [1] Type: 2   (Same) Delta: 2
              [2] Type: 2   (Same) Delta: 2
              [3] Type: 2   (Same) Delta: 2
              [4] Type: 26  (Same) Delta: 26
              [5] Type: 2   (Same) Delta: 2
              [6] Type: 2   (Same) Delta: 2
  - MethodParameters: 
      Parameters (01):
        value (0)
Method: Public testExceptions ()V
  - Code: 
      MaxStack: 3, MaxLocals: 3, Code Size: 94 bytes
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
        - LocalVariableTable: 
            Local Variables (3):
              Slot 1: e Ljava/lang/Exception; (PC 29-44)
              Slot 1: e Ljava/lang/Exception; (PC 56-71)
              Slot 0: this LTest; (PC 0-94)
        - LocalVariableTypeTable: 
            Local Variable Types (1):
              Slot 0: this LTest<TT;>;
        - StackMapTable: 
            Entries: 5
              [0] Type: 17  (Same) Delta: 17
              [1] Type: 74  (SameLocals1StackItem) Delta: 10 Stack: Obj(#56 java/lang/Exception)
              [2] Type: 90  (SameLocals1StackItem) Delta: 26 Stack: Obj(#56 java/lang/Exception)
              [3] Type: 90  (SameLocals1StackItem) Delta: 26 Stack: Obj(#216 java/lang/Throwable)
              [4] Type: 10  (Same) Delta: 10
  - Exceptions: 
      Throws: java/io/IOException, java/lang/IllegalArgumentException
Method: Public testLambdaAndStreams ()V
  - Code: 
      MaxStack: 4, MaxLocals: 5, Code Size: 83 bytes
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
        - LocalVariableTable: 
            Local Variables (4):
              Slot 0: this LTest; (PC 0-83)
              Slot 1: names Ljava/util/List; (PC 23-83)
              Slot 2: printer Ljava/util/function/Consumer; (PC 29-83)
              Slot 3: count J (PC 70-83)
        - LocalVariableTypeTable: 
            Local Variable Types (3):
              Slot 0: this LTest<TT;>;
              Slot 1: names Ljava/util/List<Ljava/lang/String;>;
              Slot 2: printer Ljava/util/function/Consumer<Ljava/lang/String;>;
Method: Public testGenerics (Ljava/util/List;Ljava/util/Map;)V
  - Code: 
      MaxStack: 3, MaxLocals: 5, Code Size: 47 bytes
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (4):
              PC 0 -> Line 137
              PC 27 -> Line 138
              PC 43 -> Line 139
              PC 46 -> Line 140
        - LocalVariableTable: 
            Local Variables (4):
              Slot 4: num Ljava/lang/Number; (PC 27-43)
              Slot 0: this LTest; (PC 0-47)
              Slot 1: numbers Ljava/util/List; (PC 0-47)
              Slot 2: map Ljava/util/Map; (PC 0-47)
        - LocalVariableTypeTable: 
            Local Variable Types (4):
              Slot 4: num TE;
              Slot 0: this LTest<TT;>;
              Slot 1: numbers Ljava/util/List<TE;>;
              Slot 2: map Ljava/util/Map<Ljava/lang/String;TE;>;
        - StackMapTable: 
            Entries: 2
              [0] Type: 252 (Append 1) Delta: 7 Locals: [Obj(#125 java/util/Iterator)]
              [1] Type: 250 (Chop 1) Delta: 38
  - MethodParameters: 
      Parameters (02):
        numbers (0)
        map (0)
  - Signature: 
      Signature: <E:Ljava/lang/Number;>(Ljava/util/List<TE;>;Ljava/util/Map<Ljava/lang/String;TE;>;)V
Method: Public, Synchronized testSyncAndMath ()V
  - Code: 
      MaxStack: 3, MaxLocals: 5, Code Size: 35 bytes
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (6):
              PC 0 -> Line 147
              PC 2 -> Line 148
              PC 6 -> Line 149
              PC 10 -> Line 150
              PC 22 -> Line 151
              PC 34 -> Line 152
        - LocalVariableTable: 
            Local Variables (2):
              Slot 0: this LTest; (PC 0-35)
              Slot 1: result D (PC 2-35)
        - LocalVariableTypeTable: 
            Local Variable Types (1):
              Slot 0: this LTest<TT;>;
        - StackMapTable: 
            Entries: 2
              [0] Type: 255 (Full) Delta: 15
                  Locals: [Obj(#8 Test), Double, Obj(#2 java/lang/Object)]
                  Stack:  [Obj(#216 java/lang/Throwable)]
              [1] Type: 250 (Chop 1) Delta: 6
Method: Public, Varargs testVarargs ([Ljava/lang/String;)V
  - Code: 
      MaxStack: 2, MaxLocals: 2, Code Size: 14 bytes
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (2):
              PC 0 -> Line 159
              PC 13 -> Line 160
        - LocalVariableTable: 
            Local Variables (2):
              Slot 0: this LTest; (PC 0-14)
              Slot 1: args [Ljava/lang/String; (PC 0-14)
        - LocalVariableTypeTable: 
            Local Variable Types (1):
              Slot 0: this LTest<TT;>;
  - MethodParameters: 
      Parameters (01):
        args (0)
Method: Public, Native nativeMethodExample ()V
Method: Public testInnerClasses ()V
  - Code: 
      MaxStack: 3, MaxLocals: 2, Code Size: 27 bytes
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (4):
              PC 0 -> Line 187
              PC 11 -> Line 190
              PC 20 -> Line 196
              PC 26 -> Line 197
        - LocalVariableTable: 
            Local Variables (2):
              Slot 0: this LTest; (PC 0-27)
              Slot 1: r Ljava/lang/Runnable; (PC 20-27)
        - LocalVariableTypeTable: 
            Local Variable Types (1):
              Slot 0: this LTest<TT;>;
Method: Public, Static main ([Ljava/lang/String;)V
  - Code: 
      MaxStack: 3, MaxLocals: 3, Code Size: 57 bytes
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
        - LocalVariableTable: 
            Local Variables (2):
              Slot 0: args [Ljava/lang/String; (PC 0-57)
              Slot 1: test LTest; (PC 16-57)
        - LocalVariableTypeTable: 
            Local Variable Types (1):
              Slot 1: test LTest<Ljava/lang/Integer;>;
        - StackMapTable: 
            Entries: 2
              [0] Type: 255 (Full) Delta: 47
                  Locals: [Obj(#252 [Ljava/lang/String;), Obj(#8 Test)]
                  Stack:  [Obj(#56 java/lang/Exception)]
              [1] Type: 0   (Same) Delta: 0
  - MethodParameters: 
      Parameters (01):
        args (0)
Method: Private, Static, Synthetic lambda$testLambdaAndStreams$1 (Ljava/lang/String;)Z
  - Code: 
      MaxStack: 2, MaxLocals: 1, Code Size: 7 bytes
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (1):
              PC 0 -> Line 126
        - LocalVariableTable: 
            Local Variables (1):
              Slot 0: s Ljava/lang/String; (PC 0-7)
Method: Private, Static, Synthetic lambda$testLambdaAndStreams$0 (Ljava/lang/String;)V
  - Code: 
      MaxStack: 2, MaxLocals: 1, Code Size: 13 bytes
      Sub-Attributes:
        - LineNumberTable: 
            Line Numbers (1):
              PC 0 -> Line 119
        - LocalVariableTable: 
            Local Variables (1):
              Slot 0: s Ljava/lang/String; (PC 0-13)

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
      [0] Ref: #298, Args: [#269]
      [1] Ref: #298, Args: [#271]
      [2] Ref: #304, Args: [#273, #275, #278]
      [3] Ref: #304, Args: [#273, #279, #278]
      [4] Ref: #304, Args: [#280, #282, #285]
      [5] Ref: #298, Args: [#286]
      [6] Ref: #298, Args: [#288]
      [7] Ref: #298, Args: [#290]
      [8] Ref: #298, Args: [#292]
      [9] Ref: #298, Args: [#294]
      [10] Ref: #298, Args: [#296]
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
