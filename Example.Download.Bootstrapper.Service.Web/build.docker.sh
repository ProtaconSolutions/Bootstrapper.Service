if [ -z "$1" ]
  then
    echo "Usage: ./build.docker.sh [image-tag]"
    exit 1
fi

DOCKER_IMAGE_NAME="build/temp/$1"

docker build --no-cache -t ${DOCKER_IMAGE_NAME} -f 'Dockerfile.build' .

CONTAINER_ID=$(docker run -d ${DOCKER_IMAGE_NAME})

docker cp ${CONTAINER_ID}:"/var/download.bootstrapper.service/bin/Debug/netcoreapp2.0" ./dist/

docker rm -f ${CONTAINER_ID} || true
docker rmi -f ${DOCKER_IMAGE_NAME} || true