using BluraySharp.Common;
using PesMuxer.MuxProject;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsmStudio.ProjectManager.Compile
{
	class PgsEntryDescriptor
	{
		public PgsEntryDescriptor(ushort pid, string pesFile, TimeSpan length, BdLang lang)
		{
			this.Pid = pid;
			this.PesFile = pesFile;
			this.Length = length;
			this.Lang = lang;
		}

		public ushort Pid { get; private set; }
		public string PesFile { get; private set; }
		public TimeSpan Length { get; private set; }
		public BdLang Lang { get; private set; }
	}
}
