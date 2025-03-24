FROM ubuntu:noble
ENV DATA_DIR="/data"
WORKDIR /app
VOLUME ${DATA_DIR}
RUN apt-get update -y && apt-get install -y software-properties-common \
    && add-apt-repository ppa:dotnet/backports && apt-get update -y \
    && apt-get install -y aspnetcore-runtime-9.0
COPY publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "MiniTwit.dll", "--urls", "http://0.0.0.0:8080"]
