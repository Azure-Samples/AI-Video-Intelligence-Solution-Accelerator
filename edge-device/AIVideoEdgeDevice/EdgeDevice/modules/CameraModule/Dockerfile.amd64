FROM azureaivideo/opencvsharp:4.1.0 AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

FROM azureaivideo/opencvsharp-runtime:4.1.0
WORKDIR /app
COPY --from=build-env /app/out ./
COPY --from=build-env /out ./
COPY ./simulated-images/ ./simulated-images/

# ldconfig complains about non-symbolic links, but we don't care for Docker
# since there is never a system update of any sort
RUN useradd -ms /bin/bash moduleuser && ldconfig /app 2> /dev/null
USER moduleuser

ENTRYPOINT ["dotnet", "CameraModule.dll"]