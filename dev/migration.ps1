param($project, $command, $parameters)

If ("Ops", "Promenade" -NotContains $project) {
    Throw "'$($project)' is not a valid project name! Please us Ops or Promenade."
}

Push-Location -Path "..\src\Ops.DataProvider.SqlServer.$project"
dotnet ef migrations $command -s ..\Ops.Web\Ops.Web.csproj --context Ocuda.Ops.DataProvider.SqlServer.$project.Context $parameters
Pop-Location
