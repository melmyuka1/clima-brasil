<#
.SYNOPSIS
  Sobe a Api e o Web do Clima Brasil de uma vez so, cada um na sua janela.

.DESCRIPTION
  O sistema e composto por dois processos independentes (veja docs/architecture.md):
  WeatherDashboard.Api (coleta os dados e os expoe por HTTP) e WeatherDashboard.Web
  (o site). Os dois precisam estar rodando ao mesmo tempo. Este script so automatiza
  abrir os dois - nao substitui entender a arquitetura, so evita digitar dois comandos
  em dois terminais toda vez.
#>

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host "Subindo WeatherDashboard.Api  (http://localhost:5282 - swagger em /swagger)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$root'; dotnet run --project src/WeatherDashboard.Api"

Write-Host "Aguardando a Api iniciar..." -ForegroundColor DarkGray
Start-Sleep -Seconds 6

Write-Host "Subindo WeatherDashboard.Web  (http://localhost:5170)..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "Set-Location '$root'; dotnet run --project src/WeatherDashboard.Web"

Write-Host "Aguardando o Web iniciar..." -ForegroundColor DarkGray
Start-Sleep -Seconds 4

Start-Process "http://localhost:5170"

Write-Host ""
Write-Host "Duas janelas novas do PowerShell foram abertas: uma para a Api, outra para o Web." -ForegroundColor Green
Write-Host "Feche as duas janelas (ou Ctrl+C em cada uma) para encerrar a aplicacao."
