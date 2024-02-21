async function advert_type(group) {
  var needs = document.getElementById("advert-needs");
  var option1 = document.createElement("option");
  var option2 = document.createElement("option");
  var option3 = document.createElement("option");

  while (document.getElementById("advert-needs").length > 0) {
    document.getElementById("advert-needs").remove(0);
  }

  document.getElementById("checkbox_orc").checked = false;
  document.getElementById("checkbox_orc").removeAttribute("disabled");

  switch (group) {
    case "Serviços":
      document.querySelector(".checkbox_orc").style.display = "flex";
      document.querySelector(".status-content").style.display = "none";
      option1.value = "Serviço";
      option1.text = "Prestar Serviço";
      option2.value = "Contratar";
      option2.text = "Contratar";
      needs.appendChild(option1);
      needs.appendChild(option2);
      needs.setAttribute("onchange", "hidePrice()");
      break;

    case "Imóveis":
      document.querySelector(".checkbox_orc").style.display = "none";
      document.querySelector(".status-content").style.display = "block";
      option1.value = "Venda";
      option1.text = "Vender";
      option2.value = "Arrendar";
      option2.text = "Arrendar";
      option3.value = "Procura";
      option3.text = "Procurar";
      needs.appendChild(option1);
      needs.appendChild(option2);
      needs.appendChild(option3);
      break;

    default:
      document.querySelector(".checkbox_orc").style.display = "none";
      document.querySelector(".status-content").style.display = "block";
      option1.value = "Venda";
      option1.text = "Vender";
      option2.value = "Compra";
      option2.text = "Comprar";
      needs.appendChild(option1);
      needs.appendChild(option2);
      break;
  }
}
function hidePrice() {
  if (document.getElementById("advert-needs").value == "Contratar") {
    document.getElementById("checkbox_orc").checked = true;
    document.getElementById("checkbox_orc").disabled = true;
    under_budget();
  } else if (document.getElementById("group-selector").value == "Serviços") {
    var price = document.getElementById("advert-price").value;
    if (price > 0) {
      document.getElementById("advert-price").style.display = "flex";
      document.getElementById("checkbox_orc").checked = false;
      document.getElementById("checkbox_orc").disabled = false;
    } else {
      document.getElementById("advert-price").style.display = "none";
      document.getElementById("checkbox_orc").checked = true;
      document.getElementById("checkbox_orc").disabled = false;
    }
  }
}
function employment() {
  document.querySelector(".status-content").style.display = "none";
  document.querySelector(".period-content").style.display = "flex";
  document.querySelector(".contract-content").style.display = "flex";
  document.getElementById("price-title").innerHTML = "Salário";
  document.getElementById("advert-price").style.display = "none";
  document.querySelector(".price-range-content").style.display = "flex";
  document.querySelector(".advert-images-content").style.display = "none";
  document.querySelector(".advert-needs-content").style.display = "none";
}
async function loadCategories(page, id) {
  var group = document.getElementById("group-selector").value;
  var data = "&group=" + encodeURIComponent(group);
  var categories = document.getElementsByClassName("category-selector");
  if (page == "edit-advert") {
    var url = "/admin/" + page + "/" + id + "?handler=Categories" + data;
  } else {
    var url = "/" + page + "?handler=Categories" + data;
  }
  while (document.getElementById("category-selector").length > 0) {
    document.getElementById("category-selector").remove(0);
  }
  if ((document.getElementById("category-selector").style.display = "none")) {
    document.getElementById("category-selector").style.display = "flex";
  }
  advert_type(group);
  if (group === "Emprego") {
    employment();
  } else {
    document.querySelector(".period-content").style.display = "none";
    document.querySelector(".contract-content").style.display = "none";
    document.getElementById("price-title").innerHTML = "Preço";
    document.getElementById("advert-price").style.display = "block";
    document.querySelector(".price-range-content").style.display = "none";
    document.querySelector(".advert-images-content").style.display = "block";
    document.querySelector(".advert-needs-content").style.display = "block";
  }
  var request = new XMLHttpRequest();
  request.open("get", url, true);
  request.onload = function () {
    if (this.status >= 200 && this.status < 400) {
      var jsondata = JSON.parse(this.response);
      if (jsondata !== null) {
        for (let i = 0; i < jsondata.length; i++) {
          var option = jsondata[i];
          for (let index = 0; index < categories.length; index++) {
            var optionElement = document.createElement("option");
            optionElement.value = option;
            optionElement.text = option;
            optionElement.setAttribute("class", "category-option");
            categories[index].add(optionElement);
          }
        }
      }
    } else {
      return;
    }
  };
  request.send();
}
async function under_budget() {
  var checkbox = document.getElementById("checkbox_orc");
  if (checkbox.checked) {
    document.querySelector(".checkbox_orc").style.marginTop = "20px";
    document.getElementById("advert-price").style.display = "none";
    document.querySelector(".negotiable-content").style.display = "none";
    document.getElementById("price-advert-alert").innerHTML = "";
    document.getElementById("advert-price").value = 0;
    document.getElementById("advert-price").setAttribute("placeholder", "0 €");
    document.getElementById("advert-price_min").value = "";
    document
      .getElementById("advert-price_min")
      .setAttribute("placeholder", "0 €");
    document.getElementById("advert-price_max").value = "";
    document
      .getElementById("advert-price_max")
      .setAttribute("placeholder", "0 €");
  } else {
    document.getElementById("advert-price").style.display = "flex";
    document.querySelector(".negotiable-content").style.display = "block";
  }
}
async function loadMunicipalities(page, id) {
  var city = document.getElementById("cities-selector").value;
  var data = "&city=" + encodeURIComponent(city);
  var municipalities = document.getElementsByClassName("municipality-selector");
  if (page == "edit-advert") {
    var url = "/admin/" + page + "/" + id + "?handler=Municipalities" + data;
  } else {
    var url = "/" + page + "?handler=Municipalities" + data;
  }

  while (document.getElementById("municipality-selector").length > 0) {
    document.getElementById("municipality-selector").remove(0);
  }
  if (
    (document.getElementById("municipality-content").style.display = "none")
  ) {
    document.getElementById("municipality-content").style.display = "flex";
    document.getElementById("locality-content").style.display = "flex";
  }

  var request = new XMLHttpRequest();
  request.open("get", url, true);
  request.onload = function () {
    if (this.status >= 200 && this.status < 400) {
      var jsondata = JSON.parse(this.response);
      if (jsondata !== null) {
        for (let i = 0; i < jsondata.length; i++) {
          var option = jsondata[i];
          for (let index = 0; index < municipalities.length; index++) {
            var optionElement = document.createElement("option");
            optionElement.value = option;
            optionElement.text = option;
            optionElement.setAttribute("class", "municipality-option");
            municipalities[index].add(optionElement);
          }
        }
      }
    } else {
      return;
    }
  };
  request.send();
}
function PreviewImage(image) {
  var img = new FileReader();
  img.readAsDataURL(document.getElementById(image).files[0]);
  img.onload = function (img) {
    document.getElementById("button_" + image).src = img.target.result;
  };
}
function caracter_counter(text, counter, total) {
  var caracters = document.getElementById(text);
  document.getElementById(counter).innerHTML =
    caracters.value.trim().length + "/" + total;
  if (caracters.value.trim().length == total) {
    document.getElementById(counter).style.color = "orange";
  }
}
function price_format(element1, element2) {
  var formatter = new Intl.NumberFormat("pt-PT", {
    style: "currency",
    currency: "AOA",
  });
  var price = document.getElementById(element1);
  var price_max = document.getElementById(element2);
  var formatted_price1;
  var formatted_price2;
  if (price.value > 0 && price_max.value > 0) {
    document.getElementById("price-advert-alert").style.color = "#186db3";
    formatted_price1 = formatter.format(price.value);
    formatted_price2 = formatter.format(price_max.value);
    document.getElementById("price-advert-alert").innerHTML =
      formatted_price1 + " - " + formatted_price2;
  } else if (price.value > 0) {
    document.getElementById("price-advert-alert").style.color = "#186db3";
    formatted_price1 = formatter.format(price.value);
    document.getElementById("price-advert-alert").innerHTML = formatted_price1;
  } else {
    document.getElementById("price-advert-alert").style.color =
      "rgb(211, 26, 26)";
  }
  document.getElementById("price-advert-alert").style.display = "flex";
  document.getElementById("price-advert-alert").style.width = "100%";
  document.getElementById("price-advert-alert").style.paddingLeft = "10px";
}
function empty_check(element, span, text) {
  var counter = document.getElementById(element);
  if (counter.value.length < 1) {
    document.getElementById(span).innerHTML = text;
    document.getElementById(span).style.display = "flex";
    counter.style.backgroundColor = "rgb(214, 232, 245)";
  }
  return;
}
async function form_validate() {
  var title = document.getElementById("advert-title");
  var group = document.getElementById("group-selector");
  var description = document.getElementById("advert-text");
  var price = document.getElementById("advert-price");
  var period = document.getElementById("period-select");
  var contract = document.getElementById("contract-select");
  var city = document.getElementById("cities-selector");
  var locality = document.getElementById("advert-locality");
  var contact = document.getElementById("advert-contact");
  var checkbox_orc = document.getElementById("checkbox_orc");

  if (title.value.trim().length == 0) {
    document.getElementById("title-advert-alert").innerHTML =
      "Introduz um titulo para o teu anúncio";
    window.scroll(0, 20);
    return;
  } else if (title.value.trim().length < 6) {
    document.getElementById("title-advert-alert").innerHTML =
      "Introduz um titulo com pelo menos com 5 caracteres";
    window.scroll(0, 20);
    return;
  } else {
    document.getElementById("title-advert-alert").innerHTML = "";
  }

  if (group.value == "Escolhe a categoria") {
    document.getElementById("category-advert-alert").innerHTML =
      "Escolhe uma categoria";
    window.scroll(0, 20);
    return;
  } else {
    document.getElementById("category-advert-alert").innerHTML = "";
  }
  if (description.value.trim().length < 81) {
    document.getElementById("description-advert-alert").innerHTML =
      "Introduz uma descrição para o seu anúncio<br/>Não te esqueças de todos os pormenores";
    if (window.innerWidth < 900) {
      window.scroll(0, 600);
    } else {
      window.scroll(0, 900);
    }
    return;
  } else {
    document.getElementById("description-advert-alert").innerHTML = "";
  }

  if (group.value !== "Emprego" && !checkbox_orc.checked) {
    if (price.value.length == 0 || parseInt(price.value) < 1) {
      document.getElementById("price-advert-alert").innerHTML =
        "Introduz um preço válido";
      if (window.innerWidth < 900) {
        window.scroll(0, 650);
      } else {
        window.scroll(0, 1000);
      }
      return;
    } else if (price.value.match(/^[0-9]+$/)) {
      document.getElementById("price-advert-alert").innerHTML = "";
    } else {
      document.getElementById("price-advert-alert").innerHTML =
        "Introduz um preço válido com apenas números";
      window.scroll(0, 1100);
      return;
    }
  } else if (group.value == "Emprego") {
    var price_min = parseInt(document.getElementById("advert-price_min").value);
    var price_max = parseInt(document.getElementById("advert-price_max").value);
    if (price_max < price_min) {
      document.getElementById("price-advert-alert").innerHTML =
        "O valor máximo não pode ser inferior ao valor mínímo";
      document.getElementById("price-advert-alert").style.color =
        "rgb(211, 26, 26)";
      window.scroll(0, 850);
      return;
    }
    if (period.value == "") {
      document.getElementById("period-advert-alert").innerHTML =
        "Selecione um tipo de emprego";
      window.scroll(0, 1150);
      return;
    } else {
      document.getElementById("period-advert-alert").innerHTML = "";
    }
    if (contract.value == "") {
      document.getElementById("contract-advert-alert").innerHTML =
        "Selecione um tipo de contracto";
      window.scroll(0, 1150);
      return;
    } else {
      document.getElementById("contract-advert-alert").innerHTML = "";
    }
  }

  if (city.value == "Província") {
    document.getElementById("location-advert-alert").innerHTML =
      "Define uma localização para o anúncio";
    window.scroll(0, 1500);
    return;
  } else {
    document.getElementById("location-advert-alert").innerHTML = "";
  }

  if (locality.value.trim().length == 0) {
    document.getElementById("location-advert-alert").innerHTML =
      "Introduz uma localidade (ex. Benfica ou Maianga)";
    window.scroll(0, 1500);
    return;
  } else {
    document.getElementById("location-advert-alert").innerHTML = "";
  }

  if (contact.value.trim().length == 0) {
    document.getElementById("contact-advert-alert").innerHTML =
      "Introduz um contacto telefónico";
    window.scroll(0, 1500);
    return;
  } else if (contact.value.replaceAll(" ", "").trim().length > 14) {
    document.getElementById("contact-advert-alert").innerHTML =
      "Introduz apenas um número de telefone. Podes colocar outros contactos na descrição do anúncio.";
    window.scroll(0, 1500);
    return;
  } else if (contact.value.match(/^(?=.*[0-9])[- +()0-9]+$/)) {
    document.getElementById("contact-advert-alert").innerHTML = "";
  } else {
    document.getElementById("contact-advert-alert").innerHTML =
      "Introduz um contacto de telefone válido";
    window.scroll(0, 1500);
    return;
  }
  buttonLoading(
    "button-text",
    "div-loading",
    "loading-button",
    "form-container"
  );
}
var login = document.getElementById("login-form");
var register = document.getElementById("register-form");
var login_button = document.getElementById("button-login");
var register_button = document.getElementById("button-register");
function login_form() {
  login.style.display = "flex";
  register.style.display = "none";
  login_button.style.borderBottom = "2px solid black";
  register_button.style.borderBottom = "1px solid black";
  login_button.style.fontWeight = "bold";
  register_button.style.fontWeight = "normal";
  window.scroll(0, 20);
}
function register_form() {
  login.style.display = "none";
  register.style.display = "flex";
  login_button.style.borderBottom = "1px solid black";
  register_button.style.borderBottom = "2px solid black";
  login_button.style.fontWeight = "normal";
  register_button.style.fontWeight = "bold";
  window.scroll(0, 20);
}
var cor = "rgb(210, 230, 241)";
async function submitContact(login) {
  if (login !== "logged") {
    if (document.getElementById("name").value.trim() === "") {
      document.getElementById("name").style.backgroundColor = cor;
      document.querySelector(".alert-panel").style.display = "flex";
      document.querySelector(".alert-message").innerHTML =
        "Introduza o seu nome";
      window.scroll(0, 20);
      return;
    } else if (document.getElementById("email").value.trim() === "") {
      document.getElementById("email").style.backgroundColor = cor;
      document.querySelector(".alert-panel").style.display = "flex";
      document.querySelector(".alert-message").innerHTML =
        "Introduza o seu email";
      window.scroll(0, 20);
      return;
    } else if (
      emailIsValid(document.getElementById("email").value.trim()) == false
    ) {
      document.getElementById("email").style.backgroundColor = cor;
      document.querySelector(".alert-panel").style.display = "flex";
      document.querySelector(".alert-message").innerHTML =
        "O email introduzido não é válido";
      window.scroll(0, 20);
      return;
    }
  }
  if (document.getElementById("subject").value == 0) {
    document.getElementById("subject").style.backgroundColor = cor;
    document.querySelector(".alert-panel").style.display = "flex";
    document.querySelector(".alert-message").innerHTML = "Escolha um assunto";
    window.scroll(0, 20);
    return;
  } else if (document.getElementById("Body").value.trim() === "") {
    document.getElementById("Body").style.backgroundColor = cor;
    document.querySelector(".alert-panel").style.display = "flex";
    document.querySelector(".alert-message").innerHTML =
      "Descreva o assunto que deseja tratar";
    window.scroll(0, 20);
    return;
  } else {
    var div = document.getElementById("contact_panel");
    div.style.display = "flex";
    window.scroll(0, 20);
    setTimeout(function () {
      document.getElementById("submitContactForm").submit();
    }, 5000);
  }
}
function emailIsValid(email) {
  return /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(
    email
  );
}
async function emailValidation(email, form, span) {
  if (document.getElementById(email).value.trim().length > 0) {
    if (emailIsValid(document.getElementById(email).value.trim()) == true) {
      document.getElementById(form).submit();
    } else {
      document.getElementById(span).innerHTML = "Este email não é válido!";
      return;
    }
  } else {
    document.getElementById(span).innerHTML = "Introduz um email";
    return;
  }
}
function Filter_Adverts() {
  var filter = document.getElementById("filter-selector").value;
  window.location.href = "/minha-conta?status=" + filter;
}
function PanelDisable() {
  document.querySelector(".panel-disable").style.display = "flex";
}
function adaptText(text, total) {
  var title = document.getElementsByClassName(text);
  for (let i = 0; i < title.length; i++) {
    var str = title[i].innerHTML;
    if (str.length >= total) {
      title[i].innerHTML = str.substring(total, str - total) + "...";
    }
  }
}
async function validPassword() {
  var password = document.getElementById("user-password");
  var new_password = document.getElementById("user-new-password");
  var new_r_password = document.getElementById("user-new-r-password");
  if (password && password.value.trim().length == 0) {
    document.getElementById("alert-message-container").style.display = "flex";
    password.style.backgroundColor = "rgb(31, 140, 212, 0.2)";
    document.getElementById("alert-message-title").innerHTML =
      "Introduza a password actual!";
  }
  if (new_password.value.trim().length < 6) {
    document.getElementById("alert-message-container").style.display = "flex";
    new_password.style.backgroundColor = "rgb(31, 140, 212, 0.2)";
    new_r_password.style.backgroundColor = "rgb(31, 140, 212, 0.2)";
    document.getElementById("alert-message-title").innerHTML =
      "A nova password deve conter pelo menos 6 caracteres!";
  } else if (new_password.value.trim() !== new_r_password.value.trim()) {
    document.getElementById("alert-message-container").style.display = "flex";
    new_password.style.backgroundColor = "rgb(31, 140, 212, 0.2)";
    new_r_password.style.backgroundColor = "rgb(31, 140, 212, 0.2)";
    document.getElementById("alert-message-title").innerHTML =
      "As passwords não são iguais!";
  } else {
    document.getElementById("changePassword").submit();
  }
}
async function validateRegister() {
  username = document.getElementById("account-username");
  email = document.getElementById("account-email");
  password = document.getElementById("account-password");
  if (username.value.trim().length < 5) {
    document.getElementById("alert-message-container").style.display = "flex";
    document.getElementById("message").innerHTML =
      "Introduz um nome de utilizador com pelo menos 5 caracteres";
  } else if (emailIsValid(email.value.trim()) == false) {
    document.getElementById("alert-message-container").style.display = "flex";
    document.getElementById("message").innerHTML = "Introduz um email válido";
  } else if (password.value.trim().length < 6) {
    document.getElementById("alert-message-container").style.display = "flex";
    document.getElementById("message").innerHTML =
      "Introduz uma password com pelo menos 6 caracteres";
  } else {
    username.value = username.value.replaceAll(" ", "");
    buttonLoading(
      "button-text",
      "div-loading",
      "loading-button",
      "register-form"
    );
  }
}
async function loadAdvertsFilters(currentpage, page) {
  var localizacao = document.getElementById("search-city").value;
  var tipo = document.getElementById("search-city").dataset.name;
  var status = document.getElementById("filter-status").value;
  var min_price = parseInt(document.getElementById("min-price").value);
  var max_price = parseInt(document.getElementById("max-price").value);
  var orderby = document.getElementById("orderby").value;
  var text = document.getElementById("searchbar-text").value.trim();
  var negotiable = document.getElementById("filter-negotiable").value;
  var user = document.getElementById("filter-user").value;
  let divLoadingPage = document.querySelector(".div-loading");
  var totalpages = "";
  divLoadingPage.style.display = "flex";
  var url = window.location.pathname + "?handler=" + page;
  var path = "";
  let urlParams = window.location.search;
  let params = new URLSearchParams(urlParams);

  if (params.has("categoria")) {
    path = path + "&categoria=" + params.get("categoria");
  } else if (params.has("sub_categoria")) {
    path = path + "&sub_categoria=" + params.get("sub_categoria");
  }

  if (localizacao != "") {
    path =
      path +
      "&" +
      encodeURIComponent(tipo) +
      "=" +
      encodeURIComponent(localizacao);
  } else {
    if (params.has("municipio")) {
      path = path + "&municipio=" + params.get("municipio");
    } else if (params.has("provincia")) {
      path = path + "&provincia=" + params.get("provincia");
    }
  }

  if (status !== "") {
    path = path + "&estado=" + encodeURIComponent(status);
  }
  if (min_price !== "" && min_price > 0) {
    path = path + "&min_preco=" + encodeURIComponent(min_price);
  }
  if (max_price !== "" && max_price > 0) {
    path = path + "&max_preco=" + encodeURIComponent(max_price);
  }
  if (orderby !== "") {
    path = path + "&ordenar=" + encodeURIComponent(orderby);
  }
  if (text !== "") {
    path = path + "&pesquisa=" + encodeURIComponent(text);
  }
  if (negotiable !== "") {
    path = path + "&negotiable=" + encodeURIComponent(negotiable);
  }
  if (user !== "") {
    path = path + "&user=" + encodeURIComponent(user);
  }

  /* if (type !== "") {
    path = path + "&type=" + encodeURIComponent(text);
  } */
  if (currentpage !== "" && currentpage !== undefined) {
    path = path + "&pagina=" + encodeURIComponent(currentpage);
  } else if (currentpage == "") {
    currentpage = 1;
  }
  window.history.replaceState(
    null,
    null,
    window.location.pathname + "?" + path
  );
  url = url + path;
  var adverts = document.getElementById("anuncios");
  while (document.getElementById("anuncios").firstChild) {
    document.getElementById("anuncios").firstChild.remove();
  }
  var request = new XMLHttpRequest();
  request.open("get", url, true);
  request.onload = function () {
    if (this.status >= 200 && this.status < 400) {
      var jsondata = JSON.parse(this.response);
      if (jsondata !== null) {
        var start = (currentpage - 1) * 20;
        var max = start + 19;
        if (max > jsondata.length) {
          max = jsondata.length - 1;
        }

        for (let i = start; i <= max; i++) {
          var advert = document.createElement("a");
          var img = document.createElement("img");
          var title = document.createElement("h4");
          var price = document.createElement("p");
          var employment_price_title = document.createElement("p");
          var location = document.createElement("p");
          var date = document.createElement("p");
          var info = document.createElement("div");
          var type = document.createElement("span");
          var url =
            "anuncio?" +
            urlAdjusts(jsondata[i].title) +
            "&id=" +
            jsondata[i].id;
          advert.setAttribute("class", "advert-content");
          advert.setAttribute("href", url);
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
              type.setAttribute("class", "advert-type advert-type-employment");
            }
            type.innerHTML = jsondata[i].type.toUpperCase();
          }
          if (jsondata[i].groupName == "Emprego") {
            info.appendChild(type);
            //Title
            title.innerHTML = jsondata[i].title;
            title.setAttribute("class", "employment-title");

            var formatter = new Intl.NumberFormat("pt-PT", {
              style: "currency",
              currency: "AOA",
            });
            if (jsondata[i].price_min > 0 && jsondata[i].price_max > 0) {
              price.innerHTML =
                jsondata[i].price_min + " - " + jsondata[i].price_max;
            } else {
              price.innerHTML = "Não definida";
            }
            price.setAttribute("class", "employment-price");

            location.innerHTML =
              jsondata[i].municipality + ", " + jsondata[i].city;
            location.setAttribute("class", "employment-location");
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
            date.setAttribute("class", "employment-date");
            employment_price_title.setAttribute(
              "class",
              "employment-price-title"
            );
            employment_price_title.innerHTML = "Faixa salarial";
            info.appendChild(employment_price_title);
            info.appendChild(price);
            info.appendChild(location);
            info.appendChild(date);
            advert.appendChild(title);
            advert.appendChild(info);
            advert.setAttribute("class", "employment-content");
            adverts.appendChild(advert);
          } else {
            advert.appendChild(type);
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
                advert_date.getMinutes();
            } else {
              var month = jsondata[i].date_month;
              date.innerHTML =
                advert_date.getDate() +
                " " +
                month.charAt(0).toUpperCase() +
                month.slice(1);
            }

            totalpages = jsondata.length;
            date.setAttribute("class", "advert-date");
            info.appendChild(title);
            info.appendChild(price);
            info.appendChild(location);
            info.appendChild(date);
            advert.appendChild(img);
            advert.appendChild(info);
            adverts.appendChild(advert);
          }
        }
        pagination(currentpage, totalpages / 20, page);
        divLoadingPage.style.display = "none";
      }
    } else {
      return;
    }
  };
  request.send();
}

