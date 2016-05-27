using BluraySharp.Extension.Ssls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsmStudio.ProjectManager.Compile
{
	public class EsEntryDescriptor
	{
		private EsEntry entryInfo;

		public EsEntryDescriptor(EsEntry esEntry, TimeSpan syncOffset)
		{
			this.entryInfo = esEntry;
			this.SyncOffset = syncOffset;
		}
		
		public FileInfo Source { get { return entryInfo.Source; } }
		public TimeSpan SyncOffset { get; private set; }
	}
}
