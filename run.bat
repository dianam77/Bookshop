@echo off
start "ProjectA" ./AdminBookShop/publish/AdminBookShop.exe --urls "http://localhost:5000"
start "ProjectB" ./Bookshop/publish/Bookshop.exe --urls "http://localhost:5001"