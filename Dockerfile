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
# docker cp . devef:/
# docker run -it --name devef mcr.microsoft.com/dotnet/sdk:8.0-noble

 # Stage 1: Build
 FROM mcr.microsoft.com/dotnet/sdk:8.0-noble AS build 
 WORKDIR /src
 COPY ./Modul295PraxisArbeit . 
 RUN dotnet restore "Modul295PraxisArbeit.csproj"
 RUN dotnet publish "Modul295PraxisArbeit.csproj" -c Release -o /publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /publish

# Copy the published application from the build stage
COPY --from=build /publish .

# Expose the application port
#COPY GptApp.pfx /https/GptApp.pfx
EXPOSE 443

# Run the application
ENTRYPOINT ["dotnet", "testhost.dll", "--urls", "https://*:443"]