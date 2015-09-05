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


        public async Task<Object> PayCoin([FromUri]Int32 value)
        {
            return await Task.Run(() =>
            {
                return String.Empty;
            });
        }

        public async Task<Object> GetChange()
        {
            return await Task.Run(() => { return String.Empty; });
        }

        public async Task<Object> BuyItem()
        {
            return await Task.Run(() => { return String.Empty; });

        }

        public async Task<Object> Initialize()
        {
            return await Task.Run(() =>
            {
                return String.Empty;
            });

        }
    }
}
