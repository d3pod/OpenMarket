function loginfb() {
  FB.init({
    appId: "App_Id",
    cookie: true,
    status: true,
    xfbml: false,
  });
  FB.login(function (response) {
    if (response.authResponse) {
      FB.api("/me?fields=id,email,name", function (response) {
        var session = response.id + "/" + response.name + "/" + response.email;
        setSocialSession(session);
      });
    }
  });
}

function setSocialSession(response) {
  var cookieValue = response.replace(" ", "");
  var d = new Date();
  d.setTime(d.getTime() + 60 * 60 * 1000);
  var expires = "expires=" + d.toUTCString();
  var name = "uinef=" + cookieValue + "; " + expires;
  document.cookie = name;
  document.getElementById("facebookButton").click();
}