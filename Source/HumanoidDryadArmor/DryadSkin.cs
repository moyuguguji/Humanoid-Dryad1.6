using System.Collections.Generic;
using Verse;

namespace HumanoidDryad
{
    /// <summary>
    /// One dryad caste: its PawnKindDef defName plus the two body-art texPaths, and the
    /// modern "stem" the vanilla PawnKindDef uses.
    ///
    /// Why two art sets exist here:
    ///   Modern - Textures/Things/Pawn/Animal/&lt;Kind&gt;/&lt;Stem&gt;_{east,north,south}.png
    ///   Old    - Textures/Things/Pawn/Animal/OLD/&lt;Kind&gt;/&lt;Stem&gt;_{east,north,south}.png
    ///
    /// The Old files are copied out of ExTextures/OLD/Animal_OLD at build time. That is
    /// mandatory rather than cosmetic: ExTextures/ is NOT a RimWorld content root (it is
    /// not among Textures/Sounds/Strings/Patches/AssetBundles/Assets, and nothing in the
    /// mod or the game references the name), so ContentFinder/GraphicDatabase can never
    /// reach a texPath that points into it.
    ///
    /// The stems are NOT all equal to the defName -- e.g. Dryad_Woodmaker draws
    /// "DryadWoodmaker" -- so this table has to be explicit. Every entry was verified
    /// against the vanilla PawnKindDef bodyGraphicData and against the files on disk.
    /// </summary>
    public class DryadSkinEntry
    {
        public readonly string DefName;

        /// <summary>Caste folder under Things/Pawn/Animal (also the defName).</summary>
        public readonly string KindName;

        /// <summary>File stem vanilla uses; NOT always equal to the defName.</summary>
        public readonly string Stem;

        public DryadSkinEntry(string defName, string kindName, string stem)
        {
            DefName = defName;
            KindName = kindName;
            Stem = stem;
        }

        public string ModernTexPath
        {
            get { return "Things/Pawn/Animal/" + KindName + "/" + Stem; }
        }

        public string OldTexPath
        {
            get { return "Things/Pawn/Animal/OLD/" + KindName + "/" + Stem; }
        }
    }

    public static class DryadSkinDatabase
    {
        public static readonly List<DryadSkinEntry> All = new List<DryadSkinEntry>
        {
            new DryadSkinEntry("Dryad_Basic",         "Dryad_Basic",         "Dryad_Basic"),
            new DryadSkinEntry("Dryad_Woodmaker",     "Dryad_Woodmaker",     "DryadWoodmaker"),
            new DryadSkinEntry("Dryad_Berrymaker",    "Dryad_Berrymaker",    "DryadBerrymaker"),
            new DryadSkinEntry("Dryad_Medicinemaker", "Dryad_Medicinemaker", "DryadMedicinemaker"),
            new DryadSkinEntry("Dryad_Gaumaker",      "Dryad_Gaumaker",      "DryadGaumaker"),
            new DryadSkinEntry("Dryad_Carrier",       "Dryad_Carrier",       "DryadCarrier"),
            new DryadSkinEntry("Dryad_Clawer",        "Dryad_Clawer",        "Dryad_Clawer"),
            new DryadSkinEntry("Dryad_Barkskin",      "Dryad_Barkskin",      "Dryad_Barkskin"),
        };

        private static readonly Dictionary<string, DryadSkinEntry> byDef =
            new Dictionary<string, DryadSkinEntry>();

        static DryadSkinDatabase()
        {
            foreach (DryadSkinEntry e in All)
            {
                byDef[e.DefName] = e;
            }
        }

        public static DryadSkinEntry For(string defName)
        {
            if (defName == null)
            {
                return null;
            }
            DryadSkinEntry e;
            return byDef.TryGetValue(defName, out e) ? e : null;
        }

        /// <summary>
        /// Resolve a pawn's caste. Uses kindDef first (that is what the vanilla body
        /// graphic is keyed on) and falls back to the ThingDef, because the render node
        /// is attached to the pawn's render tree rather than to a specific def type.
        /// </summary>
        public static DryadSkinEntry ForPawn(Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }
            DryadSkinEntry e = pawn.kindDef != null ? For(pawn.kindDef.defName) : null;
            if (e != null)
            {
                return e;
            }
            return pawn.def != null ? For(pawn.def.defName) : null;
        }
    }
}
