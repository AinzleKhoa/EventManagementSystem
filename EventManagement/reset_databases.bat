@echo off
setlocal EnableExtensions EnableDelayedExpansion
chcp 65001 >nul

set "SERVER=%~1"
set "AUTH_MODE=%~2"
set "SQL_USER=%~3"
set "SQL_PASSWORD=%~4"
set "SQL_DIR=%~dp0.."

if "%SERVER%"=="" set "SERVER=localhost"
if "%AUTH_MODE%"=="" set "AUTH_MODE=sql"

where sqlcmd >nul 2>nul
if errorlevel 1 (
    echo [ERROR] sqlcmd was not found.
    echo Install SQL Server command-line tools or run this script on a machine with sqlcmd available.
    exit /b 1
)

set "AUTH_ARGS=-E"
if /I "%AUTH_MODE%"=="sql" (
    if "%SQL_USER%"=="" set "SQL_USER=sa"
    if "%SQL_PASSWORD%"=="" set "SQL_PASSWORD=123456"
    set "AUTH_ARGS=-U !SQL_USER! -P !SQL_PASSWORD!"
) else (
    if /I not "%AUTH_MODE%"=="windows" (
        echo [ERROR] Invalid auth mode: %AUTH_MODE%
        echo Usage:
        echo   reset_databases.bat [server] [windows]
        echo   reset_databases.bat [server] sql [user] [password]
        exit /b 1
    )
)

echo ==================================================
echo Resetting EventManagement databases
echo Server: %SERVER%
echo Auth:   %AUTH_MODE%
echo SQL dir:%SQL_DIR%
echo ==================================================
echo.

set "DROP_SQL=%TEMP%\eventmanagement_drop_databases_%RANDOM%%RANDOM%.sql"

(
    echo USE master;
    echo GO
    for %%D in (
        NotificationDB
        ChatDB
        WithdrawalDB
        CategoryDB
        VoucherDB
        SocialDB
        BookingDB
        EventsDB
        AuthDB
    ) do (
        echo IF DB_ID(N'%%D'^) IS NOT NULL
        echo BEGIN
        echo     ALTER DATABASE [%%D] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        echo     DROP DATABASE [%%D];
        echo END
        echo GO
    )
) > "%DROP_SQL%"

echo [1/10] Dropping existing databases...
sqlcmd -S "%SERVER%" %AUTH_ARGS% -b -f 65001 -i "%DROP_SQL%"
if errorlevel 1 (
    echo [ERROR] Failed while dropping databases.
    del "%DROP_SQL%" >nul 2>nul
    exit /b 1
)

del "%DROP_SQL%" >nul 2>nul

call :RunSql "1. AuthDB.sql" || exit /b 1
call :RunSql "2. EventsDB.sql" || exit /b 1
call :RunSql "3. BookingDB.sql" || exit /b 1
call :RunSql "4. SocialDB.sql" || exit /b 1
call :RunSql "5. VoucherDB.sql" || exit /b 1
call :RunSql "6. CategoryDB.sql" || exit /b 1
call :RunSql "7. WithdrawalDB.sql" || exit /b 1
call :RunSql "8. ChatDB.sql" || exit /b 1
call :RunSql "9. NotificationDB.sql" || exit /b 1

echo.
echo [DONE] All EventManagement databases were recreated successfully.
exit /b 0

:RunSql
set "SQL_FILE=%~1"
set "SQL_PATH=%SQL_DIR%\%SQL_FILE%"

if not exist "%SQL_PATH%" (
    echo [ERROR] Missing SQL file: %SQL_PATH%
    exit /b 1
)

echo Running %SQL_FILE%...
sqlcmd -S "%SERVER%" %AUTH_ARGS% -b -f 65001 -i "%SQL_PATH%"
if errorlevel 1 (
    echo [ERROR] Failed while running %SQL_FILE%.
    exit /b 1
)

exit /b 0
