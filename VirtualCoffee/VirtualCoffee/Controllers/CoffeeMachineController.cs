using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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

    public class CoffeeMachineController : ApiController
    {
        CoffeDataContext _ctx;

        public CoffeeMachineController(CoffeDataContext ctx)
        {
            _ctx = ctx;
        }

        public CoffeeMachineController()
        {
            _ctx = new CoffeDataContext();
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
                _ctx.Load();
                PurseModel model = new PurseModel();
                model.Coins = new CoinsInPurseModel[_ctx.UserPurse.Item.Coins.Count];
                Int32 i = 0;

                foreach (var coin in _ctx.UserPurse.Item.Coins.ToArray())
                {
                    model.Coins[i] = new CoinsInPurseModel() { value = coin.Value, count = coin.Count.ToString() };
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
                _ctx.Load();
                PurseModel model = new PurseModel();
                model.Coins = new CoinsInPurseModel[_ctx.CoffeMachinePurse.Item.Coins.Count];
                Int32 i = 0;

                foreach (var coin in _ctx.CoffeMachinePurse.Item.Coins.ToArray())
                {
                    model.Coins[i] = new CoinsInPurseModel()
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
        /// Пользователь платит монетку
        /// </summary>
        /// <param name="value">Номинал монетки</param>
        /// <returns>Сведения об ошибке</returns>
        [Route("api/pay")]
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
                        _ctx.Save();

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
        public async Task<Object> GetChange()
        {
            return await Task.Run(() =>
            {
                try
                {
                    Double sum = _ctx.PurchaseInfo.Item.PayedSum;
                    if (this.GetTotalMoneyInMachine() <= sum)
                    {
                        throw new Exception("В автомате не хватает денег на сдачу!");
                    }

                    Double sumToMessage = sum;
                    Dictionary<String, Int32> coinsToChange = new Dictionary<string, int>();
                    coinsToChange.Add("10", 0);
                    coinsToChange.Add("5", 0);
                    coinsToChange.Add("2", 0);
                    coinsToChange.Add("1", 0);

                    for (Int32 i = 0; i < coinsToChange.Count; i++)
                    {
                        var coin = coinsToChange.ElementAt(i);
                        Int32 intCoin = Int32.Parse(coin.Key);
                        Int32 coinInMachine =
                            _ctx.CoffeMachinePurse.Item.Coins.First(x => x.Value == coin.Key).Count;
                        while ((sum >= intCoin) && (coinInMachine > 0))
                        {
                            sum -= intCoin;
                            coinsToChange[coin.Key]++;
                            coinInMachine--;
                        }
                    }

                    foreach (var coin in coinsToChange)
                    {
                        _ctx.UserPurse.Item.Coins.First(x => x.Value == coin.Key).Count += coin.Value;
                        _ctx.CoffeMachinePurse.Item.Coins.First(x => x.Value == coin.Key).Count -= coin.Value;
                    }

                    _ctx.PurchaseInfo.Item.PayedSum = 0;
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
        /// Покупка товара пользователем
        /// </summary>
        /// <param name="item">Наименование товара</param>
        /// <returns>Сведения об ошибке или благодарность за покупку</returns>
        [Route("api/buy")]
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
        public async Task<Object> Initialize()
        {
            return await Task.Run(() =>
            {
                this._ctx.Init();
                return new { error = String.Empty };
            });

        }

        /// <summary>
        /// Рассчитывает общую сумму денег в машине
        /// </summary>
        /// <returns>Сумма номиналов монет</returns>
        protected Double GetTotalMoneyInMachine()
        {
            Double result = 0;
            foreach (var coin in this._ctx.CoffeMachinePurse.Item.Coins)
            {
                result += coin.Count * (Int32.Parse(coin.Value));
            }

            return result;
        }
    }
}
