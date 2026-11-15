@echo off
cd services
wt ^
    new-tab --title "Service: Connector" "%CD%/dev-market-connector-service.bat" ^
    ; new-tab --title "Service: Ingestion" "%CD%/dev-phoenix-ingestions-service.bat" ^
    ; new-tab --title "Service: Client" "%CD%/dev-phoenix-client-service.bat"