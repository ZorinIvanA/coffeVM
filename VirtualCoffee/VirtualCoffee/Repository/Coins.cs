using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

using System.Xml.Serialization;

namespace VirtualCoffee.Repository
{
    /// <summary>
    /// Монета
    /// </summary>
    [XmlType("Coin")]
    public class Coin
    {
        [XmlAttribute("kind")]
        public String Value { get; set; }
        [XmlAttribute("number")]
        public Int32 Count { get; set; }
    }
}