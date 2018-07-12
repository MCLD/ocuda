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

$(document).on('fileselect', ':file', function (evkent, filePath) {
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

function updateStub(stub, text) {
    // From https://gist.github.com/mathewbyrne/1280286
    var slug = text.toLowerCase()
        .replace(/\s+/g, '-')
        .replace(/&/g, '-and-')
        .replace(/[^\w\-]+/g, '')
        .replace(/\-\-+/g, '-')
        .replace(/^-+/, '')
        .replace(/-+$/, '');
    stub.val(slug);
}

function validateStub(stub, stubCheckUrl) {
    var stubValidation = stub.parent().find('span');
    if (stub.val().trim() != "") {
        stub.val(stub.val().trim());

        stubValidation.removeClass("text-danger text-success");
        stubValidation.text("Checking stub availability...")

        $.post(stubCheckUrl, {
            stub: stub.val(),
            sectionId: $("#SectionId").val()
        }, function (response) {
            if (response) {
                stub.removeClass("valid");
                stub.addClass("input-validation-error");
                stubValidation.removeClass("field-validation-valid text-success");
                stubValidation.addClass("field-validation-error");
                stubValidation.text("The stub is already in use.");
            }
            else {
                stub.removeClass("input-validation-error");
                stub.addClass("valid");
                stubValidation.removeClass("field-validation-error");
                stubValidation.addClass("field-validation-valid text-success");
                stubValidation.text("The stub is available.");
            }
        });
    }
    else {
        stub.removeClass("valid");
        stub.addClass("input-validation-error");
        stubValidation.removeClass("field-validation-valid text-success");
        stubValidation.addClass("field-validation-error");
        stubValidation.text("The Stub field is required.");
    }
}

function clearStubValidation(stub) {
    if (stub.val().trim() != "") {
        var stubValidation = stub.parent().find('span');
        stubValidation.text("");
    }
}

$.validator.setDefaults({
    ignore: ".validation-ignore"
});