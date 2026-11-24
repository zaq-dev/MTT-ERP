FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish MTT-ERP.Api/MTT-ERP.Api.csproj -c Release -o /app/api
RUN dotnet publish MTT-ERP.Web/MTT-ERP.Web.csproj -c Release -o /app/web

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/api ./api
COPY --from=build /app/web ./web

# Run API on internal port 5050 and Blazor Web on public port 8080
ENV ApiBaseUrl="http://127.0.0.1:5050/"
ENV ASPNETCORE_ENVIRONMENT="Production"
EXPOSE 8080

CMD ["sh", "-c", "dotnet /app/api/MTT-ERP.Api.dll --urls http://127.0.0.1:5050 & dotnet /app/web/MTT-ERP.Web.dll --urls http://0.0.0.0:${PORT:-8080}"]