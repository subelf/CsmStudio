using BluraySharp.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace BluraySharp.Extension.Ssls
{
	public class EsEntry
	{
		public EsEntry(FileInfo source) { this.Source = source; }

		public FileInfo Source { get; private set; }
	}

	public class EsGroup
	{
		public EsGroup()
		{
			this.Entries = new Dictionary<EsTrack, EsEntry>();
			SyncOffset = TimeSpan.Zero;
		}

		public IDictionary<EsTrack, EsEntry> Entries { get; }		
		public TimeSpan SyncOffset { get; set; }
	}
	
	public class EsTrack
	{
		public EsTrack(BdStreamCodingType type, BdLang lang)
		{
			this.Lang = lang;
			this.Type = type;
		}

		public BdStreamCodingType Type { get; private set; }
		public BdLang Lang { get; private set; }
	}

}
