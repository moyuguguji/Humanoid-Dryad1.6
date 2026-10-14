using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanoidDryad
{
    /// <summary>
    /// Texture style switching for the Gauranlen family graphics.
    ///
    /// Follows the same shape as ET Homunculus' cosmetic overlay settings: a
    /// ModSettings object drives translated option rows, and the choice is applied
    /// to the live defs immediately (with a cached-graphic refresh) rather than
    /// needing a restart.
    /// </summary>
    public static class DryadTextureStyles
    {
        // ------------------------------------------------------------------
        // Option tables. Every path is relative to a mod's Textures folder.
        // ------------------------------------------------------------------

        private static readonly string[] TreePaths =
        {
            "Things/Plant/GauranlenTree",
            "Things/Plant/GauranlenTree_Shade",
            "Things/Plant/GauranlenTree_Bamboo"
        };

        private static readonly string[] TreeLabelKeys =
        {
            "HD_StyleTree_0", "HD_StyleTree_1", "HD_StyleTree_2"
        };

        private static readonly string[] MossPaths =
        {
            "Things/Plant/GauranlenMoss",
            "Things/Plant/GauranlenMoss_ALT"
        };

        private static readonly string[] MossLabelKeys =
        {
            "HD_StyleMoss_0", "HD_StyleMoss_1"
        };

        private static readonly string[] DebrisPaths =
        {
            "Things/Mote/GauranlenDebris/GauranlenDebris",
            "Things/Mote/GauranlenDebris_ALT/GauranlenDebris"
        };

        private static readonly string[] DebrisLabelKeys =
        {
            "HD_StyleDebris_0", "HD_StyleDebris_1"
        };

        private static readonly string[] LeafPaths =
        {
            "Things/Plant/EF/EL",
            "Things/Plant/EF/EL2",
            "Things/Plant/EF/EL_ALT"
        };

        private static readonly string[] LeafLabelKeys =
        {
            "HD_StyleLeaf_0", "HD_StyleLeaf_1", "HD_StyleLeaf_2"
        };

        // Defs touched by each option group.
        private static readonly string[] TreeDefNames = { "Plant_TreeGauranlen" };
        private static readonly string[] MossDefNames = { "Plant_MossGauranlen" };
        private static readonly string[] DebrisDefNames = { "GauranlenDebrisA", "GauranlenDebrisB", "GauranlenDebrisC" };
        private static readonly string[] LeafDefNames = { "GauranlenLeaf" };

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        public static int Wrap(int value, int count)
        {
            if (count <= 0)
            {
                return 0;
            }
            value %= count;
            if (value < 0)
            {
                value += count;
            }
            return value;
        }

        public static string Label(string[] labelKeys, int index)
        {
            if (labelKeys == null || labelKeys.Length == 0)
            {
                return string.Empty;
            }
            return labelKeys[Wrap(index, labelKeys.Length)].Translate();
        }

        private static bool DefsReady()
        {
            return DefDatabase<ThingDef>.GetNamedSilentFail("Plant_TreeGauranlen") != null;
        }

        // ------------------------------------------------------------------
        // Settings UI (drawn inside the mod's own settings window)
        // ------------------------------------------------------------------

        public static void DrawSettings(Listing_Standard list, DryadArmorSettings settings)
        {
            if (list == null || settings == null)
            {
                return;
            }

            list.Gap();
            list.Label("HD_TextureStyleHeader".Translate());
            list.GapLine();

            settings.StyleTree = DrawCycleRow(list, "HD_StyleTree".Translate(), TreeLabelKeys, settings.StyleTree);
            settings.StyleMoss = DrawCycleRow(list, "HD_StyleMoss".Translate(), MossLabelKeys, settings.StyleMoss);
            settings.StyleDebris = DrawCycleRow(list, "HD_StyleDebris".Translate(), DebrisLabelKeys, settings.StyleDebris);
            settings.StyleLeaf = DrawCycleRow(list, "HD_StyleLeaf".Translate(), LeafLabelKeys, settings.StyleLeaf);

            list.Gap();
            list.Label("HD_TextureStyleFooter".Translate());
        }

        private static int DrawCycleRow(Listing_Standard list, string label, string[] labelKeys, int value)
        {
            float lineHeight = Text.LineHeight;
            Rect row = list.GetRect(lineHeight);

            float buttonWidth = 26f;
            float valueWidth = Mathf.Max(140f, row.width * 0.4f);

            float valueX = row.xMax - valueWidth - buttonWidth - 4f;
            float prevX = valueX - buttonWidth - 4f;
            float nextX = row.xMax - buttonWidth;

            Rect labelRect = new Rect(row.x, row.y, Mathf.Max(0f, prevX - row.x - 4f), lineHeight);
            Rect prevRect = new Rect(prevX, row.y, buttonWidth, lineHeight);
            Rect valueRect = new Rect(valueX, row.y, valueWidth, lineHeight);
            Rect nextRect = new Rect(nextX, row.y, buttonWidth, lineHeight);

            Widgets.Label(labelRect, label);

            if (Widgets.ButtonText(prevRect, "<"))
            {
                value = Wrap(value - 1, labelKeys.Length);
            }
            if (Widgets.ButtonText(valueRect, Label(labelKeys, value)))
            {
                value = Wrap(value + 1, labelKeys.Length);
            }
            if (Widgets.ButtonText(nextRect, ">"))
            {
                value = Wrap(value + 1, labelKeys.Length);
            }

            return Wrap(value, labelKeys.Length);
        }

        // ------------------------------------------------------------------
        // Applying the choice
        // ------------------------------------------------------------------

        public static void Apply(DryadArmorSettings settings)
        {
            if (settings == null || !DefsReady())
            {
                return;
            }

            bool changed = false;

            string treePath = TreePaths[Wrap(settings.StyleTree, TreePaths.Length)];
            for (int i = 0; i < TreeDefNames.Length; i++)
            {
                changed |= SetThingTexPath(TreeDefNames[i], treePath);
            }

            string mossPath = MossPaths[Wrap(settings.StyleMoss, MossPaths.Length)];
            for (int i = 0; i < MossDefNames.Length; i++)
            {
                changed |= SetThingTexPath(MossDefNames[i], mossPath);
            }

            string debrisPath = DebrisPaths[Wrap(settings.StyleDebris, DebrisPaths.Length)];
            for (int i = 0; i < DebrisDefNames.Length; i++)
            {
                changed |= SetFleckTexPath(DebrisDefNames[i], debrisPath + (char)('A' + i));
            }

            string leafPath = LeafPaths[Wrap(settings.StyleLeaf, LeafPaths.Length)];
            for (int i = 0; i < LeafDefNames.Length; i++)
            {
                changed |= SetFleckTexPath(LeafDefNames[i], leafPath);
            }

            // The settings window re-applies every frame, so only pay for the
            // cache/mesh refresh when a path actually moved.
            if (changed)
            {
                RefreshGraphics();
            }
        }

        private static bool SetThingTexPath(string defName, string texPath)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null || def.graphicData == null || def.graphicData.texPath == texPath)
            {
                return false;
            }
            def.graphicData.texPath = texPath;
            ClearCachedGraphic(def);
            return true;
        }

        private static bool SetFleckTexPath(string defName, string texPath)
        {
            FleckDef def = DefDatabase<FleckDef>.GetNamedSilentFail(defName);
            if (def == null || def.graphicData == null || def.graphicData.texPath == texPath)
            {
                return false;
            }
            def.graphicData.texPath = texPath;
            ClearCachedGraphic(def);
            return true;
        }

        /// <summary>
        /// Drop a def's cached Graphic (private field "graphicInt"), so the next
        /// access rebuilds it from the new texPath. Reflection keeps this working
        /// across game versions where the field may move up or down the hierarchy.
        /// </summary>
        private static void ClearCachedGraphic(object obj)
        {
            if (obj == null)
            {
                return;
            }
            Type type = obj.GetType();
            while (type != null)
            {
                FieldInfo field = type.GetField("graphicInt", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field != null)
                {
                    try
                    {
                        field.SetValue(obj, null);
                    }
                    catch (Exception)
                    {
                        // A read-only or renamed field just means nothing to clear here.
                    }
                    return;
                }
                type = type.BaseType;
            }
        }

        /// <summary>
        /// GraphicDatabase caches materials per path; clear it so the swapped paths
        /// are loaded, then rebuild the map meshes that hold the plants.
        /// </summary>
        private static void RefreshGraphics()
        {
            ClearGraphicDatabaseCache();

            if (Find.Maps == null)
            {
                return;
            }

            for (int m = 0; m < Find.Maps.Count; m++)
            {
                Map map = Find.Maps[m];
                if (map == null || map.listerThings == null)
                {
                    continue;
                }

                List<Thing> things = map.listerThings.AllThings;
                if (things == null)
                {
                    continue;
                }

                for (int i = 0; i < things.Count; i++)
                {
                    Thing thing = things[i];
                    if (thing == null || thing.def == null)
                    {
                        continue;
                    }
                    if (!IsStyledThing(thing.def.defName))
                    {
                        continue;
                    }

                    ClearCachedGraphic(thing);

                    if (map.mapDrawer != null)
                    {
                        map.mapDrawer.MapMeshDirty(thing.Position, MapMeshFlagDefOf.Things);
                    }
                }
            }
        }

        private static bool IsStyledThing(string defName)
        {
            if (defName == null)
            {
                return false;
            }
            for (int i = 0; i < TreeDefNames.Length; i++)
            {
                if (TreeDefNames[i] == defName)
                {
                    return true;
                }
            }
            for (int i = 0; i < MossDefNames.Length; i++)
            {
                if (MossDefNames[i] == defName)
                {
                    return true;
                }
            }
            return false;
        }

        private static void ClearGraphicDatabaseCache()
        {
            try
            {
                MethodInfo method = typeof(GraphicDatabase).GetMethod(
                    "ClearCache",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                {
                    method.Invoke(null, null);
                }
            }
            catch (Exception)
            {
                // Nothing cached to clear on this build; the def swap still applies.
            }
        }
    }

    /// <summary>
    /// Applies the saved style once the defs exist, so a game that was loaded with
    /// a non-default style comes up with the right textures even before the player
    /// opens the settings window.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class DryadTextureStyleBootstrap
    {
        static DryadTextureStyleBootstrap()
        {
            try
            {
                DryadTextureStyles.Apply(DryadArmorSettings.Current);
            }
            catch (Exception ex)
            {
                Log.Warning("[Humanoid Dryad] texture style bootstrap failed: " + ex);
            }
        }
    }
}
