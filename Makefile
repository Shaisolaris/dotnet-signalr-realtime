.PHONY: restore build run test clean

restore:
	dotnet restore ./src/RealtimeApp/RealtimeApp.csproj

build:
	dotnet build ./src/RealtimeApp/RealtimeApp.csproj --no-restore

run:
	dotnet run --project ./src/RealtimeApp

test:
	dotnet test 2>/dev/null || echo "No test project configured"

clean:
	dotnet clean ./src/RealtimeApp/RealtimeApp.csproj
	rm -rf bin/ obj/

docker-build:
	docker build -t dotnet-signalr-realtime .

docker-run:
	docker run -p 8080:8080 dotnet-signalr-realtime
