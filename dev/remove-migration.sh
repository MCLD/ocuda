#!/bin/bash

if [ $# -lt 1 ]; then
  echo 1>&2 "$0: please specify provider"
  exit 2
elif [ $# -gt 1 ]; then
  echo 1>&2 "$0: please only specify provider"
  exit 2
fi

pwd=${PWD##*/}

if [ $pwd = "dev" ]; then
  cd ..
fi

echo Removing last migration in Ocuda.Ops.DataProvider.$1.Ops.Context
cd src/Ops.DataProvider.$1.Ops && \
dotnet ef migrations remove -s ../Ops.Web/Ops.Web.csproj --context Ocuda.Ops.DataProvider.$1.Ops.Context

echo Removing last migration in Ocuda.Ops.DataProvider.$1.Promenade.Context
cd ../Ops.DataProvider.$1.Promenade && \
dotnet ef migrations remove -s ../Ops.Web/Ops.Web.csproj --context Ocuda.Ops.DataProvider.$1.Promenade.Context
