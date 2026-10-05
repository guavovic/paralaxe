# Cria a junção Assets/Samples/ForestDemo -> Packages/com.guavovic.parallax/Samples~/ForestDemo.
# A pasta Samples~ fica escondida da Unity; pela junção o sample abre e é editado no próprio repositório.
# Uso: powershell -ExecutionPolicy Bypass -File Tools~/criar_juncao_sample.ps1

$root = Split-Path -Parent $PSScriptRoot
$target = Join-Path $root "Packages\com.guavovic.parallax\Samples~\ForestDemo"
$link = Join-Path $root "Assets\Samples\ForestDemo"

if (Test-Path $link) {
    Write-Output "A junção já existe: $link"
    exit 0
}

New-Item -ItemType Directory -Force (Split-Path -Parent $link) | Out-Null
New-Item -ItemType Junction -Path $link -Target $target | Out-Null
Write-Output "Junção criada: $link -> $target"