function emailForm(form, text) {
  if (document.getElementById(text).value.trim() !== "") {
    document.getElementById(form).submit();
  } else {
    var div = document.getElementById("alert-content");
    while (document.querySelector(".alert-content").children.length > 0) {
      document
        .getElementById("alert-content")
        .removeChild(document.getElementById("alert-content").lastElementChild);
    }
    var h3 = document.createElement("h3");
    var button = document.createElement("button");
    h3.innerHTML = "Não é possivel submeter a mensagem em branco";
    button.setAttribute("type", "button");
    button.innerHTML = "Fechar";
    button.setAttribute("onclick", "closePanel('alert-message-container')");
    div.appendChild(h3);
    div.appendChild(button);
    document.getElementById("alert-message-container").style.display = "flex";
  }
}
function fullscreen(img) {
  var div_fullscreen = document.getElementById("image-fullscreen");
  var image_full = document.getElementById("image-full");
  image_full.src = img.src;
  div_fullscreen.style.display = "flex";
}
function buttonLoading(
  elementText,
  elementLoading,
  elementButton,
  elementSubmit
) {
  const button = document.querySelector("." + elementButton);
  const text = document.querySelector("." + elementText);
  const spin = document.querySelector("." + elementLoading);
  text.style.display = "none";
  spin.style.display = "flex";
  button.style.backgroundColor = "transparent";
  window.scroll(0, 0);
  //Timeout apenas para o loading funcionar no Safari. Caso não use o timeout, não aparece o loading
  setTimeout(function () {}, 100);
  if (button.value.length > 0) {
    button.setAttribute("type", "submit");
    button.click();
  } else {
    document.querySelector("." + elementSubmit).submit();
  }
}
function share_link() {
  var link = window.location.href;
  navigator.clipboard.writeText(link);
  document.getElementById("linkShare").title = "Link copiado";
}
function myLink(link) {
  navigator.clipboard.writeText(
    "https://www.openmarket.com/convite?user=" + link
  );
  document.getElementById("userLink").innerHTML = "Link copiado";
}
function showAlert(element) {
  if (window.innerWidth > 900) {
    document.getElementById(element).style.display = "flex";
  }
}
function closeAlert(element) {
  if (window.innerWidth > 900) {
    document.getElementById(element).style.display = "none";
  }
}

