using System;
using Dalamud.Plugin.Services;
using Dalamud.Game.NativeWrapper;
using FFXIVClientStructs.FFXIV.Component.GUI;
namespace Questionable.Utils;

[RegisterSingleton<IGameGuiAdapter, GameGuiAdapter>]
internal sealed unsafe class GameGuiAdapter(IGameGui gameGui) : IGameGuiAdapter
{
    public bool TryGetAddonByName(string name, out AtkUnitBase* addon)
    {
        IntPtr a = gameGui.GetAddonByName(name, 1);
        if (a != IntPtr.Zero)
        {
            addon = (AtkUnitBase*)a;
            return true;
        }

        addon = null;
        return false;
    }

    public bool TryGetAddonByName<TAddon>(string name, out TAddon* addon) where TAddon : unmanaged
    {
        IntPtr a = gameGui.GetAddonByName(name, 1);
        if (a != IntPtr.Zero)
        {
            addon = (TAddon*)a;
            return true;
        }

        addon = null;
        return false;
    }
}
