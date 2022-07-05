/**
 * localcrop.js - crop an image file in the local browser prior to upload
 *
 * Copyright (c) 2022 Maricopa County Library District - MIT License
 * https://github.com/MCLD/localcrop.js
 *
 * requires: smartcrop.js - v2.0.5 - https://github.com/jwagner/smartcrop.js/
 * requires: cropperjs - v1.5.12 - https://github.com/fengyuanchen/cropperjs/
 */

// defaults in case values are not specified
const cropDefaultMaxWidth = 500;
const cropDefaultMaxHeight = 500;
const cropDefaultMaxDisplayDimension = 1000;

// global cropperjs object
var cropCropper;

function initCrop(parameters) {
    if (!parameters.uploadControl) {
        alert("The cropping tool cannot be used without an associated upload control.");
        return;
    }

    if (!parameters.previewCanvas) {
        alert("The cropping tool cannot be used without an associated preview canvas.");
        return;
    }

    let cropCanvas;
    if (parameters.cropCanvas) {
        cropCanvas = parameters.cropCanvas;
    } else {
        cropCanvas = document.createElement("canvas");
    }

    parameters.uploadControl.addEventListener("change", function (fileChangeEvent) {
        if (fileChangeEvent.target.files) {
            if (fileChangeEvent.target.files.length > 1) {
                alert("Please only select one file.");
                return;
            }
            let maxWidth = cropDefaultMaxWidth;
            let maxHeight = cropDefaultMaxHeight;
            let maxDisplayDimension = cropDefaultMaxDisplayDimension;

            if (parameters.cropWidth) {
                maxWidth = parameters.cropWidth;
            }
            if (parameters.cropHeight) {
                maxHeight = parameters.cropHeight;
            }
            if (parameters.displayDimension) {
                maxDisplayDimension = parameters.displayDimension;
            }

            let imageFile = fileChangeEvent.target.files[0];
            let imageFileType = imageFile.type;
            let reader = new FileReader();

            let baseImage = document.createElement("img");

            reader.onload = function (readerLoadEvent) {
                baseImage.onload = function (imgLoadEvent) {
                    let factor = 1;
                    if (cropCropper) {
                        cropCropper.destroy();
                        cropCropper = null;
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

                    parameters.previewCanvas.width = width;
                    parameters.previewCanvas.height = height;
                    parameters.previewCanvas
                        .getContext("2d")
                        .drawImage(imgLoadEvent.target, 0, 0, width, height);

                    let cropWidthRequested = Math.min(width, maxWidth);
                    let cropHeightRequested = Math.min(height, maxHeight);

                    if (maxWidth == maxHeight) {
                        cropWidthRequested = Math.max(cropWidthRequested, cropHeightRequested);
                        cropHeightRequested = cropWidthRequested;
                    }

                    if (parameters.onCropLoad) {
                        parameters.onCropLoad(
                            imgLoadEvent.target.width,
                            imgLoadEvent.target.height,
                            maxWidth,
                            maxHeight,
                            cropWidthRequested,
                            cropHeightRequested
                        );
                    }

                    cropCanvas.getContext("2d").imageSmoothingEnabled = true;
                    cropCanvas.getContext("2d").imageSmoothingQuality = "high";
                    cropCanvas.width = cropWidthRequested;
                    cropCanvas.height = cropHeightRequested;

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

                            cropCropper = new Cropper(parameters.previewCanvas, {
                                aspectRatio: cropHeight / cropWidth,
                                viewMode: 3,
                                dragMode: "move",
                                preview: cropCanvas,
                                scalable: false,
                                zoomable: false,
                                responsive: false,
                                toggleDragModeOnDblclick: false,
                                crop: function (cropEvent) {
                                    let width = cropEvent.detail.width / factor;
                                    let height = cropEvent.detail.height / factor;
                                    cropCanvas
                                        .getContext("2d")
                                        .drawImage(
                                            baseImage,
                                            cropEvent.detail.x / factor,
                                            cropEvent.detail.y / factor,
                                            width,
                                            height,
                                            0,
                                            0,
                                            cropWidthRequested,
                                            cropHeightRequested
                                        );
                                    if (parameters.onCropResize) {
                                        let cropSuccess =
                                            Math.floor(width) < cropWidthRequested ||
                                            Math.floor(height) < cropHeightRequested;
                                        parameters.onCropResize(
                                            cropCanvas.toDataURL(imageFileType),
                                            cropSuccess,
                                            width,
                                            height
                                        );
                                    }
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
}
