using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using VirtualCoffee.Models;
using VirtualCoffee.Repository;

namespace VirtualCoffee.Controllers
{
    public class FundsShortageException : Exception
    {
        public FundsShortageException() : base("Не хватает монет для покупки!") { }
    }

    public class LowFundsEnteredException : Exception
    {
        public LowFundsEnteredException(String item) :
            base(String.Format("Вы внесли недостаточно средств для покупки {0}!", item)) { }
    }

    public class LowGoodsInStockException : Exception
    {
        public LowGoodsInStockException(String item) :
            base(String.Format("Товар {0} закончился!", item)) { }
    }

    public class WrongGoodsException : Exception
    {
        public WrongGoodsException(String item) :
            base(String.Format("Товара {0} нет в автомате!", item)) { }
    }

    public class NotEnoughtMoneyException : Exception
    {
        public NotEnoughtMoneyException(String kind) : base(String.Format("В автомате не хватает монет достоинством {0}!", kind)) { }
    }

    public class TempPurse : PurseBase
    { }

    public class CoffeeMachineController : ApiController
    {
        CoffeDataContext _ctx;
        String _path = String.Empty;

        public CoffeeMachineController(CoffeDataContext ctx)
        {
            _ctx = ctx;
        }

        public CoffeeMachineController()
        {
            _ctx = new CoffeDataContext();
            String path = HttpContext.Current.Server.MapPath("~/App_Data") + "\\";
            _ctx.Load(path);
            _path = path;
        }

        /// <summary>
        /// Возвращает кошелёк пользователя
        /// </summary>
        /// <returns>Кошелёк</returns>
        [Route("api/userpurse")]
        public async Task<PurseModel> GetUserPurse()
        {
            return await Task.Run(() =>
            {
                PurseModel model = new PurseModel();
                model.Coins = new CoinInPurseModel[_ctx.UserPurse.Item.Coins.Count];
                Int32 i = 0;

                foreach (var coin in _ctx.UserPurse.Item.Coins.ToArray())
                {
                    model.Coins[i] = new CoinInPurseModel() { value = coin.Value, count = coin.Count.ToString() };
                    i++;
                }
                return model;
            });
        }

        /// <summary>
        /// Возвращает кошелёк машины
        /// </summary>
        /// <returns>Кошелёк машины</returns>
        [Route("api/machinepurse")]
        public async Task<PurseModel> GetMachinePurse()
        {
            return await Task.Run(() =>
            {
                PurseModel model = new PurseModel();
                model.Coins = new CoinInPurseModel[_ctx.CoffeMachinePurse.Item.Coins.Count];
                Int32 i = 0;

                foreach (var coin in _ctx.CoffeMachinePurse.Item.Coins.ToArray())
                {
                    model.Coins[i] = new CoinInPurseModel()
                    {
                        value = coin.Value,
                        count = coin.Count.ToString()
                    };
                    i++;
                }
                return model;
            });
        }

        /// <summary>
        /// Возвращает список товаров
        /// </summary>
        /// <returns>Список товаров</returns>
        [Route("api/goods")]
        public async Task<GoodsModel> GetGoods()
        {
            return await Task<GoodsModel>.Run(() =>
            {

                GoodsModel model = new GoodsModel();
                model.goods = new GoodsItemModel[_ctx.Goods.Item.Goods.Count];
                Int32 i = 0;
                foreach (var good in _ctx.Goods.Item.Goods)
                {
                    model.goods[i] = new GoodsItemModel()
                    {
                        count = good.Count,
                        price = good.Price,
                        item = good.Name
                    };
                    i++;
                }
                return model;
            });
        }

        /// <summary>
        /// Возвращает информацию о покупке (сейчас только потраченная сумма)
        /// </summary>
        /// <returns>Информация о покупке</returns>
        [Route("api/purchaseinfo")]
        public async Task<PurchaseInfoModel> GetPurchaseInfo()
        {
            return await Task.Run(() =>
            {
                PurchaseInfoModel model = new PurchaseInfoModel()
                {
                    Sum = _ctx.PurchaseInfo.Item.PayedSum
                };
                return model;
            });
        }

        /// <summary>
        /// Пользователь платит монетку
        /// </summary>
        /// <param name="value">Номинал монетки</param>
        /// <returns>Сведения об ошибке</returns>
        [Route("api/pay")]
        [HttpGet]
        public async Task<Object> PayCoin([FromUri]Int32 value)
        {
            return await Task<Object>.Run(() =>
            {
                if (_ctx.UserPurse.Item.Coins.Any(x => x.Value == value.ToString()))
                {
                    if (_ctx.UserPurse.Item.Coins.First(x => x.Value == value.ToString()).Count > 0)
                    {
                        _ctx.CoffeMachinePurse.Item.Coins.First(x => x.Value == value.ToString()).Count++;
                        _ctx.UserPurse.Item.Coins.First(x => x.Value == value.ToString()).Count--;
                        _ctx.PurchaseInfo.Item.PayedSum += value;
                        _ctx.Save(_path);

                        return new { error = String.Empty };
                    }
                    else
                    {
                        return new { error = String.Format("Монеты номиналом {0} закончились!", value) };
                    }
                }
                else
                {
                    return new { error = String.Format("Монеты номиналом {0} не существует!", value) };
                }
            });
        }

