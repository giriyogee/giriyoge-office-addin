param(
  [Parameter(Mandatory = $true)]
  [ValidateSet('dev', 'uat', 'prod')]
  [string] $Environment,

  [Parameter(Mandatory = $true)]
  [ValidatePattern('^https://[A-Za-z0-9.-]+/?$')]
  [string] $Url
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$normalizedUrl = $Url.TrimEnd('/')

$settings = @{
  dev = @{
    Manifest = 'manifest-dev.xml'
    EnvironmentFile = 'environment.dev.ts'
    OldUrls = @(
      'https://dev.giriyoge-office-addin.example.com',
      'https://localhost:4300'
    )
  }
  uat = @{
    Manifest = 'manifest-uat.xml'
    EnvironmentFile = 'environment.uat.ts'
    OldUrls = @(
      'https://uat.giriyoge-office-addin.example.com',
      'https://localhost:4400'
    )
  }
  prod = @{
    Manifest = 'manifest-prod.xml'
    EnvironmentFile = 'environment.prod.ts'
    OldUrls = @(
      'https://prod.giriyoge-office-addin.example.com',
      'https://apps.contoso.com/giriyoge-office-addin',
      'https://support.contoso.com/giriyoge-office-addin'
    )
  }
}

$target = $settings[$Environment]
$manifestPath = Join-Path $root "manifests/$($target.Manifest)"
$environmentPath = Join-Path $root "src/environments/$($target.EnvironmentFile)"

foreach ($path in @($manifestPath, $environmentPath)) {
  $content = Get-Content -LiteralPath $path -Raw

  foreach ($oldUrl in $target.OldUrls) {
    $content = $content.Replace($oldUrl, $normalizedUrl)
  }

  Set-Content -LiteralPath $path -Value $content -NoNewline
}

Write-Host "Updated $Environment URLs to $normalizedUrl"
