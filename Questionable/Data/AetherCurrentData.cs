using System.Collections.Immutable;
using System.Linq;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
namespace Questionable.Data;

internal sealed class AetherCurrentData(IDataManager dataManager)
{
    private readonly ImmutableDictionary<uint, ImmutableList<uint>> _overworldCurrents = dataManager.GetExcelSheet<AetherCurrentCompFlgSet>()
        .Where(x => x.RowId > 0)
        .Where(x => x.Territory.IsValid)
        .ToImmutableDictionary(
            x => x.Territory.RowId,
            x => x.AetherCurrents
                // API12 / 7.1: skip refs whose AetherCurrent row is missing in 7.1 data
                // (RowRef.Value throws InvalidOperationException; ValueNullable returns null).
                .Where(y => y.RowId > 0 && y.ValueNullable is not null && y.ValueNullable.Value.Quest.RowId == 0)
                .Select(y => y.RowId)
                .ToImmutableList());

    public bool IsValidAetherCurrent(uint territoryId, uint aetherCurrentId)
    {
        return _overworldCurrents.TryGetValue(territoryId, out ImmutableList<uint>? currentIds) &&
               currentIds.Contains(aetherCurrentId);
    }
}
