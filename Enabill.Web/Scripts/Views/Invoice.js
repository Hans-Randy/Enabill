var InvoiceID = 0;
var ClientID = 0;
var InvoiceStatusType = 0;
var BillingMethodType = 0;
var ProjectID = 0;
var totalAmount = 0;
var deletedLines = '';
var contactIDs = [];

$(function () {
    InvoiceID = $('#InvoiceID').val();

    if ($('#InvoiceStatusID').length > 0) {
        InvoiceStatusType = $('#InvoiceStatusID').val();
    }

    if ($('#BillingMethodID').length > 0) {
        BillingMethodType = $('#BillingMethodID').val();
    }
});

$(document).ready(function () {
    $('#IsInternal').click(function () {
        if ($("#OrderNo").val() !== null || $("#OrderNo").val() !== '') {
            return true;
        }
    });

    $('#IsInternal').click(function () {
        if ($("#OrderNo").val() === null || $("#OrderNo").val() === '') {
            $("#OrderNo").val($(this).is(':checked') ? "No invoice needed internal." : "");
        }
        else
            if ($("#OrderNo").val() === "No invoice needed internal.") {
                $("#OrderNo").val("");
            }
    });

    $('#MyClients').click(function () {
        toastr.info("Processing...");
        window.location = "/Invoice/Index?myClients=" + $('#MyClients').prop('checked');
    });

    $("input[name='InvoiceStatusTypeToggle']").change(function () {
        console.log('checkbox clicked');
        var state = $(this).is(":checked");
        var invoiceStatusType = $(this).val();

        var checkBoxes = $("input[name='InvoiceStatusType']");

        checkBoxes.each(function () {
            console.log($(this).val());
            var isDisabled = $(this).prop("disabled");
            if (!isDisabled) {
                if ($(this).val().split('|')[1] === invoiceStatusType) {
                    $(this).prop("checked", state);
                }
            }
        });
    });
});

var InvoiceIndex = {
    invoiceStatusTypeToggleChange: function () {
        console.log('checkbox clicked');
        var state = $(this).is(":checked");
        var invoiceStatusType = $(this).val();

        var checkBoxes = $("input[name='InvoiceStatusType']");

        checkBoxes.each(function () {
            var isDisabled = $(this).prop("disabled");
            if (!isDisabled) {
                if ($(this).val().split('|')[1] === invoiceStatusType) {
                    $(this).prop("checked", state);
                }
            }
        });
    },

    search: function () {
        toastr.info("Processing...");

        $.post('/Invoice/Index/', $('#InvoiceIndexFrm').serialize(), function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $('#ucIndex').html(res);
                $("input[name='InvoiceStatusTypeToggle']").change(InvoiceIndex.invoiceStatusTypeToggleChange);
            }
        });
    },

    changeDateSelector: function (toChangeTo) {
        toastr.info("Processing...");

        $.post('/Invoice/ChangeDateSelector/', { toChangeTo: toChangeTo }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(true, res.Description);
            }
            else {
                if (toChangeTo === 0) {
                    $('#dateSelectorTH').html('<a href="#" onclick="InvoiceIndex.changeDateSelector(1); return false;">Date range</a>');
                    $('#dateRangeSelector').show();
                    $('#periodSelector').hide();
                }
                else if (toChangeTo === 1) {
                    $('#dateSelectorTH').html('<a href="#" onclick="InvoiceIndex.changeDateSelector(0); return false;">Period</a>');
                    $('#dateRangeSelector').hide();
                    $('#periodSelector').show();
                }

                toastr.clear();
            }
        });
    },

    moveInvoices: function () {
        var invoiceIds = [];
        var checkBoxes = $("input[name='InvoiceStatusType']");

        checkBoxes.each(function () {
            var isChecked = $(this).prop("checked");
            if (isChecked)
                invoiceIds.push($(this).val());
        });

        console.log(invoiceIds);

        toastr.info("Processing...");

        $.post('/Invoice/MoveInvoices/', { invoiceIds: invoiceIds }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(true, res.Description);
            }
            else {
                toastr.clear();
                $('#ucIndex').html(res);
                $("input[name='InvoiceStatusTypeToggle']").change(InvoiceIndex.invoiceStatusTypeToggleChange);
            }
        });
    }
};

