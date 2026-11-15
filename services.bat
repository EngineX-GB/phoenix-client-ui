@echo off
cd services
wt ^
    new-tab --title "Service: Connector" "%CD%/market-connector-service.exe" ^
    ; new-tab --title "Service: Ingestion" "%CD%/DataIngestionRunner.exe" --api ^
    ; new-tab --title "Service: Client" "%CD%/phoenix-client-service.exe"