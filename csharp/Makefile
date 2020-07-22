NAME=DnsSrvTool
TEST_NAME=${NAME}.Test
DOC_NAME=${NAME}.Document
CSPROJ=${NAME}/${NAME}.csproj

# BUILDER=Dotnet
BUILDER=Msbuild

COVERAGE_MSBUILD_OPTION=--collect:"XPlat Code Coverage"
COVERAGE_DOTNET_OPTION=/p:CollectCoverage=true /p:CoverletOutput="./coverage/" -nr:false -m:1 /p:Threshold=80 /p:Include="${TEST_NAME}/*"

all: restore build cov_test

restore:
	dotnet restore

build:
	@echo "\e[92mBuild project:\033[0m"
ifeq ($(BUILDER), Dotnet)
	dotnet build
else ifeq ($(BUILDER), Msbuild)
	msbuild
else
	dotnet build
endif


cov_test:
	dotnet test $(TEST_NAME) $(COVERAGE_MSBUILD_OPTION) $(COVERAGE_DOTNET_OPTION)

test:
	@echo "\e[92mTest project Unit tests:\033[0m"
	dotnet test $(TEST_NAME)

release:
	dotnet --info
	dotnet build -c Release

publish-binaries:
	dotnet publish ${CSPROJ} -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true -o ./publish/bin/osx
	dotnet publish ${CSPROJ} -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true -o ./publish/bin/linux
	dotnet publish ${CSPROJ} -c Release -r win10-x64 --self-contained true /p:PublishSingleFile=true -o ./publish/bin/win10

publish-nuget:
	nuget pack ${NAME}/${NAME}.nuspec -OutputDirectory ./publish/nuget

nuget-local-export:
	@cd ${NAME} && msbuild -t:pack
	mkdir -p ~/.private_nuget
	nuget add ${NAME}/bin/Debug/${NAME}*.nupkg -s ~/.private_nuget -IncludeReferencedProjects

document:
	mono ${DOC_NAME}/docfx/docfx.console.2.49.0/tools/docfx.exe metadata ${DOC_NAME}/docfx_project/docfx.json
	mono ${DOC_NAME}/docfx/docfx.console.2.49.0/tools/docfx.exe build ${DOC_NAME}/docfx_project/docfx.json

coverage: coverage-dotnet

coverage-dotnet:
	@echo "\e[92mRunning Dotnet Code Coverage:\033[0m"
	-rm ${TEST_NAME}/TestResults/*/coverage.cobertura.xml
	-rm -r ${TEST_NAME}/TestResults/*/
	dotnet test ${TEST_NAME} $(COVERAGE_MSBUILD_OPTION)
	cp ${TEST_NAME}/TestResults/*/coverage.cobertura.xml ./coverage
	@# see https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/VSTestIntegration.md

coverage-msbuild:
	@echo "\e[92mRunning Msbuild Code Coverage:\033[0m"
	dotnet test ${TEST_NAME} $(COVERAGE_DOTNET_OPTION)
	cp ${TEST_NAME}/coverage/coverage.json ./coverage
	@echo "\e[92mYou Can find Code Coveage Results in ${TEST_NAME}/coverage:\033[0m"
	@#see https://github.com/tonerdo/coverlet/blob/master/Documentation/MSBuildIntegration.md for improvement

help:
	@echo "\e[92mThis Makefile contains the following targets:\033[0m"
	$(call get_targets_list)

define get_targets_list
	@$(MAKE) -pRrq -f $(lastword $(MAKEFILE_LIST)) : 2>/dev/null | awk -v RS= -F: '/^# File/,/^# Finished Make data base/ {if ($$1 !~ "^[#.]") {print $$1}}' | sort | egrep -v -e '^[^[:alnum:]]' -e '^$@$$'
endef


clean:
	@echo "\e[43mNo Clean yet\033[0m"
	exit(1)

.PHONY: all bin-publish build clean coverage document nuget-publish release restore test