using Anvil.Instructions;
using Anvil.Instructions.ConstantPool;
using Anvil.Interfaces;
using Anvil.Structures;
using Anvil.Structures.Attributes;
using Anvil.Structures.Attributes.Annotations;
using Anvil.Structures.Attributes.Code;
using Anvil.Structures.Attributes.Modules;
using Anvil.Structures.Attributes.Records;
using Anvil.Structures.Attributes.StackMap;
using Anvil.Structures.Attributes.StackMap.Frames;
using Anvil.Structures.Attributes.StackMap.Types;
using Anvil.Structures.ConstantPool;
using Anvil.Types;

namespace AnvilExample;

public class ClassicClassInspector
{
    private readonly ClassFile _cf;

    public ClassicClassInspector(ClassFile classFile)
    {
        _cf = classFile;
    }

    private bool _showInstructions;
    private bool _showControlFlow;
    private bool _roundTrip;

    public void Display(
        bool showInstructions = false,
        bool showControlFlow = false,
        bool roundTrip = false)
    {
        _showInstructions = showInstructions;
        _showControlFlow = showControlFlow;
        _roundTrip = roundTrip;
        PrintHeader();
        PrintConstantPool();
        PrintInterfaces();
        PrintFields();
        PrintMethods();
        PrintClassAttributes();
    }

    private void PrintHeader()
    {
        Console.WriteLine("=== Class File Header ===");
        Console.WriteLine($"Magic:        0x{_cf.Magic.Value:X8} {(_cf.Magic.Value == 0xCAFEBABE ? "(Valid)" : "(Invalid)")}");
        Console.WriteLine($"Version:      {_cf.MajorVersion}.{_cf.MinorVersion} (Java {_cf.MajorVersion - 44})");
        Console.WriteLine($"Access Flags: {_cf.AccessFlags}");

        var thisClassName = ResolveClass(_cf.ThisClass);
        var superClassName = ResolveClass(_cf.SuperClass);

        Console.WriteLine($"This Class:   #{_cf.ThisClass} ({thisClassName})");
        Console.WriteLine($"Super Class:  #{_cf.SuperClass} ({superClassName})");
        Console.WriteLine();
    }

    private void PrintConstantPool()
    {
        Console.WriteLine($"=== Constant Pool ({_cf.ConstantPoolCount} entries) ===");

        for (int i = 1; i < _cf.ConstantPool.Length; i++)
        {
            var entry = _cf.ConstantPool[i];
            if (entry == null) continue;

            Console.Write($"#{i,-4} = {entry.Tag,-18}");

            switch (entry)
            {
                case CpUtf8 utf8:
                    Console.WriteLine($" \"{utf8.Value}\"");
                    break;
                case CpInteger i4:
                    Console.WriteLine($" {i4.Bytes.Value}");
                    break;
                case CpFloat f4:
                    Console.WriteLine($" {f4.Bytes.Value}f");
                    break;
                case CpLong i8:
                    Console.WriteLine($" {i8.Bytes.Value}L");
                    break;
                case CpDouble f8:
                    Console.WriteLine($" {f8.Bytes.Value}d");
                    break;
                case CpClass cls:
                    Console.WriteLine($" #{cls.NameIndex} // {ResolveUtf8(cls.NameIndex)}");
                    break;
                case CpString str:
                    Console.WriteLine($" #{str.StringIndex} // \"{ResolveUtf8(str.StringIndex)}\"");
                    break;
                case CpFieldRef fref:
                    Console.WriteLine($" #{fref.ClassIndex}.#{fref.NameAndTypeIndex}");
                    break;
                case CpMethodRef mref:
                    Console.WriteLine($" #{mref.ClassIndex}.#{mref.NameAndTypeIndex}");
                    break;
                case CpInterfaceMethodRef imref:
                    Console.WriteLine($" #{imref.ClassIndex}.#{imref.NameAndTypeIndex}");
                    break;
                case CpNameAndType nt:
                    Console.WriteLine($" #{nt.NameIndex}:#{nt.DescriptorIndex} // {ResolveUtf8(nt.NameIndex)}:{ResolveUtf8(nt.DescriptorIndex)}");
                    break;
                case CpMethodHandle mh:
                    Console.WriteLine($" Kind: {mh.ReferenceKind}, Ref: #{mh.ReferenceIndex}");
                    break;
                case CpMethodType mt:
                    Console.WriteLine($" Descriptor: #{mt.DescriptorIndex} // {ResolveUtf8(mt.DescriptorIndex)}");
                    break;
                case CpDynamic dyn:
                    Console.WriteLine($" Bootstrap: #{dyn.BootstrapMethodAttrIndex}, NameAndType: #{dyn.NameAndTypeIndex}");
                    break;
                case CpInvokeDynamic idyn:
                    Console.WriteLine($" Bootstrap: #{idyn.BootstrapMethodAttrIndex}, NameAndType: #{idyn.NameAndTypeIndex}");
                    break;
                case CpModule mod:
                    Console.WriteLine($" #{mod.NameIndex} // {ResolveUtf8(mod.NameIndex)}");
                    break;
                case CpPackage pkg:
                    Console.WriteLine($" #{pkg.NameIndex} // {ResolveUtf8(pkg.NameIndex)}");
                    break;
                default:
                    Console.WriteLine($" ({entry.GetType().Name})");
                    break;
            }
        }
        Console.WriteLine();
    }

