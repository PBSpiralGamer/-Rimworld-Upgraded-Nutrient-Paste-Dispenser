using RimWorld;
using Verse;

namespace UpgradedNutrientPasteDispenser
{
    [DefOf]
    public static class UpgradedPasteMealDefOf
    {
        public static ThingDef MealUpgradedNutrientPaste;

        static UpgradedPasteMealDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(UpgradedPasteMealDefOf));
        }
    }
}
