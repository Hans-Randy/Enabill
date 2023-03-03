$(function () {
});

var Note = {
    view: function (workAllocationID) {
        jqDialog.renderView({
            url: '/Note/QuickEditView/',
            data: { workAllocationID: workAllocationID },
            width: 1000,
            title: 'Note',
            buttons: {
                Cancel: function () {
                    jqDialog.close();
                }
            }
        });
    }
};

var Leave = {
    approveLeave: function () {
        toastr.info("Processing...");

        var leaveIDs = [];

        $('.leaveIDs:checked').each(function () {
            leaveIDs.push($(this).attr('id'));
        });

        $.post('/Home/ApproveLeave/', { leaveIDs: leaveIDs.toString() }, function (res) {
            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");

                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                window.location = "/Home/Dashboard/";
            }
        });
    }
};

var IndexTables = {
    toggle: function (elem, tableToToggle) {
        CollapsableTable.toggle(elem);

        $.post('/Home/ToggleTable/', { tableToToggle: tableToToggle }, function (res) {
        });
    }
};