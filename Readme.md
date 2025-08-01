dotnet new sln -n DapperDemo

mkdir src
cd src

dotnet new webapi -n DapperDemo.API
dotnet new classlib -n DapperDemo.Domain
dotnet new classlib -n DapperDemo.DAL
dotnet new classlib -n DapperDemo.Database

cd ..
dotnet sln DapperDemo.sln add ./src/DapperDemo.API/DapperDemo.API.csproj
dotnet sln DapperDemo.sln add ./src/DapperDemo.Domain/DapperDemo.Domain.csproj
dotnet sln DapperDemo.sln add ./src/DapperDemo.DAL/DapperDemo.DAL.csproj
dotnet sln DapperDemo.sln add ./src/DapperDemo.Database/DapperDemo.Database.csproj
