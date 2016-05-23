using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PesMuxer.Texplate
{
	public interface ITexplateClause
	{
		string Render(ITexplate texplate);
	}
}
