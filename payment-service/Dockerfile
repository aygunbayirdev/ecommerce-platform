FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore payment-service/PaymentService.slnx
RUN dotnet publish payment-service/src/ECommercePlatform.PaymentService.Api/ECommercePlatform.PaymentService.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ECommercePlatform.PaymentService.Api.dll"]
