var url = "";
var contactIDs = [];
var activityIDs = [];
var isNew = false;

var id = 0;
var clientID = 0;

var accrualSum = 0;
var customerSum = 0;
var invoiceAmtExcl = 0;

var ClientID = 0;
var BillingMethodType = 0;
var ProjectID = 0;

$(function () {
    id = $('#InvoiceRuleID').val();
    $('#InvoiceRuleID').val('');

    isNew = id === "0";

    clientID = $('#ClientID').val();
    $('#ClientID').val('');

    if ($('#InvoiceAmountExclVAT').length > 0) {
        invoiceAmtExcl = NumberFunctions.toDouble($('#InvoiceAmountExclVAT').val());
    }

    $('#Save').hide();

    if (isNew) {
        $('#Close').hide();
        $('#Delete').hide();
    } else {
        $('#Cancel').hide();
    }
});

var RuleSetup = {
    setupRuleDefaults: function () {
        RuleSetup.setValues();

        jqDialog.renderView({
            url: "/InvoiceRule/RuleDefaults/",
            data: {},
            title: "Rule Settings",
            buttons: {
                Ok: function () {
                    jqDialog.wait();

                    if (ProjectID === null)
                        ProjectID = 0;

                    window.location = "/InvoiceRule/Create?clientID=" + ClientID + "&projectID=" + ProjectID;
                }
            }
        });
    },

    setValues: function () {
        ClientID = parseInt($('#ClientRuleList').val(), 10);
        ProjectID = parseInt($('#ProjectList').val(), 10);

        if (Number.isNaN(ProjectID)) {
            if (ClientID > 0)
                $('#projectDiv').show();
            else {
                $('#projectDiv').hide();
                $('#billingMethodDiv').hide();
                $('button:contains("Ok")').hide();
            }
        }
        else {
            if (ProjectID > 0) {
                RuleSetup.getBillingMethod(ProjectID);
                $('#billingMethodDiv').show();
                $('button:contains("Ok")').show();
            }
            else {
                $('#billingMethodDiv').hide();
                $('button:contains("Ok")').hide();
            }
        }
    },

    getProjects: function () {
        ClientID = parseInt($('#ClientRuleList').val(), 10);
        ProjectID = 0;

        $("#ProjectList").val($("#target option:first").val());

        $('#billingMethodDiv').hide();
        $('button:contains("Ok")').hide();

        if (ClientID < 1) {
            $('#projectDiv').hide();
            return;
        } else {
            $.post('/InvoiceRule/ProjectListLookup/', { clientID: ClientID }, function (res) {
                EnabillViews.updateDropDownList($('#ProjectList'), res);
            },
                'json');

            $('#projectDiv').show();

            RuleSetup.setValues();
        }
    },

    getBillingMethod: function (ProjectID) {
        $.post('/InvoiceRule/BillingMethodLookup/', { projectID: ProjectID }, function (res) {
            $('#billing_method').text(res);
        },
            'json');
    }
};

var InvoiceRule = {
    createInvoiceFromRule: function (id) {
        jqDialog.confirm({
            message: 'Would you like to create an invoice from this rule?',
            buttons: {
                Yes: function () {
                    jqDialog.wait();

                    $.post("/InvoiceRule/CreateInvoiceFromRule/", { id: id }, function (res) {
                        jqDialog.close();
                        toastr.info("Processing...");

                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            window.location = res.url;
                        }
                    });
                }
            }
        });
    },

    deleteInvoiceRule: function () {
        jqDialog.confirm({
            message: "Would you like to delete this invoice rule?",
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/InvoiceRule/Delete/', { id: id }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            window.location = "/InvoiceRule/Index/";
                        }
                    });
                }
            }
        });
    },

    saveTM: function () {
        InvoiceRule.getContactIDs();
        BillingMethod.TM();

        InvoiceRule.preSaveSetup();

        $.post(url, $('#TMRule').serialize(), function (res) {
            Result.showResult(res);
        });
    },

    saveFixedCost: function () {
        InvoiceRule.getContactIDs();
        BillingMethod.fixedCost();

        InvoiceRule.preSaveSetup();

        $.post(url, $('#FixedCostRule').serialize(), function (res) {
            Result.showResult(res);
        });
    },

    saveMonthlyFixedCost: function () {
        InvoiceRule.getContactIDs();
        BillingMethod.monthlyFixedCost();

        InvoiceRule.preSaveSetup();

        $.post(url, $('#MonthlyFixedCostRule').serialize(), function (res) {
            Result.showResult(res);
        });
    },

    saveActivityFixedCost: function () {
        InvoiceRule.getContactIDs();
        BillingMethod.activityFixedCost();

        InvoiceRule.preSaveSetup();

        $.post(url, $('#ActivityFixedCostRule').serialize(), function (res) {
            Result.showResult(res);
        });
    },

    cancel: function () {
        jqDialog.confirm({
            message: 'Are you sure you want to cancel this operation?',
            buttons: {
                Yes: function () {
                    window.location = "/InvoiceRule/Index/";
                }
            }
        });
    },

    close: function () {
        jqDialog.confirm({
            message: 'Are you sure you want to close this Invoice Rule?',
            buttons: {
                Yes: function () {
                    window.location = "/InvoiceRule/Index/";
                }
            }
        });
    },

    saveSLA: function () {
        BillingMethod.SLA();

        InvoiceRule.getContactIDs();
        InvoiceRule.preSaveSetup();

        $.post(url, $('#SLARule').serialize(), function (res) {
            Result.showResult(res);
        });
    },

    getContactIDs: function () {
        contactIDs = [];
        $(".Contacts:checked").each(function () {
            contactIDs.push($(this).val());
        });
    },

    preSaveSetup: function () {
        $('#ClientID').val(clientID);
        $('#InvoieRuleID').val(id);

        $("#ContactList").val(contactIDs.toString());
        $("#ActivityList").val(activityIDs.toString());
    }
};

