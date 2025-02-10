#!/bin/bash
RED='\033[0;31m'
GREEN='\033[1;32m'
YELLOW='\033[1;33m'
BLUE='\033[1;34m'
NC='\033[0m'

print() { 
    echo -e "$2$1$NC" 
}

print_title() { 
    echo -e "$BLUE\n# $1$NC" 
}

#print_title "Running dotnet build with warnaserror"
#dotnet build --no-incremental -warnaserror
print_title "Running dotnet build"
dotnet build --no-incremental

rc=$?
if [[ $rc != 0 ]] ; then
    print "Failed to build the project without errors" $RED
    exit $rc
fi

#print "Logging in to dockerhub"
#echo "$CI_REGISTRY_PASSWORD" | docker login -u $CI_REGISTRY_USER $CI_REGISTRY --password-stdin

dynamo_endpoint=http://localhost:8000
code=$( curl -s -I -o /dev/null --head -w "%{http_code}" -X GET $dynamo_endpoint )
if [ $code -eq "400" ] 
    then 
        print_title "DynamoDB Local already running"  
    else 
        print_title "Starting dynamodb-local container..."
        docker-compose -f docker-compose-local-aws.yml up --detach --remove-orphans
fi

print_title "Running dotnet test"
export ASPNETCORE_ENVIRONMENT=Test
dotnet test --no-build -v m --collect "XPlat Code Coverage" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.IncludeDirectory=API \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.ExcludeByFile=**/Program.cs,**/Views/**/*

rc=$?
if [[ $rc != 0 ]] ; then
    print "Failed run tests without errors" $RED
    exit $rc
fi

print "Script passed with success!" $GREEN