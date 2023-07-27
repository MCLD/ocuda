#!/bin/sh

# How to run this:
# 1. Find a Linux system (Windows Docker path sharing is a mess)
# 2. Clone the repository
# 4. Run the following: docker run -it --rm -v `pwd`:/app node:latest bash /app/dev/docker-yarn-update.sh
# 5. See what happened with git status

# Ops
cd /app/src/Ops.Web && yarn install

mkdir -p /app/src/Ops.Web/js
rm -rf /app/src/Ops.Web/js/*.js

cp /app/src/Ops.Web/node_modules/bootstrap/dist/js/bootstrap.min.js /app/src/Ops.Web/js
cp /app/src/Ops.Web/node_modules/jquery/dist/jquery.js /app/src/Ops.Web/js
cp /app/src/Ops.Web/node_modules/jquery-validation/dist/jquery.validate.js /app/src/Ops.Web/js
cp /app/src/Ops.Web/node_modules/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.js /app/src/Ops.Web/js
cp /app/src/Ops.Web/node_modules/slick-carousel/slick/slick.js /app/src/Ops.Web/js
cp /app/src/Ops.Web/node_modules/slugify/slugify.js /app/src/Ops.Web/js
cp /app/src/Ops.Web/node_modules/commonmark/dist/commonmark.js /app/src/Ops.Web/js
cp /app/src/Ops.Web/node_modules/smartcrop/smartcrop.js /app/src/Ops.Web/js
cp /app/src/Ops.Web/node_modules/cropperjs/dist/cropper.js /app/src/Ops.Web/js
cp /app/src/Ops.Web/node_modules/@popperjs/core/dist/umd/popper.js /app/src/Ops.Web/js

mkdir -p /app/src/Ops.Web/css
rm -rf /app/src/Ops.Web/css/*.css

cp /app/src/Ops.Web/node_modules/bootstrap/dist/css/bootstrap.min.css /app/src/Ops.Web/css
cp /app/src/Ops.Web/node_modules/@fortawesome/fontawesome-free/css/all.css /app/src/Ops.Web/css
cp /app/src/Ops.Web/node_modules/slick-carousel/slick/slick.css /app/src/Ops.Web/css
cp /app/src/Ops.Web/node_modules/slick-carousel/slick/slick-theme.css /app/src/Ops.Web/css
cp /app/src/Ops.Web/node_modules/cropperjs/dist/cropper.css /app/src/Ops.Web/css

## FontAwesome
rm -rf /app/src/Ops.Web/wwwroot/webfonts/*
mkdir -p /app/src/Ops.Web/wwwroot/webfonts
cp /app/src/Ops.Web/node_modules/@fortawesome/fontawesome-free/webfonts/* /app/src/Ops.Web/wwwroot/webfonts/

## Slick
rm -rf /app/src/Ops.Web/wwwroot/css/fonts
mkdir -p /app/src/Ops.Web/wwwroot/css/fonts
cp /app/src/Ops.Web/node_modules/slick-carousel/slick/fonts/* /app/src/Ops.Web/wwwroot/css/fonts/
rm /app/src/Ops.Web/wwwroot/css/ajax-loader.gif
cp /app/src/Ops.Web/node_modules/slick-carousel/slick/ajax-loader.gif /app/src/Ops.Web/wwwroot/css/

# Promenade
cd /app/src/Promenade.Web && yarn install

mkdir -p /app/src/Promenade.Web/js
rm -rf /app/src/Promenade.Web/js/*.js

cp /app/src/Promenade.Web/node_modules/jquery/dist/jquery.js /app/src/Promenade.Web/js
cp /app/src/Promenade.Web/node_modules/bootstrap/dist/js/bootstrap.min.js /app/src/Promenade.Web/js
cp /app/src/Promenade.Web/node_modules/jquery-validation/dist/jquery.validate.js /app/src/Promenade.Web/js
cp /app/src/Promenade.Web/node_modules/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.js /app/src/Promenade.Web/js
cp /app/src/Promenade.Web/node_modules/slick-carousel/slick/slick.js /app/src/Promenade.Web/js
cp /app/src/Promenade.Web/node_modules/@popperjs/core/dist/umd/popper.js /app/src/Promenade.Web/js

mkdir -p /app/src/Promenade.Web/css
rm -rf /app/src/Promenade.Web/css/*.css

cp /app/src/Promenade.Web/node_modules/bootstrap/dist/css/bootstrap.min.css /app/src/Promenade.Web/css
cp /app/src/Promenade.Web/node_modules/@fortawesome/fontawesome-free/css/all.css /app/src/Promenade.Web/css
cp /app/src/Promenade.Web/node_modules/slick-carousel/slick/slick.css /app/src/Promenade.Web/css
cp /app/src/Promenade.Web/node_modules/slick-carousel/slick/slick-theme.css /app/src/Promenade.Web/css

## FontAwesome
rm -rf /app/src/Promenade.Web/wwwroot/webfonts/*
mkdir -p /app/src/Promenade.Web/wwwroot/webfonts
cp /app/src/Promenade.Web/node_modules/@fortawesome/fontawesome-free/webfonts/* /app/src/Promenade.Web/wwwroot/webfonts/

## Slick
rm -rf /app/src/Promenade.Web/wwwroot/css/fonts/*
mkdir -p /app/src/Promenade.Web/wwwroot/css/fonts
cp /app/src/Promenade.Web/node_modules/slick-carousel/slick/fonts/* /app/src/Promenade.Web/wwwroot/css/fonts/
rm /app/src/Promenade.Web/wwwroot/css/ajax-loader.gif
cp /app/src/Promenade.Web/node_modules/slick-carousel/slick/ajax-loader.gif /app/src/Promenade.Web/wwwroot/css/

cd /app/src/Ops.Web && echo --- Ops.Web yarn outdated && yarn outdated
cd /app/src/Promenade.Web && echo --- Promenade.Web yarn outdated && yarn outdated
