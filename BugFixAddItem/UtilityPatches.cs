﻿using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;

using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace BugFixAddItem
{
    class UtilityPatches
	{
		private static IModHelper Helper => ModEntry.Instance.Helper;
		private static IMonitor Monitor => ModEntry.Instance.Monitor;

		private static HarmonyInstance Harmony => ModEntry.Instance.Harmony;

		public static void Apply()
		{
			Harmony.Patch(
				original: AccessTools.Method(typeof(Utility), nameof(Utility.addItemToInventory)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(UtilityPatches), nameof(UtilityPatches.addItemToInventory_Prefix))),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(UtilityPatches), nameof(UtilityPatches.addItemToInventory_Transpiler)))
			);
		}

        public static void addItemToInventory_Prefix(ItemGrabMenu.behaviorOnItemSelect __onAddFunction)
        {
            try
            {
                Monitor.Log($"Invoked Utility.addItemToInventory with __onAddFunction: {__onAddFunction}", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(addItemToInventory_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        
        public static IEnumerable<CodeInstruction> addItemToInventory_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator gen)
        {
            try
            {
                var codes = new List<CodeInstruction>(instructions);

                for (int i = 0; i < codes.Count - 3; i++)
                {
                    // Find any null value appearing as the last argument of a ItemGrabMenu.behaviorOnItemSelect delegate method call
                    if (codes[i].opcode == OpCodes.Call)
                    {
                        var gameVar = Game1.game1;
                        Monitor.Log($"Found OpCode: {codes[i]}", LogLevel.Debug);
                    }

                    if (codes[i].opcode == OpCodes.Ldarg_3 &&
                        codes[i + 1].opcode == OpCodes.Ldarg_0 && 
                        codes[i + 2].opcode == OpCodes.Ldnull && // The (Farmer) null value we want to change
                        codes[i + 3].opcode == OpCodes.Callvirt &&
                        codes[i + 3].operand.ToString() == "Void Invoke(StardewValley.Item, StardewValley.Farmer)")
                    {                    
                        // change (Farmer) null to Game1.player
                        codes[i + 2] = new CodeInstruction(OpCodes.Call, typeof(Game1).GetProperty("player").GetGetMethod());

                        Monitor.Log($"Edited OpCode: {codes[i + 2]}", LogLevel.Debug);
                    }
                }
                return codes.AsEnumerable();
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(addItemToInventory_Transpiler)}:\n{ex}", LogLevel.Error);
                return instructions; // use original code
            }
        }
    }
}
