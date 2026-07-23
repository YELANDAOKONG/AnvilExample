using System.Globalization;

using Anvil.Instructions;

namespace AnvilExample;

public static class BytecodeFormatter
{
    public static string FormatOperands(Instruction instruction)
    {
        ArgumentNullException.ThrowIfNull(instruction);

        return instruction switch
        {
            InsnInstruction => string.Empty,
            IntInstruction integer => integer.Value.ToString(CultureInfo.InvariantCulture),
            VarInstruction variable =>
                variable.VarIndex.ToString(CultureInfo.InvariantCulture),
            IincInstruction increment =>
                $"{increment.VarIndex}, {increment.Increment}",
            LdcInstruction constant => FormatConstant(constant),
            FieldInstruction field => FormatField(field),
            MethodInstruction method => FormatMethod(method),
            InvokeDynamicInstruction dynamic => FormatInvokeDynamic(dynamic),
            TypeInstruction type => type.Type ?? $"#{type.TypeIndex}",
            MultiANewArrayInstruction array =>
                $"{array.Type ?? $"#{array.TypeIndex}"} dims={array.Dimensions}",
            JumpInstruction jump => $"-> {jump.Target}",
            TableSwitchInstruction tableSwitch => FormatTableSwitch(tableSwitch),
            LookupSwitchInstruction lookupSwitch => FormatLookupSwitch(lookupSwitch),
            _ => string.Empty
        };
    }

    public static string FormatRawBytes(
        byte[] code,
        Instruction instruction,
        int endOffset)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(instruction);

        var offset = instruction.Offset
            ?? throw new InvalidOperationException(
                $"Instruction {instruction.OpCode} has no resolved offset.");
        if (offset < 0
            || endOffset <= offset
            || endOffset > code.Length)
        {
            throw new InvalidOperationException(
                $"Instruction {instruction.OpCode} range [{offset}, {endOffset}) "
                + $"is outside the {code.Length}-byte code array.");
        }

        return string.Join(
            " ",
            code.AsSpan(offset, endOffset - offset)
                .ToArray()
                .Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
    }

    private static string FormatConstant(LdcInstruction instruction)
    {
        return instruction.Value switch
        {
            string text => Quote(text),
            float value => $"{value.ToString(CultureInfo.InvariantCulture)}f",
            long value => $"{value.ToString(CultureInfo.InvariantCulture)}L",
            double value => $"{value.ToString(CultureInfo.InvariantCulture)}d",
            null => $"#{instruction.ConstantIndex}",
            var value => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    private static string FormatField(FieldInstruction instruction)
    {
        return instruction.Owner is null
            ? $"#{instruction.FieldRefIndex}"
            : $"{instruction.Owner}.{instruction.Name} {instruction.Descriptor}";
    }

    private static string FormatMethod(MethodInstruction instruction)
    {
        return instruction.Owner is null
            ? $"#{instruction.MethodRefIndex}"
            : $"{instruction.Owner}.{instruction.Name} {instruction.Descriptor}";
    }

    private static string FormatInvokeDynamic(InvokeDynamicInstruction instruction)
    {
        return instruction.Name is null
            ? $"#{instruction.ConstantIndex}"
            : $"bootstrap={instruction.BootstrapMethodAttrIndex} "
              + $"{instruction.Name} {instruction.Descriptor}";
    }

    private static string FormatTableSwitch(TableSwitchInstruction instruction)
    {
        var cases = instruction.Targets.Select(
            (target, index) => $"{instruction.Low + index}->{target}");
        return $"default->{instruction.DefaultTarget}; {string.Join(", ", cases)}";
    }

    private static string FormatLookupSwitch(LookupSwitchInstruction instruction)
    {
        var cases = instruction.Pairs.Select(pair => $"{pair.Key}->{pair.Target}");
        return $"default->{instruction.DefaultTarget}; {string.Join(", ", cases)}";
    }

    private static string Quote(string value)
    {
        var escaped = value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\t", "\\t", StringComparison.Ordinal);
        return $"\"{escaped}\"";
    }
}
