using System.Diagnostics.CodeAnalysis;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

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

        // porting-note(api13): upstream's version is GetRow(rowId) with a four-arm switch and a
        // throw on the default arm. Both halves are load-bearing hazards HERE and nowhere upstream:
        //
        //  * the throw. EventInfoComponent's EventQuests is a STATIC FIELD initializer, so an
        //    unmapped sheet type surfaces as TypeInitializationException and the seasonal-event
        //    panel takes QuestWindow.DrawContent down with it. Upstream keeps the switch in step
        //    with its own call sites; this file is PINNED, so every new _T<Sheet> upstream adds
        //    arrives here unmapped. BannerBg did exactly that on 2026-08-30.
        //  * GetRow. It throws on a row id TC's 7.20 sheets do not have, which is a live risk for
        //    seasonal-event rows added in later patches.
        //
        // So: resolve the row leniently, keep an explicit arm per known sheet (exact semantics),
        // and fall back to reflection over any ReadOnlySeString Name/Text column before giving up
        // on a harmless placeholder. A missing event label is a cosmetic loss; a throw is not.
        object? row = _dataManager.GetExcelSheet<T>(_clientState.ClientLanguage).GetRowOrDefault(rowId);
        string value = row switch
        {
            null => $"{typeof(T).Name}#{rowId}",
            JournalGenre g => g.Name.ExtractText(),
            JournalCategory c => c.Name.ExtractText(),
            Addon a => a.Text.ExtractText(),
            ExVersion v => v.Name.ExtractText(),
            BannerBg b => b.Name.ExtractText(),
            ContentRoulette r => r.Name.ExtractText(),
            BeastTribe bt => bt.Name.ExtractText(),
            // api13's FittingShopCategory has no named text column -- the single
            // ReadOnlySeString on the struct is generated as Unknown0.
            FittingShopCategory fs => fs.Unknown0.ExtractText(),
            _ => ExtractAnyText(row) ?? $"{typeof(T).Name}#{rowId}"
        };
        _translatedStrings[(typeof(T), rowId)] = value;
        return value;
    }

    /// <summary>
    ///     Last-resort text lookup for a sheet row this file has no explicit arm for: the first
    ///     ReadOnlySeString field named Name or Text. Keeps an unmapped sheet cosmetic instead of
    ///     fatal, so a refresh that introduces a new _T&lt;Sheet&gt; cannot fail plugin load.
    /// </summary>
    private static string? ExtractAnyText(object row)
    {
        foreach (string name in (string[])["Name", "Text"])
        {
            var field = row.GetType().GetField(name);
            if (field?.GetValue(row) is ReadOnlySeString text)
                return text.ExtractText();
        }

        return null;
    }
}