var FixedCostLines = {
    update: function () {
        toastr.info("Processing...");

        var fcAmount = $('#InvoiceAmountExclVAT').val();
        var periods = $('#AccrualPeriods').val();

        $.post('/InvoiceRule/GetNewLines/', { id: id, amount: fcAmount, periods: periods }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $('#FixedCostLines').html(res);
                invoiceAmtExcl = NumberFunctions.toDouble($('#InvoiceAmountExclVAT').val());
                FixedCostLines.updateRuleLines();
            }
        });
    },

    updateRuleLines: function () {
        customerSum = 0;
        accrualSum = 0;

        $('.custAmt').each(function () {
            customerSum += parseFloat(NumberFunctions.toDouble($(this).val()));
        });

        $('.accrualAmt').each(function () {
            accrualSum += parseFloat(NumberFunctions.toDouble($(this).val()));
        });

        $('#CustomerSum').html(NumberFunctions.toDoubleString(customerSum));
        $('#AccrualSum').html(NumberFunctions.toDoubleString(accrualSum));

        FixedCostLines.validate();
    },

    validate: function () {
        if (customerSum === invoiceAmtExcl && accrualSum === invoiceAmtExcl) {
            $('#SaveFCLines').show();
            $('#ErrorMessage').hide();
        }
        else {
            $('#SaveFCLines').hide();
            $('#ErrorMessage').show();
        }
    },

    saveFixedCostAccrualBreakdown: function () {
        toastr.info("Processing...");

        $.post('/InvoiceRule/SaveFixedCostLines/' + id, $('#fcLines').serialize(), function (res) {
            toastr.clear();
            jqDialog.quickStatus(!res.IsError, res.Description); //IsError will return true if error occurs, but we need to send a false flag to the quickStatus to flag that the action was not successful and visa versa
        });
    }
};

var BillingMethod = {
    TM: function () {
        $(".TMActivities:checked").each(function () {
            activityIDs.push($(this).val());
        });

        url = "/InvoiceRule/CreateTMRule/";

        if (!isNew)
            url = "/InvoiceRule/EditTMRule/" + id;
    },

    fixedCost: function () {
        url = "/InvoiceRule/CreateFixedCostRule/";

        if (!isNew)
            url = "/InvoiceRule/EditFixedCostRule/" + id;
    },

    monthlyFixedCost: function () {
        url = "/InvoiceRule/CreateMonthlyFixedCostRule/";

        if (!isNew)
            url = "/InvoiceRule/EditMonthlyFixedCostRule/" + id;
    },

    activityFixedCost: function () {
        $(".AfcActivities:checked").each(function () {
            activityIDs.push($(this).val());
        });

        url = "/InvoiceRule/CreateActivityFixedCostRule/";

        if (!isNew)
            url = "/InvoiceRule/EditActivityFixedCostRule/" + id;
    },

    SLA: function () {
        $(".SlaActivities:checked").each(function () {
            activityIDs.push($(this).val());
        });

        url = "/InvoiceRule/CreateSLARule/";
        if (!isNew)
            url = "/InvoiceRule/EditSLARule/" + id;
    },

    travel: function () {
        $(".TravelActivities:checked").each(function () {
            activityIDs.push($(this).val());
        });

        url = "/InvoiceRule/SaveTravelRule/";

        if (!isNew)
            url = "/InvoiceRule/Edit/" + $("#InvoiceRuleID").val();
    },

    adHoc: function () {
    }
};

var Result = {
    showResult: function (res) {
        if (res.IsError) {
            jqDialog.quickStatus(false, res.Description);
        }
        else {
            jqDialog.status({
                message: "Invoice rule saved successfully.",
                buttons: {
                    OK: function () {
                        jqDialog.wait();
                        window.location = res.url;
                    }
                }
            });
        }
    }
};

$(function () {
    $("#ActivityFixedCostRule :input").change(function () {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#ActivityFixedCostRule :input").on('keyup keydown keypress', function (event) {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#FixedCostRule :input").change(function () {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#FixedCostRule :input").on('keyup keydown keypress', function (event) {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#MonthlyFixedCostRule :input").change(function () {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#MonthlyFixedCostRule :input").on('keyup keydown keypress', function (event) {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#TMRule :input").change(function () {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#TMRule :input").on('keyup keydown keypress', function (event) {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#SLARule :input").change(function () {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});

$(function () {
    $("#SLARule :input").on('keyup keydown keypress', function (event) {
        $('#Close').hide();
        $('#Cancel').show();
        $('#Save').show();
    });
});