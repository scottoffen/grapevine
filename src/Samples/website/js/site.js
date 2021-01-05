$(document).ready(function () {
  let btn = $("#stop-server");

  btn.click(function (e) {
    e.preventDefault();
    btn.text("Stopping Server");

    $.ajax({
      url: '/api/stop',
      success: function (data) {
        btn.text("Server Stopped");
      },
      error: function () {
        btn.text("What did you do?");
      },
      type: 'Get'
    });
  });
});