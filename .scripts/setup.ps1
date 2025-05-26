if (-not (Test-Path .env)) { exit }
Get-Content .env | Where-Object { $_ -match '^([^=#]+)=(.*)$' } | ForEach-Object {
    $key = $matches[1]; $value = $matches[2]
    if ($key -eq "GODOT_BIN") {
        $oldGdtExec = [Environment]::GetEnvironmentVariable("GODOT_BIN", "User")
        if ($oldGdtExec) {
            $oldDir = Split-Path $oldGdtExec -Parent
            $path = [Environment]::GetEnvironmentVariable("PATH", "User")
            $newPath = ($path -split ';' | Where-Object { $_ -ne $oldDir }) -join ';'
            [Environment]::SetEnvironmentVariable("PATH", $newPath, "User")
            [Environment]::SetEnvironmentVariable("PATH", $newPath, "Process")
        }
        $newDir = Split-Path $value -Parent
        $path = [Environment]::GetEnvironmentVariable("PATH", "User")
        [Environment]::SetEnvironmentVariable("PATH", "$newDir;$path", "User")
        [Environment]::SetEnvironmentVariable("PATH", "$newDir;$path", "Process")
    }
    [Environment]::SetEnvironmentVariable($key, $value, 'Process')
    [Environment]::SetEnvironmentVariable($key, $value, 'User')
}
Write-Host "Environment loaded to Windows user profile."