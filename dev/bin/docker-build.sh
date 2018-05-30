#!/bin/bash

pwd=${PWD##*/}

if [ $pwd = "bin" ]; then
  cd ..
fi

pwd=${PWD##*/}

if [ $pwd = "dev" ]; then
  cd ..
fi

BRANCH=`git name-rev --name-only HEAD`
COMMIT=`git rev-parse --short HEAD`

if [[ "$BRANCH" == "master" ]]; then
  export TAG="latest";
elif [[ "$BRANCH" == "develop" ]]; then
  export TAG="develop";
else
  export TAG=$COMMIT;
fi

echo -e "\e[1mBuilding Ops branch \e[96m$BRANCH\e[39m commit \e[93m$COMMIT\e[0m"

docker build -f Dockerfile_ops -t ocuda/ops:$TAG --build-arg commit="$COMMIT" .

echo -e "\e[1mBuilding Promenade branch \e[96m$BRANCH\e[39m commit \e[93m$COMMIT\e[0m"

docker build -f Dockerfile_promenade -t ocuda/promenade:$TAG --build-arg commit="$COMMIT" .
