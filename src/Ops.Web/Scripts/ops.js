window.onunload = function () {
    $(".btn-spinner, .btn-spinner-no-validate").removeClass("disabled");
    $(".btn-spinner, .btn-spinner-no-validate").children(".fa-spinner").addClass("d-none");
}

$(".btn-spinner").on("click", function (e) {
    if ($(this).parents("form:first").valid()) {
        if ($(this).hasClass("disabled")) {
            e.preventDefault();
        }
        else {
            $(this).addClass("disabled");
            $(this).children(".fa-spinner").removeClass("d-none");
        }
    }
});

$(".btn-spinner-no-validate").on("click", function (e) {
    if ($(this).hasClass("disabled")) {
        e.preventDefault();
    }
    else {
        $(this).addClass("disabled");
        $(this).children(".fa-spinner").removeClass("d-none");
    }
});

$(document).on('change', ':file', function () {
    var input = $(this),
        numFiles = input.get(0).files ? input.get(0).files.length : 1,
        label = input.val().replace(/\\/g, '/').replace(/.*\//, '');
    input.trigger('fileselect', [numFiles, label]);
});

$(':file').on('fileselect', function (evkent, numFiles, label) {
    var input = $(this).parents('.input-group').find(':text'),
        log = numFiles > 1 ? numFiles = ' files selected' : label;

    if (input.length && label.length) {
        input.val(log);
        $('.btn-file').removeClass('btn-outline-secondary');
        $('.btn-file').addClass('btn-success');
    }
    else {
        input.val('');
        $('.btn-file').addClass('btn-outline-secondary');
        $('.btn-file').removeClass('btn-success');
    }
});
