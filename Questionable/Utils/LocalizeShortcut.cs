using System.Diagnostics.CodeAnalysis;

namespace Questionable.Utils;

// B-passthrough: I18N.DotNet is unavailable on the TC net9/API12 NuGet feed, so _L/_LF
// become identity passthroughs. Every upstream call site compiles unchanged and the
// English source strings are used verbatim (the TC client localizes game text itself).
[SuppressMessage("Style", "IDE1006:Naming Styles")]
internal static class LocalizeShortcut
{
    // no-op: the passthrough shim needs no I18N.DotNet initialization; upstream calls this at startup.
    internal static void Initialize(Questionable.Configuration configuration) { }
    internal static string _L(string input) => input;
    internal static string _LF(string input, params object[] args) => args.Length == 0 ? input : string.Format(input, args);
}
