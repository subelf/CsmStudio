using BluraySharp.Extension.Ssls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace CsmStudio.ProjectManager.Compile
{
	public class EsTrackDescriptor : IEnumerable<EsEntryDescriptor>
	{
		private IList<EsGroup> EsGroups;
		public EsTrack Track { get; private set; }

		public EsTrackDescriptor(IList<EsGroup> esGroups, EsTrack track)
		{
			this.EsGroups = esGroups;
			this.Track = track;
		}

		public IEnumerator<EsEntryDescriptor> GetEnumerator()
		{
			foreach(var iEsGroup in EsGroups)
			{
				if(iEsGroup.Entries.ContainsKey(this.Track))
				{
					yield return new EsEntryDescriptor(iEsGroup.Entries[this.Track], iEsGroup.SyncOffset);
				}
				else
				{
					yield return null;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
