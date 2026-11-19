@echo off
set ROOT=%~dp0
set LOG=%ROOT%run-log.txt
cd /d "%ROOT%src\DocAssistant.Api"
echo Creating database... output saved to run-log.txt
(
  echo ===== EF database update
  dotnet ef database update
  echo ===== DONE
) > "%LOG%" 2>&1
type "%LOG%"
echo.
echo Starting the API. Swagger will open at http://localhost:5080/swagger
echo Press Ctrl+C to stop.
echo.
dotnet run --launch-profile http
pause