var InvoiceWorkAllocations = {
    save: function () {
        var invoiceCreditAmount = NumberFunctions.toDouble($('#DirectInvoiceCredit').val());
        var hourlyRates = HourlyRate.getHourlyRates();
        var credits = InvoiceCredit.getCredits();
        var linkedWorkAllocations = WorkAllocations.getLinkedWorkAllocations();

        jqDialog.confirm({
            message: 'Would you like to save the changes made to the invoice\'s work allocations?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Invoice/SaveWorkAllocations/', { id: InvoiceID, invoiceCreditAmount: invoiceCreditAmount, hourlyRates: hourlyRates.toString(), linkedWorkAllocations: linkedWorkAllocations.toString(), credits: credits.toString() }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(!res.IsError, res.Description);
                        }
                        else {
                            toastr.clear();
                            jqDialog.quickStatus(true, "Update successful.");
                            $('#ucWorkAllocations').html(res);
                        }
                    });
                }
            }
        });
    },

    changeWorkAllocationLinkStatus: function (workAllocationID, activityName) {
        var toBeLinked = 0;
        toBeLinked = $('#Link__' + activityName + '--' + workAllocationID).attr('checked') === null ? 0 : 1;

        if (toBeLinked === 1) {
            InvoiceWorkAllocations.linkWorkAllocation(workAllocationID, activityName);
        }
        else if (toBeLinked === 0) {
            InvoiceWorkAllocations.unlinkWorkAllocation(workAllocationID, activityName);
        }
    },

    linkWorkAllocation: function (workAllocationID, activityName) {
        InvoiceCredit.updateCreditValues(workAllocationID, activityName, 0);
        InvoiceCredit.updateCreditOptions(workAllocationID, activityName);
        InvoiceCredit.updateNettAmount(workAllocationID, activityName);
        InvoiceCredit.updateTotals(activityName);
    },

    unlinkWorkAllocation: function (workAllocationID, activityName) {
        InvoiceCredit.updateCreditValues(workAllocationID, activityName, 0);
        InvoiceCredit.hideCreditOptions(workAllocationID, activityName);
        InvoiceCredit.updateNettAmount(workAllocationID, activityName);
        InvoiceCredit.updateTotals(activityName);
    },

    getWorkAllocationID: function (elem) {
        var workAllocationID = elem.attr('id');
        var length = workAllocationID.length;
        var index = workAllocationID.indexOf("--");

        workAllocationID = workAllocationID.substring(index + 2, length);
        return workAllocationID;
    },

    getActivityName: function (elem) {
        var activityName = elem.attr('id');
        var index1 = activityName.indexOf("__");
        var index2 = activityName.indexOf("--");

        activityName = activityName.substring(index1 + 2, index2 - (index1 + 2));
        return activityName;
    },

    selectAll: function () {
        var all = $('input.linked').length;
        var linked = $('input.linked:checked').length;

        if (all !== linked) {
            $('input.linked:not(input.linked:checked)').each(function () {
                $(this).prop('checked', "checked");
                InvoiceWorkAllocations.changeWorkAllocationLinkStatus(InvoiceWorkAllocations.getWorkAllocationID($(this)), InvoiceWorkAllocations.getActivityName($(this)));
            });
        }
        else {
            $('input.linked:checked').each(function () {
                $(this).removeAttr('checked');
                InvoiceWorkAllocations.changeWorkAllocationLinkStatus(InvoiceWorkAllocations.getWorkAllocationID($(this)), InvoiceWorkAllocations.getActivityName($(this)));
            });
        }
    }
};

