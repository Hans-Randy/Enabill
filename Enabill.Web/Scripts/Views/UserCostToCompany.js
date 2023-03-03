var WorkDay = '';

$(function () {
    View.initialize();
});

var View = {
    initialize: function () {
        WorkDay = $('#MonthDate').val();
    },

    skipToMonth: function (numOfMonthToAdd) {
        var beginDate = new Date(WorkDay);
        var month = (parseInt(beginDate.getMonth()) + parseInt(numOfMonthToAdd)) % 12;
        var year = (parseInt(beginDate.getMonth()) + parseInt(numOfMonthToAdd)) / 12;

        beginDate.setMonth(month);

        // code is used to prevent errors when scrolling backward from january of one year to december of the previous year.
        if (numOfMonthToAdd >= 0) {
            beginDate.setFullYear(parseInt(beginDate.getFullYear()) + year);
        }

        beginDate.setDate(1);
        View.showMonth(beginDate);
    },

    showMonth: function (selectedMonth) {
        toastr.info("Processing...");

        var date = new Date(selectedMonth);
        $.post('/UserCostToCompany/Month/', { date: date.toDateString() }, function (res) {
            if (res === -1) {
                window.location = '/Home/Passphrase?cont=UserCostToCompany';
            }

            if (res.IsError) {
                toastr.clear();
                toastr.error("Error processing.");
                jqDialog.quickStatus(false, res.Description);
            }
            else {
                toastr.clear();
                $("#ucIndex").html(res);
                View.initialize();
            }
        });
    }
};