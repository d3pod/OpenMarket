var indexAbout = 1;
var indexAboutSeller = 1;
if (window.innerWidth < 600) {
  showAbout(indexAbout);
  showAboutSeller(indexAboutSeller);
}
//Slideshow Purchase
function nextAbout(n) {
  showAbout((indexAbout += n));
}
async function showAbout(n) {
  var abouts = document.getElementsByClassName("about-purchase");

  if (n > abouts.length) {
    indexAbout = 1;
  }
  if (n < 1) {
    indexAbout = abouts.length;
  }
  for (let i = 0; i < abouts.length; i++) {
    abouts[i].style.display = "none";
  }
  abouts[indexAbout - 1].style.display = "flex";
}
//Slideshow Seller
function nextAboutSeller(n) {
  showAboutSeller((indexAboutSeller += n));
}
function showAboutSeller(n) {
  var aboutsSeller = document.getElementsByClassName("about-seller");

  if (n > aboutsSeller.length) {
    indexAboutSeller = 1;
  }
  if (n < 1) {
    indexAboutSeller = aboutsSeller.length;
  }
  for (let i = 0; i < aboutsSeller.length; i++) {
    aboutsSeller[i].style.display = "none";
  }
  aboutsSeller[indexAboutSeller - 1].style.display = "flex";
}
async function loadAdverts() {
  var location = document.getElementById("search-city").value;
  var type = document.getElementById("search-city").dataset.name;
  var data =
    "&type=" +
    encodeURIComponent(type) +
    "&location=" +
    encodeURIComponent(location);
  var adverts = document.getElementById("adverts-content");
  var url = "/index?handler=Anuncios" + data;
  while (document.getElementById("adverts-content").firstChild) {
    document.getElementById("adverts-content").firstChild.remove();
  }
  var request = new XMLHttpRequest();
  request.open("get", url, true);
  request.onload = function () {
    if (this.status >= 200 && this.status < 400) {
      var jsondata = JSON.parse(this.response);
      if (jsondata !== null) {
        for (let i = 0; i < jsondata.length; i++) {
          if (jsondata[i].groupName !== "Emprego") {
            var advert = document.createElement("a");
            var img = document.createElement("img");
            var title = document.createElement("h4");
            var price = document.createElement("p");
            var location = document.createElement("p");
            var date = document.createElement("p");
            var info = document.createElement("div");
            var type = document.createElement("span");
            var sponsored = document.createElement("span");

            var url =
              "anuncio?" +
              urlAdjusts(jsondata[i].title) +
              "&id=" +
              jsondata[i].id;
            advert.setAttribute("class", "advert-content");
            advert.setAttribute("href", url);
            var click = "viewRegister(" + jsondata[i].id + ", 'Index')";
            advert.setAttribute("onclick", click);
            img.setAttribute("title", jsondata[i].title);
            if (jsondata[i].image_filename != null) {
              img.src = "/anuncios/" + jsondata[i].image_filename;
            } else {
              img.src = "/imgs/WithoutImage.jpg";
            }

            //Type
            if (jsondata[i].type !== null) {
              if (jsondata[i].type == "Venda") {
                type.setAttribute("class", "advert-type advert-type-sell");
              } else if (
                jsondata[i].type == "Procura" ||
                jsondata[i].type == "Compra"
              ) {
                type.setAttribute("class", "advert-type advert-type-buy");
              } else if (jsondata[i].type == "Troca") {
                type.setAttribute("class", "advert-type advert-type-change");
              } else if (
                jsondata[i].type == "Contratar" ||
                jsondata[i].type == "Arrendar"
              ) {
                type.setAttribute("class", "advert-type advert-type-contract");
              } else if (jsondata[i].type == "Serviço") {
                type.setAttribute("class", "advert-type advert-type-service");
              } else if (jsondata[i].type == "Emprego") {
                type.setAttribute(
                  "class",
                  "advert-type advert-type-employment"
                );
              }
              type.innerHTML = jsondata[i].type.toUpperCase();

              info.appendChild(type);
            }
            //Title
            title.innerHTML = jsondata[i].title;
            title.setAttribute("class", "advert-title");

            var formatter = new Intl.NumberFormat("pt-PT", {
              style: "currency",
              currency: "AOA",
            });
            if (jsondata[i].orc == "true" || jsondata[i].price == 0) {
              price.innerHTML = "Sob orçamento";
            } else {
              price.innerHTML = formatter.format(jsondata[i].price);
            }

            price.setAttribute("class", "advert-price");
            location.innerHTML =
              jsondata[i].municipality + ", " + jsondata[i].city;
            location.setAttribute("class", "advert-location");
            var advert_date = new Date(jsondata[i].date);

            if (
              advert_date.getDate() +
                "/" +
                (advert_date.getMonth() + 1) +
                "/" +
                advert_date.getFullYear() ==
              new Date().getUTCDate() +
                "/" +
                (new Date().getUTCMonth() + 1) +
                "/" +
                new Date().getUTCFullYear()
            ) {
              date.innerHTML =
                "Hoje às " +
                advert_date.getHours() +
                ":" +
                String(advert_date.getMinutes()).padStart(2, "0");
            } else if (
              advert_date.getDate() +
                "/" +
                (advert_date.getMonth() + 1) +
                "/" +
                advert_date.getFullYear() ==
              new Date().getUTCDate() -
                1 +
                "/" +
                (new Date().getUTCMonth() + 1) +
                "/" +
                new Date().getUTCFullYear()
            ) {
              date.innerHTML =
                "Ontem às " +
                advert_date.getHours() +
                ":" +
                String(advert_date.getMinutes()).padStart(2, "0");
            } else {
              var month = jsondata[i].date_month;
              date.innerHTML =
                advert_date.getDate() +
                " " +
                month.charAt(0).toUpperCase() +
                month.slice(1);
            }
            date.setAttribute("class", "advert-date");
            //Sponsored
            if (jsondata[i].sponsored != null) {
              sponsored.setAttribute("class", "advert-sponsored");
              sponsored.innerHTML = jsondata[i].sponsored.toUpperCase();
              advert.appendChild(sponsored);
            }

            info.appendChild(title);
            info.appendChild(price);
            info.appendChild(location);
            info.appendChild(date);
            advert.appendChild(img);
            advert.appendChild(info);
            adverts.appendChild(advert);
          }
        }
      }
    } else {
      return;
    }
  };
  request.send();
}
async function howto_options(option) {
  var buy = document.getElementById("quero-comprar");
  var sell = document.getElementById("quero-vender");
  var button_buy = document.getElementById("button-buy");
  var button_sell = document.getElementById("button-sell");
  switch (option) {
    case "quero-comprar":
      buy.style.display = "flex";
      sell.style.display = "none";
      button_sell.style.backgroundColor = "white";
      button_sell.style.color = "black";
      button_sell.style.fontWeight = "normal";
      button_buy.style.backgroundColor = "#272727";
      button_buy.style.color = "white";
      button_buy.style.fontWeight = "bold";
      break;
    case "quero-vender":
      buy.style.display = "none";
      sell.style.display = "flex";
      button_sell.style.backgroundColor = "#272727";
      button_sell.style.color = "white";
      button_sell.style.fontWeight = "bold";
      button_buy.style.backgroundColor = "white";
      button_buy.style.color = "black";
      button_buy.style.fontWeight = "normal";
      break;
  }
}
