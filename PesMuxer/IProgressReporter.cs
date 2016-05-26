using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PesMuxer
{
	public interface IProgressReporter
	{
		float Amount { set; }
		float Progress { set; }

		bool IsCanceled { get; }
		void OnTaskEnd();
	};

}
