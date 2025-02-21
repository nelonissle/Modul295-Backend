# Pull the latest MongoDB Docker image
docker pull mongo:latest

# check if docker network mymongonet exists
$network = docker network ls --filter name=mymongonet --format "{{.Name}}"
if ($network -eq "mymongonet") {
    Write-Host "Network mymongonet exists"
} else {
    Write-Host "Network mymongonet does not exist"
    # Create a Docker network if not exists
    docker network create mymongonet
}

# check if docker container mymongo is already running
$container = docker ps -a --filter name=mymongo --format "{{.Names}}"
if ($container -eq "mymongo") {
    Write-Host "Container mymongo is already running"
} else {
    Write-Host "Container mymongo is not running"
    # Run a MongoDB Docker container
    docker run -d --network mymongonet --name mymongo -e MONGO_INITDB_ROOT_USERNAME=mongoadmin -e MONGO_INITDB_ROOT_PASSWORD=secret mongo:latest
}

# execute the following command to connect to the MongoDB container and execute mongosh show dbs
docker exec -it mymongo mongosh -u mongoadmin -p secret --eval "show dbs"


cd .\Modul295PraxisArbeit
dotnet tool install --global dotnet-ef
dotnet restore "Modul295PraxisArbeit.csproj"
dotnet publish "Modul295PraxisArbeit.csproj" -c Release -o publish
cd ..


