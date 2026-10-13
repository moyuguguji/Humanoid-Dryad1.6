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

            list.Label("HD_ArmorSettings".Translate());
            list.GapLine();

            float lineHeight = Text.LineHeight;
            float checkWidth = lineHeight + 4f;

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
            GetSettings<DryadArmorSettings>().Apply();
        }
    }

    public class DryadArmorSettings : ModSettings
    {
        public bool Dryad_Basic = true;
        public bool Dryad_Woodmaker = true;
        public bool Dryad_Berrymaker = true;
        public bool Dryad_Medicinemaker = true;
        public bool Dryad_Gaumaker = true;
        public bool Dryad_Carrier = true;
        public bool Dryad_Clawer = true;
        public bool Dryad_Barkskin = true;

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
            Apply();
        }

        public void Apply()
        {
            DryadArmorConfig.SetFromSettings(this);
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

            if (!DryadArmorConfig.IsEnabled(pawn.kindDef.defName))
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

            return DryadArmorConfig.IsEnabled(pawn.kindDef.defName);
        }
    }
}
