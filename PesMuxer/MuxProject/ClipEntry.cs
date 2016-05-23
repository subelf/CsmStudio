using PesMuxer.Texplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PesMuxer.MuxProject
{
	public class ClipEntry : TexplateContextBase
	{
		const string ClipIdKey = "ClipId";

		public ClipEntry(uint id, TimeSpan startTime, TimeSpan endTime)
		{
			this[ClipIdKey] = Clause(id.ToString("00000"));
			this["ClipStartTime"] = Clause(startTime.ToBdTimeValue().ToString());
			this["ClipEndTime"] = Clause(endTime.ToBdTimeValue().ToString());

			this["PgsEntryList"] = Clause((xTexplate) => RenderPgsEntries(xTexplate));
			this["PgsProgInfoList"] = Clause((xTexplate) => RenderPgsProgInfoes(xTexplate));
		}

		public string ClipIdName
		{
			get { return this[ClipIdKey].Render(null); }
		}

		private List<PgsEntry> pgsList = new List<PgsEntry>();

		public void AddPgs(PgsEntry pgs)
		{
			this.pgsList.Add(pgs.AssertNotNull("pgs"));
		}

		public void AddPgsList(IEnumerable<PgsEntry> pgsList)
		{
			this.pgsList.AddRange(pgsList.AssertNotNull("pgsList"));
		}

		private string RenderPgsEntries(ITexplate texplate)
		{
			return RenderPgsList(texplate, texplate.AssertNotNull("texplate")["PgsEntry"]);
		}

		private string RenderPgsProgInfoes(ITexplate texplate)
		{
			return RenderPgsList(texplate, texplate.AssertNotNull("texplate")["PgsProgInfo"]);
		}

		private string RenderPgsList(ITexplate texplate, ITexplateClause clause)
		{
			string tRet = string.Empty;

			if (clause != null)
			{
				foreach (var iPgs in this.pgsList)
				{
					using (texplate.EnterContext(iPgs))
					{
						tRet += clause.Render(texplate);
					}
				}
			}

			return tRet;
		}
	}
}
