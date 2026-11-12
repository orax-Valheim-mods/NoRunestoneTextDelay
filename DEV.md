# Getting the current state's `shortNameHash`

`AnimatorStateInfo` exposes only hashes — Unity has no runtime API that returns a state
name as a string. To know which state the animator is in, log `shortNameHash` and compare
it with `Animator.StringToHash("state name")`.

## Example

```csharp
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace NoRunestoneTextDelay;

[HarmonyPatch(typeof(TextViewer), nameof(TextViewer.ShowText))]
internal static class TextViewer_ShowText_Patch
{
    internal static void Prefix(TextViewer __instance)
    {
        __instance.StartCoroutine(LogState(__instance.m_animator));
    }

    private static IEnumerator LogState(Animator anim)
    {
        yield return new WaitForSeconds(1f);
        NoRunestoneTextDelay.Log.LogInfo(
            $"After 1s: state hash={anim.GetCurrentAnimatorStateInfo(0).shortNameHash}."
        );
    }
}
```

## Why the coroutine is required

`GetCurrentAnimatorStateInfo` reflects the animator's last update. Harmony `Prefix` and
`Postfix` run in the same call stack as `TextViewer.ShowText`, without a single frame
elapsing in between: `ShowText` only calls `SetBool("visible", true)`, and the animator
evaluates the resulting transition at its own next update (later in the same frame or
the next one). A synchronous read therefore always shows the previous state —
`textview_hideow`, hash `1819695195` — which looks like a failure but is expected. Wait
at least one frame before reading; a plain `1 s` is simple and reliable whatever the
update order.

## Checking the hash

```csharp
var state = anim.GetCurrentAnimatorStateInfo(0);
bool showing = state.shortNameHash == Animator.StringToHash("textview_show");
// Equivalent: state.IsName("textview_show"). Names are never available as strings.
```

Known hashes of the `RuneText` controller states (from its `m_TOS` hash → string table):

| Hash | State |
| --- | --- |
| 1412881455 | `textview_show` |
| 1819695195 | `textview_hideow` |
| 1137270853 | `textview_start` |

State names cannot be enumerated from game code either: only parameter names
(`Animator.parameters`) and clip names (`RuntimeAnimatorController.animationClips`) come
as strings. The state names live in the `AnimatorController` asset (`m_TOS` table), read
offline with UnityPy, AssetStudio or UABEA from the bundle
(`valheim_Data/StreamingAssets/SoftRef/Bundles/d59cfac` holds `IngameGui_TextViewer` and
its `RuneText` controller), then hardcoded in the patch.

## Finding the name from a hash with UABEANext

The reverse lookup works too: given a hash from the in-game log (for example
`1412881455`), UABEANext recovers the original string without knowing it in advance —
`m_TOS` is stored as pairs of `first` (hash) and `second` (name).

1. Open the bundle (`valheim_Data/StreamingAssets/SoftRef/Bundles/d59cfac`) in
   UABEANext; its asset list shows every asset with Name, Type and Path ID columns.
2. Type `RuneText` in the "Asset name search" box and select the `AnimatorController`
   row.
3. Click **Edit Data** in the toolbar (or right-click the row → **Edit Data**) to open
   the asset's serialized data.
4. Expand `m_TOS` and look for `first = 1412881455`: the matching `second` is
   `textview_show`.

Alternatively, select the row and click **Export**, save as "UABEA json dump" or "UABE
txt dump", and search for `1412881455` in the exported file with any text editor — the
name is part of the same entry.

This closes the loop: the log gives the hash, UABEANext turns it back into the string,
and the string is what the patch hardcodes.
