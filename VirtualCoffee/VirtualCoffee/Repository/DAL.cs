using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using System.ServiceModel;

namespace VirtualCoffee.Repository
{
    /// <summary>
    /// Базовый класс для кошельков
    /// </summary>
    public abstract class PurseBase
    {
        /// <summary>
        /// Список монет
        /// </summary>
        [XmlArray(ElementName = "Coins")]
        [XmlArrayItem("Coin")]
        public List<Coin> Coins { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public PurseBase()
        {
            Coins = new List<Coin>();
        }
    }

    /// <summary>
    /// Интерфейс для внедрения зависимости
    /// </summary>
    /// <typeparam name="T">Тип сущности</typeparam>
    public interface ICoffeeDataSet<T> where T : class
    {
        T Item { get; set; }
        void Save(String fileName);
        void Load(String fileName);
        void Initialize(String fileName);
    }

    /// <summary>
    /// Базовый класс для всего, что загружается из XML (по сути, аналог DataSet)
    /// </summary>
    /// <typeparam name="T">Тип-параметр сущности</typeparam>
    public abstract class CoffeeDataSet<T> : ICoffeeDataSet<T> where T : class
    {
        /// <summary>
        /// Сама сущность
        /// </summary>
        public T Item { get; set; }

        /// <summary>
        /// Сохранить
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        public virtual void Save(String fileName)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            XmlWriter xr = XmlWriter.Create(fileName, settings);
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(xr, this.Item, namespaces);
            }
            finally
            {
                xr.Close();
            }
        }

        /// <summary>
        /// Загрузка из XML
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        public virtual void Load(String fileName)
        {
            T result = Activator.CreateInstance<T>();
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            XmlReader xr = XmlReader.Create(fileName);
            try
            {
                result = (T)(serializer.Deserialize(xr));
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                xr.Close();
            }
            Item = result;
        }

        /// <summary>
        /// Инициализация исходными значениями
        /// </summary>
        /// <param name="fileName">Имя файла</param>
        public abstract void Initialize(String fileName);

