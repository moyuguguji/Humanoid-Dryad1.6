using System.Collections.Generic;
using Verse;

namespace HumanoidDryad
{
    /// <summary>
    /// The live "use old body art" switches, consulted by
    /// <see cref="PawnRenderNode_DryadBody"/> on every graphic rebuild and by
    /// <see cref="PawnRenderNodeWorker_DryadArmor"/> to suppress the armour overlay.
    ///
    /// Kept separate from <see cref="DryadArmorConfig"/> so the armour overlay toggles and
    /// the body-art switches cannot interfere with each other.
    ///
    /// Default is FALSE for every caste, i.e. the modern art is used until the player
    /// explicitly enables the old art.
    /// </summary>
    public static class DryadSkinConfig
    {
        private static readonly Dictionary<string, bool> useOldBody = new Dictionary<string, bool>();

        static DryadSkinConfig()
        {
            ResetToDefaults();
        }

        /// <summary>True when this caste should be drawn with the old art set.</summary>
        public static bool UseOldBody(string defName)
        {
            bool v;
            if (defName != null && useOldBody.TryGetValue(defName, out v))
            {
                return v;
            }
            return false;
        }

        public static void ResetToDefaults()
        {
            foreach (DryadSkinEntry e in DryadSkinDatabase.All)
            {
                useOldBody[e.DefName] = false;
            }
        }

        /// <summary>Push the user's saved choices into the live map.</summary>
        public static void Apply(DryadArmorSettings settings)
        {
            if (settings == null)
            {
                return;
            }
            foreach (DryadSkinEntry e in DryadSkinDatabase.All)
            {
                useOldBody[e.DefName] = settings.UseOldBody(e.DefName);
            }
        }

        /// <summary>Force every spawned pawn to rebuild its render tree.</summary>
        public static void InvalidateAllPawns()
        {
            if (Find.Maps == null)
            {
                return;
            }
            for (int m = 0; m < Find.Maps.Count; m++)
            {
                Map map = Find.Maps[m];
                if (map?.mapPawns == null)
                {
                    continue;
                }
                IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
                for (int i = 0; i < pawns.Count; i++)
                {
                    PawnRenderer renderer = pawns[i]?.Drawer?.renderer;
                    if (renderer == null)
                    {
                        continue;
                    }
                    renderer.renderTree?.SetDirty();
                    renderer.SetAllGraphicsDirty();
                }
            }
        }
    }
}
