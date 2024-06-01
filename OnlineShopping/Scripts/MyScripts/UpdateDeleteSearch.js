$().ready(function () {
    $("#btnUpdate").attr("disabled", true);
    $("#btnDel").attr("disabled", true);
    $("#btnEdit").attr("disabled", true);

    $("#btnSearch").click(function () {
        var Idno = $("#txtidno").val();
        $.post("../Home/SearchCustomer", {
            idno: Idno
        }, function (data) {
            if (data[0].mess == 0) {
                $("#btnEdit").removeAttr("disabled");
                $("#txtname").val(data[0].name);
                $("#txtemail").val(data[0].email);
                $("#txtpass").val(data[0].pass);
            } else {
                alert("No records found");
            }
        });
    });

    $("#btnEdit").click(function () {
        $("#btnUpdate").removeAttr("disabled");
        $("#btnDel").removeAttr("disabled");

    });
    $("#btnUpdate").click(function () {
        var Idno = $("#txtidno").val();
        var name = $("#txtname").val();
        var email = $("#txtemail").val();
        var pass = $("#txtpass").val();

        $.post("../Home/CustomerUpdate", {
            idno: Idno,
            name: name,
            email: email,
            pass: pass
        }, function (data) {
            if (data[0].mess == 0) {
                $("#txtname").val("");
                $("#txtemail").val("");
                $("#txtpass").val("");
                alert("The data successfully updated");
            }
        });
    });
    $("#btnDel").click(function () {
        var Idno = $("#txtidno").val();

        $.post("../Home/CustomerDelete", {
            idno: Idno

        }, function (data) {
            if (data[0].mess == 0) {
                $("#txtidno").val("");
                $("#txtname").val("");
                $("#txtemail").val("");
                $("#txtpass").val("");
                alert("Data is successfully removed.");
            }
        });
    });
});