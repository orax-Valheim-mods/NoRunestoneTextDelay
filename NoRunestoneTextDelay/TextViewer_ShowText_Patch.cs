using HarmonyLib;

namespace NoRunestoneTextDelay;

[HarmonyPatch(typeof(TextViewer), nameof(TextViewer.ShowText))]
internal static class TextViewer_ShowText_Patch
{
    // Short-name hash of the "textview_show" state in the RuneText animator.
    private const int TextViewShowHash = 1412881455;

    internal static void Prefix(TextViewer __instance, TextViewer.Style style, bool autoHide)
    {
        if (!((Player.m_localPlayer == null) & autoHide) && style == TextViewer.Style.Rune)
        {
            __instance.m_animator.Play(TextViewShowHash, 0, 1f);
        }
    }
}
