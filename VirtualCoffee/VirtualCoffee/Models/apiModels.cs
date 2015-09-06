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

    public class GoodsItemModel
    {
        public String item { get; set; }
        public Double price { get; set; }
        public Int32 count { get; set; }
    }

    public class GoodsModel
    {
        public GoodsItemModel[] goods { get; set; }
    }

    public class PurchaseInfoModel
    {
        public Double Sum { get; set; }
    }

}