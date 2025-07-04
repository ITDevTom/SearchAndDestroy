﻿using HarmonyLib;
using RimWorld;
using SearchAndDestroy.Storage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SearchAndDestroy.Harmony
{
    [HarmonyPatch(typeof(Pawn_DraftController), "set_Drafted")]
    public static class Pawn_DraftController_set_Drafted
    {
        public static void Postfix(Pawn_DraftController __instance)
        {
            ExtendedDataStorage store = Base.Instance.ExtendedDataStorage;
            if(store != null && !__instance.Drafted)
            {
                ExtendedPawnData pawnData = store.GetExtendedDataFor(__instance.pawn);
                pawnData.SD_enabled = false;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController), "GetGizmos")]
    public static class Pawn_DraftController_GetGizmos
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn_DraftController __instance)
        {
            List<Gizmo> gizmoList = __result.ToList();
            bool isPlayerPawn = __instance.pawn.Faction != null && __instance.pawn.Faction.IsPlayer;

            if (isPlayerPawn && __instance.pawn.equipment != null && __instance.pawn.Drafted && (__instance.pawn.story == null || !__instance.pawn.WorkTagIsDisabled(WorkTags.Violent)))
            {
                if (__instance.pawn.equipment.Primary == null || __instance.pawn.equipment.Primary.def.IsMeleeWeapon)
                {
                    gizmoList.Add(CreateGizmo_SearchAndDestroy_Melee(__instance));
                }
                else
                {
                    gizmoList.Add(CreateGizmo_SearchAndDestroy_Ranged(__instance));
                }
            }
            __result = gizmoList;

        }

        private static Gizmo CreateGizmo_SearchAndDestroy_Melee(Pawn_DraftController __instance)
        {
            string disabledReason = "";
            bool disabled = false;
            PawnDuty duty = __instance.pawn.mindState.duty;

            ExtendedPawnData pawnData = Base.Instance.ExtendedDataStorage.GetExtendedDataFor(__instance.pawn);

            if (__instance.pawn.Downed)
            {
                disabled = true;
                disabledReason = "SD_Reason_Downed".Translate();
            }

            //if (__instance.pawn.IsNonMutantAnimal && __instance.pawn.training.CanBeTrained(TrainableDefOf.Tameness) && !__instance.pawn.training.HasLearned(TrainableDefOf.Release))
            //{
            //    disabled = true;
            //    disabledReason = "SD_Reason_Animal".Translate();
            //}

            Gizmo gizmo = new Command_Toggle
            {
                defaultLabel = "SD_Gizmo_Melee_Label".Translate(),
                defaultDesc = "SD_Gizmo_Melee_Description".Translate(),
                hotKey = KeyBindingDefOf.Command_ItemForbid,
                Disabled = disabled,
                disabledReason = disabledReason,
                icon = ContentFinder<Texture2D>.Get(("UI/" + "SearchAndDestroy_Gizmo_Melee"), true),
                isActive = () => pawnData.SD_enabled,
                toggleAction = () =>
                {
                    Log.Message("SearchAndDestroy_Gizmo_Melee toggle");
                    pawnData.SD_enabled = !pawnData.SD_enabled;
                    __instance.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }
            };
            return gizmo;
        }
        private static Gizmo CreateGizmo_SearchAndDestroy_Ranged(Pawn_DraftController __instance)
        {
            string disabledReason = "";
            bool disabled = false;
            PawnDuty duty = __instance.pawn.mindState.duty;

            ExtendedPawnData pawnData = Base.Instance.ExtendedDataStorage.GetExtendedDataFor(__instance.pawn);

            if (__instance.pawn.Downed)
            {
                disabled = true;
                disabledReason = "SD_Reason_Downed".Translate();
            }

            // Disabled for now, was causing some mod conflicts, will probably need a custom conditional
            //if (__instance.pawn.IsNonMutantAnimal && __instance.pawn.training.CanBeTrained(TrainableDefOf.Tameness) && !__instance.pawn.training.HasLearned(TrainableDefOf.Release))
            //{
            //    disabled = true;
            //    disabledReason = "SD_Reason_Animal".Translate();
            //}

            Gizmo gizmo = new Command_Toggle
            {
                defaultLabel = "SD_Gizmo_Ranged_Label".Translate(),
                defaultDesc = "SD_Gizmo_Ranged_Description".Translate(),
                hotKey = KeyBindingDefOf.Command_ItemForbid,
                Disabled = disabled,
                disabledReason = disabledReason,
                icon = ContentFinder<Texture2D>.Get(("UI/" + "SearchAndDestroy_Gizmo_Ranged"), true),
                isActive = () => pawnData.SD_enabled,
                toggleAction = () =>
                {
                    Log.Message("SearchAndDestroy_Gizmo_Ranged toggle");
                    pawnData.SD_enabled = !pawnData.SD_enabled;
                    __instance.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true);
                }
            };
            return gizmo;
        }
    }
}
