$path = "..\The16Spaces-private"
$pathExists = Test-Path -Path $path

if($pathExists) {
    Write-Host "Directory Exists - Clearing..."
    Remove-Item -Recurse -Force "$path\Assets"
    Remove-Item -Recurse -Force "$path\ProjectSettings"
} else {
    Write-Host "Creating New Directory..."
    New-Item -ItemType "directory" -Path $path
}

Write-Host "Copying Assets..."
Copy-Item -Path ".\Assets" -Destination "$path\Assets" -Recurse
Write-Host "Copying Project Settings..."
Copy-Item -Path ".\ProjectSettings" -Destination "$path\ProjectSettings" -Recurse

cd $path


$date = Get-Date -Format "o"
Write-Host "Committing to Git..."
git add .
git commit -m "$date"
Write-Host "Pushing to GitHub..."
git push
Write-Host "Push Complete"