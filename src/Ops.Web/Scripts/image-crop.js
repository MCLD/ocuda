// override with a defined variable: cropWidth
const defaultMaxWidth = 700;
// override with a defined variable: cropHeight
const defaultMaxHeight = 700;
// override with a defined variable: cropDisplayDimension
const defaultMaxDisplayDimension = 1000;

var cropWidth;
var cropHeight;
var cropDisplayDimension;
var cropDataUrl;

const baseImage = document.getElementById("baseImage");
const cropCanvas = document.getElementById("cropCanvas");
const previewCanvas = document.getElementById("previewCanvas");

var cropper;

if (!document.getElementById("uploadedImage") || !baseImage) {
    alert("Not able to use image cropping with no uploadedImage and baseImage page elemnts.");
} else {
    document
        .getElementById("uploadedImage")
        .addEventListener("change", function (fileChangeEvent) {
            if (fileChangeEvent.target.files) {
                if (fileChangeEvent.target.files.length > 1) {
                    alert("Please only select one file.");
                    return;
                }
                let maxWidth = defaultMaxWidth;
                let maxHeight = defaultMaxHeight;
                let maxDisplayDimension = defaultMaxDisplayDimension;

                if (cropWidth) {
                    maxWidth = cropWidth;
                }
                if (cropHeight) {
                    maxHeight = cropHeight;
                }
                if (cropDisplayDimension) {
                    maxDisplayDimension = cropDisplayDimension;
                }

                let imageFile = fileChangeEvent.target.files[0];
                let imageFileType = imageFile.type;
                let reader = new FileReader();
                reader.onload = function (readerLoadEvent) {
                    baseImage.onload = function (imgLoadEvent) {
                        let factor = 1;
                        if (cropper) {
                            cropper.destroy();
                            cropper = null;
                        }

                        let longer =
                            imgLoadEvent.target.width < imgLoadEvent.target.height
                                ? imgLoadEvent.target.height
                                : imgLoadEvent.target.width;

                        if (longer > maxDisplayDimension) {
                            factor = maxDisplayDimension / longer;
                            document.getElementById("previewContainer").style.maxWidth =
                                imgLoadEvent.target.width * factor + "px";
                            document.getElementById("previewContainer").style.maxHeight = "";
                        } else {
                            document.getElementById("previewContainer").style.maxWidth =
                                imgLoadEvent.target.width + "px";
                            document.getElementById("previewContainer").style.maxHeight =
                                imgLoadEvent.target.height + "px";
                        }

                        let width = imgLoadEvent.target.width * factor;
                        let height = imgLoadEvent.target.height * factor;

                        previewCanvas.width = width;
                        previewCanvas.height = height;
                        previewCanvas
                            .getContext("2d")
                            .drawImage(imgLoadEvent.target, 0, 0, width, height);

                        let cropWidthRequested = Math.min(width, maxWidth);
                        let cropHeightRequested = Math.min(height, maxHeight);

                        if (maxWidth == maxHeight) {
                            cropWidthRequested = Math.max(cropWidthRequested, cropHeightRequested);
                            cropHeightRequested = cropWidthRequested;
                        }

                        if (document.getElementById("baseDetails")) {
                            let details = [];
                            details.push("Original image size: ");
                            details.push(imgLoadEvent.target.width);
                            details.push(" x ");
                            details.push(imgLoadEvent.target.height);
                            details.push(" px");
                            document.getElementById("baseDetails").innerHTML = details.join("");
                        }

                        if (document.getElementById("cropInfo")) {
                            let info = [];
                            if (cropWidthRequested != maxWidth || cropHeightRequested != maxHeight) {
                                info.push("Minimum possible crop: ");
                                info.push(cropWidthRequested);
                                info.push(" x ");
                                info.push(cropHeightRequested);
                                info.push(" px<br />");
                            }
                            info.push("Requested minimum crop: ");
                            info.push(maxWidth);
                            info.push(" x ");
                            info.push(maxHeight);
                            info.push(" px");
                            document.getElementById("cropInfo").innerHTML = info.join("");
                        }

                        if (cropCanvas) {
                            cropCanvas.getContext("2d").imageSmoothingEnabled = true;
                            cropCanvas.getContext("2d").imageSmoothingQuality = "high";
                            cropCanvas.width = cropWidthRequested;
                            cropCanvas.height = cropHeightRequested;
                        }

                        smartcrop
                            .crop(imgLoadEvent.target, {
                                width: cropWidthRequested,
                                height: cropHeightRequested,
                                ruleOfThirds: false,
                            })
                            .then(function (cropResult) {
                                let cropX = parseInt(cropResult.topCrop.x);
                                let cropY = parseInt(cropResult.topCrop.y);
                                let cropWidth = parseInt(cropResult.topCrop.width);
                                let cropHeight = parseInt(cropResult.topCrop.height);

                                cropper = new Cropper(previewCanvas, {
                                    aspectRatio: cropHeight / cropWidth,
                                    viewMode: 3,
                                    dragMode: "move",
                                    preview: cropCanvas,
                                    scalable: false,
                                    zoomable: false,
                                    responsive: false,
                                    toggleDragModeOnDblclick: false,
                                    crop: function(cropEvent) {
                                        redrawCrop({
                                            x: cropEvent.detail.x / factor,
                                            y: cropEvent.detail.y / factor,
                                            width: cropEvent.detail.width / factor,
                                            height: cropEvent.detail.height / factor,
                                            imageType: imageFileType,
                                            cropWidthRequested: cropWidthRequested,
                                            cropHeightRequested,
                                            cropHeightRequested,
                                        });
                                    },
                                    data: {
                                        x: cropX * factor,
                                        y: cropY * factor,
                                        width: cropWidth * factor,
                                        height: cropHeight * factor,
                                    },
                                });
                            });
                    };
                    baseImage.src = readerLoadEvent.target.result;
                };
                reader.readAsDataURL(imageFile);
            }
        });

    function redrawCrop(redrawOptions) {
        if (cropCanvas) {
            cropCanvas
                .getContext("2d")
                .drawImage(
                    baseImage,
                    redrawOptions.x,
                    redrawOptions.y,
                    redrawOptions.width,
                    redrawOptions.height,
                    0,
                    0,
                    redrawOptions.cropWidthRequested,
                    redrawOptions.cropHeightRequested
                );
            // this is the image data we want to upload
            cropDataUrl = cropCanvas.toDataURL(redrawOptions.imageType);
        }
        if (document.getElementById("cropDetails")) {
            let status = [];
            if (
                Math.floor(redrawOptions.width) < redrawOptions.cropWidthRequested ||
                Math.floor(redrawOptions.height) < redrawOptions.cropHeightRequested
            ) {
                status.push('<span style="color: red;">Crop is too small: ');
            } else {
                status.push("<span>Good crop, current size: ");
            }
            status.push(Math.floor(redrawOptions.width));
            status.push(" x ");
            status.push(Math.floor(redrawOptions.height));
            status.push(" px</span>");
            document.getElementById("cropDetails").innerHTML = status.join("");
        }
    }
}
