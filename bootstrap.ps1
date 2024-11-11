function Write-Yellow {
    param (
        $Message
    )

    Write-Host $Message -BackgroundColor Yellow -ForegroundColor Black
}

function Write-Green {
    param (
        $Message
    )

    Write-Host $Message -BackgroundColor Green -ForegroundColor Black
}

function Write-Red {
    param (
        $Message
    )

    Write-Host $Message -BackgroundColor Red -ForegroundColor Black
}

$File = "$env:USERPROFILE\.aws\credentials"
$today = Get-Date
$today = $today.AddHours(-1)
$lastWriteTime = (Get-Item $File).LastWriteTime

if ($lastWriteTime -lt $today) {
    Write-Red "AWS credentials are expired. Please refresh them before running the bootstrap."
    exit 1
}

$folderName = Split-Path -Path (Get-Location) -Leaf
if(-not ($folderName -match "phoenix-[\w]*-api")) {
    Write-Red "ERR: Repo name should conform to phoenix-{apiName}-api syntax"
    exit 1
}
$TextInfo = (Get-Culture).TextInfo
$apiname = $TextInfo.ToTitleCase(($folderName -Split '-')[1])
$apiNameLower = $apiName.ToLower()

# # create projects
Write-Yellow "Creating SLN files..."

dotnet new webapi --name "Rfsmart.Phoenix.$apiName.Web" --use-controllers
dotnet new classlib --name "Rfsmart.Phoenix.$apiName" 
dotnet new nunit --name "Rfsmart.Phoenix.$apiName.UnitTests"
dotnet new nunit --name "Rfsmart.Phoenix.$apiName.IntegrationTests"

# add classlib ref to web project
dotnet add "Rfsmart.Phoenix.$apiName.Web/Rfsmart.Phoenix.$apiName.Web.csproj" package DataDog.Trace.Bundle
dotnet add "Rfsmart.Phoenix.$apiName.Web/Rfsmart.Phoenix.$apiName.Web.csproj" reference "Rfsmart.Phoenix.$apiName/Rfsmart.Phoenix.$apiName.csproj"

# add classlib ref to test projects
dotnet add "Rfsmart.Phoenix.$apiName.UnitTests/Rfsmart.Phoenix.$apiName.UnitTests.csproj" reference "Rfsmart.Phoenix.$apiName/Rfsmart.Phoenix.$apiName.csproj"
dotnet add "Rfsmart.Phoenix.$apiName.IntegrationTests/Rfsmart.Phoenix.$apiName.IntegrationTests.csproj" reference "Rfsmart.Phoenix.$apiName/Rfsmart.Phoenix.$apiName.csproj"

# create sln
dotnet new sln --name "Rfsmart.Phoenix.$apiName.Api"
dotnet sln add "Rfsmart.Phoenix.$apiName.Web"
dotnet sln add "Rfsmart.Phoenix.$apiName"
dotnet sln add "Rfsmart.Phoenix.$apiName.UnitTests"
dotnet sln add "Rfsmart.Phoenix.$apiName.IntegrationTests"

Move-Item -Path "./Tests-Dockerfile" -Destination "./Rfsmart.Phoenix.$apiName.IntegrationTests/Dockerfile"
Move-Item -Path "./Web-Dockerfile" -Destination "./Rfsmart.Phoenix.$apiName.Web/Dockerfile"
Move-Item -Path "./appsettings.json" -Destination "./Rfsmart.Phoenix.$apiName.Web/appsettings.json" -Force
Move-Item -Path "./appsettings.development.json" -Destination "./Rfsmart.Phoenix.$apiName.Web/appsettings.development.json" -Force

# install required tools
dotnet tool restore

Write-Green "SLN files created!"

Write-Yellow "Updating deploy/iac files..."

$templateTitleCase = "TEMPLATE"
$templateLowerCase = "TEMP-LATE"

ForEach ($File in (Get-ChildItem -Recurse -File -Exclude *.ps1)) {
    if ($File.Directory -match 'obj' -or $File.Name -eq "pull_request_template.md") {
        continue
    }

    $content = (Get-Content $File)

    if ($content -match $templateLowerCase -or $content -match $templateTitleCase) {
        $n = $File.Name
        Write-Host "Updating $n..."

        $content -Replace $templateLowerCase,$apiNameLower `
            -Replace $templateTitleCase,$apiName |
            Set-Content $File
    }
}

# rename iac directory
Rename-Item -Path "./iac/deploys/$templateLowerCase-web" -NewName ($apiNameLower + "-web")

# update listener_rule_priority - needs to be different for each API
try {
    Write-Yellow "Trying to auto-detect new listener rule priority..."

    $object = aws elbv2 describe-rules --listener-arn arn:aws:elasticloadbalancing:us-east-1:136566554811:listener/app/phoenix-phxdev-cluster/5166b7fec394512c/fbf24b2fd4423e18 `
    --profile ICS-phxdev_Admin `
    --output json `
    | ConvertFrom-Json

    $highest = $object.Rules | Where-Object { $_.Priority -ne "default" } | Sort-Object -Property Priority -Descending | Select-Object -First 1

    $api = $highest.Conditions[0].Values[0]
    $priority = $highest.Priority
    $newPriority = [int]$priority + 1

    Write-Green "Detected highest listener rule priority of $priority on $api, bumping $apiName to priority $newPriority"
}
catch {
    Write-Red "Error calculating new listener rule priority. Listener rule priority must be different for each API. Enter new priority:"

    while (! ($newPriority) -or $newPriority -isnot [int]) {
        $newPriority = [int](Read-Host)
    }
}

$mainTf = "./iac/deploys/$apiName-web/main.tf"
(Get-Content $mainTf) -Replace "listener_rule_priority = 4","listener_rule_priority = $newPriority" |
    Set-Content $mainTf

Write-Host "Deploy/iac files updated!" -BackgroundColor Green -ForegroundColor Black

# Write-Host "Updating repository ruleset..." -BackgroundColor Yellow -ForegroundColor Black

$default = "RF-SMART-for-OracleCloud"
if (!($org = Read-Host "Enter organization [$default]")) { 
    $org = $default 
}

$token = $env:GH_TOKEN

if (!($token)) {
    $token = $env:GITHUB_TOKEN
}

while (!($token)) { 
    $token = Read-Host "Enter token" 
}

$Headers = @{
    "Accept" = "application/vnd.github+json"
    "Authorization" = "Bearer " + $token
    "X-GitHub-Api-Version" = "2022-11-28"
}

$Response = Invoke-WebRequest -URI "https://api.github.com/repos/$org/phoenix-$apiNameLower-api/rulesets" `
    -Headers $Headers `
    -Method Post `
    -Body (Get-Content '.github/main branch.json' | Out-String)

Write-Host "Repository ruleset updated!" -BackgroundColor Green -ForegroundColor Black