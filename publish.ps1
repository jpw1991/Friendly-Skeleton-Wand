param(
    [Parameter(Mandatory)]
    [ValidateSet('Debug','DebugServer','Release')]
    [System.String]$Target,
    
    [Parameter(Mandatory)]
    [System.String]$TargetPath,
    
    [Parameter(Mandatory)]
    [System.String]$TargetAssembly,

    [Parameter(Mandatory)]
    [System.String]$ValheimPath,

    [Parameter(Mandatory)]
    [System.String]$ProjectPath,
    
    [System.String]$DeployPath
)

# Make sure Get-Location is the script path
Push-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Path)

# If debugging server set the Valheim dedicated server path
if ($Target.Equals("DebugServer")){
    $ValheimServerPath = "${ValheimPath} dedicated server"
}

# Test some preliminaries
("$TargetPath",
 "$ValheimPath",
 "$(Get-Location)\libraries"
) | % {
    if (!(Test-Path "$_")) {Write-Error -ErrorAction Stop -Message "$_ folder is missing"}
    if ("$ValheimServerPath"){
        if (!(Test-Path "$ValheimServerPath")) {Write-Error -ErrorAction Stop -Message "$ValheimServerPath folder is missing"}
    }
}



# Plugin name without ".dll"
$name = "$TargetAssembly" -Replace('.dll')

# Create the mdb file
$pdb = "$TargetPath\$name.pdb"
if (Test-Path -Path "$pdb") {
    Write-Host "Create mdb file for plugin $name"
    Invoke-Expression "& `"$(Get-Location)\libraries\Debug\pdb2mdb.exe`" `"$TargetPath\$TargetAssembly`""
}

# Main Script
Write-Host "Publishing for $Target from $TargetPath"

if ($Target.Equals("Debug")) {
    if ($DeployPath.Equals("")){
      $DeployPaths = "$ValheimPath\BepInEx\plugins"
    }
} elseif ($Target.Equals("DebugServer")) {
    if ($DeployPath.Equals("")){
        $DeployPaths = "$ValheimPath\BepInEx\plugins", "$ValheimServerPath\BepInEx\plugins"
    }
}

if ($Target.Equals("Debug") -or $Target.Equals("DebugServer")) {
    foreach ($DeployPath in $DeployPaths) {
        $plug = New-Item -Type Directory -Path "$DeployPath\$name" -Force
        Write-Host "Copy $TargetAssembly to $plug"
        Copy-Item -Path "$TargetPath\$name.dll" -Destination "$plug" -Force
        Write-Host "Copy $name.pdb to $plug"
        Copy-Item -Path "$TargetPath\$name.pdb" -Destination "$plug" -Force
        Write-Host "Copy $name.dll.mdb to $plug"
        Copy-Item -Path "$TargetPath\$name.dll.mdb" -Destination "$plug" -Force
        Write-Host "Copy Assets to $plug"
        Copy-Item -Recurse -Path "$ProjectPath\Assets" -Destination "$plug" -Force
    }
}

if($Target.Equals("Release")) {
    Write-Host "Packaging for ThunderStore..."
    $Package="Package"
    $PackagePath="$ProjectPath\$Package"

    Write-Host "$PackagePath\$TargetAssembly"
    New-Item -Type Directory -Path "$PackagePath\plugins" -Force
    New-Item -Type Directory -Path "$PackagePath\plugins\FriendlySkeletonWand" -Force
    Copy-Item -Path "$TargetPath\$TargetAssembly" -Destination "$PackagePath\plugins\FriendlySkeletonWand\$TargetAssembly" -Force
    Copy-Item -Path "README.md" -Destination "$PackagePath\README.md" -Force
    New-Item -Type Directory -Path "$PackagePath\plugins\FriendlySkeletonWand\Assets" -Force
    Copy-Item -Path "$ProjectPath\Assets\*" -Destination "$PackagePath\plugins\FriendlySkeletonWand\Assets" -Force -Recurse
    Compress-Archive -Path "$PackagePath\*" -DestinationPath "$TargetPath\$TargetAssembly.zip" -Force
}

# Pop Location
Pop-Location