#!/bin/bash

set -e

BLD_PUSH=false
BLD_BRANCH_FOUND=false
BLD_RELEASE=false
BLD_COMMIT=$(git rev-parse --short HEAD)
BLD_VERSION=unknown
BLD_VERSION_DATE=$(date -u +'%Y%m%d_%H%M%SZ')
BLD_DATE=$(date -u +'%Y-%m-%dT%H:%M:%SZ')

if [[ -z $BLD_DOCKERFILE ]]; then
  BLD_DOCKERFILE="Dockerfile"
  echo "=== No BLD_DOCKERFILE configured, using: $BLD_DOCKERFILE"
fi

if [[ -z $BLD_DOCKER_IMAGE ]]; then
  BLD_DIRECTORY=${PWD##*/}
  BLD_DOCKER_IMAGE=${BLD_DIRECTORY,,}
  echo "=== No BLD_DOCKER_IMAGE configured, using this directory name: $BLD_DOCKER_IMAGE"
fi

if BLD_GITBRANCH=$(git symbolic-ref --short -q HEAD); then
  BLD_BRANCH=$BLD_GITBRANCH
  BLD_BRANCH_FOUND=true
else
  # Azure DevOps works in detached HEAD state, get branch from variable
  BLD_GITBRANCH=$BUILD_SOURCEBRANCHNAME
  if [[ -n $BLD_GITBRANCH ]]; then
    BLD_BRANCH=$BLD_GITBRANCH
	BLD_BRANCH_FOUND=true
  fi
fi

if [[ -z $BLD_BRANCH ]]; then
  BLD_BRANCH="unknown-branch"
fi

if [[ $BLD_BRANCH = "master" ]]; then
  BLD_DOCKER_TAG="latest"
  BLD_VERSION=${BLD_BRANCH}-${BLD_VERSION_DATE}
  BLD_PUSH=true
elif [[ $BLD_BRANCH = "develop" ]]; then
  BLD_DOCKER_TAG="develop"
  BLD_VERSION=${BLD_BRANCH}-${BLD_VERSION_DATE}
  BLD_PUSH=true
elif [[ $BLD_BRANCH =~ release/([0-9]+\.[0-9]+\.[0-9]+.*) ]]; then
  BLD_RELEASE_VERSION=${BASH_REMATCH[1]}
  BLD_DOCKER_TAG=v${BLD_RELEASE_VERSION}
  BLD_VERSION=v${BLD_RELEASE_VERSION}
  BLD_RELEASE=true
  BLD_PUSH=true
  echo "=== Building release artifacts for $BLD_RELEASE_VERSION"
else
  BLD_DOCKER_TAG=$BLD_COMMIT
  BLD_VERSION=${BLD_COMMIT}-${BLD_VERSION_DATE}
fi

if [ $# -gt 0 ]; then
  BLD_DOCKER_TAG="$1"
fi

echo "=== Building branch $BLD_BRANCH commit $BLD_COMMIT as Docker image $BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG"

if [[ $BLD_PUSH = true ]]; then
    docker build -f $BLD_DOCKERFILE -t $BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG \
        --build-arg BRANCH="$BLD_BRANCH" \
        --build-arg IMAGE_CREATED="$BLD_DATE" \
        --build-arg IMAGE_REVISION="$BLD_COMMIT" \
        --build-arg IMAGE_VERSION="$BLD_VERSION" .
else
    docker build -f $BLD_DOCKERFILE -t $BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG --target build-stage .
fi

if [[ -z $BLD_DOCKER_REPOSITORY ]]; then
  echo '=== Not pushing docker image: no Docker repository configured.'
else
  if [[ $BLD_PUSH = false ]]; then
    echo '=== Not pushing Docker image: branch is not master, develop, or versioned release'
  else
    if [[ -z $BLD_DOCKER_USERNAME || -z $BLD_DOCKER_PASSWORD ]]; then
      echo '=== Not pushing Docker image: username or password not specified'
    else
      if [[ -z $BLD_DOCKER_HOST ]]; then
        echo "=== Pushing Docker image: $BLD_DOCKER_REPOSITORY/$BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG"
      else
        echo "=== Pushing Docker image: $BLD_DOCKER_REPOSITORY/$BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG to $BLD_DOCKER_HOST"
      fi
      echo "=== Authenticating..."
      echo "$BLD_DOCKER_PASSWORD" | docker login -u "$BLD_DOCKER_USERNAME" --password-stdin "$BLD_DOCKER_HOST" || exit $?
      echo "=== Tagging image $BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG as $BLD_DOCKER_REPOSITORY/$BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG"
      docker tag $BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG $BLD_DOCKER_REPOSITORY/$BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG
      echo "=== Pushing image $BLD_DOCKER_REPOSITORY/$BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG"
      docker push $BLD_DOCKER_REPOSITORY/$BLD_DOCKER_IMAGE:$BLD_DOCKER_TAG
      echo "=== Executing logout!"
      docker logout $BLD_DOCKER_HOST
    fi
  fi
fi