var Invoice = {
    setupAdHocInvoice: function () {
        jqDialog.renderView({
            url: '/Invoice/SetupAdHocInvoice/',
            data: null,
            title: 'Create Ad Hoc invoice',
            buttons: {
                OK: function () {
                    var clientID = $('#ClientID').val();
                    var projectID = $('#ProjectID').val();
                    var contactID = $('#InvoiceContactID').val();

                    if (clientID === null || contactID === null) {
                        return;
                    }

                    jqDialog.wait();
                    window.location = '/Invoice/CreateAdHocInvoice?clientID=' + clientID + "&contactID=" + contactID + "&projectID=" + projectID;
                }
            }
        });
    },

    exportToCSV: function () {
        toastr.info("Processing...");

        var dateSelector = $('#dateSelectorTH').text();

        if (dateSelector === "Date range") {
            toastr.clear();

            jqDialog.quickStatus(false, "The Period date selector needs to be used for this functionality. It will default to the Period date selector now. Please type in a period.");
            $('#dateSelectorTH').html('<a href="#" onclick="InvoiceIndex.changeDateSelector(0); return false;">Period</a>');
            $('#dateRangeSelector').hide();
            $('#periodSelector').show();
        }
        else {
            var invoicePeriod = $("#InvoicePeriod").val();

            $.post('/Invoice/Export/', { period: invoicePeriod }, function (res) {
                if (res.IsError) {
                    toastr.clear();
                    toastr.error("Error processing.");

                    jqDialog.quickStatus(false, res.Description);
                }
                else {
                    toastr.clear();
                    jqDialog.quickStatus(true, "Email Successfully Sent.");
                }
            });
        }
    },

    getProjectsAndContacts: function () {
        var clientID = $('#ClientID').val();
        //Get Projects
        $.post('/Invoice/ProjectListLookup/', { clientID: clientID }, function (res) {
            EnabillViews.updateDropDownList($('#ProjectID'), res);
        });

        //Get Contacts
        $.post('/Invoice/ContactListLookup/', { clientID: clientID }, function (res) {
            EnabillViews.updateDropDownList($('#InvoiceContactID'), res);
        });
    },

    calculateFinalAmounts: function () {
        var exclAmount = $('#InvoiceAmountExclVAT').val();
        exclAmount = NumberFunctions.formatValueToNumberString(exclAmount);

        if (!NumberFunctions.isNumber(exclAmount)) {
            alert('Invalid Excl Amount value.');
            return;
        }

        exclAmount = parseFloat(exclAmount);

        var vatRate = $('#VatRate').val();
        var VatRateVal = "15";

        vatRate = NumberFunctions.formatValueToNumberString(vatRate);

        if (!NumberFunctions.isNumber(vatRate)) { $('#VatRate').val(VatRateVal); return; } //Forced the VatRate to be 15%.

        vatRate = parseFloat(vatRate);

        var vatAmount = vatRate > 0 ? exclAmount * (vatRate / 100.00) : 0;
        var inclAmount = exclAmount + vatAmount;

        $('#vatAmountDiv').html('R ' + NumberFunctions.toDoubleString(vatAmount));
        $('#inclAmountDiv').html('R ' + NumberFunctions.toDoubleString(inclAmount));
    },

    vatRateValidation: function () {
        var vatRate = $('#VatRate').val();
        var VatRateVal = "15";

        vatRate = NumberFunctions.formatValueToNumberString(vatRate);

        if (!NumberFunctions.isNumber(vatRate)) { $('#VatRate').val(VatRateVal); return; } //Forced the VatRate to be 15%.
    },

    useProvisionalNettAmount: function () {
        var nettAmount = $('#NettAmount').val();
        var accrualAmount = $('#AccrualAmount').val();
        $('#InvoiceAmountExclVAT').val(nettAmount);

        var type = $('#billingType').val();
        if (type === 'Time and Material' || type === 'SLA' || type === 'Ad Hoc' || type === 'Travel')
            accrualAmount = nettAmount;

        $('#AccrualAmountExclVAT').val(accrualAmount);

        Invoice.calculateFinalAmounts();

        Window.init();
    },

    save: function () {
        jqDialog.confirm({
            message: 'Any changes made will be saved. Save?',
            buttons: {
                Yes: function () {
                    var form = $('#InvoiceForm').serialize();

                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Invoice/Edit/', form, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#InvoiceDiv').html(res);
                            jqDialog.quickStatus(true, 'Invoice save successful');
                        }

                        if (BillingMethodType !== '16' && (InvoiceStatusType === '1' || InvoiceStatusType === '2')) // ADHOC = 16, Open = 1, In Progress = 2
                            bindDatePickers();
                    });
                }
            }
        });
    },

    quickSave: function () {
        var form = $('#InvoiceForm').serialize();
        var url = '/Invoice/Edit/';
        if (BillingMethodType === '16') // ADHOC = 16
            url = '/Invoice/EditAdHocInvoice/';

        $.post(url, form, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
                return false;
            }
        });

        sleep(2000); //2 seconds

        return true;
    },

    deleteInvoice: function () {
        jqDialog.confirm({
            message: "Would you like to delete this invoice?",
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Invoice/Delete/', { id: InvoiceID }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            window.location = "/Invoice/Index/";
                        }
                    });
                }
            }
        });
    },

    approveTime: function () {
        var selectedInvoicesToBeApproved = InvoicesToBeApproved.getselectedInvoicesToBeApproved();

        jqDialog.confirm({
            message: "Would you like to check for workalloction exceptions and update the Time approval status accordingly?",
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Invoice/ApproveTime/', { selectedInvoicesToBeApproved: selectedInvoicesToBeApproved.toString() }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            jqDialog.quickStatus(true, "Time approval statuses updated successfully.");
                            location.reload();
                        }
                    });
                }
            }
        });
    },

    moveToOpen: function () {
        jqDialog.confirm({
            message: "Would you like to move this invoice to the 'Open' status?",
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    contactIDs = [];
                    $(".Contacts:checked").each(function () {
                        contactIDs.push($(this).val());
                    });
                    $("#ContactList").val(contactIDs.toString());

                    if (Invoice.quickSave() === true) {
                        var isInternal = $('#IsInternal').is(':checked');
                        $.post('/Invoice/MoveToOpen/', $('#InvoiceForm').serialize(), function (res) {
                            if (res.IsError) {
                                toastr.clear();
                                toastr.error("Error processing.");

                                jqDialog.quickStatus(false, res.Description);
                            }
                            else {
                                $('#InvoiceDiv').html(res);
                                $(document).ready(function () {
                                    $('#IsInternal').click(function () {
                                        if ($("#OrderNo").val() !== null || $("#OrderNo").val() !== '') {
                                            toastr.clear();
                                            return true;
                                        }
                                    });
                                    $('#IsInternal').click(function () {
                                        if ($("#OrderNo").val() === null || $("#OrderNo").val() === '') {
                                            $("#OrderNo").val($(this).is(':checked') ? "No invoice needed internal." : "");
                                        }
                                        else
                                            if ($("#OrderNo").val() === "No invoice needed internal.") {
                                                $("#OrderNo").val("");
                                            }
                                    });

                                    bindDatePickers();
                                });

                                toastr.clear();
                                jqDialog.quickStatus(true, 'Invoice moved and saved successfully.');
                            }
                        });
                    }
                }
            }
        });
    },

    moveToInProgress: function () {
        var InvoiceIsTimeApproved = $('#InvoiceIsTimeApproved').val();
        if (InvoiceIsTimeApproved === 'True') {
            jqDialog.confirm({
                message: "Would you like to move this invoice to the 'In Progress' status?",
                buttons: {
                    Yes: function () {
                        jqDialog.close();
                        toastr.info("Processing...");

                        contactIDs = [];
                        $(".Contacts:checked").each(function () {
                            contactIDs.push($(this).val());
                        });

                        $("#ContactList").val(contactIDs.toString());

                        if (Invoice.quickSave() === true) {
                            $.post('/Invoice/MoveToInProgess/', $('#InvoiceForm').serialize(), function (res) {
                                if (res.IsError) {
                                    toastr.clear();
                                    toastr.error("Error processing.");

                                    jqDialog.quickStatus(false, res.Description);
                                }
                                else {
                                    $('#InvoiceDiv').html(res);
                                    $(document).ready(function () {
                                        $('#IsInternal').click(function () {
                                            if ($("#OrderNo").val() !== null || $("#OrderNo").val() !== '') {
                                                toastr.clear();
                                                return true;
                                            }
                                        });

                                        $('#IsInternal').click(function () {
                                            if ($("#OrderNo").val() === null || $("#OrderNo").val() === '') {
                                                $("#OrderNo").val($(this).is(':checked') ? "No invoice needed internal." : "");
                                            }
                                            else {
                                                if ($("#OrderNo").val() === "No invoice needed internal.") {
                                                    $("#OrderNo").val("");
                                                }
                                            }
                                        });

                                        bindDatePickers();
                                    });

                                    toastr.clear();
                                    jqDialog.quickStatus(true, 'Invoice moved and saved successfully.');
                                }
                            });
                        }
                    }
                }
            });
        }
        else {
            jqDialog.status({
                message: 'Cannot update Invoice Status. Not all time has been approved.',
                buttons: {
                    OK: function () {
                        jqDialog.close();
                    }
                }
            });
        }
    },

    moveToReady: function () {
        var InvoiceIsTimeApproved = $('#InvoiceIsTimeApproved').val();
        if (InvoiceIsTimeApproved === 'True') {
            jqDialog.confirm({
                message: "Would you like to move this invoice to the 'Ready' status?",
                buttons: {
                    Yes: function () {
                        jqDialog.close();
                        toastr.info("Processing...");

                        contactIDs = [];
                        $(".Contacts:checked").each(function () {
                            contactIDs.push($(this).val());
                        });
                        $("#ContactList").val(contactIDs.toString());

                        if (Invoice.quickSave() === true) {
                            $.post('/Invoice/MoveToReady/', $('#InvoiceForm').serialize(), function (res) {
                                if (res.IsError) {
                                    toastr.clear();
                                    toastr.error("Error processing.");

                                    jqDialog.quickStatus(false, res.Description);
                                }
                                else {
                                    $('#InvoiceDiv').html(res);
                                    $(document).ready(function () {
                                        $('#IsInternal').click(function () {
                                            if ($("#OrderNo").val() !== null || $("#OrderNo").val() !== '') {
                                                toastr.clear();
                                                return true;
                                            }
                                        });

                                        $('#IsInternal').click(function () {
                                            if ($("#OrderNo").val() === null || $("#OrderNo").val() === '') {
                                                $("#OrderNo").val($(this).is(':checked') ? "No invoice needed internal." : "");
                                            }
                                            else
                                                if ($("#OrderNo").val() === "No invoice needed internal.") {
                                                    $("#OrderNo").val("");
                                                }
                                        });
                                    });

                                    toastr.clear();
                                    jqDialog.quickStatus(true, 'Invoice moved and saved successfully.');
                                }
                            });
                        }
                    }
                }
            });
        }
        else {
            jqDialog.status({
                message: 'Cannot update Invoice Status. Not all time has been approved.',
                buttons: {
                    OK: function () {
                        jqDialog.close();
                    }
                }
            });
        }
    },

    moveToComplete: function () {
        var InvoiceIsTimeApproved = $('#InvoiceIsTimeApproved').val();
        if (InvoiceIsTimeApproved === 'True') {
            if (Invoice.validateComplete()) {
                jqDialog.confirm({
                    message: "Would you like to move this invoice to the 'Complete' status?",
                    buttons: {
                        Yes: function () {
                            jqDialog.close();
                            toastr.info("Processing...");

                            contactIDs = [];
                            $(".Contacts:checked").each(function () {
                                contactIDs.push($(this).val());
                            });
                            $("#ContactList").val(contactIDs.toString());

                            if (Invoice.quickSave() === true) {
                                var extNo = $('#ExternalInvoiceNo').val();
                                $.post('/Invoice/MoveToComplete/', { id: InvoiceID, extNo: extNo }, function (res) {
                                    if (res.IsError) {
                                        toastr.clear();
                                        toastr.error("Error processing.");

                                        jqDialog.quickStatus(false, res.Description);
                                    }
                                    else {
                                        toastr.clear();
                                        $('#InvoiceDiv').html(res);
                                        jqDialog.quickStatus(true, 'Invoice moved and saved successfully.');
                                    }
                                });
                            }
                        }
                    }
                });
            }
            else {
                jqDialog.quickStatus(false, 'You must complete the invoice with an External Reference number.');
            }
        }
        else {
            jqDialog.status({
                message: 'Cannot update Invoice Status. Not all time has been approved.',
                buttons: {
                    OK: function () {
                        jqDialog.close();
                    }
                }
            });
        }
    },

    validateComplete: function () {
        if ($('#ExternalInvoiceNo').val() === '') {
            return false;
        }

        return true;
    }
};

