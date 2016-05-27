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
			if(id > 100000)
			{
				throw new ArgumentOutOfRangeException("id", "Id of clip should not exceed 99999.");
			}

			this[ClipIdKey] = Clause(id.ToString("00000"));
			this["ClipStartTime"] = Clause(startTime.ToBdTimeValue().ToString());
			this["ClipEndTime"] = Clause(endTime.ToBdTimeValue().ToString());
			this["PgsProgInfoCount"] = Clause((xTexplate) => this.pgsList.Count.ToString());

			this["PgsEntryList"] = Clause((xTexplate) => RenderPgsEntries(xTexplate));
			this["PgsProgInfoList"] = Clause((xTexplate) => RenderPgsProgInfoes(xTexplate));
		}

		public string ClipIdName
		{
			get { return this[ClipIdKey].Render(null); }
		}

		private Dictionary<ushort, PgsEntry> pgsList = new Dictionary<ushort, PgsEntry>();

		public void AddPgs(PgsEntry pgs)
		{
			pgs.AssertNotNull("pgs");
			if(pgsList.ContainsKey(pgs.Pid))
			{
				throw new InvalidOperationException("A pgs with same Pid exists.");
			}
			pgsList[pgs.Pid] = pgs;
		}

		public void AddPgsList(IEnumerable<PgsEntry> pgsList)
		{
			foreach(var iPgs in pgsList)
			{
				this.AddPgs(iPgs);
			}
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
				foreach (var iPgs in this.pgsList.Values)
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
