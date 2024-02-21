async function pagination(page) {
  var new_url;
  const url_string = window.location.search;
  var url = new URLSearchParams(url_string);
  var status = url.get("status");
  var currentpage = url.get("page");
  var username = url.get("user");

  if (status == undefined && username == undefined) {
    if (currentpage != undefined) {
      url.delete("page");
    }
    new_url = url + "?page=" + page;
  } else {
    if (currentpage == undefined) {
      new_url = "?" + url + "&page=" + page;
    } else {
      url.delete("page");
      new_url = "?" + url + "&page=" + page;
    }
  }
  window.location.href = new_url;
}
function ChangeCity_Country(div, button, id) {
  const panel = document.querySelector(div);
  var panel_button = document.querySelector(button);
  panel.style.display = "flex";
  panel_button.value = id;
}
function openPanel(element) {
  var panel = document.querySelector("." + element);
  panel.style.display = "flex";
}
function closePanel(element) {
  var panel = document.querySelector("." + element);
  panel.style.display = "none";
}
function panelFormat(element_show, element_hide) {
  document.getElementsByClassName(element_show).style.display = "flex";
  document.getElementsByClassName(element_hide).style.display = "none";
}
function attributesType() {
  var attributesType = document.getElementById("attribute-type").value;
  var attributeText = document.querySelector(".attribute-name");
  var attributeCombobox = document.querySelector(".attribute-table");
  if (attributesType == "Texto") {
    attributeText.style.display = "flex";
    attributeCombobox.style.display = "none";
  } else {
    attributeText.style.display = "none";
    attributeCombobox.style.display = "flex";
  }
}
function linkStatus() {
  let id = document.getElementById("advert-status").value;
  var new_url;
  const url_string = window.location.search;
  var url = new URLSearchParams(url_string);
  var status = url.get("status");
  var currentpage = url.get("page");
  var username = url.get("user");
  if (currentpage == undefined && username == undefined) {
    if (status != undefined) {
      url.delete("status");
    }
    new_url = url + "?status=" + id;
  } else {
    if (status == undefined) {
      new_url = "?" + url + "&status=" + id;
    } else {
      url.delete("status");
      new_url = "?" + url + "&status=" + id;
    }
  }
  window.location.href = new_url;
}
function refreshCharts()
{
  var month = document.getElementById("month").value;
  window.location.href = "dashboard?month=" + month;
}