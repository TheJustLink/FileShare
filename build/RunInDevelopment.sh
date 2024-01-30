cd ../.docker
ENVIRONMENT=Development COMPOSE_PROFILES=$ENVIRONMENT APP_PORT=80 MINIO_ROOT_USER=root MINIO_ROOT_PASSWORD=password MINIO_CONSOLE_PORT=8079 docker compose up --build
read -p "Press enter to resume ..."