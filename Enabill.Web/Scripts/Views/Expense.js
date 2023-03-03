$(function () {
    if ($('.only :checkbox:checked').length > 0) {
        $('#disableControls').prop('disabled', true);
        $('#ExpenseDate').datepicker('disable');
    }
    else {
        var selection = $('#ExpenseCategoryTypeID :selected').text();

        if (selection === "Mileage") {
            $('#MileageLabel').show();
            $('#Mileage').show();
            $('#Amount').attr('readonly', 'true');
        } else {
            $('#Mileage').hide();
            $('#MileageLabel').hide();
            $('#Amount').removeAttr('readonly');
        }
    }

    $('#Cancel').hide();
    $('#Save').hide();
    $('#DeleteAttachment').hide();

    var cost = $('#Amount').val();
    cost = parseFloat(cost).toFixed(2);
    $('#Amount').val(cost);
});

//#region EXPENSE

var Expense = {
    save: function (id) {
        toastr.info("Processing...");

        var isNew = id === 0;
        var url = "/Expense/Edit/" + id;

        if (isNew)
            url = "/Expense/Create/";
        else
            closeDocument();

        $.post(url, $('#ExpenseDetailFrm').serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();

                jqDialog.status({
                    success: true,
                    message: 'Expense was saved successfully.',
                    buttons: {
                        OK: function () {
                            jqDialog.close();
                            $('#ucExpenseDetail').html(res);
                            window.location.href = res.redirectTo; //redirect
                        }
                    }
                });
            }
        });
    },

    close: function (id) {
        window.location = "/Expense/Index/" + id;
    },

    cancel: function (id) {
        jqDialog.confirm({
            message: 'Are you sure you want to cancel this operation?',
            buttons: {
                Yes: function () {
                    window.location = "/Expense/Index/" + id;
                }
            }
        });
    },

    delete: function (userID, expenseID) {
        jqDialog.confirm({
            message: 'Are you sure you want to delete this expense?',
            buttons: {
                Yes: function () {
                    toastr.info("Processing...");

                    closeDocument();

                    $.post('/Expense/DeleteExpense/', { userID: userID, expenseID: expenseID }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            jqDialog.status({
                                success: true,
                                message: 'Expense was deleted successfully.',
                                buttons: {
                                    OK: function () {
                                        jqDialog.close();
                                        $('#ucIndex').html(res);
                                        window.location = "/Expense/Index/" + userID;
                                    }
                                }
                            });
                        }
                    });
                }
            }
        });
    }
};

//#endregion

//#region EXPENSE ATTACHMENT

var ExpenseAttachment = {
    delete: function (userID, expenseID) {
        toastr.info("Processing...");

        var fileList = getCheckBoxValues();
        var callingPage = "";
        var newExpenseDate = "01-01-1900";

        closeDocument();

        $.post('/Expense/FileMoveOrDelete/', { userID: userID, expenseID: expenseID, fileList: fileList, callingPage: callingPage, newExpenseDate: newExpenseDate }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                $('#ucExpenseDetail').html(res);
                window.location.href = res.redirectTo; //redirect
            }
        });
    },

    displayDocument: function (id, expenseID, fileName, filePath) {
        if (filePath !== "")
            window.location = "/Expense/Edit/?id=" + id + "&expenseID=" + expenseID + "&fileName=" + fileName + "&filePath=" + filePath;
    }
};

//#endregion

$(function () {
    $('#Locked').change(function () {
        disableControls();
    });
});

$(function () {
    $('#ExpenseCategoryTypeID').change(
        function () {
            var method = $('option:selected', this).text();

            if (method === "Mileage") {
                $('#MileageLabel').show();
                $('#Mileage').show();
                $("#Amount").val('');
                $('#Amount').attr('readonly', 'true');
            } else {
                $("#Mileage").val('');
                $("#Amount").val('');
                $('#Mileage').hide();
                $('#MileageLabel').hide();
                $('#Amount').removeAttr('readonly');
            }
        });
});

$(function () {
    $('#Amount').on('click focusin', function () {
        this.value = this.value === 0 ? '' : this.value;
    });
});

$(function () {
    $("#Amount").blur(function () {
        var cost = $(this).val();
        cost = addZeroes(cost);
        this.value = cost;
    });
});

$(function () {
    $("#ExpenseDetailFrm :input").change(function () {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#ExpenseDetailFrm :input").on('keyup keydown keypress', function (event) {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $('.chkBox').click(function () {
        if ($("#Attachments input:checkbox:checked").length > 0) {
            $('#DeleteAttachment').show();
        }
        else {
            $('#DeleteAttachment').hide();
        }
    });
});

$(function () {
    $('#ExpenseDate').change(function () {
        expenseDate();
    });
});

$(function () {
    $('#Mileage').keyup(function () {
        expenseDate();
    });
});

function disableControls() {
    if ($('#Locked').is(':checked')) {
        $('#disableControls').prop('disabled', true);
        $('#ExpenseDate').datepicker('disable');
    } else {
        $('#disableControls').prop('disabled', false);
        $('#ExpenseDate').datepicker('enable');
    }
}

function addZeroes(num) {
    // Convert input string to a number and store as a variable.
    var value = Number(num);
    // Split the input string into two arrays containing integers/decimals
    var res = num.split(".");
    // If there is no decimal point or only one decimal place found.
    if (res.length === 1 || res[1].length < 3) {
        // Set the number to two decimal places
        value = value.toFixed(2);
    }
    // Return updated or original number.
    return value;
}

function getCheckBoxValues() {
    var chkArray = [];

    $(".chkBox:checked").each(function () {
        chkArray.push($(this).attr('id'));
    });

    return chkArray;
}

function closeDocument() {
    //DocuViewareAPI.PostCustomServerAction("docuView", true, "CloseDoc");   // To call code behind method
    if (typeof DocuViewareAPI !== "undefined")
        DocuViewareAPI.CloseDocument("DocuVieware1");
}

function expenseDate() {
    if ($('#Mileage').val() > 0) {
        var cost = $('#Mileage').val() * 4.18; //AA Rate hard coded, however will need to be configurable from the Configuration menu.

        if ($('#ExpenseDate').val() < '2020-03-01')
            cost = $('#Mileage').val() * 3.61;
        else if ($('#ExpenseDate').val() < '2022-04-01')
            cost = $('#Mileage').val() * 3.98;

        cost = parseFloat(cost).toFixed(2);
        $('#Amount').val(cost);
    }
}