var AdHocInvoice = {
    save: function () {
        jqDialog.confirm({
            message: 'Would you like to save this ad hoc invoice?',
            buttons: {
                Yes: function () {
                    jqDialog.wait();

                    contactIDs = [];
                    $(".Contacts:checked").each(function () {
                        contactIDs.push($(this).val());
                    });
                    $("#ContactList").val(contactIDs.toString());

                    $.post('/Invoice/EditAdHocInvoice/', $('#InvoiceForm').serialize(), function (res) {
                        jqDialog.close();
                        toastr.info("Processing...");

                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            //InvoiceLine.setup();
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            jqDialog.status({
                                success: true,
                                message: res.Description,
                                buttons: {
                                    OK: function () {
                                        jqDialog.close();
                                        window.location = res.url;
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

var WorkAllocations = {
    getLinkedWorkAllocations: function () {
        var linkedItems = [];
        $('input.linked:checked').each(function () {
            var workAllocationID = InvoiceWorkAllocations.getWorkAllocationID($(this));
            linkedItems.push(workAllocationID);
        });

        return linkedItems;
    }
};

var InvoicesToBeApproved = {
    getselectedInvoicesToBeApproved: function () {
        var linkedItems = [];
        $('input.linked:checked').each(function () {
            var invoiceID = InvoicesToBeApproved.getInvoiceID($(this));
            linkedItems.push(invoiceID);
        });

        return linkedItems;
    },

    getInvoiceID: function (elem) {
        var invoiceID = elem.attr('id');
        var length = invoiceID.length;
        var index = invoiceID.indexOf("__");

        invoiceID = invoiceID.substring(index + 2, length);

        return invoiceID;
    }
};

var Note = {
    quickView: function (workAllocationID) {
        jqDialog.renderView({
            url: '/Note/QuickEditView/',
            data: { workAllocationID: workAllocationID },
            title: 'Note',
            width: 800,
            buttons: {
                Save: function () {
                    CKEditorSetup.CKupdate();
                    var noteText = $('#NoteText').val();
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Note/QuickEdit/', { workAllocationID: workAllocationID, noteText: noteText }, function (res) {
                        toastr.clear();
                        jqDialog.quickStatus(!res.IsError, res.Description);
                    });
                }
            }
        });
    }
};

InvoiceCredit = {
    addEditCredit: function (workAllocationID, activityName) {
        var hours = NumberFunctions.formatValueToNumberString($('#Hours__' + activityName + '--' + workAllocationID).val());
        var hourlyRate = NumberFunctions.formatValueToNumberString($('#HourlyRate__' + activityName + '--' + workAllocationID).val());
        var creditAmount = NumberFunctions.formatValueToNumberString($("#CreditAmount__" + activityName + '--' + workAllocationID).val());

        jqDialog.renderView({
            url: '/Invoice/AddEditCreditView/',
            data: { hours: hours, hourlyRate: hourlyRate, creditAmount: creditAmount },
            title: 'Manage Invoice Credit',
            buttons: {
                OK: function () {
                    var amt = $('#CreditedAmount').val();
                    jqDialog.close();

                    amt = NumberFunctions.formatValueToNumberString(amt);
                    var grossAmt = $('#GrossAmount__' + activityName + '--' + workAllocationID).val();

                    if (!parseFloat(amt)) {
                        jqDialog.quickStatus(false, "Not a valid credit amount.");
                    }
                    else if (parseFloat(amt) <= 0) {
                        jqDialog.quickStatus(false, "Credit Amount cannot be equal to or less than 0.");
                    }
                    else {
                        InvoiceCredit.updateCreditValues(workAllocationID, activityName, amt);
                        InvoiceCredit.updateCreditOptions(workAllocationID, activityName);
                        InvoiceCredit.updateNettAmount(workAllocationID, activityName);
                        InvoiceCredit.updateTotals(activityName);
                    }
                }
            }
        });

        ManageInvoiceCredit.initialize(hours, hourlyRate);
    },

    removeCredit: function (workAllocationID, activityName) {
        InvoiceCredit.updateCreditValues(workAllocationID, activityName, 0);
        InvoiceCredit.updateCreditOptions(workAllocationID, activityName);
        InvoiceCredit.updateNettAmount(workAllocationID, activityName);
        InvoiceCredit.updateTotals(activityName);
    },

    updateCreditValues: function (workAllocationID, activityName, amount) {
        var htmlString = '<input id="CreditAmount__' + activityName + '--' + workAllocationID + '" class="credits credits' + activityName + '" type="hidden" value="' + NumberFunctions.toDouble(amount) + '" />';

        if (amount > 0)
            htmlString += 'R ' + NumberFunctions.toDoubleString(amount);

        $('td#CreditAmountTD__' + activityName + '--' + workAllocationID).html(htmlString);
    },

    isWorkAllocationsCredited: function (workAllocationID, activityName) {
        var amt = $("#CreditAmount__" + activityName + '--' + workAllocationID).val();

        return parseFloat(amt) > parseFloat(0);
    },

    updateCreditOptions: function (workAllocationID, activityName) {
        var tdItems = '<img src="../../Content/Img/add_14.png")" class="point" height="14" width="14" alt="credit" onclick="InvoiceCredit.addEditCredit(' + workAllocationID + ',\'' + activityName + '\'); return false;" runat="server" />';

        if (InvoiceCredit.isWorkAllocationsCredited(workAllocationID, activityName)) {
            tdItems = '<img src="../../../Content/Img/edit_14.png" class="point" style="height: 14px; width: 14px; margin-bottom: 2px;" alt="Edit credit" onclick="InvoiceCredit.addEditCredit(' + workAllocationID + ',\'' + activityName + '\'); return false;" runat="server" />';
            tdItems += '<img src="../../../Content/Img/Delete.gif" class="point" height="14" width="14" alt="Remove credit" onclick="InvoiceCredit.removeCredit(' + workAllocationID + ',\'' + activityName + '\'); return false;" runat="server" />';
        }

        $('#CreditOptionsTD__' + activityName + '--' + workAllocationID).html(tdItems);
    },

    hideCreditOptions: function (workAllocationID, activityName) {
        $('#CreditOptionsTD__' + activityName + '--' + workAllocationID).html('');
    },

    updateNettAmount: function (workAllocationID, activityName) {
        var grossAmt = $('#GrossAmount__' + activityName + '--' + workAllocationID).val();
        var creditAmt = $('#CreditAmount__' + activityName + '--' + workAllocationID).val();
        var nettAmt = NumberFunctions.toDoubleString(grossAmt - creditAmt);

        $('#NettAmountTD__' + activityName + '--' + workAllocationID).text('R ' + nettAmt);

        if (parseFloat(nettAmt) < 0) {
            $('#NettAmountTD__' + activityName + '--' + workAllocationID).addClass('error');
        }
        else {
            $('#NettAmountTD__' + activityName + '--' + workAllocationID).removeClass('error');
        }
    },

    getCredits: function () {
        var credits = [];

        $('input.credits').each(function () {
            if ($(this).val() > 0) {
                //alert($(this).val());

                workAllocationID = InvoiceWorkAllocations.getWorkAllocationID($(this));
                credits.push(workAllocationID + '|' + $(this).val());
            }
        });

        return credits;
    },

    updateTotals: function (activityName) {
        var totalGross = 0;

        $('.grossAmount' + activityName).each(function () {
            totalGross += parseFloat($(this).val());
        });

        $('#TotalGross__' + activityName).html('R' + NumberFunctions.toDoubleString(totalGross));

        var totalCredits = 0;

        $('input.credits' + activityName).each(function () {
            if ($(this).val() > 0) {
                totalCredits += parseFloat($(this).val());
            }
        });

        var totalNett = parseFloat(totalGross) - parseFloat(totalCredits);

        $('#TotalCredit__' + activityName).text('R ' + NumberFunctions.toDoubleString(totalCredits));
        $('#TotalNett__' + activityName).text('R ' + NumberFunctions.toDoubleString(totalNett));
    }
};

var manageHourlyRate = 0;
var manageGrossAmount = 0;

var ManageInvoiceCredit = {
    initialize: function (hours, hourlyRate) {
        manageGrossAmount = hours * hourlyRate;
        manageHourlyRate = parseFloat(hourlyRate);

        var creditedAmount = $('#CreditedAmount').val();
        var creditHours = creditedAmount / hourlyRate;
        ManageInvoiceCredit.displayValues(creditHours, creditedAmount);
    },

    calculateForHoursCredited: function () {
        var creditHours = $('#CreditByHours').val();

        if (!parseFloat(creditHours)) {
            $('#CreditByHours').val('0.00');
            creditHours = 0;
        }

        var creditedAmount = creditHours * manageHourlyRate;

        ManageInvoiceCredit.displayValues(creditHours, creditedAmount);
    },

    setUpForNonHourCalculation: function () {
        var creditedAmount = $('#CreditedAmount').val();

        if (!parseFloat(creditedAmount)) {
            $('#CreditedAmount').val('0.00');
            creditedAmount = 0;
        }

        var creditHours = creditedAmount / manageHourlyRate;

        ManageInvoiceCredit.displayValues(creditHours, creditedAmount);
    },

    displayValues: function (creditHours, creditedAmount) {
        $('#CreditByHours').val(NumberFunctions.toDoubleString(creditHours));
        $('#CreditedAmount').val(NumberFunctions.toDoubleString(creditedAmount));
    }
};

var InvoiceText = {
};

var GLAccounts = {
    createGLAccount: function () {
        jqDialog.renderView({
            url: '/Invoice/CreateGLAccount/',
            title: 'Create GL Account',
            buttons: {
                Save: function () {
                    toastr.info("Processing...");

                    $.post('/Invoice/SaveGLAccount', $('#frmCreateGLAccount').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            if (res.Description === "Please enter valid Codes.") {
                                $("#GLAccountName").val("Please enter a valid GL Account Name").css("color", "red");
                                $("#GLAccountCode").val("Please enter a valid GL Account Code").css("color", "red");

                                $("#GLAccountName").keydown(function () {
                                    $("#GLAccountName").css("color", "black");
                                });

                                $("#GLAccountCode").keydown(function () {
                                    $("#GLAccountCode").css("color", "black");
                                });

                                return false;
                            }
                            else if (res.Description === "Please enter a valid GL Account Code.") {
                                $("#GLAccountCode").val(res.Description).css("color", "red");
                                $("#GLAccountCode").keydown(function () {
                                    $("#GLAccountCode").css("color", "black");
                                });
                                return false;
                            }
                            else if (res.Description === "Please enter a valid GL Account Name.") {
                                $("#GLAccountName").val(res.Description).css("color", "red");
                                $("#GLAccountName").keydown(function () {
                                    $("#GLAccountName").css("color", "black");
                                });
                            }
                            else {
                                jqDialog.close();
                                jqDialog.quickStatus(false, res.Description);
                            }
                        }
                        else {
                            toastr.clear();
                            jqDialog.close();
                            $('#ucGLAccountIndex').html(res);
                        }
                    });
                }
            }
        });
    },

    editGLAccount: function (GLAccountID) {
        jqDialog.renderView({
            url: '/Invoice/EditGLAccount/',
            data: { id: GLAccountID },
            title: 'Edit GL Account',
            buttons: {
                Save: function () {
                    toastr.info("Processing...");

                    $.post('/Invoice/SaveGLAccount', $('#frmEditGLAccount').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");

                            if (res.Description === "Please enter a valid GL Account Name.") {
                                $("#GLAccountName").val(res.Description).css("color", "red");
                                $("#GLAccountName").keydown(function () {
                                    $("#GLAccountName").css("color", "black");
                                });
                                return false;
                            }
                            jqDialog.close();
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            jqDialog.close();
                            $('#ucGLAccountIndex').html(res);
                        }
                    });
                }
            }
        });
    }
};

var HourlyRate = {
    edit: function (workAllocationID, activityName) {
        var currentAmount = $('#HourlyRate__' + activityName + '--' + workAllocationID).val();

        $('td#HourlyRateTD__' + activityName + '--' + workAllocationID).html(
            '<input type="text" id="EditHourlyRate__' + activityName + '--' + workAllocationID + '" class="requiresNumberFormatting right" style="width:80px;" value="' + currentAmount + '" onblur="HourlyRate.finishEditing(' + workAllocationID + ',\'' + activityName + '\'); return false;" />'
        );

        $('#EditHourlyRate__' + activityName + '--' + workAllocationID).focus();
        Window.init();
    },

    finishEditing: function (workAllocationID, activityName) {
        var newAmount = $('#EditHourlyRate__' + activityName + '--' + workAllocationID).val();

        $('td#HourlyRateTD__' + activityName + '--' + workAllocationID).html(
            '<input id="HourlyRate__' + activityName + '--' + workAllocationID + '" class="hourlyRates" type="hidden" value="' + NumberFunctions.toDouble(newAmount) + '" />' +
            'R ' + NumberFunctions.toDoubleString(newAmount)
        );

        HourlyRate.recalculateGrossAmount(workAllocationID, activityName);
        InvoiceCredit.updateNettAmount(workAllocationID, activityName);
        InvoiceCredit.updateTotals(activityName);
    },

    recalculateGrossAmount: function (workAllocationID, activityName) {
        var hours = $('#Hours__' + activityName + '--' + workAllocationID).val();
        var hourlyRate = $('#HourlyRate__' + activityName + '--' + workAllocationID).val();
        var grossAmount = parseFloat(hours * hourlyRate);

        $('td#GrossAmountTD__' + activityName + '--' + workAllocationID).html(
            '<input type="hidden" id="GrossAmount__' + activityName + '--' + workAllocationID + '" class="grossAmount" value="' + NumberFunctions.toDouble(grossAmount) + '" />' +
            'R ' + NumberFunctions.toDoubleString(grossAmount)
        );
    },

    getHourlyRates: function () {
        var hourlyRates = [];
        $('input.hourlyRates').each(function () {
            if ($(this).val() > 0) {
                workAllocationID = InvoiceWorkAllocations.getWorkAllocationID($(this));
                hourlyRates.push(workAllocationID + '|' + $(this).val());
            }
        });

        return hourlyRates;
    }
};

function bindDatePickers() {
    $('#DateFrom').datepicker({
        dateFormat: 'yy-mm-dd',
        maxDate: new Date($("#DateTo").val()),
        defaultDate: new Date($("#DateFrom").val()),
        onSelect: function (dateText, inst) {
            filterObj.dateFrom = dateText;
        },
        buttonImage: '/Content/Img/date_control.png',
        buttonImageOnly: true,
        showOn: "both"
    });

    $('#DateTo').datepicker({
        dateFormat: 'yy-mm-dd',
        defaultDate: new Date($("#DateTo").val()),
        onSelect: function (dateText, inst) {
            filterObj.dateTo = dateText;
        },
        buttonImage: '/Content/Img/date_control.png',
        buttonImageOnly: true,
        showOn: "both"
    });

    $('#InvoiceDate').datepicker({
        dateFormat: 'yy-mm-dd',
        defaultDate: new Date($("#DateTo").val()),
        onSelect: function (dateText, inst) {
            filterObj.dateTo = dateText;
        },
        buttonImage: '/Content/Img/date_control.png',
        buttonImageOnly: true,
        showOn: "both"
    });
}

$(function () {
    $('#InvoiceDate').change(function () {
        var dte = new Date($(this).val());
        var y = dte.getFullYear();
        var m = dte.getMonth() + 1;
        var prd = (y * 100) + m;
        $("#Period").val(prd);
    });
});

function sleep(milliseconds) {
    var start = new Date().getTime();
    for (var i = 0; i < 1e7; i++) {
        if ((new Date().getTime() - start) > milliseconds) {
            break;
        }
    }
}