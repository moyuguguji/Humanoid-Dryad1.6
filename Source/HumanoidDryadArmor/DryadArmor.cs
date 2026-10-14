using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace HumanoidDryad
{
    public class HumanoidDryadMod : Mod
    {
        public static HumanoidDryadMod Instance;

        public HumanoidDryadMod(ModContentPack content) : base(content)
        {
            Instance = this;
            GetSettings<DryadArmorSettings>();
        }

        public override string SettingsCategory()
        {
            return "HD_ModName".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            DryadArmorSettings settings = GetSettings<DryadArmorSettings>();
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            float lineHeight = Text.LineHeight;
            float checkWidth = lineHeight + 4f;

            // ---------------------------------------------------------------
            // Section 1: armour overlay show/hide (original feature, unchanged)
            // ---------------------------------------------------------------
            list.Label("HD_ArmorSettings".Translate());
            list.GapLine();

            DrawCheckbox(ref settings.Dryad_Basic, "HD_Dryad_Basic".Translate());
            DrawCheckbox(ref settings.Dryad_Woodmaker, "HD_Dryad_Woodmaker".Translate());
            DrawCheckbox(ref settings.Dryad_Berrymaker, "HD_Dryad_Berrymaker".Translate());
            DrawCheckbox(ref settings.Dryad_Medicinemaker, "HD_Dryad_Medicinemaker".Translate());
            DrawCheckbox(ref settings.Dryad_Gaumaker, "HD_Dryad_Gaumaker".Translate());
            DrawCheckbox(ref settings.Dryad_Carrier, "HD_Dryad_Carrier".Translate());
            DrawCheckbox(ref settings.Dryad_Clawer, "HD_Dryad_Clawer".Translate());
            DrawCheckbox(ref settings.Dryad_Barkskin, "HD_Dryad_Barkskin".Translate());

            list.Gap();
            list.Label("HD_ArmorFooter".Translate());

            // ---------------------------------------------------------------
            // Section 2: old body art (this copy's added feature)
            //
            // Same checkbox interaction as the armour toggles above, so the two lists
            // read as one consistent settings page. All eight default to OFF.
            // ---------------------------------------------------------------
            list.GapLine();
            list.Label("HD_SkinSettings".Translate());
            list.GapLine();

            for (int i = 0; i < DryadSkinDatabase.All.Count; i++)
            {
                DryadSkinEntry entry = DryadSkinDatabase.All[i];
                bool useOld = settings.UseOldBody(entry.DefName);
                DrawCheckbox(ref useOld, ("HD_Skin_" + entry.DefName).Translate());
                settings.SetUseOldBody(entry.DefName, useOld);
            }

            list.End();
            settings.Apply();

            void DrawCheckbox(ref bool val, string label)
            {
                Rect row = list.GetRect(lineHeight);
                Rect checkRect = new Rect(row.x, row.y, checkWidth, lineHeight);
                Widgets.Checkbox(checkRect.position, ref val, checkRect.width);
                Rect labelRect = new Rect(row.x + checkWidth + 4f, row.y, row.width - checkWidth - 4f, lineHeight);
                Widgets.Label(labelRect, label);
            }
        }

        public override void WriteSettings()
        {
            DryadArmorSettings s = GetSettings<DryadArmorSettings>();
            s.Apply();
            // Body graphics are cached per node, so a change only shows after a rebuild.
            DryadSkinConfig.InvalidateAllPawns();
        }
    }

    public class DryadArmorSettings : ModSettings
    {
        // --- armour overlay toggles (original feature) ---
        public bool Dryad_Basic = true;
        public bool Dryad_Woodmaker = true;
        public bool Dryad_Berrymaker = true;
        public bool Dryad_Medicinemaker = true;
        public bool Dryad_Gaumaker = true;
        public bool Dryad_Carrier = true;
        public bool Dryad_Clawer = true;
        public bool Dryad_Barkskin = true;

        // --- old body art switches (added by this copy) ---
        // Field names mirror the caste defNames, matching how the armour toggles above
        // are laid out, and they all default to false = modern art.
        public bool OldBody_Dryad_Basic = false;
        public bool OldBody_Dryad_Woodmaker = false;
        public bool OldBody_Dryad_Berrymaker = false;
        public bool OldBody_Dryad_Medicinemaker = false;
        public bool OldBody_Dryad_Gaumaker = false;
        public bool OldBody_Dryad_Carrier = false;
        public bool OldBody_Dryad_Clawer = false;
        public bool OldBody_Dryad_Barkskin = false;

        private static bool FieldFor(string defName, DryadArmorSettings s, out bool value)
        {
            switch (defName)
            {
                case "Dryad_Basic": value = s.OldBody_Dryad_Basic; return true;
                case "Dryad_Woodmaker": value = s.OldBody_Dryad_Woodmaker; return true;
                case "Dryad_Berrymaker": value = s.OldBody_Dryad_Berrymaker; return true;
                case "Dryad_Medicinemaker": value = s.OldBody_Dryad_Medicinemaker; return true;
                case "Dryad_Gaumaker": value = s.OldBody_Dryad_Gaumaker; return true;
                case "Dryad_Carrier": value = s.OldBody_Dryad_Carrier; return true;
                case "Dryad_Clawer": value = s.OldBody_Dryad_Clawer; return true;
                case "Dryad_Barkskin": value = s.OldBody_Dryad_Barkskin; return true;
            }
            value = false;
            return false;
        }

        public bool UseOldBody(string defName)
        {
            bool v;
            return defName != null && FieldFor(defName, this, out v) && v;
        }

        public void SetUseOldBody(string defName, bool value)
        {
            switch (defName)
            {
                case "Dryad_Basic": OldBody_Dryad_Basic = value; break;
                case "Dryad_Woodmaker": OldBody_Dryad_Woodmaker = value; break;
                case "Dryad_Berrymaker": OldBody_Dryad_Berrymaker = value; break;
                case "Dryad_Medicinemaker": OldBody_Dryad_Medicinemaker = value; break;
                case "Dryad_Gaumaker": OldBody_Dryad_Gaumaker = value; break;
                case "Dryad_Carrier": OldBody_Dryad_Carrier = value; break;
                case "Dryad_Clawer": OldBody_Dryad_Clawer = value; break;
                case "Dryad_Barkskin": OldBody_Dryad_Barkskin = value; break;
            }
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref Dryad_Basic, "Dryad_Basic", true);
            Scribe_Values.Look(ref Dryad_Woodmaker, "Dryad_Woodmaker", true);
            Scribe_Values.Look(ref Dryad_Berrymaker, "Dryad_Berrymaker", true);
            Scribe_Values.Look(ref Dryad_Medicinemaker, "Dryad_Medicinemaker", true);
            Scribe_Values.Look(ref Dryad_Gaumaker, "Dryad_Gaumaker", true);
            Scribe_Values.Look(ref Dryad_Carrier, "Dryad_Carrier", true);
            Scribe_Values.Look(ref Dryad_Clawer, "Dryad_Clawer", true);
            Scribe_Values.Look(ref Dryad_Barkskin, "Dryad_Barkskin", true);

            Scribe_Values.Look(ref OldBody_Dryad_Basic, "OldBody_Dryad_Basic", false);
            Scribe_Values.Look(ref OldBody_Dryad_Woodmaker, "OldBody_Dryad_Woodmaker", false);
            Scribe_Values.Look(ref OldBody_Dryad_Berrymaker, "OldBody_Dryad_Berrymaker", false);
            Scribe_Values.Look(ref OldBody_Dryad_Medicinemaker, "OldBody_Dryad_Medicinemaker", false);
            Scribe_Values.Look(ref OldBody_Dryad_Gaumaker, "OldBody_Dryad_Gaumaker", false);
            Scribe_Values.Look(ref OldBody_Dryad_Carrier, "OldBody_Dryad_Carrier", false);
            Scribe_Values.Look(ref OldBody_Dryad_Clawer, "OldBody_Dryad_Clawer", false);
            Scribe_Values.Look(ref OldBody_Dryad_Barkskin, "OldBody_Dryad_Barkskin", false);

            Apply();
        }

        public void Apply()
        {
            DryadArmorConfig.SetFromSettings(this);
            DryadSkinConfig.Apply(this);
        }
    }

    public static class DryadArmorConfig
    {
        public static List<KeyValuePair<string, bool>> AllEntries;

        private static Dictionary<string, bool> map = new Dictionary<string, bool>();

        static DryadArmorConfig()
        {
            ResetToDefaults();
        }

        public static void SetFromSettings(DryadArmorSettings s)
        {
            map["Dryad_Basic"] = s.Dryad_Basic;
            map["Dryad_Woodmaker"] = s.Dryad_Woodmaker;
            map["Dryad_Berrymaker"] = s.Dryad_Berrymaker;
            map["Dryad_Medicinemaker"] = s.Dryad_Medicinemaker;
            map["Dryad_Gaumaker"] = s.Dryad_Gaumaker;
            map["Dryad_Carrier"] = s.Dryad_Carrier;
            map["Dryad_Clawer"] = s.Dryad_Clawer;
            map["Dryad_Barkskin"] = s.Dryad_Barkskin;
            SyncEntries();
        }

        public static void ResetToDefaults()
        {
            map["Dryad_Basic"] = true;
            map["Dryad_Woodmaker"] = true;
            map["Dryad_Berrymaker"] = true;
            map["Dryad_Medicinemaker"] = true;
            map["Dryad_Gaumaker"] = true;
            map["Dryad_Carrier"] = true;
            map["Dryad_Clawer"] = true;
            map["Dryad_Barkskin"] = true;
            SyncEntries();
        }

        public static bool IsEnabled(string defName)
        {
            if (map.TryGetValue(defName, out bool enabled))
                return enabled;
            return false;
        }

        private static void SyncEntries()
        {
            AllEntries = new List<KeyValuePair<string, bool>>
            {
                new KeyValuePair<string, bool>("Dryad_Basic", map["Dryad_Basic"]),
                new KeyValuePair<string, bool>("Dryad_Woodmaker", map["Dryad_Woodmaker"]),
                new KeyValuePair<string, bool>("Dryad_Berrymaker", map["Dryad_Berrymaker"]),
                new KeyValuePair<string, bool>("Dryad_Medicinemaker", map["Dryad_Medicinemaker"]),
                new KeyValuePair<string, bool>("Dryad_Gaumaker", map["Dryad_Gaumaker"]),
                new KeyValuePair<string, bool>("Dryad_Carrier", map["Dryad_Carrier"]),
                new KeyValuePair<string, bool>("Dryad_Clawer", map["Dryad_Clawer"]),
                new KeyValuePair<string, bool>("Dryad_Barkskin", map["Dryad_Barkskin"]),
            };
        }
    }

    public class PawnRenderNode_DryadArmor : PawnRenderNode
    {
        public PawnRenderNode_DryadArmor(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree) { }

        public override Graphic GraphicFor(Pawn pawn)
        {
            if (pawn?.kindDef == null)
                return BaseContent.BadGraphic;

            // Old body art and the armour overlay do not mix: the overlay is drawn on top
            // of the body and would simply cover the old art, which is what the player
            // wants to look at. Suppressing here (as well as in the worker) keeps this
            // true even if some other code path calls GraphicFor directly.
            if (!DryadArmorConfig.IsEnabled(pawn.kindDef.defName)
                || DryadSkinConfig.UseOldBody(pawn.kindDef.defName))
                return BaseContent.BadGraphic;

            string texPath = "Things/Pawn/Animal/Apparel/GAS_Armor/GtypeArmor_"
                           + pawn.kindDef.defName;

            return GraphicDatabase.Get<Graphic_Multi>(texPath, ShaderDatabase.Cutout);
        }
    }

    public class PawnRenderNodeWorker_DryadArmor : PawnRenderNodeWorker
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms))
                return false;

            Pawn pawn = parms.pawn;
            if (pawn?.kindDef == null)
                return false;

            if (!DryadArmorConfig.IsEnabled(pawn.kindDef.defName))
                return false;

            // Enabling the old body art hides the armour overlay for that caste, even
            // though the user's armour toggle stays on. The saved armour setting is
            // deliberately NOT modified -- we only skip drawing -- so switching back to
            // the modern art restores the armour exactly as the player had configured it.
            if (DryadSkinConfig.UseOldBody(pawn.kindDef.defName))
                return false;

            return true;
        }
    }
}
