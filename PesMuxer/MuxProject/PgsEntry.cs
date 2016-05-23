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
		public PgsEntry(ushort pid, string pesPath, CultureInfo lang)
		{
			this["PgsPID"] = Clause(pid.ToString());
			this["PgsPes"] = Clause(pesPath.AssertNotNull("pesPath"));
			this["PgsMui"] = Clause(pesPath + ".mui");
			this["PgsLang"] = Clause(lang.AssertNotNull("lang").ThreeLetterISOLanguageName);
		}
	}
}
