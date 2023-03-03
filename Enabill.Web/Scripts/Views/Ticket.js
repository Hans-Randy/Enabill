var id = 0;
var ticketType = 0;

$(function () {
});

var Ticket = {
    viewTickets: function (clientID, statusID) {
        toastr.info("Processing...");

        ticketDateFrom = $('#DateFrom').val();
        ticketDateTo = $('#DateTo').val();
        ticketType = $('#TicketTypeFilter').val();
        if ($('#FilterBy').val() !== undefined) {
            filterBy = $('#FilterBy').val();
        }
        else {
            filterBy = 1;
        }

        $.post('/Ticket/ViewTickets/', { clientID: clientID, statusID: statusID, dateFrom: ticketDateFrom, dateTo: ticketDateTo, ticketType: ticketType, filterBy: filterBy }, function (res) {
            if (!res.IsError) {
                $('#ViewTicketsTD').html(res);
                id = $('#TicketID').val();
                Ticket.viewTicketLines(id);
            }

            toastr.clear();
        });
    },

    viewTicketLines: function (id) {
        $.post('/Ticket/ViewTicketLines/', { id: id }, function (res) {
            $('#TicketLinesTD').html(res);
        });
    },

    createTicket: function () {
        var message = "";
        jqDialog.renderView({
            url: '/Ticket/CreateTicket/',
            title: 'Create Ticket',
            width: 700,
            buttons: {
                Submit: function () {
                    message = Ticket.validateTicket(0);

                    if (message === '') {
                        jqDialog.close();
                        toastr.info("Processing...");

                        if ($('#TicketStatus').val() === 5) {
                            Ticket.closeTicket(id, postText, timeSpent);
                        }
                        else {
                            if (id === 0) {
                                $.post('/Ticket/SubmitNewTicket/', $('#frmNewTicket').serialize(), function (res) {
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
                            else {
                                var assignedUser = $('#AssignedUser').val();
                                var ticketStatus = $('#TicketStatus').val();
                                var postText = $('#PostText').val();
                                var ticketType = $('#TicketType').val();
                                var priority = $('#Priority').val();
                                if (priority.length <= 0) {
                                    priority = 0;
                                }
                                var effort = $('#Effort').val();
                                effort = 0;
                                //                              if (effort.length <= 0) {
                                //                                    effort = 0;
                                //                                }
                                alert(postText);
                                $.post('/Ticket/SubmitTicketLine/', { id: id, postText: postText, assignedUser: assignedUser, ticketStatus: ticketStatus, ticketType: ticketType, priority: priority, effort: effort }, function (res) {
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
                    }
                }
            }
        });
    },

    deleteTicket: function () {
        var message = "";
        jqDialog.renderView({
            url: '/Ticket/ShowDeleteTicketView/',
            title: 'Delete Ticket',
            width: 500,
            buttons: {
                Delete: function () {
                    toastr.info("Processing...");

                    $.post('/Ticket/DeleteTicket/', $('#frmDeleteTicket').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            toastr.success("Success processing.");
                            $('#ucIndex').html(res);
                            jqDialog.quickStatus(true, "Request completed successfully.");
                        }
                    });
                }
            }
        });
    },

    reactivateTicket: function () {
        var message = "";
        jqDialog.renderView({
            url: '/Ticket/ShowReactivateTicketView/',
            title: 'Reactivate Ticket',
            width: 500,
            buttons: {
                Reactivate: function () {
                    toastr.info("Processing...");

                    $.post('/Ticket/ReActivateTicket/', $('#frmReActivateTicket').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            toastr.success("Success processing.");
                            $('#ucIndex').html(res);
                            jqDialog.quickStatus(true, "Request completed successfully.");
                        }
                    });
                }
            }
        });
    },

    findTicket: function () {
        var message = "";
        jqDialog.renderView({
            url: '/Ticket/ShowFindTicketView/',
            title: 'Find Ticket(s)',
            width: 250,
            buttons: {
                Find: function () {
                    window.location = '/Ticket/FindTicket?ticketReference=' + $('#TicketToFind').val();
                }
            }
        });
    },

    refreshTickets: function () {
        ticketDateFrom = $('#DateFrom').val();
        ticketDateTo = $('#DateTo').val();
        ticketType = $('#TicketTypeFilter').val();
        filterBy = $('#FilterBy').val();

        $.post('/Ticket/RefreshTickets/', { dateFrom: ticketDateFrom, dateTo: ticketDateTo, ticketType: ticketType, filterBy: filterBy }, function (res) {
            $('#ucIndex').html(res);
        });
    },

    emailNotification: function (id) {
        var message = "";
        jqDialog.renderView({
            url: '/Ticket/ShowSendEmailNotificationView/',
            title: 'Send Email Notification',
            data: { id: id },
            width: 600,
            buttons: {
                Send: function () {
                    toastr.info("Processing...");

                    $.post('/Ticket/SendEmailNotification/', $('#frmEmailNotification').serialize(), function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            toastr.success("Success processing.");
                            $('#ucIndex').html(res);
                            jqDialog.quickStatus(true, "Email notification send.");
                        }
                    });
                }
            }
        });
    },

    validateTicket: function (selectedTicketID) {
        CKEditorSetup.CKupdate();

        id = selectedTicketID;
        var postText = $('#PostText').val();
        var errorMessage = "";

        if (id === 0) {
            if ($('#Subject').val().length <= 0) {
                errorMessage = 'The ticket requires a subject.';
                jqDialog.quickStatus(false, errorMessage);
                return errorMessage;
            }

            if ($('#Subject').val().length > 0 && $('#Subject').val().length < 3) {
                errorMessage = 'The ticket subject must be 3 or more characters long.';
                jqDialog.quickStatus(false, errorMessage);
                return errorMessage;
            }

            if ($('#Subject').val().length > 64) {
                errorMessage = 'The ticket subject must less than 64 characters long.';
                jqDialog.quickStatus(false, errorMessage);
                return errorMessage;
            }

            if ($('#TicketType').val() === 1) {
                errorMessage = 'Please select a TicketType.';
                jqDialog.quickStatus(false, errorMessage);
                return errorMessage;
            }

            if ($('#Project').val() === 0) {
                errorMessage = 'Please select a Project.';
                jqDialog.quickStatus(false, errorMessage);
                return errorMessage;
            }

            if ($('#AssignedUser').val() === 0) {
                errorMessage = 'Please assign the request to a User.';
                jqDialog.quickStatus(false, errorMessage);
                return errorMessage;
            }

            if ($('#Priority').val() === 0 || $('#Priority').val().length <= 0) {
                errorMessage = 'Please select the priority.';
                jqDialog.quickStatus(false, errorMessage);
                return errorMessage;
            }
        }

        if ($('#PostText').val().length <= 0) {
            errorMessage = 'The reply text cannot be blank.';
            jqDialog.quickStatus(false, errorMessage);

            return errorMessage;
        }

        return errorMessage;
    },

    submitTicketLine: function (selectedTicketID) {
        CKEditorSetup.CKupdate();

        id = selectedTicketID;
        var postText = $('#PostText').val();
        var timeSpent = $('#TimeSpent').val();

        if ($('#TicketSubject').val().length <= 0) {
            jqDialog.quickStatus(false, 'Ticket subject cannot be blank.');
            return false;
        }

        if ($('#PostText').val().length <= 0) {
            jqDialog.quickStatus(false, 'The reply text cannot be blank.');
            return false;
        }

        if ($('#TicketType').val() === 1) {
            errorMessage = 'Please select a TicketType.';
            jqDialog.quickStatus(false, errorMessage);
            return false;
        }

        if ($('#AssignedUser').val() === 0) {
            errorMessage = 'Please assign the request to a User.';
            jqDialog.quickStatus(false, errorMessage);
            return false;
        }

        if ($('#Project').val() === 0) {
            errorMessage = 'Please select a Project.';
            jqDialog.quickStatus(false, errorMessage);
            return false;
        }

        if ($('#TicketStatus').val() === 5) {
            jqDialog.confirm({
                message: 'Do you want to close this ticket?',
                buttons: {
                    Yes: function () {
                        jqDialog.close();
                        toastr.info("Processing...");

                        $.post('/Ticket/SubmitTicketLine/', $('#frmSubmitTicketLine').serialize(), function (res) {
                            if (res.IsError) {
                                toastr.clear();
                                toastr.error("Error processing.");
                                jqDialog.quickStatus(false, res.Description);
                            }
                            else {
                                toastr.clear();
                                $('#ucViewDetails').html(res);
                            }
                        });
                    }
                }
            });
        }
    },

    refreshProjects: function () {
        toastr.info("Processing...");

        var clientID = $('#Client').val();

        $.post('/Ticket/ProjectListLookup/', { clientID: clientID }, function (res) {
            EnabillViews.updateDropDownList($('#Project'), res);
            Ticket.setSubject();
            Ticket.refreshUsers();
        });

        toastr.clear();
    },

    setSubject: function () {
        var defaultSubject = $('#Client option:selected').text() + ':' + $('#Project option:selected').text();
        $('#Subject').val(defaultSubject);
        Ticket.refreshUsers();
    },

    refreshUsers: function () {
        var projectID = $('#Project').val();

        $.post('/Ticket/UserListLookup/', { projectID: projectID }, function (res) {
            EnabillViews.updateDropDownList($('#AssignedUser'), res);
        });
    }
};