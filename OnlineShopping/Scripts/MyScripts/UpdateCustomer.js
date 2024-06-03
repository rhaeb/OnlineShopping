$(document).ready(function () {
    var dobValue = "@ViewBag.DateOfBirth";
    if (dobValue) {
        var date = new Date(dobValue);
        if (!isNaN(date.getTime())) { // Check if the date is valid
            var formattedDate = date.toISOString().split('T')[0];
            $("#dob").val(formattedDate);
        } else {
            console.error("Invalid date format:", dobValue);
        }
    }

    $("#btnUpdate").click(function () {
        console.log("Update button clicked");
        var id = $("#cusID").text();
        var fname = $("#fname").val();
        var lname = $("#lname").val();
        var email = $("#email").val();
        var pass = $("#pass").val();
        var uname = $("#uname").val();
        var dob = $("#dob").val();
        var gender = $("#gender").val();
        console.log("Data to be sent:", id, fname, lname, email, pass, uname, dob, gender); // Add this line

        $.post("/Home/CustomerUpdateEdit", {
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
                window.location.href = '/Home/ListAllCustomers';
            } else {
                console.log("Failed to update customer");
            }
        });
    });
    
    $("#btnBack").click(function () {
        window.location.href = '/Home/ListAllCustomers';
    });

    $("#btnDelete").click(function () {
        var id = $("#cusID").text();
        console.log("Delete button clicked");

        $.post("/Home/CustomerDelete", {
            id: id

        }, function (data) {
            if (data[0].mess == 0) {
                $("#fname").val("");
                $("#lname").val("");
                $("#email").val("");
                $("#pass").val("");
                $("#uname").val("");
                $("#dob").val("");
                $("#gender").val("");
                console.log("Data is successfully removed.");
                window.location.href = '/Home/ListAllCustomers';
            }
        });
    });
});
