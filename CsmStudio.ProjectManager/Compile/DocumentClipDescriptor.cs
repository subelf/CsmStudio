using BluraySharp.Common;
using BluraySharp.Common.BdStandardPart;
using BluraySharp.Extension.Ssls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsmStudio.ProjectManager.Compile
{
	class DocumentClipDescriptor
	{
		public DocumentClipDescriptor(uint clipId, TimeSpan inTimeOffset, BdViFormat format, BdViFrameRate rate) :
			this(clipId, inTimeOffset, format, rate,  null)
		{
		}

		public DocumentClipDescriptor(uint clipId, TimeSpan inTimeOffset, BdViFormat format, BdViFrameRate rate, SyncSubtitleList ssls)
		{
			Format = format;
			Rate = rate;
			ClipId = clipId;
			InTimeOffset = inTimeOffset;

			if (ssls == null)
			{
				Tracks = new List<EsTrackDescriptor>();
				EsGroups = new List<EsGroup>();
			}
			else
			{
				Tracks = (from iTracks in ssls.EsTracks select new EsTrackDescriptor(ssls.EsGroups, iTracks)).ToList();
				EsGroups = ssls.EsGroups;
			}
		}

		public uint ClipId { get; private set; }
		public TimeSpan InTimeOffset { get; private set; }
		public BdViFormat Format { get; private set; }
		public BdViFrameRate Rate { get; private set; }
		public IList<EsTrackDescriptor> Tracks { get; private set; }
		public IList<EsGroup> EsGroups { get; private set; }
	}
}
