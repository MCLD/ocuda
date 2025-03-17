function ResetSpinners(target) {
    if (target != null) {
        target.removeClass("disabled");
        target.children(".fa-spinner").addClass("d-none");
    }
    else {
        $(".btn-spinner, .btn-spinner-no-validate").removeClass("disabled");
        $(".btn-spinner, .btn-spinner-no-validate").children(".fa-spinner").addClass("d-none");
    }
}

window.onunload = function () {
    ResetSpinners();
};

$(".btn-spinner").on("click", function (e) {
    var parentForm = $(this).parents("form:first");
    if (parentForm.length == 0 || parentForm.valid()) {
        if ($(this).hasClass("disabled")) {
            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();
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

function copyToClipboard(element) {
    var $tempCopy = $("<input>");
    $("body").append($tempCopy);
    $tempCopy.val($(element).text()).select();
    document.execCommand("copy");
    $tempCopy.remove();
}
