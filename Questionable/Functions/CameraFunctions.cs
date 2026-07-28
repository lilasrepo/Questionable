//Taken and adapted from https://github.com/awgil/ffxiv_navmesh/blob/master/vnavmesh/Movement/OverrideCamera.cs.

using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
namespace Questionable.Functions;

internal sealed unsafe class CameraFunctions : IDisposable
{
    private readonly ILogger<CameraFunctions> _logger;
    private readonly IObjectTable _objectTable;

    private readonly bool IgnoreUserInput = true; // if true - override even if user tries to change camera orientation, otherwise override only if user does nothing
    // sig walk-back to game 7.1 (matches AutoDuty/OverrideCamera.cs + ffxiv_navmesh/Movement/OverrideCamera.cs).
    [Signature("40 53 48 83 EC 70 44 0F 29 44 24 ?? 48 8B D9")]
    private Hook<RMICameraDelegate> _rmiCameraHook = null!;
    private float DesiredAltitude;
    private float DesiredAzimuth;

    public CameraFunctions(IGameInteropProvider gameInteropProvider, ILogger<CameraFunctions> logger, IObjectTable objectTable)
    {
        _logger = logger;
        gameInteropProvider.InitializeFromAttributes(this);
        _objectTable = objectTable;
    }

    public bool Enabled
    {
        get => _rmiCameraHook.IsEnabled;
        set
        {
            if (value)
                _rmiCameraHook.Enable();
            else
                _rmiCameraHook.Disable();
        }
    }

    public void Dispose() => _rmiCameraHook.Dispose();

    private static float Deg2Rad(int degrees) => degrees * ((float)Math.PI / 180f);

    // from https://github.com/NightmareXIV/ECommons/blob/master/ECommons/MathHelpers/Angle.cs
    private static float Normalized(float r)
    {
        while (r < -MathF.PI)
        {
            r += 2 * MathF.PI;
        }

        while (r > MathF.PI)
        {
            r -= 2 * MathF.PI;
        }

        return r;
    }


    internal void Face(Vector3 pos)
    {
        _logger.LogDebug("Facing " + pos);
        Enabled = true;
        if (_objectTable[0] == null)
            return;
        Vector3 diff = pos - _objectTable[0]!.Position;
        DesiredAzimuth = MathF.Atan2(diff.X, diff.Z) + Deg2Rad(180);
        DesiredAltitude = Deg2Rad(-30);
    }

    private void RMICameraDetour(Camera* self, int inputMode, float speedH, float speedV)
    {
        // B1: API12 Camera struct lacks DirH/DirV/InputDeltaH/InputDeltaV (game-7.5 fields).
        // Pass through to original; auto-camera-face during quests is disabled.
        // TODO(api12): port via raw struct offsets if camera-face becomes critical.
        _rmiCameraHook.Original(self, inputMode, speedH, speedV);
        Enabled = false;
    }

    private delegate void RMICameraDelegate(Camera* self, int inputMode, float speedH, float speedV);
}
