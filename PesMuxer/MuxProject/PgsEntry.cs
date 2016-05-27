using BluraySharp.Common;
using BluraySharp;
using PesMuxer.Texplate;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PesMuxer.MuxProject
{
    public class PgsEntry : TexplateContextBase
	{
		public PgsEntry(ushort pid, string pesPath, BdLang lang)
		{
			if (pid < 0x1200 || pid >= 0x1220)
			{
				throw new ArgumentOutOfRangeException("pid", "Pid of pgs should between [0x1200, 0x1220).");
			}

			this["PgsPID"] = Clause((this.Pid = pid).ToString());
			this["PgsPes"] = Clause(pesPath.AssertNotNull("pesPath"));
			this["PgsMui"] = Clause(pesPath + ".mui");
			this["PgsLang"] = Clause(lang.AssertNotNull("lang").ToIsoLangCode());
		}

		public ushort Pid { get; private set; }
	}
}
