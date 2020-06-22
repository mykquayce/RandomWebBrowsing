#! /bin/bash

cat ./Dockerfile | egrep --ignore-case '^FROM \S+' | awk '{system("docker pull " $2)}'
cat ./docker-compose.yml | egrep --ignore-case '\bimage: \S+' | awk '{system("docker pull " $2)}'

docker build --tag eassbhhtgu/randomwebbrowsing:latest .

if [ $? -ne 0 ]; then
   exit 1
fi

for s in randomwebbrowsing_rabbitmq_pass randomwebbrowsing_rabbitmq_user
do
  docker secret ls | tail --line +2 | grep $s >nul

  if [ $? -ne 0 ]; then
    openssl rand -base64 201 | sed 's/[^0-9A-Za-z]//g' | sed -z 's/\n//g' | docker secret create $s -
  fi
done

docker stack ls | tail --line +2 | grep 'randomwebbrowsing'

if [ $? -ne 0 ]; then
    docker stack deploy --compose-file ./docker-compose.yml randomwebbrowsing
else
    docker service ls | tail --line +2 | grep 'randomwebbrowsing_rabbitmq' | awk '{system("docker service update --image " $5 " " $2)}'
fi
