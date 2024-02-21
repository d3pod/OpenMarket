if (window.innerWidth < 900) {
  //Botão da barra de pesquisa (lupa)
  var img = document.createElement("img");
  img.src = "/imgs/search.png";
  document.querySelector(".search-button").innerHTML = "";
  document.querySelector(".search-button").appendChild(img);
}
if (window.innerWidth > 800) {
  function showCities() {
    document.getElementById("nav-cities").style.display = "flex";
    document.getElementById("search-city").setAttribute("placeholder", "");
  }
}
function hideCities() {
  document.getElementById("nav-cities").style.display = "none";
  document.getElementById("search-city").setAttribute("placeholder", "Portugal");
}
function locationName(name, type) {
  document.getElementById("search-city").value = name;
  document.getElementById("search-city").setAttribute("data-name", type);
}
async function search() {
  var text = document.getElementById("searchbar-text").value.trim();
  var location = document.getElementById("search-city").value;
  var type = document.getElementById("search-city").getAttribute("data-name");
  /* if (type == null) {
      location = "Portugal";
      type = "pais";
    } */
  if (text !== "") {
    var search_text = text.replaceAll(" ", "%20");
    if (type == null) {
      window.location.href = "/anuncios?pesquisa=" + search_text;
    } else {
      window.location.href =
        "/anuncios?pesquisa=" + search_text + "&" + type + "=" + location;
    }
  } else {
    if (type == null) {
      window.location.href = "/anuncios";
    } else {
      window.location.href = "/anuncios?" + type + "=" + location;
    }
  }
}
function closeDiv(id) {
  document.getElementById(id).style.display = "none";
}
async function loadSuggestions(page) {
  var text = document.getElementById("searchbar-text").value.trim();
  if (text.length > 3) {
    const url_string = window.location.search;
    var url = new URLSearchParams(url_string);
    var category = url.get("categoria");
    var sub_category = url.get("sub_categoria");
    var localizacao = document.getElementById("search-city").value;
    var tipo = document.getElementById("search-city").dataset.name;
    var url = "/" + page + "?handler=Sugestoes";
    if (
      (localizacao !== "" && tipo !== null) ||
      (localizacao !== undefined && tipo !== undefined)
    ) {
      url =
        url +
        "&tipo=" +
        encodeURIComponent(tipo) +
        "&localizacao=" +
        encodeURIComponent(localizacao);
    }
    if (text !== "") {
      url = url + "&pesquisa=" + encodeURIComponent(text);
    }
    if (sub_category !== undefined && sub_category !== null) {
      url = url + "&sub_ategoria=" + encodeURIComponent(sub_category);
    } else if (category !== undefined && category !== null) {
      url = url + "&categoria=" + encodeURIComponent(category);
    }
    var suggestions = document.getElementById("nav-results");
    while (document.getElementById("nav-results").firstChild) {
      document.getElementById("nav-results").firstChild.remove();
    }

    var request = new XMLHttpRequest();
    request.open("get", url, true);
    request.onload = function () {
      if (this.status >= 200 && this.status < 400) {
        var jsondata = JSON.parse(this.response);
        if (jsondata !== null) {
          for (let i = 0; i < jsondata.length; i++) {
            var suggestion = document.createElement("a");
            var title = document.createElement("p");
            var image = document.createElement("img");
            title.innerHTML = jsondata[i].title;
            if (jsondata[i].image_filename != null) {
              image.src = "/anuncios/" + jsondata[i].image_filename;
            } else if (jsondata[i].groupName == "Emprego") {
              image.src = "/imgs/procurar.png";
            } else {
              image.src = "/imgs/WithoutImage.jpg";
            }
            let link = "/anuncio?" + jsondata[i].title + "&id=" + jsondata[i].id;
            suggestion.setAttribute("href", link);
            image.setAttribute("class", "nav-image");
            title.setAttribute("class", "nav-title");
            suggestion.setAttribute("class", "nav-suggestion");
            suggestion.appendChild(image);
            suggestion.appendChild(title);
            suggestions.appendChild(suggestion);
          }
          document.getElementById("nav-results").style.display = "flex";
        }
      } else {
        return;
      }
    };
    request.send();
  } else {
    closeDiv("nav-results");
  }
}
