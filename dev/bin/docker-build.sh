#!/bin/bash

IMAGE="ocuda"

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

if [ "$#" -gt 0 ]; then
  IMAGE="$1"
fi

if [ "$#" -gt 1 ]; then
  TAG="$2"
fi

echo -e "\e[1mBuilding Ops branch \e[96m$BRANCH\e[39m commit \e[93m$COMMIT\e[0m"

docker build -f Dockerfile_ops -t $IMAGE/ops:$TAG --build-arg commit="$COMMIT" .

echo -e "\e[1mBuilding Promenade branch \e[96m$BRANCH\e[39m commit \e[93m$COMMIT\e[0m"

docker build -f Dockerfile_promenade -t $IMAGE/promenade:$TAG --build-arg commit="$COMMIT" .
