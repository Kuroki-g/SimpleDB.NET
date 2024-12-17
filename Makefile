build:
	dotnet build
test:
	dotnet test --nologo --logger:junit
test-cov:
	dotnet tool run dotnet-coverage collect "dotnet test --nologo" -s .runsettings
	rm -rf coverage/*
	dotnet tool run reportgenerator -reports:coverage.xml -targetdir:"coverage" -reporttypes:Html

