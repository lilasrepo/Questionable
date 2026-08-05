using System.Diagnostics.CodeAnalysis;
using Lumina.Excel.Sheets;

namespace Questionable.Utils;

// B-passthrough: I18N.DotNet is unavailable on the TC net9/API13 NuGet feed, so _L/_LF
// become identity passthroughs. Every upstream call site compiles unchanged and the
// English source strings are used verbatim (the TC client localizes game text itself).
//
// _T is deliberately NOT a passthrough. Despite living next to _L/_LF it never touches
// I18N.DotNet -- it reads the *game's own* localized Excel text for a row id, which works
// on TC exactly as upstream intends and is in fact more useful here. Only difference from
// upstream: this Lumina exposes ExtractText() rather than ToMacroString() (the helper the
// rest of this tree already uses).
[SuppressMessage("Style", "IDE1006:Naming Styles")]
internal static class LocalizeShortcut
{
    private static IDataManager _dataManager = null!;
    private static IClientState _clientState = null!;

    // Signature tracks upstream, which grew dataManager/clientState parameters for _T. The
    // configuration argument is unused here -- it only drove I18N.DotNet debug wrapping.
    internal static void Initialize(Configuration configuration, IDataManager dataManager,
        IClientState clientState)
    {
        _dataManager = dataManager;
        _clientState = clientState;
    }

    internal static string _L(string input) => input;

    internal static string _LF(string input, params object[] args) =>
        args.Length == 0 ? input : string.Format(input, args);

    private static readonly Dictionary<(Type, uint), string> _translatedStrings = [];

    public static string _T<T>(uint rowId) where T : struct, IExcelRow<T>
    {
        if (_translatedStrings.TryGetValue((typeof(T), rowId), out var match))
            return match;

        string value = _dataManager.GetExcelSheet<T>(_clientState.ClientLanguage).GetRow(rowId) switch
        {
            JournalGenre g => g.Name.ExtractText(),
            JournalCategory c => c.Name.ExtractText(),
            Addon a => a.Text.ExtractText(),
            ExVersion v => v.Name.ExtractText(),
            _ => throw new InvalidOperationException($"No known Name/Text mapping for {typeof(T).Name}")
        };
        _translatedStrings[(typeof(T), rowId)] = value;
        return value;
    }
}
