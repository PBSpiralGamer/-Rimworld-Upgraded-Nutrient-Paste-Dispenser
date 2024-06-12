using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace UpgradedNutrientPasteDispenser;

[HotSwappable]
public class Building_UpgradedNutrientPasteDispenser : Building_NutrientPasteDispenser
{
    const FoodTypeFlags meatFlags = FoodTypeFlags.Meat; // | FoodTypeFlags.AnimalProduct;
    const FoodTypeFlags veggieFlags = FoodTypeFlags.VegetableOrFruit | FoodTypeFlags.Seed | FoodTypeFlags.Fungus;

    public override ThingDef DispensableDef => UpgradedPasteMealDefOf.MealUpgradedNutrientPaste;

    public override bool HasEnoughFeedstockInHoppers()
    {
        if (!def.HasModExtension<UpgradedNutrientPasteDispenserModExtension>())
        {
            return base.HasEnoughFeedstockInHoppers();
        }

        var modExtension = def.GetModExtension<UpgradedNutrientPasteDispenserModExtension>();
        var requiredMeatNutrition = modExtension.meatNutritionCostPerDispense;
        var requiredVeggieNutrition = modExtension.veggieNutritionCostPerDispense;

        float meatNutrition = 0f;
        float veggieNutrition = 0f;
        for (int i = 0; i < AdjCellsCardinalInBounds.Count; i++)
        {
            IntVec3 c = AdjCellsCardinalInBounds[i];
            Thing foodThing = null;
            Thing hopperThing = null;
            List<Thing> thingList = c.GetThingList(Map);
            foreach (Thing thing in thingList)
            {
                if (IsAcceptableFeedstock(thing.def))
                {
                    foodThing = thing;
                }
                if (thing.IsHopper())
                {
                    hopperThing = thing;
                }
            }
            if (foodThing != null && hopperThing != null)
            {
                if ((foodThing.def.ingestible.foodType & meatFlags) != FoodTypeFlags.None)
                {
                    meatNutrition += foodThing.stackCount * foodThing.GetStatValue(StatDefOf.Nutrition);
                }
                else if ((foodThing.def.ingestible.foodType & veggieFlags) != FoodTypeFlags.None)
                {
                    veggieNutrition += foodThing.stackCount * foodThing.GetStatValue(StatDefOf.Nutrition);
                }
            }
            if (meatNutrition >= requiredMeatNutrition && veggieNutrition >= requiredVeggieNutrition)
            {
                return true;
            }
        }
        return false;
    }

    public override Thing TryDispenseFood()
    {
        if (!def.HasModExtension<UpgradedNutrientPasteDispenserModExtension>())
        {
            return base.TryDispenseFood();
        }

        if (!CanDispenseNow)
        {
            return null;
        }

        var modExtension = def.GetModExtension<UpgradedNutrientPasteDispenserModExtension>();
        var requiredMeatNutrition = modExtension.meatNutritionCostPerDispense - 0.0001f;
        var requiredVeggieNutrition = modExtension.veggieNutritionCostPerDispense - 0.0001f;

        List<ThingDef> ingredientList = [];

        do
        {
            Thing thing = FindFeedInAnyHopperOfType(meatFlags);
            if (thing == null)
            {
                Log.Error("Did not find enough meat in hoppers while trying to dispense.");
                return null;
            }
            int numThingsToTake = Mathf.Min(thing.stackCount, Mathf.CeilToInt(requiredMeatNutrition / thing.GetStatValue(StatDefOf.Nutrition)));
            requiredMeatNutrition -= numThingsToTake * thing.GetStatValue(StatDefOf.Nutrition);
            ingredientList.Add(thing.def);
            thing.SplitOff(numThingsToTake);
        }
        while (!(requiredMeatNutrition <= 0f));

        do
        {
            Thing thing = FindFeedInAnyHopperOfType(veggieFlags);
            if (thing == null)
            {
                Log.Error("Did not find enough veggie food in hoppers while trying to dispense.");
                return null;
            }
            int numThingsToTake = Mathf.Min(thing.stackCount, Mathf.CeilToInt(requiredVeggieNutrition / thing.GetStatValue(StatDefOf.Nutrition)));
            requiredVeggieNutrition -= numThingsToTake * thing.GetStatValue(StatDefOf.Nutrition);
            ingredientList.Add(thing.def);
            thing.SplitOff(numThingsToTake);
        }
        while (!(requiredVeggieNutrition <= 0f));

        def.building.soundDispense.PlayOneShot(new TargetInfo(Position, Map));
        Thing dispensableThing = ThingMaker.MakeThing(DispensableDef);
        CompIngredients compIngredients = dispensableThing.TryGetComp<CompIngredients>();
        for (int i = 0; i < ingredientList.Count; i++)
        {
            compIngredients.RegisterIngredient(ingredientList[i]);
        }
        return dispensableThing;
    }

    public virtual Thing FindFeedInAnyHopperOfType(FoodTypeFlags foodType)
    {
        for (int i = 0; i < AdjCellsCardinalInBounds.Count; i++)
        {
            Thing foodThing = null;
            Thing hopperThing = null;
            List<Thing> thingList = AdjCellsCardinalInBounds[i].GetThingList(Map);
            for (int j = 0; j < thingList.Count; j++)
            {
                Thing thing3 = thingList[j];
                if (IsAcceptableFeedstock(thing3.def))
                {
                    foodThing = thing3;
                }
                if (thing3.IsHopper())
                {
                    hopperThing = thing3;
                }
            }
            if (foodThing != null && hopperThing != null && (foodThing.def.ingestible.foodType & foodType) != FoodTypeFlags.None)
            {
                return foodThing;
            }
        }
        return null;
    }
}