function pagination(page, totalpages, page_url) {
  var pagina = parseInt(page);
  var totalpagina = Math.ceil(totalpages);
  var pagination = document.querySelector(".pagination");
  while (pagination.children.length > 0) {
    pagination.removeChild(pagination.lastElementChild);
  }

  if (totalpagina < 6) {
    for (let i = 1; i < totalpagina; i++) {
      var line = document.createElement("li");
      line.setAttribute(
        "onclick",
        "loadAdvertsFilters('" + i + "', '" + page_url + "')"
      );
      if (pagina == i) {
        line.setAttribute("class", "page-item active");
      } else {
        line.setAttribute("class", "page-item");
      }
      var link = document.createElement("a");
      link.setAttribute("class", "page-link");
      link.innerHTML = i;
      line.appendChild(link);
      pagination.appendChild(line);
    }
  } else if (totalpagina > 5 && pagina > 3) {
    if (pagina <= totalpagina - 2) {
      for (var i = pagina - 2; i <= pagina + 2; i++) {
        var line = document.createElement("li");
        line.setAttribute(
          "onclick",
          "loadAdvertsFilters('" + i + "', '" + page_url + "')"
        );
        if (pagina == i) {
          line.setAttribute("class", "page-item active");
        } else {
          line.setAttribute("class", "page-item");
        }
        var link = document.createElement("a");
        link.setAttribute("class", "page-link");
        link.innerHTML = i;
        line.appendChild(link);
        pagination.appendChild(line);
      }
    } else {
      if (pagina == totalpagina - 1) {
        for (let i = pagina - 3; i <= totalpagina; i++) {
          var line = document.createElement("li");
          line.setAttribute(
            "onclick",
            "loadAdvertsFilters('" + i + "', '" + page_url + "')"
          );
          if (pagina == i) {
            line.setAttribute("class", "page-item active");
          } else {
            line.setAttribute("class", "page-item");
          }
          var link = document.createElement("a");
          link.setAttribute("class", "page-link");
          link.innerHTML = i;
          line.appendChild(link);
          pagination.appendChild(line);
        }
      } else if (pagina == totalpagina) {
        for (let i = pagina - 4; i <= totalpagina; i++) {
          var line = document.createElement("li");
          line.setAttribute(
            "onclick",
            "loadAdvertsFilters('" + i + "', '" + page_url + "')"
          );
          if (pagina == i) {
            line.setAttribute("class", "page-item active");
          } else {
            line.setAttribute("class", "page-item");
          }
          var link = document.createElement("a");
          link.setAttribute("class", "page-link");
          link.innerHTML = i;
          line.appendChild(link);
          pagination.appendChild(line);
        }
      }
    }
  } else if (totalpagina > 5 && pagina < 4) {
    for (let i = 1; i <= 5; i++) {
      var line = document.createElement("li");
      line.setAttribute(
        "onclick",
        "loadAdvertsFilters('" + i + "', '" + page_url + "')"
      );
      if (pagina == i) {
        line.setAttribute("class", "page-item active");
      } else {
        line.setAttribute("class", "page-item");
      }
      var link = document.createElement("a");
      link.setAttribute("class", "page-link");
      link.innerHTML = i;
      line.appendChild(link);
      pagination.appendChild(line);
    }
  }
}
