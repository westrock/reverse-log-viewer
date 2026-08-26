param(
    [string]$TargetPath
)

# Find the cert by subject and friendly name
$cert = Get-ChildItem Cert:\CurrentUser\My |
    Where-Object {
        $_.Subject -like "*EFigarsky*" -and
        $_.FriendlyName -like "*Code-signing Cert*"
    } |
    Select-Object -First 1

if (-not $cert) {
    Write-Error "Code signing certificate not found."
    exit 1
}

$thumb = $cert.Thumbprint

# Run signtool and capture exit code
& "C:\Program Files (x86)\Microsoft SDKs\ClickOnce\SignTool\signtool.exe" sign /sha1 $thumb `
    /tr http://timestamp.digicert.com `
    /td sha256 /fd sha256 $TargetPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "Signing failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
} else {
    Write-Host "Signing succeeded for $TargetPath"
}
