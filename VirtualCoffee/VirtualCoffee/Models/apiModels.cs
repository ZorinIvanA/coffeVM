using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VirtualCoffee.Models
{
    /// <summary>
    /// Монетка в кошельке (модель)
    /// </summary>
    public class CoinInPurseModel
    {
        public String value { get; set; }
        public String count { get; set; }
    }

    /// <summary>
    /// Модель кошелька
    /// </summary>
    public class PurseModel
    {
        public CoinInPurseModel[] Coins { get; set; }
    }

    /// <summary>
    /// Модель товара
    /// </summary>
    public class GoodsItemModel
    {
        public String item { get; set; }
        public Double price { get; set; }
        public Int32 count { get; set; }
    }

    /// <summary>
    /// Модель списка товаров
    /// </summary>
    public class GoodsModel
    {
        public GoodsItemModel[] goods { get; set; }
    }

    /// <summary>
    /// Модель информации о покупке
    /// </summary>
    public class PurchaseInfoModel
    {
        public Double Sum { get; set; }
    }

}