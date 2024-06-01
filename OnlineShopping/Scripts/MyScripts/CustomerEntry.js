$(document).ready(function () {
    $("#btnclick").click(function () {
        var lname = $("#lname").val();
        var fname = $("#fname").val();
        var email = $("#email").val();
        var pass = $("#pass").val();
        var username = $("#username").val();
        var dob = $("#dob").val();
        var gender = $("#gender").val();

        if (gender.length != 1) {
            alert("Please enter only one letter for gender.")
        } else {

            $.post('../Home/CustomerAddEntry', {
                fname: fname,
                lname: lname,
                email: email,
                pass: pass,
                username: username,
                dob: dob,
                gender: gender,
            }, function (data) {
                if (data[0].success == 1) {
                    alert("Successfully saved!");
                    $("#lname").val("");
                    $("#fname").val("");
                    $("#email").val("");
                    $("#pass").val("");
                    $("#username").val("");
                    $("#dob").val("");
                    $("#gender").val("");
                } else {
                    alert("Something missing!");
                }
            });
        }
    });
});
