using BluraySharp.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BluraySharp.Extension.Ssls
{
	public class SyncSubtitleList : IXmlSerializable
	{
		public IList<EsTrack> EsTracks { get; }
		public IList<EsGroup> EsGroups { get; }

		public XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}

		public void ReadXml(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		public void WriteXml(XmlWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}
