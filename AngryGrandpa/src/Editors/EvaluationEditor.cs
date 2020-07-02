﻿using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using static System.Math;

namespace AngryGrandpa
{
	internal class EvaluationEditor : IAssetEditor
	{
		protected static IModHelper Helper => ModEntry.Instance.Helper;
		protected static IMonitor Monitor => ModEntry.Instance.Monitor;
		protected static ModConfig Config => ModConfig.Instance;

		protected static ITranslationHelper i18n = Helper.Translation;

		public static readonly List<string> EvaluationStrings =
			new List<string>
		{
			"1CandleResult",
			"2CandleResult",
			"3CandleResult",
			"4CandleResult",
			"1CandleReevaluation",
			"2CandleReevaluation",
			"3CandleReevaluation",
			"4CandleReevaluation"
		};

		public bool CanEdit<_T> (IAssetInfo asset)
		{
			return asset.AssetNameEquals($"Strings\\StringsFromCSFiles");
		}

		public void Edit<_T> (IAssetData asset)
		{
			// Can't edit these assets without an active game for spouse, NPC, and year info

			if (!Context.IsWorldReady)
			{ return; }

			// Prepare tokens

			string pastYears;
			int yearsPassed = Max(Game1.year - 1, Config.YearsBeforeEvaluation); // Accurate dialogue even for delayed event
			if (yearsPassed >= 10)
			{
				if (Config.GrandpaDialogue == "Nuclear") { pastYears = i18n.Get("GrandpaDuringManyYears.Nuclear"); }
				else { pastYears = i18n.Get("GrandpaDuringManyYears"); }
			}
			else // yearsPassed < 10
			{
				pastYears = i18n.Get("GrandpaDuringPastYears").ToString().Split('|')[yearsPassed];
			}

			string spouseOrLewis;
			if (Game1.player.isMarried()) { spouseOrLewis = "%spouse"; }
			else {spouseOrLewis = Game1.getCharacterFromName<NPC>("Lewis").displayName; }

			string fifthCandle = "";
			bool inOneYear = Game1.year == 1 || (Game1.year == 2 && Game1.currentSeason == "spring" && Game1.dayOfMonth == 1);
			if (Utility.getGrandpaScore() >= 21 && inOneYear)
			{ 
				fifthCandle = i18n.Get("FifthCandle." + Config.GrandpaDialogue);
			}

			var tokens = new
			{
				pastYears,
				spouseOrLewis,
				fifthCandle
			};

			// Prepare data

			var data = asset.AsDictionary<string, string>().Data;

			// Main patching loop

			if (asset.AssetNameEquals($"Strings\\StringsFromCSFiles"))
			{
				foreach (string entry in EvaluationStrings)
				{
					string gameKey = i18n.Get(entry + ".gameKey");
					string modKey = entry + "." + Config.GrandpaDialogue;
					if (Config.GenderNeutrality) { modKey += "-gn"; }
					string value = i18n.Get(modKey, tokens);

					data[gameKey] = value;
				}
			}

			Monitor.Log($"Edited {asset.AssetName}", LogLevel.Debug);
		}
	}
}