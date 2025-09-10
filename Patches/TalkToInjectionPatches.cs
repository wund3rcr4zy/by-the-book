using System.Text;
using System.Text.RegularExpressions;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace ByTheBook.Patches
{
    /// <summary>
    /// Utilities and config used by the patches below.
    /// </summary>
    internal static class TalkToInjection
    {
        // Make public to avoid CS0122 when called from other patches/files.
        public static bool Verbose = false;

        // New: hard mute for all logs (overrides Verbose)
        public static bool Silent = true;

        // Centralized UI label we match/force.
        public static readonly string TalkToUIName = "Talk To";
        public static readonly string InspectUIName = "Inspect";

        /// <summary>
        /// Small helpers so all logging respects the Silent toggle.
        /// </summary>
        internal static void Log(string msg)
        {
            if (Silent) return;
            Debug.Log(msg);
        }

        internal static void VLog(string msg)
        {
            if (Silent || !Verbose) return;
            Debug.Log(msg);
        }

        /// <summary>
        /// Is this human on-duty at a crime-scene call (as guard OR responder)?
        /// Matches GameplayController.enforcerCalls: guard or any id in response list.
        /// </summary>
        public static bool IsOnDutyCrimeSceneResponder(Human target, out string why)
        {
            why = "no gameplay controller";
            if (target == null) { why = "no target"; return false; }

            var gc = GameplayController._instance;
            if (gc == null) return false;

            int id = target.humanID; // Human id per docs.

            // We only consider crime scene calls and 'responding'/'arrived' states.
            // states: logged, responding, arrived, completed
            var calls = gc.enforcerCalls;
            if (calls == null || calls.Count == 0)
            {
                why = "no enforcer calls";
                return false;
            }

            bool anyCrimeCalls = false;
            foreach (var kv in calls)
            {
                var call = kv.Value;
                if (call == null) continue;

                if (!call.isCrimeScene) continue;
                anyCrimeCalls = true;

                var st = call.state;
                bool active =
                    st == GameplayController.EnforcerCallState.responding ||
                    st == GameplayController.EnforcerCallState.arrived;

                if (!active) continue;

                // guard match?
                if (call.guard == id)
                {
                    why = "guard match";
                    return true;
                }

                // responder list match?
                var resp = call.response;
                if (resp != null)
                {
                    for (int i = 0; i < resp.Count; i++)
                    {
                        if (resp[i] == id)
                        {
                            why = "responder match";
                            return true;
                        }
                    }
                }
            }

            if (!anyCrimeCalls)
            {
                why = "no crime-scene calls";
                return false;
            }

            why = "crime-scene calls found, but guard/responder mismatch";
            return false;
        }

        /// <summary>
        /// Get a stable ID for logs (Interactable.id in game data).
        /// </summary>
        private static int GetInteractableId(Interactable inter) => inter != null ? inter.id : 0; // docs: Interactable.id int :contentReference[oaicite:2]{index=2}

        /// <summary>
        /// Normalize a string for loose comparisons (lowercase + collapse whitespace).
        /// </summary>
        private static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = Regex.Replace(s, @"\s+", " ");
            return s.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Determine the display name that the player sees for an action.
        /// Prefer the per-slot override on InteractableCurrentAction; otherwise fall back to the InteractionAction's reference name.
        /// </summary>
        internal static string GetActionUiName(InteractablePreset.InteractionAction ia, Interactable.InteractableCurrentAction ica)
        {
            // overrideInteractionName lives on Interactable.InteractableCurrentAction, not on InteractionAction
            // (nested type shown on Interactable docs). :contentReference[oaicite:3]{index=3}
            var overrideName = ica != null ? ica.overrideInteractionName : null;
            if (!string.IsNullOrEmpty(overrideName)) return overrideName;

            // Fallback: InteractionAction.interactionName (a string reference key on the preset). :contentReference[oaicite:4]{index=4}
            return ia != null ? ia.interactionName : string.Empty;
        }

        private static bool LooksLikeTalkTo(string labelOrKey)
        {
            var n = Normalize(labelOrKey);
            return n == "talk to" || n == "talkto" || n == "talk";
        }

        private static bool LooksLikeInspect(string labelOrKey) // <-- added
        {
            var n = Normalize(labelOrKey);
            return n == "inspect";
        }

        /// <summary>
        /// Find a "Talk To" action in the interactable dictionary (by best-effort name match).
        /// </summary>
        public static bool TryFindTalkTo(Interactable inter, out Interactable.InteractableCurrentAction found, out InteractablePreset.InteractionKey key)
        {
            found = null;
            key = default;

            if (inter == null || inter.currentActions == null) return false;

            foreach (var kv in inter.currentActions)
            {
                var ica = kv.Value;
                var ia = ica?.currentAction;
                if (ia == null) continue;

                var displayName = GetActionUiName(ia, ica);
                if (LooksLikeTalkTo(displayName) || LooksLikeTalkTo(ia.interactionName))
                {
                    found = ica;
                    key = kv.Key;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Find an "Inspect" action in the interactable dictionary (by best-effort name match).
        /// </summary>
        public static bool TryFindInspect(Interactable inter, out Interactable.InteractableCurrentAction found, out InteractablePreset.InteractionKey key) // <-- added
        {
            found = null;
            key = default;

            if (inter == null || inter.currentActions == null) return false;

            foreach (var kv in inter.currentActions)
            {
                var ica = kv.Value;
                var ia = ica?.currentAction;
                if (ia == null) continue;

                var displayName = GetActionUiName(ia, ica);
                if (LooksLikeInspect(displayName) || LooksLikeInspect(ia.interactionName))
                {
                    found = ica;
                    key = kv.Key;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Force-enable and display an existing action entry.
        /// </summary>
        public static bool ForceEnable(Interactable.InteractableCurrentAction ica)
        {
            if (ica == null) return false;
            bool changed = false;

            if (!ica.enabled) { ica.enabled = true; changed = true; }
            if (!ica.display) { ica.display = true; changed = true; }

            // We intentionally do NOT change ia.specialCase; we only surface visibility/enable state.
            return changed;
        }

        /// <summary>
        /// Safe HUD refresh – helps older saves display the forced action immediately.
        /// </summary>
        public static void RefreshHud()
        {
            var ic = InteractionController._instance;
            if (ic == null) return;

            // These exist on the controller per docs; refresh both text and icon lines.
            ic.UpdateInteractionText();
            ic.UpdateInteractionIcons();
        }

        /// <summary>
        /// Dump the currentActions map for debugging (skips empty to avoid spam).
        /// Public for cross-file calls.
        /// </summary>
        public static void DumpActions(Interactable inter, string tag)
        {
            if (!Verbose || inter == null) return;
            if (inter.currentActions == null || inter.currentActions.Count == 0) return; // avoid spamming empty dumps

            var sb = new StringBuilder();
            sb.Append($"[BTB][ACTIONS-DUMP {tag}] inter#{GetInteractableId(inter)} {{ ");

            int idx = 0;
            foreach (var kv in inter.currentActions)
            {
                var k = kv.Key;
                var v = kv.Value;
                var ia = v?.currentAction;

                string name = GetActionUiName(ia, v);
                string keyOverride = ia == null ? "none" : ia.keyOverride.ToString();
                string sc = ia == null ? "none" : ia.specialCase.ToString();

                sb.Append($"#{idx}:{k} => ui='{name}', keyOverride={keyOverride}, specialCase={sc}, enabled={v?.enabled ?? false}, display={v?.display ?? false}; ");
                idx++;
            }

            sb.Append(" }");
            VLog(sb.ToString());
        }

        /// <summary>
        /// Print out what the player is looking at, and how/why a target qualifies.
        /// Public for cross-file calls.
        /// </summary>
        public static void DumpLookSnapshot(Interactable inter, Human target)
        {
            if (!Verbose) return;

            var sb = new StringBuilder();
            sb.Append("[BTB][LOOK-SNAPSHOT] ");

            if (inter == null)
            {
                sb.Append("no current interactable.");
                VLog(sb.ToString());
                return;
            }

            string who = "none";
            if (target != null)
            {
                who = $"{target.humanID}:{target.citizenName}";
            }

            sb.Append($"inter#{GetInteractableId(inter)}, actor={who}");

            // Enforcer-call matching detail
            string why;
            bool onDuty = IsOnDutyCrimeSceneResponder(target, out why);
            sb.Append($", onDutyCrimeSceneResponder={onDuty} (why: {why})");

            VLog(sb.ToString());

            // Also dump actions for fast correlation
            DumpActions(inter, "PRE-INJECT");
        }
    }

    /// <summary>
    /// Patch: after Interactable recomputes actions, make sure "Talk To" is visible/enabled
    /// for on-duty enforcer responders.
    /// </summary>
    [HarmonyPatch(typeof(Interactable), nameof(Interactable.UpdateCurrentActions))]
    internal static class Interactable_UpdateCurrentActions_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(Interactable __instance)
        {
            // Only care about actors (citizens/enforcers) – Interactable has isActor (Actor). :contentReference[oaicite:5]{index=5}
            Human human = null;
            var actor = __instance?.isActor;
            if (actor != null)
            {
                human = actor as Human;
            }

            if (human == null)
            {
                if (TalkToInjection.Verbose) TalkToInjection.DumpActions(__instance, "PRE-INJECT");
                return;
            }

            // Snapshot (pre)
            TalkToInjection.DumpLookSnapshot(__instance, human);

            string why;
            bool qualifies = TalkToInjection.IsOnDutyCrimeSceneResponder(human, out why);

            if (!TalkToInjection.TryFindTalkTo(__instance, out var talk, out var key))
            {
                TalkToInjection.VLog($"[BTB][ACTIONS-REFRESH] No 'Talk To' slot found (why: {why}). inter#{__instance.id}");
                return;
            }

            // If the target qualifies, force visibility/enablement; else leave vanilla logic.
            if (qualifies)
            {
                bool changed = TalkToInjection.ForceEnable(talk);
                TalkToInjection.VLog($"[BTB][ACTIONS-REFRESH] {(changed ? "Forced enable/display" : "Already enabled")} for 'Talk To' (on-duty: {why}). inter#{__instance.id}");

                // Also force-enable "Inspect" if present (added)
                if (TalkToInjection.TryFindInspect(__instance, out var inspect, out var inspectKey))
                {
                    bool changedInspect = TalkToInjection.ForceEnable(inspect);
                    TalkToInjection.VLog($"[BTB][ACTIONS-REFRESH] {(changedInspect ? "Forced enable/display" : "Already enabled")} for 'Inspect' (on-duty: {why}). inter#{__instance.id}");
                    if (changedInspect) TalkToInjection.RefreshHud();
                }

                if (changed)
                {
                    // Help older saves display immediately
                    TalkToInjection.RefreshHud();
                }
            }
            else
            {
                TalkToInjection.VLog($"[BTB][ACTIONS-REFRESH] Target is NOT on-duty responder (why: {why}). No injection. inter#{__instance.id}");
            }
        }
    }

    /// <summary>
    /// Patch: when the player changes look target, log a detailed snapshot and ensure UI is up-to-date.
    /// </summary>
    [HarmonyPatch(typeof(InteractionController), nameof(InteractionController.OnPlayerLookAtInteractableChange))]
    internal static class InteractionController_OnPlayerLookAtInteractableChange_Patch
    {
        [HarmonyPostfix]
        private static void Postfix(InteractionController __instance)
        {
            // InteractionController.currentLookingAtInteractable is an InteractableController; get its .interactable. :contentReference[oaicite:6]{index=6}
            var interCtrl = __instance?.currentLookingAtInteractable;
            var inter = interCtrl != null ? interCtrl.interactable : null;

            Human human = null;
            var actor = inter?.isActor;
            if (actor != null) human = actor as Human;

            // Extra verbose snapshot with enforcer-call reasoning + pre-inject dump (skips if empty).
            TalkToInjection.DumpLookSnapshot(inter, human);

            // If we just forced an action recently, make sure HUD icons/text are synced
            TalkToInjection.RefreshHud();
        }
    }

    /// <summary>
    /// Patch: when setting interaction, log selecting "Talk To".
    /// Signature per docs:
    /// SetCurrentPlayerInteraction(InteractionKey key, Interactable newInteractable, Interactable.InteractableCurrentAction newCurrentAction, bool fpsItem=false, int forcePriority=-1)
    /// </summary>
    [HarmonyPatch(typeof(InteractionController), nameof(InteractionController.SetCurrentPlayerInteraction))]
    internal static class InteractionController_SetCurrentPlayerInteraction_Patch
    {
        [HarmonyPrefix]
        private static void Prefix(
            InteractionController __instance,
            InteractablePreset.InteractionKey key,
            Interactable newInteractable,
            Interactable.InteractableCurrentAction newCurrentAction,
            bool fpsItem,
            int forcePriority)
        {
            // Prefer the provided interactable; otherwise fall back to what the player is looking at.
            var inter = newInteractable ?? __instance?.currentLookingAtInteractable?.interactable;
            if (inter == null) return;

            var ica = newCurrentAction;
            if (ica == null && inter.currentActions != null)
            {
                inter.currentActions.TryGetValue(key, out ica);
            }

            var ia = ica?.currentAction;
            var label = TalkToInjection.GetActionUiName(ia, ica);

            if (!string.IsNullOrEmpty(label) && (label.Equals(TalkToInjection.TalkToUIName) || NormalizeStatic(label)))
            {
                var human = inter.isActor as Human;
                string who = (human != null) ? $"{human.humanID}:{human.citizenName}" : "unknown";
                TalkToInjection.Log($"[BTB][INTERACT] Selecting '{label}' on {who}");
            }

            bool NormalizeStatic(string s)
            {
                var n = Regex.Replace(s ?? string.Empty, @"\s+", " ").Trim().ToLowerInvariant();
                return n == "talk to" || n == "talkto";
            }
        }
    }
}
