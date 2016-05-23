using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PesMuxer.Texplate
{
	public class Texplate : ITexplate
	{
		private Stack<ITexplateContext> contextStack =
			new Stack<ITexplateContext>();

		public ITexplateClause this[string name]
		{
			get
			{
				foreach(var iCtx in contextStack)
				{
					var tClause = iCtx[name];
					if(tClause != null)
					{
						return tClause;
					}
				}

				return null;
			}
		}

		public IDisposable EnterContext(ITexplateContext context)
		{
			return new ContextStackEntry(this, context);
		}

		private sealed class ContextStackEntry : IDisposable
		{
			public ContextStackEntry(Texplate stack, ITexplateContext context)
			{
				this.stack = stack;
				this.context = context;

				this.stack.contextStack.Push(this.context);
			}

			private Texplate stack;
			private ITexplateContext context;

			private bool isDisposed = false;
			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			private void Dispose(bool isDisposing)
			{
				if (!isDisposed)
				{
					if (isDisposing)
					{
						//managed res.
						var tContext = this.stack.contextStack.Pop();
						if (!object.ReferenceEquals(tContext, this.context))
						{
							throw new InvalidOperationException("Texplate stack is ruined.");
						}
					}					

					this.isDisposed = true;
				}
			}

			~ContextStackEntry()
			{
				this.Dispose(false);
			}
		}
	}
}