        /// <summary>
        /// Выдаёт сдачу наименьшим количеством монеток
        /// </summary>
        /// <returns>Сообщение об успехе или об ошибке</returns>
        [Route("api/getchange")]
        [HttpGet]
        public async Task<Object> GetChange()
        {
            return await Task.Run(() =>
            {
                try
                {
                    Double sum = _ctx.PurchaseInfo.Item.PayedSum;


                    Double sumToMessage = sum;
                    PurseBase coinsToChange = MakeChange(sum);

                    MakeUpdatesInPurses(coinsToChange);

                    return new
                    {
                        error = String.Empty,
                        message = String.Format("Сдача в размере {0} выдана!", sumToMessage)
                    };
                }
                catch (Exception e)
                {
                    return new
                    {
                        error = e.Message,
                        message = String.Empty
                    };
                }
            });
        }

        /// <summary>
        /// Добавляет изменения в "БД"
        /// </summary>
        /// <param name="coinsToChange">Монеты для сдачи</param>
        protected void MakeUpdatesInPurses(PurseBase coinsToChange)
        {
            foreach (var coin in coinsToChange.Coins)
            {
                _ctx.UserPurse.Item.Coins.First(x => x.Value == coin.Value).Count += coin.Count;
                _ctx.CoffeMachinePurse.Item.Coins.First(x => x.Value == coin.Value).Count -= coin.Count;
            }
            _ctx.PurchaseInfo.Item.PayedSum = 0;
            _ctx.Save(_path);
        }

        /// <summary>
        /// Готовит сдачу к выдаче
        /// </summary>
        /// <param name="sum">Сумма сдачи</param>
        /// <returns>Монеты для сдачи</returns>
        protected PurseBase MakeChange(Double sum)
        {
            if (_ctx.CoffeMachinePurse.Item.Sum <= sum)
            {
                throw new Exception("В автомате не хватает денег на сдачу!");
            }

            TempPurse purse = new TempPurse();
            purse.Coins.Add(new Coin() { Value = "10", Count = 0 });
            purse.Coins.Add(new Coin() { Value = "5", Count = 0 });
            purse.Coins.Add(new Coin() { Value = "2", Count = 0 });
            purse.Coins.Add(new Coin() { Value = "1", Count = 0 });

            Double sumToWork = sum;

            for (Int32 i = 0; i < purse.Coins.Count; i++)
            {
                var coin = purse.Coins[i];
                Int32 intCoin = Int32.Parse(coin.Value);
                Int32 coinInMachine =
                    _ctx.CoffeMachinePurse.Item.Coins.First(x => x.Value == coin.Value).Count;
                while ((sumToWork >= intCoin) && (coinInMachine > 0))
                {
                    sumToWork -= intCoin;
                    purse.Coins[i].Count++;
                    coinInMachine--;
                }
            }

            if (purse.Sum == sum)
            {
                return purse;
            }
            else
            {
                throw new NotEnoughtMoneyException("1");
            }
        }

        /// <summary>
        /// Покупка товара пользователем
        /// </summary>
        /// <param name="item">Наименование товара</param>
        /// <returns>Сведения об ошибке или благодарность за покупку</returns>
        [Route("api/buy")]
        [HttpGet]
        public async Task<Object> BuyItem([FromUri]String item)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (!_ctx.Goods.Item.Goods.Any(x => x.Name == item))
                    {
                        throw new WrongGoodsException(item);
                    }
                    var goodsItem = _ctx.Goods.Item.Goods.First(x => x.Name == item);
                    if (_ctx.PurchaseInfo.Item.PayedSum < goodsItem.Price)
                    {
                        throw new LowFundsEnteredException(goodsItem.Name);
                    }

                    if (goodsItem.Count <= 0)
                    {
                        throw new LowGoodsInStockException(goodsItem.Name);
                    }

                    _ctx.Goods.Item.Goods.First(x => x.Name == item).Count--;
                    _ctx.PurchaseInfo.Item.PayedSum -= goodsItem.Price;
                    _ctx.Save(_path);
                    return new
                    {
                        error = String.Empty,
                        message = String.Format("Вы купили {0}, спасибо за покупку!", goodsItem.Name)
                    };
                }
                catch (Exception e)
                {
                    return new
                    {
                        error = e.Message,
                        message = String.Empty
                    };
                }
            });
        }

        /// <summary>
        /// Сброс системы - инициализация стартовыми значениями
        /// </summary>
        /// <returns>Сведения об ошибке</returns>
        [Route("api/init")]
        [HttpGet]
        public async Task<Object> Initialize()
        {
            return await Task.Run(() =>
            {
                this._ctx.Init(_path);
                return new { error = String.Empty };
            });

        }

    }
}
