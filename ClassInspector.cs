using System.Text;
using Spectre.Console;
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

public class ClassInspector
{
    private readonly ClassFile _cf;

    public ClassInspector(ClassFile classFile)
    {
        _cf = classFile;
    }

    private bool _showInstructions;
    private bool _roundTrip;

    public void Display(bool showInstructions = false, bool roundTrip = false)
    {
        _showInstructions = showInstructions;
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
        var grid = new Grid();
        grid.AddColumn(new GridColumn().NoWrap().PadRight(4).Width(15));
        grid.AddColumn(new GridColumn().PadRight(2));
        grid.AddColumn(new GridColumn());

        var magicStatus = _cf.Magic.Value == 0xCAFEBABE ? "[green](Valid)[/]" : "[red](Invalid)[/]";
        var javaVersion = _cf.MajorVersion - 44;

        grid.AddRow("[bold]Magic[/]", $"0x{_cf.Magic.Value:X8}", magicStatus);
        grid.AddRow("[bold]Version[/]", $"{_cf.MajorVersion}.{_cf.MinorVersion}", $"(Java {javaVersion})");
        grid.AddRow("[bold]Access Flags[/]", $"[yellow]{_cf.AccessFlags}[/]", "");
        
        var thisClass = ResolveClass(_cf.ThisClass);
        var superClass = ResolveClass(_cf.SuperClass);
        
        grid.AddRow("[bold]This Class[/]", $"#{_cf.ThisClass}", $"[blue]{Markup.Escape(thisClass)}[/]");
        grid.AddRow("[bold]Super Class[/]", $"#{_cf.SuperClass}", $"[blue]{Markup.Escape(superClass)}[/]");

        var panel = new Panel(grid)
            .Header("Class File Header")
            .BorderColor(Color.Cyan1)
            .Expand();

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private void PrintConstantPool()
    {
        var table = new Table();
        table.Title($"Constant Pool ({_cf.ConstantPoolCount} entries)");
        table.Border(TableBorder.Rounded);
        table.AddColumn("Idx");
        table.AddColumn("Tag");
        table.AddColumn("Value / Details");

        for (int i = 1; i < _cf.ConstantPool.Length; i++)
        {
            var entry = _cf.ConstantPool[i];
            if (entry == null) continue;

            var details = GetConstantPoolDetails(entry);
            table.AddRow(
                new Markup($"[grey]#{i}[/]"), 
                new Markup($"[olive]{entry.Tag}[/]"), 
                details);
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }

    private Markup GetConstantPoolDetails(CpInfo entry)
    {
        var text = entry switch
        {
            CpUtf8 utf8 => $"\"[cyan]{Markup.Escape(utf8.Value)}[/]\"",
            CpInteger i4 => $"[yellow]{i4.Bytes.Value}[/]",
            CpFloat f4 => $"[yellow]{f4.Bytes.Value}f[/]",
            CpLong i8 => $"[yellow]{i8.Bytes.Value}L[/]",
            CpDouble f8 => $"[yellow]{f8.Bytes.Value}d[/]",
            CpClass cls => $"#{cls.NameIndex} // [blue]{Markup.Escape(ResolveUtf8(cls.NameIndex))}[/]",
            CpString str => $"#{str.StringIndex} // \"[cyan]{Markup.Escape(ResolveUtf8(str.StringIndex))}[/]\"",
            CpFieldRef fref => $"#{fref.ClassIndex}.#{fref.NameAndTypeIndex}",
            CpMethodRef mref => $"#{mref.ClassIndex}.#{mref.NameAndTypeIndex}",
            CpInterfaceMethodRef imref => $"#{imref.ClassIndex}.#{imref.NameAndTypeIndex}",
            CpNameAndType nt => $"#{nt.NameIndex}:#{nt.DescriptorIndex} // [green]{Markup.Escape(ResolveUtf8(nt.NameIndex))}[/]:[grey]{Markup.Escape(ResolveUtf8(nt.DescriptorIndex))}[/]",
            CpMethodHandle mh => $"Kind: {mh.ReferenceKind}, Ref: #{mh.ReferenceIndex}",
            CpMethodType mt => $"Descriptor: #{mt.DescriptorIndex} // [grey]{Markup.Escape(ResolveUtf8(mt.DescriptorIndex))}[/]",
            CpDynamic dyn => $"Bootstrap: #{dyn.BootstrapMethodAttrIndex}, NameAndType: #{dyn.NameAndTypeIndex}",
            CpInvokeDynamic idyn => $"Bootstrap: #{idyn.BootstrapMethodAttrIndex}, NameAndType: #{idyn.NameAndTypeIndex}",
            CpModule mod => $"#{mod.NameIndex} // [blue]{Markup.Escape(ResolveUtf8(mod.NameIndex))}[/]",
            CpPackage pkg => $"#{pkg.NameIndex} // [blue]{Markup.Escape(ResolveUtf8(pkg.NameIndex))}[/]",
            _ => $"[grey]({entry.GetType().Name})[/]"
        };
        return new Markup(text);
    }

    private void PrintInterfaces()
    {
        if (_cf.InterfacesCount.Value == 0) return;

        var tree = new Tree($"[bold]Interfaces ({_cf.InterfacesCount})[/]");
        foreach (var ifaceIndex in _cf.Interfaces)
        {
            tree.AddNode($"#{ifaceIndex} [blue]{Markup.Escape(ResolveClass(ifaceIndex))}[/]");
        }
        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();
    }

    private void PrintFields()
    {
        if (_cf.FieldsCount.Value == 0) return;

        var tree = new Tree($"[bold]Fields ({_cf.FieldsCount})[/]");
        foreach (var field in _cf.Fields)
        {
            var name = ResolveUtf8(field.NameIndex);
            var desc = ResolveUtf8(field.DescriptorIndex);
            var node = tree.AddNode($"[gold1]{field.AccessFlags}[/] [green]{Markup.Escape(name)}[/] [grey]{Markup.Escape(desc)}[/]");
            
            foreach (var attr in field.Attributes)
            {
                AddAttributeNode(node, attr);
            }
        }
        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();
    }

    private void PrintMethods()
    {
        if (_cf.MethodsCount.Value == 0) return;

        var tree = new Tree($"[bold]Methods ({_cf.MethodsCount})[/]");
        foreach (var method in _cf.Methods)
        {
            var name = ResolveUtf8(method.NameIndex);
            var desc = ResolveUtf8(method.DescriptorIndex);
            var node = tree.AddNode($"[gold1]{method.AccessFlags}[/] [green]{Markup.Escape(name)}[/] [grey]{Markup.Escape(desc)}[/]");

            foreach (var attr in method.Attributes)
            {
                AddAttributeNode(node, attr);
            }
        }
        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();
    }

    private void PrintClassAttributes()
    {
        if (_cf.AttributesCount.Value == 0) return;

        var tree = new Tree($"[bold]Class Attributes ({_cf.AttributesCount})[/]");
        foreach (var attr in _cf.Attributes)
        {
            AddAttributeNode(tree, attr);
        }
        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();
    }

    private void AddAttributeNode(IHasTreeNodes parentNode, AttributeInfo attr)
    {
        var attrName = ResolveUtf8(attr.AttributeNameIndex);
        var body = attr.ResolveBody(_cf.ConstantPool);

        if (body == null)
        {
            parentNode.AddNode($"[red]{Markup.Escape(attrName)}[/] (Raw Length: {attr.AttributeLength})");
            return;
        }

        var node = parentNode.AddNode($"[cyan]{Markup.Escape(attrName)}[/]");
        FormatAttributeBody(node, body);
    }

    private void FormatAttributeBody(IHasTreeNodes node, IAttribute body)
    {
        switch (body)
        {
            case ConstantValueAttribute cv:
                node.AddNode($"Value Index: #{cv.ConstantValueIndex} ({GetConstantValue(cv.ConstantValueIndex)})");
                break;

            case CodeAttribute code:
                node.AddNode($"MaxStack: [yellow]{code.MaxStack}[/], MaxLocals: [yellow]{code.MaxLocals}[/], Code Size: [yellow]{code.Code.Length}[/] bytes");
                if (_showInstructions && code.Code.Length > 0)
                {
                    PrintInstructions(node, code);
                }
                if (_roundTrip && code.Code.Length > 0)
                {
                    PrintRoundTrip(node, code);
                }
                if (code.Attributes.Length > 0)
                {
                    var subNode = node.AddNode("Sub-Attributes");
                    foreach (var subAttr in code.Attributes) AddAttributeNode(subNode, subAttr);
                }
                break;

            case StackMapTableAttribute smt:
                var smtNode = node.AddNode($"Entries: {smt.Entries.Length}");
                FormatStackMapTable(smtNode, smt);
                break;

            case ExceptionsAttribute ex:
                var exNode = node.AddNode("Throws");
                foreach (var idx in ex.ExceptionIndexTable) exNode.AddNode(Markup.Escape(ResolveClass(idx)));
                break;

            case InnerClassesAttribute inner:
                var innerNode = node.AddNode($"Inner Classes ({inner.NumberOfClasses})");
                foreach (var cls in inner.Classes)
                {
                    innerNode.AddNode($"{Markup.Escape(ResolveClass(cls.InnerClassInfoIndex))} (Outer: {Markup.Escape(ResolveClass(cls.OuterClassInfoIndex))}) - [yellow]{cls.InnerClassAccessFlags}[/]");
                }
                break;

            case EnclosingMethodAttribute enc:
                node.AddNode($"Class: {Markup.Escape(ResolveClass(enc.ClassIndex))}");
                if (enc.MethodIndex.Value != 0)
                    node.AddNode($"Method: {Markup.Escape(ResolveNameAndType(enc.MethodIndex))}");
                break;

            case SyntheticAttribute:
                node.AddNode("[grey](Synthetic)[/]");
                break;

            case DeprecatedAttribute:
                node.AddNode("[red](Deprecated)[/]");
                break;

            case SignatureAttribute sig:
                node.AddNode($"Signature: [grey]{Markup.Escape(ResolveUtf8(sig.SignatureIndex))}[/]");
                break;

            case SourceFileAttribute src:
                node.AddNode($"Source File: [green]{Markup.Escape(ResolveUtf8(src.SourceFileIndex))}[/]");
                break;

            case LineNumberTableAttribute lnt:
                var lntTable = new Table().NoBorder().HideHeaders();
                lntTable.AddColumn("PC");
                lntTable.AddColumn("Line");
                foreach (var entry in lnt.LineNumberTable)
                    lntTable.AddRow($"PC {entry.StartPc}", $"Line {entry.LineNumber}");
                node.AddNode(lntTable);
                break;

            case LocalVariableTableAttribute lvt:
                var lvtTable = new Table().Border(TableBorder.Rounded);
                lvtTable.AddColumns("Slot", "Name", "Descriptor", "Range");
                foreach (var entry in lvt.LocalVariableTable)
                {
                    lvtTable.AddRow(
                        entry.Index.ToString(),
                        Markup.Escape(ResolveUtf8(entry.NameIndex)),
                        Markup.Escape(ResolveUtf8(entry.DescriptorIndex)),
                        $"{entry.StartPc}-{entry.StartPc + entry.Length}"
                    );
                }
                node.AddNode(lvtTable);
                break;

            case LocalVariableTypeTableAttribute lvtt:
                var lvttTable = new Table().Border(TableBorder.Rounded);
                lvttTable.AddColumns("Slot", "Name", "Signature", "Range");
                foreach (var entry in lvtt.LocalVariableTypeTable)
                {
                    lvttTable.AddRow(
                        entry.Index.ToString(),
                        Markup.Escape(ResolveUtf8(entry.NameIndex)),
                        Markup.Escape(ResolveUtf8(entry.SignatureIndex)),
                        $"{entry.StartPc}-{entry.StartPc + entry.Length}"
                    );
                }
                node.AddNode(lvttTable);
                break;

            case BootstrapMethodsAttribute bsm:
                var bsmNode = node.AddNode($"Bootstrap Methods ({bsm.NumBootstrapMethods})");
                for (int i = 0; i < bsm.BootstrapMethods.Length; i++)
                {
                    var m = bsm.BootstrapMethods[i];
                    var args = string.Join(", ", m.BootstrapArguments.Select(a => "#" + a.Value));
                    bsmNode.AddNode($"[[{i}]] Ref: #{m.BootstrapMethodRef}, Args: [[{args}]]");
                }
                break;

            case MethodParametersAttribute mpa:
                var mpTable = new Table().NoBorder().HideHeaders();
                mpTable.AddColumn("Name");
                mpTable.AddColumn("Flags");
                foreach (var p in mpa.Parameters)
                {
                    var pName = p.NameIndex.Value == 0 ? "<unnamed>" : ResolveUtf8(p.NameIndex);
                    mpTable.AddRow(Markup.Escape(pName), p.AccessFlags.ToString());
                }
                node.AddNode(mpTable);
                break;

            case ModuleAttribute mod:
                FormatModuleDetails(node, mod);
                break;

            case ModulePackagesAttribute mpkg:
                node.AddNode($"Packages: {string.Join(", ", mpkg.PackageIndex.Select(idx => Markup.Escape(ResolvePackage(idx))))}");
                break;

            case ModuleMainClassAttribute mmc:
                node.AddNode($"Main Class: {Markup.Escape(ResolveClass(mmc.MainClassIndex))}");
                break;

            case NestHostAttribute nh:
                node.AddNode($"Host: {Markup.Escape(ResolveClass(nh.HostClassIndex))}");
                break;

            case NestMembersAttribute nm:
                node.AddNode($"Members: {string.Join(", ", nm.Classes.Select(idx => Markup.Escape(ResolveClass(idx))))}");
                break;

            case RecordAttribute rec:
                var recNode = node.AddNode($"Components ({rec.ComponentsCount})");
                foreach (var comp in rec.Components)
                {
                    var cNode = recNode.AddNode($"{Markup.Escape(ResolveUtf8(comp.NameIndex))} : {Markup.Escape(ResolveUtf8(comp.DescriptorIndex))}");
                    foreach (var attr in comp.Attributes) AddAttributeNode(cNode, attr);
                }
                break;

            case PermittedSubclassesAttribute perm:
                node.AddNode($"Permitted: {string.Join(", ", perm.Classes.Select(idx => Markup.Escape(ResolveClass(idx))))}");
                break;

            case RuntimeVisibleAnnotationsAttribute rva:
                FormatAnnotations(node, rva.Annotations);
                break;
            case RuntimeInvisibleAnnotationsAttribute ria:
                FormatAnnotations(node, ria.Annotations);
                break;
            case AnnotationDefaultAttribute ad:
                node.AddNode($"Default: {FormatElementValue(ad.DefaultValue)}");
                break;

            default:
                node.AddNode($"[grey](Not fully implemented printer for {body.GetType().Name})[/]");
                break;
        }
    }

    private void FormatModuleDetails(IHasTreeNodes node, ModuleAttribute mod)
    {
        node.AddNode($"Name: [blue]{Markup.Escape(ResolveModule(mod.ModuleNameIndex))}[/] {mod.ModuleFlags} v{Markup.Escape(ResolveUtf8(mod.ModuleVersionIndex))}");

        if (mod.Requires.Length > 0)
        {
            var reqNode = node.AddNode("Requires");
            foreach (var req in mod.Requires)
                reqNode.AddNode($"{Markup.Escape(ResolveModule(req.RequiresIndex))} ({req.RequiresFlags}) v{Markup.Escape(ResolveUtf8(req.RequiresVersionIndex))}");
        }

        if (mod.Exports.Length > 0)
        {
            var expNode = node.AddNode("Exports");
            foreach (var exp in mod.Exports)
            {
                var text = $"{Markup.Escape(ResolvePackage(exp.ExportsIndex))} ({exp.ExportsFlags})";
                if (exp.ExportsToCount.Value > 0)
                    text += $" to {string.Join(", ", exp.ExportsToIndex.Select(i => Markup.Escape(ResolveModule(i))))}";
                expNode.AddNode(text);
            }
        }
    }

    private void FormatAnnotations(IHasTreeNodes parent, Annotation[] annotations)
    {
        foreach (var ann in annotations)
        {
            var annNode = parent.AddNode($"@{Markup.Escape(ResolveUtf8(ann.TypeIndex))}");
            foreach (var pair in ann.ElementValuePairs)
            {
                annNode.AddNode($"{Markup.Escape(ResolveUtf8(pair.ElementNameIndex))} = {FormatElementValue(pair.Value)}");
            }
        }
    }

    private string FormatElementValue(ElementValue val)
    {
        return val switch
        {
            ConstElementValue c => GetConstantValue(c.ConstValueIndex),
            EnumElementValue e => $"{Markup.Escape(ResolveUtf8(e.TypeNameIndex))}.{Markup.Escape(ResolveUtf8(e.ConstNameIndex))}",
            ClassElementValue c => $"{Markup.Escape(ResolveUtf8(c.ClassInfoIndex))}.class",
            ArrayElementValue a => $"{{ {string.Join(", ", a.Values.Select(FormatElementValue))} }}",
            AnnotationElementValue av => $"@{Markup.Escape(ResolveUtf8(av.Annotation.TypeIndex))}(...)",
            _ => "?"
        };
    }

    private void FormatStackMapTable(IHasTreeNodes parent, StackMapTableAttribute smt)
    {
        int frameIndex = 0;
        foreach (var frame in smt.Entries)
        {
            var text = frame switch
            {
                SameFrame same => $"[grey]Same[/] (Delta: {same.FrameType})",
                SameLocals1StackItemFrame s1 => $"[grey]SameLocals1Stack[/] (Delta: {s1.FrameType - 64}) Stack: {FormatVerificationType(s1.Stack)}",
                SameLocals1StackItemFrameExtended s1e => $"[grey]SameLocals1StackExt[/] (Delta: {s1e.OffsetDelta.Value}) Stack: {FormatVerificationType(s1e.Stack)}",
                ChopFrame chop => $"[grey]Chop {251 - chop.FrameType}[/] (Delta: {chop.OffsetDelta.Value})",
                SameFrameExtended se => $"[grey]SameExt[/] (Delta: {se.OffsetDelta.Value})",
                AppendFrame app => $"[grey]Append {app.FrameType - 251}[/] (Delta: {app.OffsetDelta.Value}) Locals: {FormatVerificationTypes(app.Locals)}",
                FullFrame full => $"[grey]Full[/] (Delta: {full.OffsetDelta.Value}) L:{FormatVerificationTypes(full.Locals)} S:{FormatVerificationTypes(full.Stack)}",
                _ => $"Unknown Frame {frame.FrameType}"
            };
            
            parent.AddNode($"[[{frameIndex++}]] {text}");
        }
    }


    private string FormatVerificationTypes(VerificationTypeInfo[] types)
    {
        if (types.Length == 0) return "[[]]";
        return "[[" + string.Join(", ", types.Select(FormatVerificationType)) + "]]";
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
            ObjectVariableInfo obj => $"Obj(#{obj.CPoolIndex} {Markup.Escape(ResolveClass(obj.CPoolIndex))})",
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
            CpUtf8 u => $"\"{Markup.Escape(u.Value)}\"",
            CpInteger i => i.Bytes.Value.ToString(),
            CpFloat f => $"{f.Bytes.Value}f",
            CpLong l => $"{l.Bytes.Value}L",
            CpDouble d => $"{d.Bytes.Value}d",
            CpString s => $"\"{Markup.Escape(ResolveUtf8(s.StringIndex))}\"",
            _ => $"#{index.Value}"
        };
    }

    private void PrintInstructions(IHasTreeNodes parentNode, CodeAttribute code)
    {
        var body = MethodBody.FromCodeAttribute(code, _cf.ConstantPool);
        var insnNode = parentNode.AddNode("[bold aqua]Instructions[/]");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumns("PC", "OpCode", "Operands", "Labels");

        foreach (var insn in body.Instructions)
        {
            var pc = insn.Offset?.ToString("X4") ?? "????";
            var labels = insn.Labels.Count > 0
                ? $"[teal]{string.Join(", ", insn.Labels.Select(l => Markup.Escape(l.ToString())))}[/]"
                : "";

            var (opcode, operands) = FormatInstruction(insn);

            table.AddRow(
                new Markup($"[grey]{pc}[/]"),
                new Markup($"[yellow]{opcode}[/]"),
                new Markup(operands),
                new Markup(labels));
        }

        insnNode.AddNode(table);

        if (body.TryCatchBlocks.Count > 0)
        {
            var tcNode = insnNode.AddNode("[bold fuchsia]TryCatchBlocks[/]");
            foreach (var block in body.TryCatchBlocks)
            {
                var type = block.CatchType ?? "finally";
                tcNode.AddNode(
                    $"try [[[teal]{Markup.Escape(block.Start.ToString())}[/]..[teal]{Markup.Escape(block.End.ToString())}[/]) " +
                    $"catch([green]{Markup.Escape(type)}[/]) -> [teal]{Markup.Escape(block.Handler.ToString())}[/]");
            }
        }
    }

    private static (string OpCode, string Operands) FormatInstruction(Instruction insn)
    {
        switch (insn)
        {
            case InsnInstruction:
                return (insn.OpCode.ToString(), "");

            case IntInstruction i:
                return (insn.OpCode.ToString(), i.Value.ToString());

            case VarInstruction v:
                return (insn.OpCode.ToString(), v.VarIndex.ToString());

            case IincInstruction iinc:
                return (insn.OpCode.ToString(), $"{iinc.VarIndex}, {iinc.Increment}");

            case LdcInstruction ldc:
            {
                var value = ldc.Value switch
                {
                    string s => $"\"{Markup.Escape(s)}\"",
                    float f => $"{f}f",
                    long l => $"{l}L",
                    double d => $"{d}d",
                    null => $"#{ldc.ConstantIndex}",
                    var v => v.ToString()!
                };
                return (insn.OpCode.ToString(), value);
            }

            case FieldInstruction f:
                if (f.Owner != null)
                    return (insn.OpCode.ToString(), $"{Markup.Escape(f.Owner)}.{Markup.Escape(f.Name!)} {Markup.Escape(f.Descriptor!)}");
                return (insn.OpCode.ToString(), $"#{f.FieldRefIndex}");

            case MethodInstruction m:
                if (m.Owner != null)
                    return (insn.OpCode.ToString(), $"{Markup.Escape(m.Owner)}.{Markup.Escape(m.Name!)} {Markup.Escape(m.Descriptor!)}");
                return (insn.OpCode.ToString(), $"#{m.MethodRefIndex}");

            case InvokeDynamicInstruction id:
                return (insn.OpCode.ToString(), $"#{id.BootstrapMethodAttrIndex}");

            case TypeInstruction t:
                if (t.Type != null)
                    return (insn.OpCode.ToString(), Markup.Escape(t.Type));
                return (insn.OpCode.ToString(), $"#{t.TypeIndex}");

            case MultiANewArrayInstruction ma:
                if (ma.Type != null)
                    return (insn.OpCode.ToString(), $"{Markup.Escape(ma.Type)} dims={ma.Dimensions}");
                return (insn.OpCode.ToString(), $"#{ma.TypeIndex} dims={ma.Dimensions}");

            case JumpInstruction j:
                return (insn.OpCode.ToString(), $"-> [teal]{Markup.Escape(j.Target.ToString())}[/]");

            case TableSwitchInstruction ts:
                return (insn.OpCode.ToString(),
                    $"low={ts.Low} high={ts.High} default=[teal]{Markup.Escape(ts.DefaultTarget.ToString())}[/]");

            case LookupSwitchInstruction ls:
                return (insn.OpCode.ToString(),
                    $"npairs={ls.Pairs.Count} default=[teal]{Markup.Escape(ls.DefaultTarget.ToString())}[/]");

            default:
                return (insn.OpCode.ToString(), "");
        }
    }

    private void PrintRoundTrip(IHasTreeNodes parentNode, CodeAttribute code)
    {
        var body = MethodBody.FromCodeAttribute(code, _cf.ConstantPool);
        var cp = new ConstantPoolBuilder();
        var regenerated = body.ToCodeAttribute(cp);

        var originalBytes = code.Code;
        var regeneratedBytes = regenerated.Code;
        var sameLength = originalBytes.Length == regeneratedBytes.Length;

        var rtNode = parentNode.AddNode("[bold lime]Round-Trip[/]");

        if (sameLength)
        {
            var match = true;
            for (var i = 0; i < originalBytes.Length; i++)
            {
                if (originalBytes[i] != regeneratedBytes[i])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                rtNode.AddNode("[green]PASS[/] Bytecode is identical.");
            }
            else
            {
                rtNode.AddNode("[yellow]DIFF[/] Same length but bytes differ (expected — CP indices may vary).");
            }
        }
        else
        {
            rtNode.AddNode($"[yellow]LENGTH DIFF[/] Original: {originalBytes.Length}, Regenerated: {regeneratedBytes.Length}");
        }

        rtNode.AddNode($"Instructions: {body.Instructions.Count}, TryCatch: {body.TryCatchBlocks.Count}, CP entries: {cp.Count}");
    }
}
