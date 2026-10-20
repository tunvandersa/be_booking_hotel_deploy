FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Chỉ copy file cấu hình và restore thư viện để tận dụng Cache
COPY ["do-an-epu.sln", "./"]
COPY ["do-an-epu/do-an-epu.csproj", "do-an-epu/"]
RUN dotnet restore "do-an-epu/do-an-epu.csproj"

# 2. Copy toàn bộ mã nguồn còn lại và tiến hành build
COPY . .
WORKDIR "/src/do-an-epu"
RUN dotnet publish "do-an-epu.csproj" -c Release -o /app/publish

# 3. Chuyển sang môi trường chạy (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "do-an-epu.dll"]