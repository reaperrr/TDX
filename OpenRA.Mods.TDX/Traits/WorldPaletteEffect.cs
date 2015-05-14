#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Traits;

namespace OpenRA.Mods.TDX.Traits
{
	[Desc("Fades the world from/to black at the start/end of the game, and can (optionally) desaturate the world",
	"or enable daytime-dependent lighting, including day/night cycles.")]
	public class WorldPaletteEffectInfo : ITraitInfo
	{
		public readonly string[] ExcludePalettes = { "flashyeffects", "cursor", "chrome", "colorpicker", "fog", "shroud" };

		[Desc("Time (in ticks) to fade between black and InitialEffect/MenuEffect",
		"and from current effect to black (when closing map etc.).")]
		public readonly int FadeLength = 10;

		public readonly int MorningDayTransitionLength = 60 * 25;
		public readonly int DayLength = 2 * 60 * 25;
		public readonly int DayEveningTransitionLength = 60 * 25;
		public readonly int EveningNightTransitionLength = 60 * 25;
		public readonly int NightLength = 2 * 60 * 25;
		public readonly int NightMorningTransitionLength = 60 * 25;

		public readonly int MorningFactorRed = 75;
		public readonly int MorningFactorGreen = 85;
		public readonly int MorningFactorBlue = 85;

		public readonly int DaylightFactorRed = 100;
		public readonly int DaylightFactorGreen = 100;
		public readonly int DaylightFactorBlue = 100;

		public readonly int EveningFactorRed = 85;
		public readonly int EveningFactorGreen = 60;
		public readonly int EveningFactorBlue = 75;

		public readonly int NightFactorRed = 35;
		public readonly int NightFactorGreen = 45;
		public readonly int NightFactorBlue = 60;

		[Desc("Should the game cycle through daytimes.")]
		public readonly bool DayNightCycle = false;

		[Desc("Effect style to fade to when loading map. Accepts values of None, Desaturated, Morning, Day, Evening and Night.")]
		public readonly WorldPaletteEffect.LightingEffectType InitialEffect = WorldPaletteEffect.LightingEffectType.None;

		public object Create(ActorInitializer init) { return new WorldPaletteEffect(this); }
	}

	public class WorldPaletteEffect : IPaletteModifier, ITickRender, IWorldLoaded
	{
		public enum LightingEffectType { None, Black, Desaturated, Morning, Day, Evening, Night }
		public readonly WorldPaletteEffectInfo Info;
		readonly bool dayNightCycle;

		bool dayEnds;
		bool nightEnds;

		int remainingFrames;
		int fadeLength;
		LightingEffectType from = LightingEffectType.Black;
		LightingEffectType to = LightingEffectType.Black;

		public WorldPaletteEffect(WorldPaletteEffectInfo info)
		{
			Info = info;
			dayNightCycle = Info.DayNightCycle;
			dayEnds = false;
			nightEnds = false;
		}

		public void WorldLoadFade(LightingEffectType type)
		{
			fadeLength = Info.FadeLength;
			remainingFrames = fadeLength;
			from = to;
			to = type;
		}

		public void MorningDayFade(LightingEffectType type)
		{
			fadeLength = Info.MorningDayTransitionLength;
			remainingFrames = fadeLength;
			from = to;
			to = type;
		}

		public void DayDuration(LightingEffectType type)
		{
			fadeLength = Info.DayLength;
			remainingFrames = fadeLength;
			from = to;
			to = type;
			dayEnds = true;
		}

		public void DayEveningFade(LightingEffectType type)
		{
			fadeLength = Info.DayEveningTransitionLength;
			remainingFrames = fadeLength;
			from = to;
			to = type;
			dayEnds = false;
		}

		public void EveningNightFade(LightingEffectType type)
		{
			fadeLength = Info.EveningNightTransitionLength;
			remainingFrames = fadeLength;
			from = to;
			to = type;
		}

		public void NightDuration(LightingEffectType type)
		{
			fadeLength = Info.NightLength;
			remainingFrames = fadeLength;
			from = to;
			to = type;
			nightEnds = true;
		}

