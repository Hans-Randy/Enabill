$(function () {
    windowHeight = $(window).height();
    windowWidth = $(window).width();

    Layout.init();
});


var Layout = {

    init: function () {

        $('body').css({ 'height': windowHeight });

        $('#login_bg').height('100%');
        $('#login_bg').width('auto');

        $('.errormodel').css({ 'margin-top': -172.5 - $('.errormodel').height() - 10 + 'px' });

        if ($('#login_bg').outerWidth() > windowWidth) {
            $('#login_bg').height('auto');
            $('#login_bg').width('90%');
        }
    }
};