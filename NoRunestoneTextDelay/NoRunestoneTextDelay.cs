using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace NoRunestoneTextDelay;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
//[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
internal class NoRunestoneTextDelay : BaseUnityPlugin
{
    public const string PluginGUID = "orax.NoRunestoneTextDelay";
    public const string PluginName = "NoRunestoneTextDelay";
    public const string PluginVersion = "0.1.0";

    internal static ManualLogSource Log;
    private Harmony _harmony;

    private void Awake()
    {
        Log = base.Logger;
        Log.LogInfo("NoRunestoneTextDelay has landed");

        _harmony = new Harmony(PluginGUID);
        _harmony.PatchAll();
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
    }
}