		public void NightMorningFade(LightingEffectType type)
		{
			fadeLength = Info.NightMorningTransitionLength;
			remainingFrames = fadeLength;
			from = to;
			to = type;
			nightEnds = false;
		}

		public void TickRender(WorldRenderer wr, Actor self)
		{
			if (remainingFrames > 0)
				remainingFrames--;

			if (!dayNightCycle)
				return;

			if (to == LightingEffectType.Morning && remainingFrames == 0)
				MorningDayFade(LightingEffectType.Day);
			if (to == LightingEffectType.Day && remainingFrames == 0 && !dayEnds)
				DayDuration(LightingEffectType.Day);
			if (to == LightingEffectType.Day && remainingFrames == 0 && dayEnds)
				DayEveningFade(LightingEffectType.Evening);
			if (to == LightingEffectType.Evening && remainingFrames == 0)
				EveningNightFade(LightingEffectType.Night);
			if (to == LightingEffectType.Night && remainingFrames == 0 && !nightEnds)
				NightDuration(LightingEffectType.Night);
			if (to == LightingEffectType.Night && remainingFrames == 0 && nightEnds)
				NightMorningFade(LightingEffectType.Morning);
		}

		public Color ColorForEffect(LightingEffectType t, Color orig)
		{
			switch (t)
			{
				case LightingEffectType.Black:
					return Color.FromArgb(orig.A, Color.Black);
				case LightingEffectType.Desaturated:
					var lum = (int)(255 * orig.GetBrightness());
					return Color.FromArgb(orig.A, lum, lum, lum);
				case LightingEffectType.Morning:
					var mored = (int)(orig.R * Info.MorningFactorRed / 100).Clamp(0, 255);
					var mogreen = (int)(orig.G * Info.MorningFactorGreen / 100).Clamp(0, 255);
					var moblue = (int)(orig.B * Info.MorningFactorBlue / 100).Clamp(0, 255);
					return Color.FromArgb(orig.A, mored, mogreen, moblue);
				case LightingEffectType.Day:
					var red = (int)(orig.R * Info.DaylightFactorRed / 100).Clamp(0, 255);
					var green = (int)(orig.G * Info.DaylightFactorGreen / 100).Clamp(0, 255);
					var blue = (int)(orig.B * Info.DaylightFactorBlue / 100).Clamp(0, 255);
					return Color.FromArgb(orig.A, red, green, blue);
				case LightingEffectType.Evening:
					var evred = (int)(orig.R * Info.EveningFactorRed / 100).Clamp(0, 255);
					var evgreen = (int)(orig.G * Info.EveningFactorGreen / 100).Clamp(0, 255);
					var evblue = (int)(orig.B * Info.EveningFactorBlue / 100).Clamp(0, 255);
					return Color.FromArgb(orig.A, evred, evgreen, evblue);
				case LightingEffectType.Night:
					var nired = (int)(orig.R * Info.NightFactorRed / 100).Clamp(0, 255);
					var nigreen = (int)(orig.G * Info.NightFactorGreen / 100).Clamp(0, 255);
					var niblue = (int)(orig.B * Info.NightFactorBlue / 100).Clamp(0, 255);
					return Color.FromArgb(orig.A, nired, nigreen, niblue);
				default:
				case LightingEffectType.None:
					return orig;
			}
		}

		public void AdjustPalette(IReadOnlyDictionary<string, MutablePalette> palettes)
		{
			if (to == LightingEffectType.None && remainingFrames == 0)
				return;

			foreach (var kvp in palettes)
			{
				if (Info.ExcludePalettes.Contains(kvp.Key))
					continue;

				var pal = kvp.Value;

				for (var x = 0; x < Palette.Size; x++)
				{
					var orig = pal.GetColor(x);
					var t = ColorForEffect(to, orig);

					if (remainingFrames == 0)
						pal.SetColor(x, t);
					else
					{
						var f = ColorForEffect(from, orig);
						pal.SetColor(x, Exts.ColorLerp((float)remainingFrames / fadeLength, t, f));
					}
				}
			}
		}

		public void WorldLoaded(World w, WorldRenderer wr)
		{
			WorldLoadFade(Info.InitialEffect);
		}
	}
}
