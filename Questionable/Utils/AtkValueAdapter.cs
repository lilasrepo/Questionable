using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Component.GUI;
using AtkValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;
namespace Questionable.Utils;

internal static class AtkValueAdapter
{
    public static unsafe string? ReadString(AtkValue value)
    {
        if (value.Type == AtkValueType.Undefined)
            return null;

        if (value.String.HasValue)
            return MemoryHelper.ReadSeStringNullTerminated(new(value.String)).WithCertainMacroCodeReplacements();

        return null;
    }
}
