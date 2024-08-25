# run

export DOCKER_DEFAULT_PLATFORM=linux/amd64
docker build --tag myhome MySmartHomeCore

docker run -p 9000:5000 -v /Users/valda/src/mysmarthome/.out/data:/app/data -v /Users/valda/src/mysmarthome/.out/appsettings.json:/app/appsettings.json myhome
docker run -p 9000:5000 -v /Users/valda/src/mysmarthome/.out/data:/app/data myhome
