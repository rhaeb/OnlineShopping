$(document).ready(function () {
    if ('@ViewBag.image') {
        $('#imageContainer').html('<img src="../Home/Image?filename=@HttpUtility.UrlEncode(ViewBag.image)" width="50%" alt="Product Image" />');
    } else {
        $('#imageContainer').html('');
    }

    $("#btnedit").click(function () {
        console.log("Update button clicked");
        var id = $("#cusID").text();
        var fname = $("#fname").val();
        var lname = $("#lname").val();
        var email = $("#email").val();
        var pass = $("#pass").val();
        var uname = $("#uname").val();
        var dob = $("#dob").val();
        var gender = $("#gender").val();
        console.log("Data to be sent:", id, fname, lname, email, pass, uname, dob, gender);

        $.post("../CustomerUpdateEdit", {
            id: id,
            fname: fname,
            lname: lname,
            email: email,
            pass: pass,
            uname: uname,
            dob: dob,
            gender: gender
        }, function (response) {
            if (response[0].mess == 0) {
                alert("Customer updated successfully");
                window.location.href = '/Home/ListAllProducts';
            } else {
                alert("Failed to update product");
            }
        });
    });

    $("#btndelete").click(function () {
        var email = $("#email").val();
        var pass = $("#pass").val();

        $.post("../Home/CustomerDelete", {
            email: email,
            pass: pass

        }, function (data) {
            if (data[0].mess == 0) {
                $("#fname").val("");
                $("#lname").val("");
                $("#email").val("");
                $("#pass").val("");
                $("#uname").val("");
                $("#dob").val("");
                $("#gender").val("");
                alert("Data is successfully removed.");
                window.location.href = '/Home/ListAllCustomers';
            }
        });
    });
});
