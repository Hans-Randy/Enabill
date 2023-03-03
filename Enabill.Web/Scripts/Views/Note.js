var calledFrom = "/Note/Index/";

$(function () {
});

var Note = {
    editNote: function (workAllocationID, noteID) {
        jqDialog.renderView({
            url: '/Note/QuickEditView/',
            data: { workAllocationID: workAllocationID },
            width: 1000,
            title: 'Note',
            buttons: {
                Save: function () {
                    CKEditorSetup.CKupdate();
                    var noteText = $('#NoteText').val();

                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Note/QuickEdit/', { workAllocationID: workAllocationID, noteText: noteText, calledFrom: calledFrom }, function (res) {
                        if (res.IsError) {
                            toastr.clear();
                            toastr.error("Error processing.");
                            jqDialog.quickStatus(false, res.Description);
                        }
                        else {
                            toastr.clear();
                            $('#Note' + noteID).html(res);
                        }
                    });
                }
            }
        });
    }
};

var NoteSearch = {
    search: function () {
        toastr.info("Processing...");
        jqDialog.wait();
        NoteSearch.prepostUpdate();

        $.post('/Note/Index/', $('#NoteSearch').serialize(), function (res) {
            jqDialog.close();
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $('#NoteList').html(res);
            }
        });
    },

    prepostUpdate: function () {
        var activityIDs = [];
        $('.activitySelect:checked').each(function () {
            activityIDs.push($(this).val());
        });
        $('#ActivityList').val(activityIDs);

        var userIDs = [];
        $('.userSelect:checked').each(function () {
            userIDs.push($(this).val());
        });
        $('#UserList').val(userIDs);
    },

    selectUsers: function () {
        $('.userSelect').attr("checked", "checked");
    },

    deselectUsers: function () {
        $('.userSelect').removeAttr("checked");
    },

    toggleClientActivities: function (clientID) {
        if ($('.c' + clientID + ':checked').length === $('.c' + clientID).length) {
            $('.c' + clientID).removeAttr("checked");
        }
        else {
            $('.c' + clientID).attr("checked", "checked");
        }
    },

    toggleProjectActivities: function (projectID) {
        if ($('.p' + projectID + ':checked').length === $('.p' + projectID).length) {
            $('.p' + projectID).removeAttr("checked");
        }
        else {
            $('.p' + projectID).attr("checked", "checked");
        }
    }
};