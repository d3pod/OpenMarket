var IndexImages = 1;
showSlides(IndexImages);
function nextSlide(n) {
  showSlides((IndexImages += n));
}
function currentSlide(n) {
  showSlides((IndexImages = n));
}
async function showSlides(n) {
  var slides = document.getElementsByClassName("advert-image");
  var dots = document.getElementsByClassName("dot");

  if (slides.length == 1) {
    document.querySelector(".advert-img.previus").style.display = "none";
    document.querySelector(".advert-img.next").style.display = "none";
  }

  if (n > slides.length) {
    IndexImages = 1;
  }
  if (n < 1) {
    IndexImages = slides.length;
  }
  for (let i = 0; i < slides.length; i++) {
    slides[i].style.display = "none";
  }
  for (let i = 0; i < dots.length; i++) {
    dots[i].className = dots[i].className.replace(" active", "");
  }
  slides[IndexImages - 1].style.display = "block";
  dots[IndexImages - 1].className += " active";
}

async function showContact(id, element, type) {
  var contact = document.getElementById(element);
  var data =
    "&advert=" + encodeURIComponent(id) + "&type=" + encodeURIComponent(type);
  var url = "/anuncio?handler=Contact" + data;

  var link = document.createElement("a");
  var request = new XMLHttpRequest();
  request.open("get", url, true);
  request.onload = function () {
    if (this.status >= 200 && this.status < 400) {
      var jsondata = JSON.parse(this.response);
      if (jsondata !== null && type == "phone") {
        var number = jsondata;
        if (number.length == 9) {
          number =
            number.substring(0, 3) +
            " " +
            number.substring(3, 6) +
            " " +
            number.substring(6, 9);
          contact.innerHTML = "";
          link.innerHTML = number;
          link.setAttribute("href", "tel:" + number);
          contact.appendChild(link);
        } else if (number.length == 13) {
          number =
            number.substring(0, 4) +
            " " +
            number.substring(4, 7) +
            " " +
            number.substring(7, 10) +
            " " +
            number.substring(10, 13);
          contact.innerHTML = "";
          link.innerHTML = number;
          link.setAttribute("href", "tel:" + number);
          contact.appendChild(link);
        } else if (number.length == 14) {
          number =
            number.substring(0, 5) +
            " " +
            number.substring(5, 8) +
            " " +
            number.substring(8, 11) +
            " " +
            number.substring(11, 14);
          contact.innerHTML = "";
          link.innerHTML = number;
          link.setAttribute("href", "tel:" + number);
          contact.appendChild(link);
        }
      }
    } else {
      return;
    }
  };
  request.send();
}
function hidebutton() {
  document.querySelector(".buttonContact").style.display = "none";
}
function favorites_over() {
  document.getElementById("advert-favorite").src = "/imgs/heart-line.png";
}
function favorites_out() {
  document.getElementById("advert-favorite").src = "/imgs/heart.png";
}
async function add_to_favorites(id, user) {
  var favorite = document.getElementById("advert-favorite");
  var favorite_added = document.getElementById("advert-favorite-added");
  var data =
    "&advert=" + encodeURIComponent(id) + "&user=" + encodeURIComponent(user);
  var url = "/anuncio?handler=Favorite" + data;

  var request = new XMLHttpRequest();
  request.open("get", url, true);
  request.onload = function () {
    if (this.status >= 200 && this.status < 400) {
      favorite.style.display = "none";
      favorite_added.style.display = "flex";
    } else {
      return;
    }
  };
  request.send();
}
async function remove_favorites(id, user) {
  var favorite = document.getElementById("advert-favorite");
  var favorite_added = document.getElementById("advert-favorite-added");
  var data =
    "&advert=" + encodeURIComponent(id) + "&user=" + encodeURIComponent(user);
  var url = "/anuncio?handler=Remove" + data;

  var request = new XMLHttpRequest();
  request.open("get", url, true);
  request.onload = function () {
    if (this.status >= 200 && this.status < 400) {
      favorite.style.display = "flex";
      favorite_added.style.display = "none";
    } else {
      return;
    }
  };
  request.send();
}
function showEmailForm() {
  document.getElementById("email-form").style.display = "flex";
}
var touchStart = 0;
var touchMove = 0;
function position(event) {
  touchStart = event.touches[0].clientX;
}
function swipe(event) {
  touchMove = event.touches[0].clientX;
}
function swipeend() {
  if (touchMove > 0) {
    if (touchMove > touchStart) {
      nextSlide(-1);
    } else {
      nextSlide(1);
    }
    touchStart = 0;
    touchMove = 0;
  }
}
