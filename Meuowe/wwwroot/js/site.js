// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

// navigation bar search function
/**********************************************/
function searchUsers() {
    var userName;
    if (document.getElementsByName('userName')[0] != null) {
        userName = document.getElementsByName('userName')[0].value;
    }
    $.ajax({
        type: "GET",
        data: {
            "userName": userName
        },
        url: "/?handler=SearchDropDownViewComponent",
        success: function (result) {
            $("#userList").empty(result);
            $("#userList").append(result);
        }
    });
}



