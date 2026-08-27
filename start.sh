#!/usr/bin/env bash
# Sobe a Api e o Web do Clima Brasil de uma vez só (Linux/macOS/Git Bash).
#
# O sistema é composto por dois processos independentes (veja docs/architecture.md):
# WeatherDashboard.Api (coleta os dados e os expõe por HTTP) e WeatherDashboard.Web
# (o site). Os dois precisam estar rodando ao mesmo tempo. Este script só automatiza
# abrir os dois — não substitui entender a arquitetura, só evita abrir dois terminais
# na mão toda vez. Ctrl+C encerra os dois processos.

set -e
root="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$root"

cleanup() {
  echo ""
  echo "Encerrando Api e Web..."
  kill "$api_pid" "$web_pid" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

echo "Subindo WeatherDashboard.Api  (http://localhost:5282 · swagger em /swagger)..."
dotnet run --project src/WeatherDashboard.Api &
api_pid=$!

sleep 6

echo "Subindo WeatherDashboard.Web  (http://localhost:5170)..."
dotnet run --project src/WeatherDashboard.Web &
web_pid=$!

echo ""
echo "Api (PID $api_pid) e Web (PID $web_pid) rodando. Ctrl+C encerra os dois."
wait
