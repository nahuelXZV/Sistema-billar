param (
    [Parameter(Mandatory=$true)]
    [string]$OldName,

    [Parameter(Mandatory=$true)]
    [string]$NewName
)

Write-Host "Renombrando proyecto de $OldName a $NewName..."

# Renombrar carpetas
Get-ChildItem -Directory -Recurse | Where-Object {
    $_.Name -like "*$OldName*"
} | Sort-Object FullName -Descending | ForEach-Object {
    Rename-Item $_.FullName $_.Name.Replace($OldName, $NewName)
}

# Renombrar archivos
Get-ChildItem -File -Recurse | Where-Object {
    $_.Name -like "*$OldName*"
} | ForEach-Object {
    Rename-Item $_.FullName $_.Name.Replace($OldName, $NewName)
}

# Reemplazar texto dentro de archivos
Get-ChildItem -File -Recurse -Include *.cs,*.csproj,*.sln,*.json,*.md |
ForEach-Object {
    (Get-Content $_.FullName) -replace $OldName, $NewName |
    Set-Content $_.FullName
}

Write-Host "✅ Renombrado completado"
