$(function () {
    Forecast.setAutoComplete();
    Forecast.setAutoCompleteProject();
    Forecast.setAutoCompleteUser();
    $('.NonTM').toggleClass("show");
});

var Forecast = {
    save: function (id) {
        toastr.info("Processing...");

        var url = '/Forecast/SaveForecastHeader/';
        if (id === 0) {
            $.post(url, $('#ForecastCreateFrm').serialize(), function (res) {
                if (res.IsError) {
                    toastr.clear();
                    toastr.error("Error processing.");
                    jqDialog.quickStatus(false, res.Description);
                }
                else {
                    toastr.clear();
                    toastr.success("Success processing.");
                    $('#ucIndex').html(res);
                    jqDialog.quickStatus(true, 'Forecast successfully added.');
                }
            });
        }
        else {
            $.post(url, $('#ForecastEditForecastFrm').serialize(), function (res) {
                if (res.IsError) {
                    toastr.clear();
                    toastr.error("Error processing.");
                    jqDialog.quickStatus(false, res.Description);
                }
                else {
                    toastr.clear();
                    toastr.success("Success processing.");
                    $('#ucIndex').html(res);
                    jqDialog.quickStatus(true, 'Forecast successfully updated.');
                }
            });
        }
    },

    edit: function (forecastHeaderID) {
        toastr.info("Processing...");

        var url = '/Forecast/EditForecast/';
        $.post(url, { forecastHeaderID: forecastHeaderID, form: $('#ProjectDetailFrm').serialize() }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                toastr.success("Success processing.");
                $('#ucIndex').html(res);
                jqDialog.quickStatus(true, 'Forecast successfully updated.');
            }
        });
    },

    copy: function (forecastDetailID) {
        jqDialog.renderView({
            url: '/Forecast/ShowCopyPeriodPartialView/',
            data: { forecastDetailID: forecastDetailID },
            title: 'Copy Period',
            buttons: {
                Save: function () {
                    var form = $('#CopyPeriodFrm').serialize();
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Forecast/CopyForecast/', form, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            toastr.success("Success processing.");
                            $('#ucIndex').html(res);
                            jqDialog.quickStatus(true, 'Forecast copied successfully.');
                        }
                    });
                }
            }
        });
    },

    editDetail: function (forecastDetailID) {
        jqDialog.renderView({
            url: '/Forecast/ShowEditDetailPartialView/',
            data: { forecastDetailID: forecastDetailID },
            title: 'Edit Detail',
            width: 800,
            buttons: {
                Save: function () {
                    if ($('#ReasonForChange').val().length <= 0) {
                        jqDialog.quickStatus(false, 'Reason for change cannot be blank.');
                    }
                    else {
                        var form = $('#ForecastEditDetailFrm').serialize();
                        jqDialog.close();
                        toastr.info("Processing...");

                        $.post('/Forecast/SaveForecastDetail/', form, function (res) {
                            if (res.IsError) {
                                toastr.clear();
                                toastr.error("Error processing.");
                                jqDialog.quickStatus(false, res.Description);
                            }
                            else {
                                toastr.clear();
                                toastr.success("Success processing.");
                                $('#ucEditForecast').html(res);
                                jqDialog.quickStatus(true, 'Forecast detail edit successfull.');
                            }
                        });
                    }
                }
            }
        });
    },

    linkInvoices: function (forecastHeaderID, period) {
        jqDialog.renderView({
            url: '/Forecast/ShowLinkInvoicesPartialView/',
            data: { forecastHeaderID: forecastHeaderID, period: period },
            title: 'Link Invoices',
            width: 800,
            buttons: {
                Save: function () {
                    var form = $('#LinkInvoicesFrm').serialize();
                    jqDialog.close();
                    toastr.info("Processing...");

                    var forecastInvoiceIDs = [];

                    $('.forecastInvoiceIDs:checked').each(function () {
                        forecastInvoiceIDs.push($(this).attr('id'));
                    });

                    $.post('/Forecast/LinkInvoices/?selectedInvoices=' + forecastInvoiceIDs, form, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            toastr.success("Success processing.");
                            $('#ucIndex').html(res);
                            jqDialog.quickStatus(true, 'Invoices successfully linked.');
                        }
                    });
                }
            }
        });
    },

    expand: function (forecastHeaderID) {
        $('.ShowDetailedHistoryHeader' + forecastHeaderID).each(function () {
            $(this).toggleClass("hide");
        });

        $('.ShowDetailedHistoryLines' + forecastHeaderID).each(function () {
            $(this).toggleClass("hide");
        });
        return false;
    },

    showLinkedInvoices: function (forecastInvoiceHeaderID) {
        $('.ShowInvoiceDetailLines' + forecastInvoiceHeaderID
        ).each(function () {
            $(this).toggleClass("hide");
        });

        return false;
    },

    createDefaultReference: function () {
        jqDialog.renderView({
            url: '/Forecast/ShowCreateDefaultReferencePartialView/',
            data: null,
            title: 'Create Default Reference',
            buttons: {
                Save: function () {
                    var form = $('#CreateDefaultReferenceFrm').serialize();
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Forecast/CreateDefaultReference/', form, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            toastr.success("Success processing.");
                            $('#ucIndex').html(res);
                            jqDialog.quickStatus(true, 'Default Reference sucessfully created.');
                        }
                    });
                }
            }
        });
    },

    viewDefaultReferences: function () {
        jqDialog.renderView({
            url: '/Forecast/ShowViewDefaultReferencesPartialView/',
            data: null,
            title: 'View Default References',
            buttons: {
                Create: function () {
                    Forecast.createDefaultReference();
                }
            }
        });
    },

    billingMethodChanged: function () {
        var billingMethodID = $('#BillingMethod').val();

        if (billingMethodID === 1) {
            $('.NonTM').toggleClass("hide");
            $('.TM').toggleClass("hide");
        }
        else {
            $('.TM').toggleClass("hide");
            $('.NonTM').toggleClass("hide");
            $('.NonTM').toggleClass("show", false);
        }
        return false;
    },

    recalculateForecastAmount: function () {
        var hourlyRate = $('#ForecastHourlyRate').val();
        hourlyRate = NumberFunctions.formatValueToNumberString(hourlyRate);
        hourlyRate = parseFloat(hourlyRate);
        if (hourlyRate === 'NaN') {
            hourlyRate = 1;
        }

        var allocationPercentage = $('#ForecastAllocationPercentage').val();
        allocationPercentage = NumberFunctions.formatValueToNumberString(allocationPercentage);
        allocationPercentage = parseFloat(allocationPercentage);
        if (allocationPercentage === 'NaN') {
            allocationPercentage = 1;
        }

        var workableDays = $('#ForecastWorkableDays').val();
        workableDays = NumberFunctions.formatValueToNumberString(workableDays);
        workableDays = parseInt(workableDays);
        if (workableDays === 'NaN') {
            workableDays = 1;
        }

        var calculatedAmount = hourlyRate * allocationPercentage * workableDays * 7;
        if (calculatedAmount > 0) {
            $('#TMForecastAmount').val(NumberFunctions.toDoubleString(calculatedAmount));
        }
        Forecast.useDefaultReference();
        return false;
    },

    useDefaultReference: function () {
        var defaultReference = $('#DefaultReference').val();
        $('#Reference').val(defaultReference);
        return false;
    },

    setAutoComplete: function () {
        $("#Client").autocomplete({
            source: "/Forecast/Lookup/",
            minLength: 1,
            select: function (event, ui) {
                ui.item ? $("#clientIDs").val(ui.item.id) : $("#clientIDs").val("");
            }
        });
    },

    setAutoCompleteProject: function () {
        $("#Project").autocomplete({
            source: "/Forecast/LookupProject/?client=" + $("#Client").val(),
            minLength: 1,
            select: function (event, ui) {
                ui.item ? $("#projectIDs").val(ui.item.id) : $("#projectIDs").val("");
            }
        });
        Forecast.setAutoCompleteProject();
    },

    setAutoCompleteUser: function () {
        $("#HeaderResource").autocomplete({
            source: "/Forecast/LookupUser/",
            minLength: 1,
            select: function (event, ui) {
                ui.item ? $("#HeaderResourceIDs").val(ui.item.id) : $("#HeaderResourceIDs").val("");
            }
        });
    },

    refreshInvoiceList: function () {
        var period = $('#MonthList').val();
        var forecastHeaderID = $('#ForecastHeaderID').val();
        Forecast.linkInvoices(forecastHeaderID, period);
    },

    reloadCreateForm: function () {
        var yearMonth = $('#MonthList').val();
        var region = $('#Region').val();
        var division = $('#Division').val();
        var billingMethod = $('#BillingMethod').val();
        var invoiceCategory = $('#InvoiceCategory').val();
        var client = $('#Client').val();
        var project = $('#Project').val();
        var probability = $('#Probability').val();
        var remark = $('#Remark').val();

        window.location = '/Forecast/CreateForecast?yearMonth=' + yearMonth + '&region=' + region + '&division=' + division + '&billingMethod=' + billingMethod + '&invoiceCategory=' + invoiceCategory + '&probability=' + probability + '&client=' + client + '&project=' + project + '&remark=' + remark;
    },

    reloadEditForm: function () {
        var forecastHeaderID = $('#ForecastHeaderID').val();
        var yearMonth = $('#MonthList').val();

        window.location = '/Forecast/EditForecast?forecastHeaderID=' + forecastHeaderID + '&mostRecentDetailID=0' + '&yearMonth=' + yearMonth;
    },

    selectReferences: function (references) {
        jqDialog.renderView({
            url: '/Forecast/SelectReferences/',
            data: { references: references },
            title: 'Select References',
            buttons: {
                Select: function () {
                    var referenceIDs = [];
                    referenceIDs.push($('#References').val());

                    var form = $('#frmForeCastSearch').serialize();
                    $('#SelectedReferences').val(referenceIDs.toString());
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Forecast/SetHistoryItemReferences/', { references: referenceIDs.toString() }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucIndex').html(res);
                        }
                    });
                }
            }
        });
    },

    selectClients: function (clients) {
        jqDialog.renderView({
            url: '/Forecast/SelectClients/',
            data: { clients: clients },
            title: 'Select Clients',
            buttons: {
                Select: function () {
                    var clientIDs = [];
                    clientIDs.push($('#Clients').val());

                    var form = $('#frmForeCastSearch').serialize();
                    $('#SelectedClients').val(clientIDs.toString());
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Forecast/SetHistoryItemClients/', { clients: clientIDs.toString() }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#ucIndex').html(res);
                        }
                    });
                }
            }
        });
    }
};