        /// <summary>
        /// Конструктор
        /// </summary>
        public CoffeeDataSet()
        {
            Item = Activator.CreateInstance<T>();
        }
    }

    /// <summary>
    /// Кошелёк пользователя
    /// </summary>
    public class UserPurse : PurseBase
    {
        public UserPurse() : base() { }
    }

    /// <summary>
    /// Набор данных для кошелька пользователя
    /// </summary>
    public class UserPurseDataSet : CoffeeDataSet<UserPurse>, ICoffeeDataSet<UserPurse>
    {
        public override void Initialize(string fileName)
        {
            this.Item.Coins.Clear();
            this.Item.Coins.Add(new Coin() { Count = 10, Value = "1" });
            this.Item.Coins.Add(new Coin() { Count = 30, Value = "2" });
            this.Item.Coins.Add(new Coin() { Count = 20, Value = "5" });
            this.Item.Coins.Add(new Coin() { Count = 15, Value = "10" });

            this.Save(fileName);
        }
    }

    /// <summary>
    /// Товар
    /// </summary>
    [XmlType("GoodsItem")]
    public class GoodsItem
    {
        [XmlAttribute(AttributeName = "kind")]
        public String Name { get; set; }
        [XmlAttribute(AttributeName = "price")]
        public Double Price { get; set; }
        [XmlAttribute(AttributeName = "number")]
        public Int32 Count { get; set; }
    }

    /// <summary>
    /// Файл с данными о кошельке машины
    /// </summary>
    [XmlSerializerFormat]
    [XmlRoot(ElementName = "CoffeePurse")]
    public class CoffeMachinePurse : PurseBase
    {

    }

    /// <summary>
    /// Набор данных для кошелька кофе-машины
    /// </summary>
    public class CoffeMachinePurseDataSet : CoffeeDataSet<CoffeMachinePurse>, ICoffeeDataSet<CoffeMachinePurse>
    {
        /// <summary>
        /// Инициализация стартовыми значениями
        /// </summary>
        /// <param name="fileName">Имя файла с данными о машине</param>
        public override void Initialize(String fileName)
        {
            Item.Coins.Clear();

            Item.Coins.Add(new Coin() { Value = "1", Count = 100 });
            Item.Coins.Add(new Coin() { Value = "2", Count = 100 });
            Item.Coins.Add(new Coin() { Value = "5", Count = 100 });
            Item.Coins.Add(new Coin() { Value = "10", Count = 100 });

            Save(fileName);
        }
    }

    /// <summary>
    /// Товары в кофе-машине
    /// </summary>
    [XmlRoot("CoffeGoods")]
    public class CoffeMachineStock
    {
        /// <summary>
        /// Список товаров в машине
        /// </summary>
        [XmlArray(ElementName = "Goods")]
        [XmlArrayItem("GoodsItem")]
        public List<GoodsItem> Goods { get; set; }
        /// <summary>
        /// Конструктор
        /// </summary>
        public CoffeMachineStock()
            : base()
        {
            Goods = new List<GoodsItem>();
        }
    }

    /// <summary>
    /// Набор данных для товаров
    /// </summary>
    public class GoodsDataSet : CoffeeDataSet<CoffeMachineStock>, ICoffeeDataSet<CoffeMachineStock>
    {
        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="fileName"></param>
        public override void Initialize(string fileName)
        {
            Item.Goods.Clear();

            Item.Goods.Add(new GoodsItem() { Name = "tea", Count = 10, Price = 13 });
            Item.Goods.Add(new GoodsItem() { Name = "coffee", Count = 20, Price = 18 });
            Item.Goods.Add(new GoodsItem() { Name = "cappuccino", Count = 10, Price = 21 });
            Item.Goods.Add(new GoodsItem() { Name = "juice", Count = 15, Price = 35 });

            Save(fileName);
        }
    }

    /// <summary>
    /// Сведения о покупке
    /// </summary>
    public class BuyData
    {
        public Double PayedSum { get; set; }
    }

    /// <summary>
    /// Набор данных для покупки
    /// </summary>
    public class BuyDataSet : CoffeeDataSet<BuyData>
    {
        public override void Initialize(string fileName)
        {
            Item.PayedSum = 0;

            Save(fileName);
        }
    }

    /// <summary>
    /// Типа контекст данных
    /// </summary>
    public class CoffeDataContext
    {
        public GoodsDataSet Goods { get; set; }
        public CoffeMachinePurseDataSet CoffeMachinePurse { get; set; }
        public UserPurseDataSet UserPurse { get; set; }
        public BuyDataSet PurchaseInfo { get; set; }

        public CoffeDataContext()
        {
            Goods = new GoodsDataSet();
            CoffeMachinePurse = new CoffeMachinePurseDataSet();
            UserPurse = new UserPurseDataSet();
            PurchaseInfo = new BuyDataSet();
        }

        public virtual void Load(String baseUrl)
        {
            Goods.Load(baseUrl + "goods.xml");
            CoffeMachinePurse.Load(baseUrl + "coffee.xml");
            UserPurse.Load(baseUrl + "user.xml");
            PurchaseInfo.Load(baseUrl + "purchase.xml");
        }

        public virtual void Save(String baseUrl)
        {
            Goods.Save(baseUrl + "goods.xml");
            CoffeMachinePurse.Save(baseUrl + "coffee.xml");
            UserPurse.Save(baseUrl + "user.xml");
            PurchaseInfo.Save(baseUrl + "purchase.xml");
        }

        public virtual void Init(String baseUrl)
        {
            Goods.Initialize(baseUrl + "goods.xml");
            CoffeMachinePurse.Initialize(baseUrl + "coffee.xml");
            UserPurse.Initialize(baseUrl + "user.xml");
            PurchaseInfo.Initialize(baseUrl + "purchase.xml");
        }

    }
}