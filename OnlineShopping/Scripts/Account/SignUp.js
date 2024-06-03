$(document).ready(function () {
    $("#showPassword").change(function () {
        var passField = $("#pass");
        if (this.checked) {
            passField.attr("type", "text");
        } else {
            passField.attr("type", "password");
        }
    });

    $("#btnclick").click(function () {
        var lname = $("#lname").val();
        var fname = $("#fname").val();
        var email = $("#email").val();
        var pass = $("#pass").val();
        var confirmPass = $("#confirmPass").val();
        var username = $("#username").val();
        var dob = $("#dob").val();
        var gender = $("#gender").val();

        if (pass !== confirmPass) {
            $("#errorMessage").text("Passwords do not match.");
            return;
        }
        if (gender.length != 1) {
            $("#errorMessage").text("Please enter only one letter for gender.")
        } else {

            $.post('../Account/SignUpForm', {
                fname: fname,
                lname: lname,
                email: email,
                pass: pass,
                username: username,
                dob: dob,
                gender: gender,
            }, function (data) {
                if (data[0].success == 1) {
                    alert("Account successfully created!");
                    $('#errorMessage').text("");
                    $("#lname").val("");
                    $("#fname").val("");
                    $("#email").val("");
                    $("#pass").val("");
                    $("#username").val("");
                    $("#dob").val("");
                    $("#gender").val("");
                    window.location.href = '/Account/Login';
                } else {
                    var errorMessage = data[0].errorMessage;
                    if (errorMessage) {
                        $('#errorMessage').text(errorMessage);
                    } else {
                        alert("Failed to create account.");
                    }
                }
            });
        }
    });
});