    private void PrintInterfaces()
    {
        Console.WriteLine($"=== Interfaces ({_cf.InterfacesCount}) ===");
        foreach (var ifaceIndex in _cf.Interfaces)
        {
            Console.WriteLine($"Interface: #{ifaceIndex} ({ResolveClass(ifaceIndex)})");
        }
        Console.WriteLine();
    }

    private void PrintFields()
    {
        Console.WriteLine($"=== Fields ({_cf.FieldsCount}) ===");
        foreach (var field in _cf.Fields)
        {
            var name = ResolveUtf8(field.NameIndex);
            var desc = ResolveUtf8(field.DescriptorIndex);
            Console.WriteLine($"Field: {field.AccessFlags} {name} {desc}");

            foreach (var attr in field.Attributes)
            {
                PrintAttribute(attr, "  ");
            }
        }
        Console.WriteLine();
    }

    private void PrintMethods()
    {
        Console.WriteLine($"=== Methods ({_cf.MethodsCount}) ===");
        foreach (var method in _cf.Methods)
        {
            var name = ResolveUtf8(method.NameIndex);
            var desc = ResolveUtf8(method.DescriptorIndex);
            Console.WriteLine($"Method: {method.AccessFlags} {name} {desc}");

            foreach (var attr in method.Attributes)
            {
                PrintAttribute(attr, "  ");
            }
        }
        Console.WriteLine();
    }

    private void PrintClassAttributes()
    {
        Console.WriteLine($"=== Class Attributes ({_cf.AttributesCount}) ===");
        foreach (var attr in _cf.Attributes)
        {
            PrintAttribute(attr, "");
        }
        Console.WriteLine();
    }

    private void PrintAttribute(AttributeInfo attr, string indent)
    {
        var attrName = ResolveUtf8(attr.AttributeNameIndex);
        Console.Write($"{indent}- {attrName}: ");

        var body = attr.ResolveBody(_cf.ConstantPool);
        if (body == null)
        {
            Console.WriteLine($"(Raw Length: {attr.AttributeLength})");
            return;
        }

        Console.WriteLine();
        PrintAttributeBody(body, indent + "    ");
    }

