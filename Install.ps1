# build docker dotnet server image
docker build -t neloserver .

# run docker cluster
docker compose up -d

# wait for 3 seconds
Start-Sleep -Seconds 10

# execute the following command to connect to the MongoDB container and execute mongosh show dbs
docker exec mymongo mongosh -u mongoadmin -p secret --eval "show dbs"

#docker compose down