using RimWorld;
using UnityEngine;
using Verse;

namespace HumanoidDryad
{
    /// <summary>
    /// Body render node for dryads, replacing the vanilla
    /// <see cref="PawnRenderNode_AnimalPart_Body"/> on the Animal render tree.
    ///
    /// WHY override GraphicFor and not TexPathFor:
    /// the vanilla AnimalPart.GraphicFor does NOT consult PawnRenderNode.TexPathFor
    /// (verified in IL: it reads PawnKindLifeStage.bodyGraphicData.get_Graphic() directly).
    /// Only the base PawnRenderNode.GraphicFor goes through TexPathFor. So the only place
    /// we can swap the body art for an animal is GraphicFor itself.
    ///
    /// The Modern path simply defers to base, so mesh, rotation, dessicated/rotting
    /// handling, alternate graphics and colour all stay exactly vanilla. The Old path
    /// mirrors what the base does: build a Graphic_Multi from a texPath whose files are
    /// named "&lt;stem&gt;_{east,north,south}", using the same shader and draw size the
    /// vanilla body graphic uses.
    /// </summary>
    public class PawnRenderNode_DryadBody : PawnRenderNode_AnimalPart_Body
    {
        public PawnRenderNode_DryadBody(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree)
            : base(pawn, props, tree)
        {
        }

        public override Graphic GraphicFor(Pawn pawn)
        {
            DryadSkinEntry entry = DryadSkinDatabase.ForPawn(pawn);
            if (entry == null || !DryadSkinConfig.UseOldBody(entry.DefName))
            {
                return base.GraphicFor(pawn);
            }

            GraphicData vanilla = VanillaBodyGraphicData(pawn);
            Vector2 drawSize = vanilla != null ? vanilla.drawSize : Vector2.one;

            return GraphicDatabase.Get<Graphic_Multi>(
                entry.OldTexPath,
                ShaderDatabase.Cutout,
                drawSize,
                Color.white);
        }

        /// <summary>
        /// The vanilla body GraphicData for this pawn's current life stage, used only to
        /// inherit the draw size so the Old art is scaled identically to the Modern art.
        /// </summary>
        private static GraphicData VanillaBodyGraphicData(Pawn pawn)
        {
            PawnKindLifeStage stage = pawn?.ageTracker?.CurKindLifeStage;
            if (stage == null)
            {
                return null;
            }
            if (pawn.gender == Gender.Female && stage.femaleGraphicData != null)
            {
                return stage.femaleGraphicData;
            }
            return stage.bodyGraphicData;
        }
    }
}
