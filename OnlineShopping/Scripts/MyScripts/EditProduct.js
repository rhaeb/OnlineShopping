$().ready(function () {
    $("#btnUpdate").attr("disabled", true);
    $("#btnDel").attr("disabled", true);
    $("#btnEdit").attr("disabled", true);

    $("#btnSearch").click(function (event) {
        event.preventDefault();
        var Idno = $("#txtidno").val();
        $.ajax({
            url: "../Home/GetProductById",
            type: 'GET',
            data: { id: Idno },
            success: function (response) {
                if (response.success) {
                    $("#btnEdit").attr("disabled", false);
                    $('#txtname').val(response.data.Name);
                    $('#txtdesc').val(response.data.Description);
                    $('#txtprice').val(response.data.Price);
                    $('#txtqty').val(response.data.Quantity);
                    if (response.data.Image) {
                        $('#imageContainer').html('<img src="../Home/Image?filename=' + encodeURIComponent(response.data.Image) + '" width="50%" alt="Product Image" />');
                    } else {
                        $('#imageContainer').html('');
                    }
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                alert('An error occurred while fetching product data.');
            }
        });
    });

    $("#btnEdit").click(function (event) {
        event.preventDefault();
        $("#btnUpdate").removeAttr("disabled");
        $("#btnDel").removeAttr("disabled");

    });

    $("#btnUpdate").click(function (event) {
        event.preventDefault();
        var formData = new FormData();
        formData.append("idno", $("#txtidno").val());
        formData.append("name", $("#txtname").val());
        formData.append("desc", $("#txtdesc").val());
        var price = parseFloat($("#txtprice").val());
        var quantity = parseInt($("#txtqty").val());
        formData.append("price", price);
        formData.append("qty", quantity);
        var imageFile = $('input[name="img"]')[0].files[0];
        if (imageFile) {
            formData.append("image", imageFile);
        }

        $.ajax({
            url: "../Home/ProductUpdate",
            type: "POST",
            data: formData,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data[0].mess == 0) {
                    $("#txtidno").val("");
                    $("#txtname").val("");
                    $("#txtdesc").val("");
                    $("#txtprice").val("");
                    $("#txtqty").val("");
                    $('#imageContainer').html('');
                    alert("The data successfully updated");
                } else {
                    alert(data[0].error || 'An error occurred while updating the product.');
                }
            },
            error: function () {
                alert('An error occurred while updating the product.');
            }
        });
    });


    $("#btnDel").click(function (event) {
        event.preventDefault();
        var Idno = $("#txtidno").val();

        $.post("../Home/ProductDelete", {
            idno: Idno

        }, function (data) {
            if (data[0].mess == 0) {
                $("#txtidno").val("");
                $("#txtname").val("");
                $("#txtdesc").val("");
                $("#txtprice").val("");
                $("#txtqty").val("");
                $('#imageContainer').html('');
                alert("Data is successfully removed.");
            }
        });
    });
});
