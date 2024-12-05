build:
	dotnet build
test:
	dotnet test
test-cov:
	dotnet tool run dotnet-coverage collect "dotnet test --nologo" -s .test.runsettings
	dotnet tool run reportgenerator -reports:coverage.xml -targetdir:"coverage" -reporttypes:Html

