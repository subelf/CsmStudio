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
		public IDictionary<EsTrack, EsEntry> Entries { get; }
		
		public TimeSpan SyncOffset { get; set; }
	}
	
	public class EsTrack
	{
		public CultureInfo Language { get; set; }
		public BdStreamCodingType Type { get; set; }
	}

}
