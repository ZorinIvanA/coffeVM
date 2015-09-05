using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualCoffee.Models
{
    public class CoinsInPurseModel
    {
        public String value { get; set; }
        public String count { get; set; }
    }

    public class PurseModel
    {
        public CoinsInPurseModel[] Coins { get; set; }
    }
}