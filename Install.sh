#!/bin/zsh

# Build Docker .NET server image
docker build -t neloserver .

# Run Docker cluster
docker compose up -d

sleep 3


# Execute the following command to connect to the MongoDB container and execute mongosh show dbs
docker exec mymongo mongosh -u mongoadmin -p secret --eval "show dbs"

#docker compose down