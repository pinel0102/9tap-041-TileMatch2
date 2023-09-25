using System.Collections.Generic;
using System.Xml.Serialization;

namespace Gpm.Dlst
{    
    [XmlRoot("exclude"), XmlType("exclude")]
    public class ExcludeVO
    {
        [XmlArray("excludeList")]
        [XmlArrayItem("exclude")]
        public List<string> excludeList;
    }
}