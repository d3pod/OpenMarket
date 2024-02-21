window.addEventListener("resize", windowSize);
function windowSize() {
  let posts = document.getElementById("chat-posts");
  let chat = document.getElementById("chat-container");
  if (window.outerWidth > 1000) {
    chat.setAttribute("style", "height:" + (window.innerHeight - 90) + "px");
  } else if (window.outerWidth < 600) {
    chat.setAttribute("style", "height:" + (window.innerHeight - 105) + "px");
  } else if (
    window.outerWidth > 601 &&
    window.outerWidth < 1199 &&
    window.outerHeight >= window.outerWidth
  ) {
    chat.setAttribute("style", "height:" + (window.innerHeight - 110) + "px");
  } else if (
    window.outerHeight > 600 &&
    window.outerHeight < 1100 &&
    window.outerWidth < 800 &&
    window.outerWidth > window.outerHeight
  ) {
    chat.setAttribute("style", "height:" + (window.innerHeight - 135) + "px");
  } else if (
    window.outerHeight > 300 &&
    window.outerHeight < 599 &&
    window.outerHeight < window.outerWidth
  ) {
    chat.setAttribute("style", "height:" + (window.innerHeight - 120) + "px");
  }
  posts.scrollTop = posts.scrollHeight;
}
if (
  window.innerWidth < 600 &&
  document.querySelector(".mobile-notification") != null
) {
  document.querySelector(".mobile-notification").style.display = "none";
}
function showChats(status) {
  if (status == "closed") {
    document.getElementById("sidebar-users-chat").style.display = "flex";
  } else {
    document.getElementById("sidebar-users-chat").style.display = "none";
  }
}
function sendButton(user) {
  if (window.innerWidth < 600) {
    showChats();
  }
  var chat = document.getElementById("chat-content");
  var content = document.getElementById("new-post");
  var button = document.createElement("button");
  var input = document.createElement("input");
  while (content.firstChild) {
    content.firstChild.remove();
  }
  chat.style.backgroundColor = "rgb(250,250,250)";
  document.getElementById("chat-header").style.backgroundColor =
    "rgb(240,240,240)";
  button.setAttribute("id", "new-message-button");
  input.setAttribute("id", "new-message");
  input.setAttribute("placeholder", "Mensagem...");
  input.setAttribute("type", "text");
  button.setAttribute("onclick", "newPost('" + user + "')");
  button.title = "Enviar";
  button.innerHTML = "Enviar";

  content.appendChild(input);
  content.appendChild(button);
  chats(user);
  chat.style.display = "flex";
  document.getElementById("new-post").style.display = "flex";
}
function chats(user) {
  var list = document.getElementById("list-chats");
  var url = window.location.pathname + "?handler=Conversas";
  while (list.firstChild) {
    list.firstChild.remove();
  }

  var request = new XMLHttpRequest();
  request.open("get", url, true);
  request.onload = function () {
    if (this.status >= 200 && this.status < 400) {
      var jsondata = JSON.parse(this.response);
      if (jsondata !== null) {
        if (window.innerWidth < 600) {
          var span = document.createElement("span");
          span.innerHTML = "&times;";
          span.setAttribute("onclick", "showChats()");
          span.setAttribute("id", "closeList");
          list.appendChild(span);
        }
        for (let i = 0; i < jsondata.length; i++) {
          var button = document.createElement("button");
          button.setAttribute("value", jsondata[i].sender);
          button.innerHTML = jsondata[i].sender_username;
          button.title = jsondata[i].sender_username;
          if (window.innerWidth < 600) {
            button.setAttribute(
              "onclick",
              "sendButton('" +
                jsondata[i].sender +
                "');posts('" +
                jsondata[i].sender +
                "');showChats();"
            );
          } else {
            button.setAttribute(
              "onclick",
              "sendButton('" +
                jsondata[i].sender +
                "'); posts('" +
                jsondata[i].sender +
                "');"
            );
          }
          if (jsondata[i].status == "Por ler") {
            button.setAttribute("class", "to_read");
          }
          if (jsondata[i].sender == user) {
            button.setAttribute("class", "opened");
          }

          list.appendChild(button);
        }
      }
    } else {
      return;
    }
  };
  request.send();
}
function posts(user) {
  var sendMessage = document.getElementById("chat-new-post");
  var message = document.getElementById("new-message");
  var posts = document.getElementById("chat-posts");
  var url = window.location.pathname + "?handler=Mensagens";
  var info = document.getElementById("chat-info");
  sendMessage.style.backgroundColor = "rgb(240,240,240)";

  if (message.value != "") {
    url =
      url +
      "&message=" +
      encodeURIComponent(message.value) +
      "&user=" +
      encodeURIComponent(user);
  } else {
    url = url + "&user=" + encodeURIComponent(user);
  }
  while (posts.firstChild) {
    posts.firstChild.remove();
  }
  while (info.firstChild) {
    info.firstChild.remove();
  }
  message.value = "";
  var request = new XMLHttpRequest();
  request.open("get", url, true);
  request.onload = function () {
    if (this.status >= 200 && this.status < 400) {
      var jsondata = JSON.parse(this.response);
      if (jsondata !== null) {
        var user_info = document.createElement("h4");

        var index = 0;
        while (jsondata[index].sender != user) {
          index++;
        }
        user_info.innerHTML = "Conversa com " + jsondata[index].sender_username;
        info.appendChild(user_info);
        for (let i = 0; i < jsondata.length; i++) {
          var post = document.createElement("div");
          var user_message = document.createElement("p");
          var date = document.createElement("p");

          user_message.innerHTML = jsondata[i].message;
          var sender = jsondata[i].sender;
          var account = jsondata[i].account;
          if (sender == account) {
            post.setAttribute("class", "sended-posts");
            user_message.setAttribute("class", "posts post-sended");
          } else {
            post.setAttribute("class", "received-posts");
            user_message.setAttribute("class", "posts post-received");
          }
          date.setAttribute("class", "post-date");
          let read_date = new Date(jsondata[i].date);

          var message_date =
            read_date.getDate() +
            "/" +
            (read_date.getMonth() + 1) +
            "/" +
            read_date.getFullYear();
          var date_now =
            new Date().getUTCDate() +
            "/" +
            (new Date().getUTCMonth() + 1) +
            "/" +
            new Date().getUTCFullYear();
          var date_yesterday =
            new Date().getUTCDate() -
            1 +
            "/" +
            (new Date().getUTCMonth() + 1) +
            "/" +
            new Date().getUTCFullYear();
          if (message_date == date_now) {
            date.innerHTML =
              read_date.getHours() +
              ":" +
              String(read_date.getMinutes()).padStart(2, "0");
          } else if (message_date == date_yesterday) {
            date.innerHTML =
              "Ontem às " +
              read_date.getHours() +
              ":" +
              String(read_date.getMinutes()).padStart(2, "0");
          } else {
            var month = jsondata[i].date_month;
            date.innerHTML =
              read_date.getDate() +
              " " +
              month.charAt(0).toUpperCase() +
              month.slice(1);
          }
          post.appendChild(user_message);
          post.appendChild(date);
          posts.appendChild(post);
          posts.scrollTop = posts.scrollHeight;
        }
      }
    } else {
      return;
    }
  };
  request.send();
}
function newPost(user) {
  var message = document.getElementById("new-message");
  if (message.value != "") {
    posts(user);
  }
  message.focus();
}
