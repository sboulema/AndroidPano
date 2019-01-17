# First Stage
FROM microsoft/dotnet:sdk

RUN mkdir /build
WORKDIR /build

COPY . /dotnetapp
RUN dotnet restore

RUN dotnet publish -c Release -o out

# Second Stage
FROM microsoft/dotnet:aspnetcore-runtime

WORKDIR /app
COPY --from=0 /build/out .

ENTRYPOINT dotnet WebVRPano.dll
