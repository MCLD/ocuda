#!/bin/sh

# How to run this:
# 1. Find a Linux system (Windows Docker path sharing is a mess)
# 2. Clone the repository
# 4. Run the following: docker run -it --rm -v `pwd`:/app mcr.microsoft.com/dotnet/core/sdk:3.1 bash /app/dev/docker-yarn-update.sh
# 5. See what happened with git status

curl -sL https://deb.nodesource.com/setup_12.x | bash -
curl -sL https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add -
echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list
apt-get update && apt-get -y install yarn
rm /app/src/Ops.Web/node_modules/*
cd /app/src/Ops.Web && yarn
rm /app/src/Ops.Web/wwwroot/webfonts/*
cp /app/src/Ops.Web/node_modules/@fortawesome/fontawesome-free/webfonts/* /app/src/Ops.Web/wwwroot/webfonts/
rm /app/src/Promenade.Web/node_modules/*
cd /app/src/Promenade.Web && yarn
rm /app/src/Promenade.Web/wwwroot/webfonts/*
cp /app/src/Promenade.Web/node_modules/@fortawesome/fontawesome-free/webfonts/* /app/src/Promenade.Web/wwwroot/webfonts/
