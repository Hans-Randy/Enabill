var threadID = 0;

$(function () {
    View.setup();
});

var View = {
    setup: function () {
        threadID = $('#ThreadID').val();
    }
};

var Feedback = {
    view: function (feedbackThreadID) {
        $.post('/Feedback/QuickView/', { feedbackThreadID: feedbackThreadID }, function (res) {
            $('#FeedbackThreadTD').html(res);
            View.setup();
        });
    },

    submitFeedbackPost: function () {
        CKEditorSetup.CKupdate();

        if (threadID === 0) //new
        {
            if ($('#Subject').val().length <= 0) {
                jqDialog.quickStatus(false, 'The thread requires a subject.');
                return;
            }

            if ($('#Subject').val().length > 0 && $('#Subject').val().length < 3) {
                jqDialog.quickStatus(false, 'The thread subject must be 3 or more characters long.');
                return;
            }

            if ($('#Subject').val().length > 64) {
                jqDialog.quickStatus(false, 'The thread subject must less than 64 characters long.');
                return;
            }
        }

        if ($('#PostText').val().length <= 0) {
            jqDialog.quickStatus(false, 'The feedback cannot be blank.');
            return;
        }

        jqDialog.confirm({
            message: 'Would you like to submit this feedback post',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    if (threadID <= 0) {
                        $.post('/Feedback/SubmitNewThread/', $('#FeedbackFrm').serialize(), function (res) {
                            if (res.IsError) {
                                toastr.clear();
                                toastr.error("Error processing.");

                                jqDialog.quickStatus(false, res.Description);
                            }
                            else {
                                toastr.clear();
                                $('#ucIndex').html(res);
                                View.setup();
                            }
                        });
                    }
                    else {
                        var postText = $('#PostText').val();

                        $.post('/Feedback/SubmitThreadPost/', { feedbackThreadID: threadID, postText: postText }, function (res) {
                            if (res.IsError) {
                                toastr.clear();
                                toastr.error("Error processing.");

                                jqDialog.quickStatus(false, res.Description);
                            }
                            else {
                                toastr.clear();
                                $('#ucIndex').html(res);
                                View.setup();
                            }
                        });
                    }
                }
            }
        });
    },

    closeThread: function () {
        jqDialog.confirm({
            message: 'Do you want to close this thread?',
            buttons: {
                Yes: function () {
                    jqDialog.close();
                    toastr.info("Processing...");

                    $.post('/Feedback/CloseThread/', { threadID: threadID }, function (res) {
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