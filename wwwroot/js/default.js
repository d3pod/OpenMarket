function setCookies() {
  var d = new Date();
  d.setTime(d.getTime() + 30 * 24 * 60 * 60 * 1000);
  var expires = "expires=" + d.toUTCString();
  document.cookie = "accepted=1;" + expires;
  document.querySelector(".cookies").style.display = "none";
}
function urlAdjusts(texto) {
  let url = texto;
  let Acentos = "ÄÅÁÂÀÃäáâàãÉÊËÈéêëèÍÎÏÌíîïìÖÓÔÒÕöóôòõÜÚÛüúûùÇçñ";
  let semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCcn";
  for (let i = 0; i < Acentos.length; i++) 
  {
    url = url.replace(Acentos[i], semAcentos[i]);
  }
  return encodeURIComponent(url);
}
function viewRegister(id, pagina) {
  var data =
    "&anuncio=" + encodeURIComponent(id) + "&pagina=" + encodeURIComponent(pagina);
  var url = "?handler=View" + data;
  var request = new XMLHttpRequest();
  request.open("get", url, true);
  request.send();
}
function cookieAlert(page, time) {
  var d = new Date();
  d.setTime(d.getTime() + time * 24 * 60 * 60 * 1000);
  var expires = "expires=" + d.toUTCString();
  document.cookie = page + "=1;" + expires;
}
function closePanel(element, form) {
  var panel = document.querySelector("." + element);
  panel.style.display = "none";
  if (form != null) {
    document.getElementById(form).submit();
  }
}