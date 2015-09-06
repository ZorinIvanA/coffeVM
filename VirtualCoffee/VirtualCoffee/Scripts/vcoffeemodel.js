//Модель представления кофе-машины
function vCoffeeModel() {
    var self = this;

    self.baseUrl = window.location.href;
    self.error = ko.observable();

    //Кошелёк пользователя
    self.userPurse = ko.observableArray();
    self.getUserPurse = function () {
        return $.ajax({ url: this.baseUrl + "/api/userpurse" });
    };
    self.fillUserPurse = function (coins) {
        $.each(coins, function (index, coinItem) {
            self.userPurse.push({
                kind: coinItem.value,
                count: coinItem.count
            });
        });
    }

    //Кошелёк автомата
    self.machinePurse = ko.observableArray();
    self.getMachinePurse = function () {
        return $.ajax({ url: this.baseUrl + "/api/machinepurse" });
    };
    self.fillMachinePurse = function (coins) {

        $.each(coins, function (index, coinItem) {
            self.machinePurse.push({
                kind: coinItem.value,
                count: coinItem.count
            });
        });
    }

    //Товары
    self.goods = ko.observableArray();
    self.getGoods = function () {
        return $.ajax({ url: this.baseUrl + "/api/goods" });
    }
    self.fillGoods = function (dataGoods) {
        $.each(dataGoods.goods, function (index, goodsItem) {
            self.goods.push({
                kind: goodsItem.item,
                price: goodsItem.price,
                count: goodsItem.count
            });
        });
    }

    //Дополнительные сведения
    self.sum = ko.observable();
    self.getPurchaseInfo = function () {
        return $.ajax({ url: this.baseUrl + "/api/purchaseinfo" });
    };

    //Загрузка всех данных из файла
    self.load = function () {
        this.goods.removeAll();
        this.userPurse.removeAll();
        this.machinePurse.removeAll();
        var obj = this;
        $.when(
            this.getGoods(),
            this.getUserPurse(),
            this.getMachinePurse(),
            this.getPurchaseInfo()
            ).then(function (dataGoods, dataUser, dataMachine, dataPurchase) {
                obj.fillGoods(dataGoods[0]);
                obj.fillUserPurse(dataUser[0].Coins);
                obj.fillMachinePurse(dataMachine[0].Coins);
                self.sum(dataPurchase[0].Sum);
            });
    };

    //Инициализация стартовыми значениями
    self.init = function () {
        $.when(
            $.ajax({ url: self.baseUrl + "api/init" })).then(function () {
                self.load();
            });
    }

    //Заплатить монетку
    self.pay = function (coin) {
        $.when(
            $.ajax({ url: self.baseUrl + "api/pay?value=" + coin.kind })).then(function (data) {
                self.showError(data);
                self.load();
            });
    }

    //Купить товар
    self.buy = function (goodsItem) {
        $.when(
            $.ajax({ url: self.baseUrl + "api/buy?item=" + goodsItem.kind })).then(function (data) {
                self.showError(data);
                self.load();
                if (!data.error) {
                    alert("Спасибо за покупку!")
                }
            });

    }

    //Попросить сдачу
    self.askChange = function () {
        $.when(
            $.ajax({ url: self.baseUrl + "api/getchange" })).then(function (data) {
                self.load();
                self.showError(data);
            });
    }

    //Показать или скрыть ошибку
    self.showError = function (data) {
        if (data.error) {
            self.error("Ошибка! " + data.error);
        }
        else { self.error(""); }
    }
}

