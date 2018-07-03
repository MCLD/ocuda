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
    var fileInput = $(this),
        filePath = fileInput.val().replace(/\\/g, '/').replace(/.*\//, '');
    fileInput.trigger('fileselect', filePath);
});

$(':file').on('fileselect', function (evkent, filePath) {
    var file = $(this)[0].files[0],
        fileData = new FormData(),
        fileDisplay = $(this).parents('.input-group').find(':text');

    fileData.append("fileName", file.name);
    fileData.append("fileSize", file.size);

    $.ajax({
        url: '/Admin/Files/ValidateFileBeforeUpload',
        type: 'POST',
        contentType: false,
        processData: false,
        data: fileData,
        async: true,
        success: function (result) {
            if (result == "Valid") {
                fileDisplay.val(filePath);
                $('.btn-file').removeClass('btn-outline-secondary');
                $('.btn-file').addClass('btn-success');
                return true;
            }
            else {
                $(this).val('');
                fileDisplay.val('');
                $('.btn-file').addClass('btn-outline-secondary');
                $('.btn-file').removeClass('btn-success');
                alert(result);
                return false;
            }
        },
        error: function (err) {
            alert(err.statusText);
            return false;
        }
    });
});