    private void PrintAttributeBody(IAttribute body, string indent)
    {
        switch (body)
        {
            case ConstantValueAttribute cv:
                Console.WriteLine($"{indent}Value Index: #{cv.ConstantValueIndex} ({GetConstantValue(cv.ConstantValueIndex)})");
                break;

            case CodeAttribute code:
                Console.WriteLine($"{indent}MaxStack: {code.MaxStack}, MaxLocals: {code.MaxLocals}, Code Size: {code.Code.Length} bytes");
                if (_showInstructions && code.Code.Length > 0)
                {
                    PrintInstructions(classCode: code, indent: indent + "  ");
                }
                if (_showControlFlow && code.Code.Length > 0)
                {
                    PrintControlFlow(code, indent + "  ");
                }
                if (_roundTrip && code.Code.Length > 0)
                {
                    PrintRoundTrip(code, indent + "  ");
                }
                if (code.Attributes.Length > 0)
                {
                    Console.WriteLine($"{indent}Sub-Attributes:");
                    foreach (var subAttr in code.Attributes)
                    {
                        PrintAttribute(subAttr, indent + "  ");
                    }
                }
                break;

            case StackMapTableAttribute smt:
                Console.WriteLine($"{indent}Entries: {smt.Entries.Length}");
                PrintStackMapTable(smt, indent);
                break;

            case ExceptionsAttribute ex:
                Console.WriteLine($"{indent}Throws: {string.Join(", ", ex.ExceptionIndexTable.Select(idx => ResolveClass(idx)))}");
                break;

            case InnerClassesAttribute inner:
                Console.WriteLine($"{indent}Inner Classes ({inner.NumberOfClasses}):");
                foreach (var cls in inner.Classes)
                {
                    Console.WriteLine($"{indent}  {ResolveClass(cls.InnerClassInfoIndex)} (Outer: {ResolveClass(cls.OuterClassInfoIndex)}) - {cls.InnerClassAccessFlags}");
                }
                break;

            case EnclosingMethodAttribute enc:
                Console.WriteLine($"{indent}Class: {ResolveClass(enc.ClassIndex)}");
                if (enc.MethodIndex.Value != 0)
                    Console.WriteLine($"{indent}Method: {ResolveNameAndType(enc.MethodIndex)}");
                break;

            case SyntheticAttribute:
                Console.WriteLine($"{indent}(Synthetic)");
                break;

            case DeprecatedAttribute:
                Console.WriteLine($"{indent}(Deprecated)");
                break;

            case SignatureAttribute sig:
                Console.WriteLine($"{indent}Signature: {ResolveUtf8(sig.SignatureIndex)}");
                break;

            case SourceFileAttribute src:
                Console.WriteLine($"{indent}Source File: {ResolveUtf8(src.SourceFileIndex)}");
                break;

            case LineNumberTableAttribute lnt:
                Console.WriteLine($"{indent}Line Numbers ({lnt.LineNumberTableLength}):");
                foreach (var entry in lnt.LineNumberTable)
                {
                    Console.WriteLine($"{indent}  PC {entry.StartPc} -> Line {entry.LineNumber}");
                }
                break;

            case LocalVariableTableAttribute lvt:
                Console.WriteLine($"{indent}Local Variables ({lvt.LocalVariableTableLength}):");
                foreach (var entry in lvt.LocalVariableTable)
                {
                    Console.WriteLine($"{indent}  Slot {entry.Index}: {ResolveUtf8(entry.NameIndex)} {ResolveUtf8(entry.DescriptorIndex)} (PC {entry.StartPc}-{entry.StartPc + entry.Length})");
                }
                break;

            case LocalVariableTypeTableAttribute lvtt:
                Console.WriteLine($"{indent}Local Variable Types ({lvtt.LocalVariableTypeTableLength}):");
                foreach (var entry in lvtt.LocalVariableTypeTable)
                {
                    Console.WriteLine($"{indent}  Slot {entry.Index}: {ResolveUtf8(entry.NameIndex)} {ResolveUtf8(entry.SignatureIndex)}");
                }
                break;

            case BootstrapMethodsAttribute bsm:
                Console.WriteLine($"{indent}Bootstrap Methods ({bsm.NumBootstrapMethods}):");
                for (int i = 0; i < bsm.BootstrapMethods.Length; i++)
                {
                    var m = bsm.BootstrapMethods[i];
                    Console.WriteLine($"{indent}  [{i}] Ref: #{m.BootstrapMethodRef}, Args: [{string.Join(", ", m.BootstrapArguments.Select(a => "#" + a.Value))}]");
                }
                break;

            case MethodParametersAttribute mpa:
                Console.WriteLine($"{indent}Parameters ({mpa.ParametersCount}):");
                foreach (var p in mpa.Parameters)
                {
                    var pName = p.NameIndex.Value == 0 ? "<unnamed>" : ResolveUtf8(p.NameIndex);
                    Console.WriteLine($"{indent}  {pName} ({p.AccessFlags})");
                }
                break;

            case ModuleAttribute mod:
                PrintModuleDetails(mod, indent);
                break;

            case ModulePackagesAttribute mpkg:
                Console.WriteLine($"{indent}Packages: {string.Join(", ", mpkg.PackageIndex.Select(idx => ResolvePackage(idx)))}");
                break;

            case ModuleMainClassAttribute mmc:
                Console.WriteLine($"{indent}Main Class: {ResolveClass(mmc.MainClassIndex)}");
                break;

            case NestHostAttribute nh:
                Console.WriteLine($"{indent}Host: {ResolveClass(nh.HostClassIndex)}");
                break;

            case NestMembersAttribute nm:
                Console.WriteLine($"{indent}Members: {string.Join(", ", nm.Classes.Select(idx => ResolveClass(idx)))}");
                break;

            case RecordAttribute rec:
                Console.WriteLine($"{indent}Components ({rec.ComponentsCount}):");
                foreach (var comp in rec.Components)
                {
                    Console.WriteLine($"{indent}  {ResolveUtf8(comp.NameIndex)} : {ResolveUtf8(comp.DescriptorIndex)}");
                    foreach (var attr in comp.Attributes) PrintAttribute(attr, indent + "    ");
                }
                break;

            case PermittedSubclassesAttribute perm:
                Console.WriteLine($"{indent}Permitted: {string.Join(", ", perm.Classes.Select(idx => ResolveClass(idx)))}");
                break;

            case RuntimeVisibleAnnotationsAttribute rva:
                PrintAnnotations(rva.Annotations, indent);
                break;
            case RuntimeInvisibleAnnotationsAttribute ria:
                PrintAnnotations(ria.Annotations, indent);
                break;
            case AnnotationDefaultAttribute ad:
                Console.Write($"{indent}Default: ");
                PrintElementValue(ad.DefaultValue);
                Console.WriteLine();
                break;

            default:
                Console.WriteLine($"{indent}(Not fully implemented printer for {body.GetType().Name})");
                break;
        }
    }

    private void PrintModuleDetails(ModuleAttribute mod, string indent)
    {
        Console.WriteLine($"{indent}Name: {ResolveModule(mod.ModuleNameIndex)} {mod.ModuleFlags} v{ResolveUtf8(mod.ModuleVersionIndex)}");

        if (mod.Requires.Length > 0)
        {
            Console.WriteLine($"{indent}Requires:");
            foreach (var req in mod.Requires)
                Console.WriteLine($"{indent}  {ResolveModule(req.RequiresIndex)} ({req.RequiresFlags}) v{ResolveUtf8(req.RequiresVersionIndex)}");
        }

        if (mod.Exports.Length > 0)
        {
            Console.WriteLine($"{indent}Exports:");
            foreach (var exp in mod.Exports)
            {
                Console.Write($"{indent}  {ResolvePackage(exp.ExportsIndex)} ({exp.ExportsFlags})");
                if (exp.ExportsToCount.Value > 0)
                    Console.Write($" to {string.Join(", ", exp.ExportsToIndex.Select(i => ResolveModule(i)))}");
                Console.WriteLine();
            }
        }
    }

    private void PrintAnnotations(Annotation[] annotations, string indent)
    {
        foreach (var ann in annotations)
        {
            Console.Write($"{indent}@{ResolveUtf8(ann.TypeIndex)}(");
            for (int i = 0; i < ann.ElementValuePairs.Length; i++)
            {
                var pair = ann.ElementValuePairs[i];
                Console.Write($"{ResolveUtf8(pair.ElementNameIndex)}=");
                PrintElementValue(pair.Value);
                if (i < ann.ElementValuePairs.Length - 1) Console.Write(", ");
            }
            Console.WriteLine(")");
        }
    }

    private void PrintElementValue(ElementValue val)
    {
        switch (val)
        {
            case ConstElementValue c:
                Console.Write(GetConstantValue(c.ConstValueIndex));
                break;
            case EnumElementValue e:
                Console.Write($"{ResolveUtf8(e.TypeNameIndex)}.{ResolveUtf8(e.ConstNameIndex)}");
                break;
            case ClassElementValue c:
                Console.Write($"{ResolveUtf8(c.ClassInfoIndex)}.class");
                break;
            case ArrayElementValue a:
                Console.Write("{");
                for (int i = 0; i < a.Values.Length; i++)
                {
                    PrintElementValue(a.Values[i]);
                    if (i < a.Values.Length - 1) Console.Write(", ");
                }
                Console.Write("}");
                break;
            case AnnotationElementValue av:
                Console.Write($"@{ResolveUtf8(av.Annotation.TypeIndex)}(...)");
                break;
        }
    }

    private void PrintStackMapTable(StackMapTableAttribute stackMap, string indent)
    {
        int frameIndex = 0;
        foreach (var frame in stackMap.Entries)
        {
            Console.Write($"{indent}  [{frameIndex++}] Type: {frame.FrameType,-3} ");

            switch (frame)
            {
                case SameFrame same:
                    Console.WriteLine($"(Same) Delta: {same.FrameType}");
                    break;
                case SameLocals1StackItemFrame same1:
                    Console.WriteLine($"(SameLocals1StackItem) Delta: {same1.FrameType - 64} Stack: {FormatVerificationType(same1.Stack)}");
                    break;
                case SameLocals1StackItemFrameExtended same1Ext:
                    Console.WriteLine($"(SameLocals1StackItemExt) Delta: {same1Ext.OffsetDelta.Value} Stack: {FormatVerificationType(same1Ext.Stack)}");
                    break;
                case ChopFrame chop:
                    Console.WriteLine($"(Chop {251 - chop.FrameType}) Delta: {chop.OffsetDelta.Value}");
                    break;
                case SameFrameExtended sameExt:
                    Console.WriteLine($"(SameExt) Delta: {sameExt.OffsetDelta.Value}");
                    break;
                case AppendFrame append:
                    Console.WriteLine($"(Append {append.FrameType - 251}) Delta: {append.OffsetDelta.Value} Locals: {FormatVerificationTypes(append.Locals)}");
                    break;
                case FullFrame full:
                    Console.WriteLine($"(Full) Delta: {full.OffsetDelta.Value}");
                    Console.WriteLine($"{indent}      Locals: {FormatVerificationTypes(full.Locals)}");
                    Console.WriteLine($"{indent}      Stack:  {FormatVerificationTypes(full.Stack)}");
                    break;
            }
        }
    }

    private string FormatVerificationTypes(VerificationTypeInfo[] types)
    {
        if (types.Length == 0) return "[]";
        return "[" + string.Join(", ", types.Select(FormatVerificationType)) + "]";
    }

    private string FormatVerificationType(VerificationTypeInfo info)
    {
        return info switch
        {
            TopVariableInfo => "Top",
            IntegerVariableInfo => "Int",
            FloatVariableInfo => "Float",
            DoubleVariableInfo => "Double",
            LongVariableInfo => "Long",
            NullVariableInfo => "Null",
            UninitializedThisVariableInfo => "UninitThis",
            ObjectVariableInfo obj => $"Obj(#{obj.CPoolIndex} {ResolveClass(obj.CPoolIndex)})",
            UninitializedVariableInfo u => $"Uninit(Offset: {u.Offset.Value})",
            _ => "?"
        };
    }

    // --- Helpers ---

    private string ResolveUtf8(TUShort index)
    {
        if (index.Value == 0) return "";
        if (index.Value < _cf.ConstantPool.Length && _cf.ConstantPool[index.Value] is CpUtf8 utf8) return utf8.Value;
        return $"#<{index.Value}>";
    }

    private string ResolveClass(TUShort classIndex)
    {
        if (classIndex.Value == 0) return "None";
        if (classIndex.Value < _cf.ConstantPool.Length && _cf.ConstantPool[classIndex.Value] is CpClass cpClass)
        {
            return ResolveUtf8(cpClass.NameIndex);
        }
        return $"#<{classIndex.Value}>";
    }

    private string ResolvePackage(TUShort index)
    {
        if (index.Value < _cf.ConstantPool.Length && _cf.ConstantPool[index.Value] is CpPackage pkg)
            return ResolveUtf8(pkg.NameIndex);
        return $"#<{index.Value}>";
    }

    private string ResolveModule(TUShort index)
    {
        if (index.Value < _cf.ConstantPool.Length && _cf.ConstantPool[index.Value] is CpModule mod)
            return ResolveUtf8(mod.NameIndex);
        return $"#<{index.Value}>";
    }

    private string ResolveNameAndType(TUShort index)
    {
        if (index.Value < _cf.ConstantPool.Length && _cf.ConstantPool[index.Value] is CpNameAndType nt)
        {
            return $"{ResolveUtf8(nt.NameIndex)}:{ResolveUtf8(nt.DescriptorIndex)}";
        }
        return $"#<{index.Value}>";
    }

    private string GetConstantValue(TUShort index)
    {
        if (index.Value >= _cf.ConstantPool.Length) return $"#{index.Value}";
        var entry = _cf.ConstantPool[index.Value];
        return entry switch
        {
            CpUtf8 u => $"\"{u.Value}\"",
            CpInteger i => i.Bytes.Value.ToString(),
            CpFloat f => $"{f.Bytes.Value}f",
            CpLong l => $"{l.Bytes.Value}L",
            CpDouble d => $"{d.Bytes.Value}d",
            CpString s => $"\"{ResolveUtf8(s.StringIndex)}\"",
            _ => $"#{index.Value}"
        };
    }

    private void PrintInstructions(CodeAttribute classCode, string indent)
    {
        var body = MethodBody.FromCodeAttribute(classCode, _cf.ConstantPool);
        Console.WriteLine($"{indent}Instructions ({body.Instructions.Count}):");

        for (var index = 0; index < body.Instructions.Count; index++)
        {
            var insn = body.Instructions[index];
            var pc = insn.Offset?.ToString("X4") ?? "????";
            var endOffset = index + 1 < body.Instructions.Count
                ? body.Instructions[index + 1].Offset
                : classCode.Code.Length;
            var bytes = BytecodeFormatter.FormatRawBytes(
                classCode.Code,
                insn,
                endOffset
                ?? throw new InvalidOperationException(
                    "The next instruction has no resolved offset."));
            var labels = insn.Labels.Count > 0
                ? $"  ; labels: [{string.Join(", ", insn.Labels)}]"
                : "";

            Console.WriteLine(
                $"{indent}  [{pc}] {bytes,-24} "
                + $"{insn.OpCode,-18} {BytecodeFormatter.FormatOperands(insn)}"
                + labels);
        }

        if (body.TryCatchBlocks.Count > 0)
        {
            Console.WriteLine($"{indent}TryCatchBlocks:");
            foreach (var block in body.TryCatchBlocks)
            {
                var type = block.CatchType ?? "finally";
                Console.WriteLine($"{indent}  try [{block.Start}..{block.End}) catch({type}) -> {block.Handler}");
            }
        }
    }

    private void PrintControlFlow(CodeAttribute code, string indent)
    {
        var body = MethodBody.FromCodeAttribute(code, _cf.ConstantPool);
        var graph = ControlFlowGraphBuilder.Build(body, code.Code.Length);
        var reachableCount = graph.Blocks.Count(block => block.IsReachable);
        var exceptionEdgeCount = graph.Edges.Count(edge =>
            edge.Kind == ControlFlowEdgeKind.Exception);
        var backwardEdgeCount = graph.Edges.Count(edge => edge.IsBackward);

        Console.WriteLine($"{indent}Control Flow Graph:");
        Console.WriteLine(
            $"{indent}  Blocks: {graph.Blocks.Count}, "
            + $"Edges: {graph.Edges.Count}, "
            + $"Reachable: {reachableCount}, "
            + $"Exits: {graph.ExitBlocks.Count}, "
            + $"Backward: {backwardEdgeCount}, "
            + $"Exception: {exceptionEdgeCount}");

        Console.WriteLine($"{indent}  Basic Blocks:");
        foreach (var block in graph.Blocks)
        {
            var states = new List<string>();
            if (block == graph.Entry)
            {
                states.Add("entry");
            }

            if (graph.ExitBlocks.Contains(block))
            {
                states.Add("exit");
            }

            if (!block.IsReachable)
            {
                states.Add("unreachable");
            }

            var state = states.Count == 0
                ? "reachable"
                : string.Join(", ", states);
            var successors = graph.Edges
                .Where(edge => edge.Source == block)
                .Select(FormatEdgeTarget)
                .ToList();

            Console.WriteLine(
                $"{indent}    {block.Id,-4} "
                + $"[{block.StartOffset:X4}..{block.EndOffset:X4}) "
                + $"{state}; instructions={block.Instructions.Count}; "
                + $"successors="
                + (successors.Count == 0
                    ? "<exit>"
                    : string.Join(", ", successors)));
        }

        Console.WriteLine($"{indent}  Edges:");
        foreach (var edge in graph.Edges)
        {
            var detail = edge.Detail is null ? string.Empty : $" ({edge.Detail})";
            var backward = edge.IsBackward ? " [backward]" : string.Empty;
            Console.WriteLine(
                $"{indent}    {edge.Source.Id} -> {edge.Target.Id}: "
                + $"{edge.Kind}{detail}{backward}");
        }
    }

    private static string FormatEdgeTarget(ControlFlowEdge edge)
    {
        var detail = edge.Detail is null ? string.Empty : $":{edge.Detail}";
        return $"{edge.Kind}{detail}->{edge.Target.Id}";
    }

    private void PrintRoundTrip(CodeAttribute code, string indent)
    {
        var body = MethodBody.FromCodeAttribute(code, _cf.ConstantPool);
        var constantPool = new ConstantPoolBuilder();
        var regenerated = body.ToCodeAttribute(constantPool);
        var identical = code.Code.SequenceEqual(regenerated.Code);

        Console.WriteLine($"{indent}Round-Trip:");
        if (identical)
        {
            Console.WriteLine($"{indent}  PASS: Bytecode is identical.");
        }
        else if (code.Code.Length == regenerated.Code.Length)
        {
            Console.WriteLine(
                $"{indent}  DIFF: Same length but bytes differ "
                + "(constant-pool indices may vary).");
        }
        else
        {
            Console.WriteLine(
                $"{indent}  LENGTH DIFF: Original={code.Code.Length}, "
                + $"Regenerated={regenerated.Code.Length}.");
        }

        Console.WriteLine(
            $"{indent}  Instructions={body.Instructions.Count}, "
            + $"TryCatch={body.TryCatchBlocks.Count}, "
            + $"CP entries={constantPool.Count}");
    }
}
