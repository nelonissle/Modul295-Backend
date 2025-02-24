#
# Docker Modul 295 Praxis Arbeit
# dotnet server
#
# Docker erstellen:
# docker build -t neloserver .
#
# docker run --rm -e GPT_CONNECTION_STRING="Server=db;Database=gptppp;User=gptadmin;Password=1234;" -p 8087:80 neloserver
# docker run --rm -it -p 8443:443 -e GPT_CONNECTION_STRING=$GPT_CONNECTION_STRING -e GPT_APIKEY=$GPT_APIKEY -e ASPNETCORE_URLS="https://+" -e ASPNETCORE_HTTPS_PORTS=443 -e ASPNETCORE_Kestrel__Certificates__Default__Password="1234" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/GptApp.pfx gptapp
# docker run -d --restart unless-stopped -p 8443:443 -e GPT_CONNECTION_STRING=$GPT_CONNECTION_STRING -e GPT_APIKEY=$GPT_APIKEY -e ASPNETCORE_URLS="https://+" -e ASPNETCORE_HTTPS_PORTS=443 -e ASPNETCORE_Kestrel__Certificates__Default__Password="1234" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/GptApp.pfx gptapp
# docker mappings: (Verzeichnis) -v HOST:CONTAINER (Netwerkport) -p HOSTPOST:CONTAINERPORT
#
# docker cp . devef:/
# docker run --rm -it --name devef mcr.microsoft.com/dotnet/sdk:8.0-noble

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-noble AS build 
WORKDIR /src
COPY . . 
RUN cd Modul295PraxisArbeit
RUN dotnet restore
RUN dotnet publish -c Release -o /publish
RUN cd ..

RUN cd TestDataInserter
RUN dotnet restore
RUN dotnet publish -c Release -o /publish2
RUN cd ..

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-noble AS final

# copy test cli
WORKDIR /test
COPY --from=build /publish2 .

# copy server app
WORKDIR /publish
COPY --from=build /publish .

# Expose the application port
#COPY GptApp.pfx /https/GptApp.pfx
EXPOSE 443 8080

# set environment variables (must be name of docker-compose file of mongodb container)
ENV MONGO_CONNECTION_STRING="mongodb://mongoadmin:secret@mymongo:27017"

VOLUME [ "/publish/Logs" ]

# Run the application
#ENTRYPOINT ["dotnet", "Modul295PraxisArbeit.dll", "--urls", "https://*:8080"]
ENTRYPOINT ["dotnet", "Modul295PraxisArbeit.dll"]
