#! /bin/bash

# pull Docker images
cat ./Dockerfile | egrep --ignore-case '^FROM \S+' | awk '{system("docker pull " $2)}'
cat ./docker-compose.yml | egrep --ignore-case '\bimage: \S+' | awk '{system("docker pull " $2)}'


# build our Docker image
docker build --tag eassbhhtgu/randomwebbrowsing:latest .

if [ $? -ne 0 ]; then
   exit 1
fi


# create Docker secrets (if necessary)
for s in randomwebbrowsing_rabbitmq_pass randomwebbrowsing_rabbitmq_user
do
  docker secret ls | tail --line +2 | grep $s

  if [ $? -ne 0 ]; then
    openssl rand -base64 201 | \
        sed 's/[^0-9A-Za-z]//g' | \
        sed -z 's/\n//g' | \
        docker secret create $s -
  fi
done


# does Docker Stack exist?
docker stack ls | tail --line +2 | grep 'randomwebbrowsing'

if [ $? -ne 0 ]; then
    # create one
    docker stack deploy --compose-file ./docker-compose.yml randomwebbrowsing
else
    # update it
    docker service ls | \
        tail --line +2 | \
        grep 'randomwebbrowsing_rabbitmq' | \
        awk '{system("docker service update --image " $5 " " $2)}'
fi
