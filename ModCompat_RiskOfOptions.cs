using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace GoldChestForAll
{
    internal class ModCompat_RiskOfOptions
    {
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Initialize()
        {
            RiskOfOptions.ModSettingsManager.SetModDescription("Makes the healthbar look like how it does in ROR1 when immune", "com.DestroyedClone.GoldChestForAll", "GoldChestForAll");

            ModSettingsManager.AddOption(new FloatFieldOption(GCFAPlugin.CfgCostMultiplier));
            ModSettingsManager.AddOption(new CheckBoxOption(GCFAPlugin.CfgIntent));
            ModSettingsManager.AddOption(new FloatFieldOption(GCFAPlugin.CfgCostMultiplierPerPlayerAdditive));
        }


    }
}
