#! /bin/bash

docker pull jaegertracing/all-in-one:latest
docker pull mcr.microsoft.com/dotnet/core/runtime:3.1
docker pull mcr.microsoft.com/dotnet/core/sdk:3.1
docker pull rabbitmq:management

docker build --tag eassbhhtgu/randomwebbrowsing .

if [ $? -ne 0 ]; then
   exit 1
fi

docker secret ls | tail --line +2 | grep randomwebbrowsing_rabbitmq_pass

if [ $? -ne 0 ]; then
   openssl rand -base64 201 | sed 's/[^0-9A-Za-z]//g' | sed -z 's/\n//g' | docker secret create randomwebbrowsing_rabbitmq_pass -
fi

docker secret ls | tail --line +2 | grep randomwebbrowsing_rabbitmq_user

if [ $? -ne 0 ]; then
   openssl rand -base64 60 | sed 's/[^0-9A-Za-z]//g' | sed -z 's/\n//g' | docker secret create randomwebbrowsing_rabbitmq_user -
fi

docker stack ls | tail --line +2 | grep 'randomwebbrowsing'

if [ $? -ne 0 ]; then
    docker stack deploy --compose-file ./docker-compose.yml randomwebbrowsing
else
    docker service ls | tail --line +2 | grep 'randomwebbrowsing_rabbitmq' | awk '{system("docker service update --image " $5 " " $2)}'
fi

