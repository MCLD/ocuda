window.onunload = function () {
    ResetSpinners();
}

function ResetSpinners() {
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

$(document).on('change', ':file', function (e) {
    var fileInput = $(this),
        filePath = fileInput.val().replace(/\\/g, '/').replace(/.*\//, '');

    if (fileInput.hasClass("btn-thumbnail")) {
        validateThumbnail(e, filePath);
    }
    else {
        validateFile(e, filePath);
    }

});

$(document).on('fileselect', ':file', function (e) {
    var fileInput = $(this),
        filePath = fileInput.val().replace(/\\/g, '/').replace(/.*\//, '');

    if (fileInput.hasClass("btn-thumbnail")) {
        validateThumbnail(e, filePath);
    }
    else {
        validateFile(e, filePath);
    }
});

function validateFile(e, filePath) {
    var file = $(e.target)[0].files[0],
        fileButton = e.target.parentElement,
        fileData = new FormData(),
        fileDisplay = $(e.target).parents('.input-group').find(':text'),
        fileNameField = $('#File_Name');

    fileData.append("fileName", file.name);
    fileData.append("fileSize", file.size);

    if ($("#AcceptedFileExtensions").length > 0) {
        fileData.append("fileExtensions", $("#AcceptedFileExtensions").val());
    }

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

                if (fileNameField.val().length == 0) {
                    fileNameField.val(file.name.split('.')[0]);
                }

                $(fileButton).removeClass('btn-outline-secondary');
                $(fileButton).addClass('btn-success');
                return true;
            }
            else {
                $(e.target).val('');
                fileDisplay.val('');
                $(fileButton).addClass('btn-outline-secondary');
                $(fileButton).removeClass('btn-success');
                alert(result);
                return false;
            }
        },
        error: function (err) {
            alert(err.statusText);
            return false;
        }
    });
}

function validateThumbnail(e, filePath) {
    var fileInput = $(e.target),
        file = fileInput[0].files[0],
        fileData = new FormData(),
        fileDisplay = fileInput.parents('.input-group').find(':text'),
        fileButton = fileInput.parents('.form-control'),
        fileIcon = fileButton.find('span'),
        img = new Image();

    img.src = window.URL.createObjectURL(file);

    img.onload = function () {
        var imgHeight = img.naturalHeight,
            imgWidth = img.naturalWidth;

        fileData.append("fileName", file.name);
        fileData.append("fileSize", file.size);
        fileData.append("imgHeight", imgHeight);
        fileData.append("imgWidth", imgWidth);

        $.ajax({
            url: '/Admin/Files/ValidateThumbnailBeforeUpload',
            type: 'POST',
            contentType: false,
            processData: false,
            data: fileData,
            async: true,
            success: function (result) {
                if (result == "Valid") {
                    fileDisplay.val(filePath);
                    fileButton.removeClass('btn-outline-secondary');
                    fileButton.addClass('btn-success');
                    fileIcon.removeClass('fa-file-medical');
                    fileIcon.addClass('fa-file-image');
                    return true;
                }
                else {
                    clearThumbnailInput(e);
                    alert(result);
                    return false;
                }
            },
            error: function (err) {
                clearThumbnailInput(e);
                alert(err.statusText);
                return false;
            }
        });
    }

    img.onerror = function () {
        clearThumbnailInput(e);
        alert("Error: Failed to load image.");
        return false;
    }
}

function clearThumbnailInput(e) {
    var fileInput = $(e.target),
        fileDisplay = fileInput.parents('.input-group').find(':text'),
        fileButton = fileInput.parents('.form-control'),
        fileIcon = fileButton.find('span');

    fileInput.val('');
    fileDisplay.val('');
    fileButton.addClass('btn-outline-secondary');
    fileButton.removeClass('btn-success');
    fileIcon.removeClass('fa-file-image');
    fileIcon.addClass('fa-file-medical');
}

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

function validateStub(stub, id, stubCheckUrl) {
    var stubValidation = stub.parent().find('span');
    var sectionId = $("#SectionId");
    if (stub.val().trim() != "") {
        stub.val(stub.val().trim());

        stubValidation.removeClass("text-danger text-success");
        stubValidation.text("Checking stub availability...")

        $.post(stubCheckUrl, {
            item: {
                id: id.val(),
                stub: stub.val(),
                sectionId: sectionId.val()
            }
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

function removeAttachmentItem(url, id) {
    $.post(url, { id: id }, function (response) {
        if (response.success == true) {
            var attachmentItemId = "#attachmentItem_" + id;
            $(attachmentItemId).remove();

            var itemCount = $("#attachmentItemList").children().length;
            if (itemCount < 1) {
                $("#attachmentsInputRow").remove();
            }
        }
        else {
            alert(response.message);
        }
    });
}