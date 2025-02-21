Step 1:
docker run mongo

Step2: 
 docker run -d --network mymongonet --name mymongo -e MONGO_INITDB_ROOT_USERNAME=mongoadmin -e MONGO_INITDB_ROOT_PASSWORD=secret mongo

Step3:
mongosh -u mongoadmin -p secret
show dbs