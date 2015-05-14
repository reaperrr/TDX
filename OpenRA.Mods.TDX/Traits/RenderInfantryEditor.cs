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
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.TDX.Traits
{
	public class RenderInfantryEditorInfo : ITraitInfo, ILegacyEditorRenderInfo, Requires<RenderSpritesInfo>
	{
		public object Create(ActorInitializer init) { return new RenderInfantryEditor(init, this); }

		// HACK, too lazy to figure out how to get the palette properly
		public string EditorPalette { get { return "terrain"; } }
		public string EditorImage(ActorInfo actor, SequenceProvider sequenceProvider, string race)
		{
			var rsi = actor.Traits.Get<RenderSpritesInfo>();
			return rsi.GetImage(actor, sequenceProvider, race);
		}
	}

	public class RenderInfantryEditor
	{
		public RenderInfantryEditor(ActorInitializer init, RenderInfantryEditorInfo info) { }
	}
}
