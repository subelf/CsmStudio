using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PesMuxer.Texplate
{
	public class TexplateContextBase : ITexplateContext
	{
		private Dictionary<string, ITexplateClause> dictionary =
			new Dictionary<string, ITexplateClause>();

		public ITexplateClause this[string name]
		{
			get
			{
				var tName = name.AssertNotNull("name").ToLower();

				if (dictionary.ContainsKey(tName))
				{
					return dictionary[tName];
				}
				else
				{
					return null;
				}
			}
			protected set
			{
				var tName = name.AssertNotNull("name").ToLower();

				dictionary[tName.ToLower()] = value;
			}
		}

		protected static ITexplateClause Clause(string clauser)
		{
			return new TexplateClause(xTexplate => clauser);
		}

		protected static ITexplateClause Clause(Func<string> clauseRenderer)
		{
			return new TexplateClause(xTexplate => clauseRenderer());
		}

		protected static ITexplateClause Clause(Func<ITexplate, string> clauseRenderer)
		{
			return new TexplateClause(clauseRenderer);
		}

		protected static ITexplateClause Clause(StreamReader streamReader)
		{
			return new TexplateClause(streamReader);
		}
	}
}
