#!/bin/bash

if [ $# -lt 2 ]; then
  echo 1>&2 "$0: please specify two arguments: provider and migration name"
  exit 2
elif [ $# -gt 2 ]; then
  echo 1>&2 "$0: please only specify two arguments: provider and migration name"
  exit 2
fi

pwd=${PWD##*/}

if [ $pwd = "dev" ]; then
  cd ..
fi

echo Adding migration to Ocuda.Ops.DataProvider.$1.Ops.Context named $2...
cd src/Ops.DataProvider.$1.Ops && \
dotnet ef migrations add -s ../Ops.Web/Ops.Web.csproj --context Ocuda.Ops.DataProvider.$1.Ops.Context $2

echo Adding migration to Ocuda.Ops.DataProvider.$1.Promenade.Context named $2...
cd ../Ops.DataProvider.$1.Promenade && \
dotnet ef migrations add -s ../Ops.Web/Ops.Web.csproj --context Ocuda.Ops.DataProvider.$1.Promenade.Context $2
