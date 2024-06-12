using System;

namespace UpgradedNutrientPasteDispenser;

// Allows using zetrith.hotswap with this mod
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
internal class HotSwappableAttribute : Attribute
{
}
