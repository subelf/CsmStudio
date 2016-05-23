using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PesMuxer.Texplate
{
	public interface ITexplate : ITexplateContext
	{
		IDisposable EnterContext(ITexplateContext context);
	}
}
